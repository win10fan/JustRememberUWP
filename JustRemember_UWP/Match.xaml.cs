using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace JustRemember_UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Match : Page
	{
		public DispatcherTimer timerNow;
		public Match()
		{
			this.InitializeComponent();
			timerNow = new DispatcherTimer();
			timerNow.Interval = TimeSpan.FromMilliseconds(100);
			timerNow.Tick += TimerNow_Tick;
			timerNow.Start();
			//Over dialog
			overMSG = new MessageDialog("Try again?", "Time up.");
			overMSG.Commands.Add(new UICommand("OK"){ Invoked = delegate { ResetRound(); } });
			//Load progress lose dialog
			lostPG = new MessageDialog("If you continue.\nCurrent progress will get replaced.\nContinue?", "Warning!");
			lostPG.Commands.Add(new UICommand("Continue") { Invoked = delegate { Utilities.isSmallLoaderMode = false; /*TODO:Reload new file*/} });
			lostPG.Commands.Add(new UICommand("Cancel") { Id = 0 });
			lostPG.CancelCommandIndex = 0;
			//Load file
			//TODO:Add load file function. So it can work with otherpage
			LoadFile(Utilities.selected.Title, Utilities.selected.Content);
			randomEngine = new Random();
		}
		MessageDialog overMSG;
		MessageDialog lostPG;
		Random randomEngine;
		#region Match component
		public int currentProgress;
		public string currentFilename;
		public List<textlist> textList = new List<textlist>();
		public List<string> choiceList = new List<string>();
		#endregion
		#region UI Controller
		bool _pse;
		public bool Pause
		{
			get
			{
				return _pse;
			}
			set
			{
				_pse = value;
				if (value)
				{
					pauseButton.Content = "Paused...";
				}
				else
				{
					pauseButton.Content = currentFilename;
				}
			}
		}
		#endregion
		
		public void LoadFile(string content)
		{
			LoadFile("", content);
		}

		public void LoadFile(string filename, string content)
		{
			//Update title
			currentFilename = filename;
			//Load file normal
			textList.Clear();
			string replacedText = content.ReplaceForMatch();
			List<string> txtList = new List<string>();
			txtList.Clear();
			txtList.AddRange(replacedText.Split(' '));
			txtList.RemoveAll(item => item == " " || item == "\n" || item == "\t");
			foreach (var i in txtList)
			{
				if (i.EndsWith("█"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.space));
				}
				else if (i.EndsWith("▼"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.newline));
				}
				else if (i.EndsWith("→"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.tab));
				}
				else
				{
					textList.Add(new textlist(i));
				}
			}
			var plainTxt = new List<string>();
			foreach (textlist t in textList)
			{
				plainTxt.Add(t.ToString());
			}
			//textList.ForEach(item => plainTxt.Add(item.ToString()));
			HashSet<string> hash = new HashSet<string>(plainTxt);
			choiceList.Clear();
			choiceList.AddRange(hash);
			//
			ResetRound();
		}

		public void ResetRound()
		{
			Utilities.newStat = new statInfo();
			Utilities.newStat.dateandTime = DateTime.Now.ToString(@"dd MM yyyy - hh:mm:ss");
			Utilities.newStat.currentMode = Utilities.currentSettings.defaultMode;
			progressCounter.Value = 0;
			progressCounter.Maximum = textList.Count;
			currentProgress = -1;
			//Update choice
			//TODO: Add choice update
			//SpawnChoice(-1, langManager.GetTextValue("ToonWK.main.ready"));
			currentChoiceMode = mode.begin;
			CheckTotalChoice();
			dpTxtcontrol.Items.Clear();
			selectedList?.Clear();
			selectedList = new List<string>();
			timeCounterText.Text = "--:--\r\n--:--";
			wrongCounter.Text = "0";
			for (int i = 0; i < textList.Count; i++)
			{
				Utilities.newStat.wrongPerchoice.Add(0);
			}
			Utilities.newStat.totalLimitTime = Utilities.currentSettings.limitTime;
			Utilities.newStat.totalWords = textList.Count;
			Utilities.newStat.totalChoice = Utilities.currentSettings.totalChoice;
			Utilities.newStat.useTimeLimit = Utilities.currentSettings.isLimitTime;
			//Make it support seed system
			randomEngine = new Random();
		}

		enum mode { begin, normal, end}
		mode currentChoiceMode;

		public void CheckTotalChoice()
		{
			//Condition
			//First choice
			if (currentChoiceMode != mode.normal)
			{
				ch0.Visibility = ch1.Visibility = ch2.Visibility = ch3.Visibility = ch4.Visibility = Visibility.Collapsed;
				chImportant.Visibility = Visibility.Visible;
			}
			else if (currentChoiceMode == mode.normal)
			{
				//Other
				ch0.Visibility = Utilities.currentSettings.totalChoice >= 1 ? Visibility.Visible : Visibility.Collapsed;
				ch1.Visibility = Utilities.currentSettings.totalChoice >= 2 ? Visibility.Visible : Visibility.Collapsed;
				ch2.Visibility = Utilities.currentSettings.totalChoice >= 3 ? Visibility.Visible : Visibility.Collapsed;
				ch3.Visibility = Utilities.currentSettings.totalChoice >= 4 ? Visibility.Visible : Visibility.Collapsed;
				ch4.Visibility = Utilities.currentSettings.totalChoice >= 5 ? Visibility.Visible : Visibility.Collapsed;
				chImportant.Visibility = Visibility.Collapsed;
			}
		}

		private async void TimerNow_Tick(object sender, object e)
		{
			if (pauseMenu.IsPaneOpen)
			{
				//Paused
				pauseButton.Content = "Paused...";
				return;
			}
			if (currentProgress <= 0 || currentProgress >= textList.Count - 1)
			{
				//Paused
				pauseButton.Content = currentFilename;
				return;
			}
			if (Utilities.newStat == null)
			{
				//Game not ready yet!
				return;
			}
			//Update time
			if (Utilities.currentSettings.isLimitTime)
			{
				Utilities.newStat.totalTime += 0.1f;
				timeCounterText.Text = Utilities.newStat.totalTime.ToStringAsTime() + System.Environment.NewLine + Utilities.newStat.totalLimitTime.ToStringAsTime();
			}
			else
			{
				timeCounterText.Text = "--:--" + Environment.NewLine + "--:--";
			}
			wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
			if (Utilities.newStat.totalTime >= Utilities.newStat.totalLimitTime)
			{
				//OVER
				timerNow.Stop();
				await overMSG.ShowAsync();
				timerNow.Start();
				//TODO:Add end page
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			pauseMenu.IsPaneOpen = !pauseMenu.IsPaneOpen;
			Pause = pauseMenu.IsPaneOpen;
			if (pauseMenu.IsPaneOpen)
			{
				pauseButton.Content = Pause ? "Paused" : currentFilename;
			}
		}

		private void HideSecondPage_Click(object sender, RoutedEventArgs e)
		{
			otherPage.Visibility = Visibility.Collapsed;
			HideSecondPage.Visibility = Visibility.Collapsed;
			Utilities.isSmallLoaderMode = false;
		}

		void ActivateOtherpage()
		{
			otherPage.Visibility = Visibility.Visible;
			HideSecondPage.Visibility = Visibility.Visible;
			Utilities.isSmallLoaderMode = otherPage.Visibility == Visibility.Visible;
		}

		private void loadOther_Click(object sender, RoutedEventArgs e)
		{
			if (Utilities.isSmallLoaderMode)
			{
				return;
			}
			ActivateOtherpage();
			//
			otherPage.Navigate(typeof(Selector));
		}

		private void loadSession_Click(object sender, RoutedEventArgs e)
		{
			if (Utilities.isSmallLoaderMode)
			{
				return;
			}
			ActivateOtherpage();
			//
			////TODO:Load session page
		}

		private void settingPage_Click(object sender, RoutedEventArgs e)
		{
			if (Utilities.isSmallLoaderMode)
			{
				return;
			}
			ActivateOtherpage();
			//
			otherPage.Navigate(typeof(QuickSettings));
		}

		private async void mainmenuPage_Click(object sender, RoutedEventArgs e)
		{
			if (currentProgress < 1 || currentProgress > textList.Count - 1)
			{
				Frame.Navigate(typeof(MainPage));
				Utilities.isSmallLoaderMode = false;
			}
			else
			{
				await lostPG.ShowAsync();
			}
		}

		private void exitApp_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Exit();
		}

		public int currentValidChoice;

		int GetRealChoice(List<string> ncl)
		{
			int real = 0;
			foreach (string s in ncl)
			{
				if (s == textList[currentProgress].Text)
				{
					return ncl.IndexOf(s);
				}
			}
			return real;
		}

		void SpawnChoice(int num, string customText)
		{
			//Update choice text
			switch (num)
			{
				case 0:
					ch0.Content = customText;
					break;
				case 1:
					ch1.Content = customText;
					break;
				case 2:
					ch2.Content = customText;
					break;
				case 3:
					ch3.Content = customText;
					break;
				case 4:
					ch4.Content = customText;
					break;
				default:
					break;
			}
		}

		void SpawnNewChoices()
		{
			currentValidChoice = randomEngine.Next(0, Utilities.currentSettings.totalChoice - 1);
			List<string> newchoiceL = new List<string>(choiceList);
			newchoiceL.RemoveAt(GetRealChoice(newchoiceL));
			for (int i = 0; i < Utilities.currentSettings.totalChoice; i++)
			{
				if (i == currentValidChoice)
				{
					SpawnChoice(i, textList[currentProgress].Text);
				}
				else
				{
					int rndch = randomEngine.Next(0, newchoiceL.Count);
					SpawnChoice(i, newchoiceL[rndch]);
					newchoiceL.RemoveAt(rndch);
				}
			}
		}

		public void ChooseChoice(int choice)
		{
			if (choice < 0)
			{
				//Command choice
				if (choice == -1)
				{
					currentProgress += 1;
					SpawnNewChoices();
					currentChoiceMode = mode.normal;
					CheckTotalChoice();
				}
				else if (choice == -5)
				{
					//TODO:Function to Goto end page
					Utilities.currentSettings.stat.Add(Utilities.newStat);
					Frame.Navigate(typeof(End));
					//endPage.SetActive(true);
					//end_primaryButton.Select();
					//Update ending page
					//List<Vector2> graphList = new List<Vector2>();
					//for (int i = 0; i < Utilities.newStat.wrongPerchoice.Count; i++)
					//{
					//	float xsize = (250 / Utilities.newStat.totalWords) * i;
					//	float ysize = (120 / Utilities.newStat.totalChoice) * Utilities.newStat.wrongPerchoice[i];
					//	graphList.Add(new Vector2(xsize, ysize));
					//}
					//endGraphWPC.Points = graphList;
					//endGraphWPC.SetAllDirty();
					//string timeTotal = "N/A";
					//string timeUsed = "N/A";
					//if (current.isLimitTime)
					//{
					//	timeUsed = Utilities.newStat.totalTime.ToStringAsTime();
					//	timeTotal = Utilities.newStat.totalLimitTime.ToStringAsTime();
					//}
					//endStatDisplay.text = string.Format(langManager.GetTextValue("ToonWK.end.detail"), Utilities.newStat.totalWords, Utilities.newStat.totalWrong, timeUsed, timeTotal);
				}
			}
			//Check choice number range
			else if (choice > -1)
			{
				//Normal
				//
				//Check if right or wrong
				if (choice == currentValidChoice)
				{
					//It right
					//Update text display
					AddMainText(textList[currentProgress].Text, textList[currentProgress].Command);
					//Consider to move next
					if (currentProgress + 1 > textList.Count - 1)
					{
						//It can't go anymore
						//Spawn Ending choice;
						currentChoiceMode = mode.end;
						CheckTotalChoice();
					}
					else
					{
						//It still can go. Move next
						currentProgress += 1;
						SpawnNewChoices();
						currentChoiceMode = mode.normal;
						CheckTotalChoice();
					}
				}
				else
				{
					if (Utilities.newStat.currentMode == challageMode.Easy)
					{
						//Move to next
						Utilities.newStat.wrongPerchoice[currentProgress] += 1;
						wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
						Utilities.newStat.totalTime += 1;
						//UpdateDisplay
						AddMainText(textList[currentProgress].Text, textList[currentProgress].Command, true);
						//Move next
						if (currentProgress + 1 > textList.Count - 1)
						{
							//It can't go anymore
							//Spawn Ending choice;
							CheckTotalChoice();
							ch0.Visibility = ch1.Visibility = ch2.Visibility = ch3.Visibility = ch4.Visibility = Visibility.Collapsed;
							chImportant.Visibility = Visibility.Visible;
						}
						else
						{
							//It still can go. Move next
							currentProgress += 1;
							SpawnNewChoices();
						}
					}
					else if (Utilities.newStat.currentMode == challageMode.Normal)
					{
						//Hide wrong choice
						switch (choice)
						{
							case 0:
								ch0.Visibility = Visibility.Collapsed;
								break;
							case 1:
								ch1.Visibility = Visibility.Collapsed;
								break;
							case 2:
								ch2.Visibility = Visibility.Collapsed;
								break;
							case 3:
								ch3.Visibility = Visibility.Collapsed;
								break;
							case 4:
								ch4.Visibility = Visibility.Collapsed;
								break;
						}
						//It wrong
						Utilities.newStat.wrongPerchoice[currentProgress] += 1;
						wrongCounter.Text = Utilities.newStat.totalWrong.ToString();
						Utilities.newStat.totalTime += 1;
					}
					else if (Utilities.newStat.currentMode == challageMode.Hard)
					{
						ResetRound();
					}
				}
				progressCounter.Value = currentProgress;
			}
		}
		//TODO:Add prenote system???!?!?!!!?

		public List<string> selectedList { get; set; }		
		public Paragraph lastmodifyParagraph;
		void AddMainText(string text, cmd command, bool isItWrongChoice)
		{
			if (isItWrongChoice)
			{
				dpTxtcontrol.Items.Add(text);
			}
			else
			{
				if (Utilities.currentSettings.showWrongContent)
				{
					dpTxtcontrol.Items.Add($"*{text}*");
				}
				else
				{
					dpTxtcontrol.Items.Add(text.ObscureText());
				}
			}
			//TODO:Add a way to add textblock into ItemsControl or something
			//Add text to richtext
			//if (command == cmd.tab)
			//{
			//	lastmodifyParagraph = new Paragraph();
			//	if (isItWrongChoice)
			//	{
			//		lastmodifyParagraph.Foreground = new SolidColorBrush((Color)Resources["SystemAccentColor"]);
			//	}
			//	else
			//	{
			//		lastmodifyParagraph.Margin = new Thickness(10, 0, 0, 0);
			//	}
			//	mainDisplayText.Blocks.Add(lastmodifyParagraph);
			//}
			//Run context = new Run();
			//if (isItWrongChoice)
			//{
			//	if (Utilities.currentSettings.showWrongContent)
			//	{
			//		context.Text = text;
			//	}
			//	else
			//	{
			//		context.Text = text.ObscureText();
			//	}
			//}
			//else
			//{
			//	context.Text = text;
			//}
			//context.Text += " ";
			//lastmodifyParagraph.Inlines.Add(context);
			//mainDisplayText.Blocks.RemoveAt(mainDisplayText.Blocks.Count - 1);
			//mainDisplayText.Blocks.Add(lastmodifyParagraph);
			////Check if user want to show wrong content or not
			////Format text to accent color at wrong choice
			//if (command == cmd.newline)
			//{
			//	lastmodifyParagraph.Inlines.Add(new LineBreak());
			//}
			//Scroll text to bottom
			//TODO:Add setting... Automatic scroll text to bottom
			displayTextScroll.ScrollToVerticalOffset(displayTextScroll.ExtentHeight);
		}

		void AddMainText(string text,cmd command)
		{
			AddMainText(text, command, false);
		}

		void AddMainText(string text)
		{
			AddMainText(text, cmd.space,false);
		}		
		
		private void ch0_Click(object sender, RoutedEventArgs e)
		{
			ChooseChoice(0);
		}

		private void ch1_Click(object sender, RoutedEventArgs e)
		{
			ChooseChoice(1);
		}

		private void ch2_Click(object sender, RoutedEventArgs e)
		{
			ChooseChoice(2);
		}

		private void ch3_Click(object sender, RoutedEventArgs e)
		{
			ChooseChoice(3);
		}

		private void ch4_Click(object sender, RoutedEventArgs e)
		{
			ChooseChoice(4);
		}

		private void chImportant_Click(object sender, RoutedEventArgs e)
		{
			if (currentProgress <= 0)
			{
				ChooseChoice(-1);
			}
			else
			{
				ChooseChoice(-5);
			}
		}
	}

	public class Main
	{
		/*#region Match system
		
		public static void LoadFile(string content)
		{
			LoadFile("", content);
		}
		
		public void LoadFile(string filename, string content)
		{
			currentFilename = filename;
			//Load file normal
			Debug.Log("Load file!");
			textList.Clear();
			string replacedText = content.ReplaceForMatch();
			List<string> txtList = new List<string>();
			txtList.Clear();
			txtList.AddRange(replacedText.Split(' '));
			txtList.RemoveAll(item => item == " " || item == "\n" || item == "\t");
			foreach (var i in txtList)
			{
				if (i.EndsWith("█"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.space));
				}
				else if (i.EndsWith("▼"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.newline));
				}
				else if (i.EndsWith("→"))
				{
					textList.Add(new textlist(i.Substring(0, i.Length - 1), cmd.tab));
				}
				else
				{
					textList.Add(new textlist(i));
				}
			}
			var plainTxt = new List<string>();
			foreach (textlist t in textList)
			{
				plainTxt.Add(t.ToString());
			}
			//textList.ForEach(item => plainTxt.Add(item.ToString()));
			HashSet<string> hash = new HashSet<string>(plainTxt);
			choiceList.Clear();
			choiceList.AddRange(hash);
			//
			showPrenote = false;
			SetPage(page.main);
			mainPageTitleText.text = filename;
			//
			ResetRound();
		}

		public Text mainPageTitleText;

		public statInfo newStat = new statInfo();
		public Image mainPageProgressbar;
		public RectTransform mainPageChoiceParent;
		public Text mainDisplayText;
		public Text mainTimerDisplay;
		public Text mainTotalWrongNow;
		public void ResetRound()
		{
			newStat = new statInfo();
			newStat.dateandTime = System.DateTime.Now.ToString(@"dd MM yyyy - hh:mm:ss");
			newStat.currentMode = current.defaultMode;
			mainPageProgressbar.fillAmount = 0;
			currentProgress = -1;
			KillChild(mainPageChoiceParent);
			SpawnChoice(-1, langManager.GetTextValue("ToonWK.main.ready"));
			mainDisplayText.text = "";
			mainTimerDisplay.text = "--:--\r\n--:--";
			for (int i = 0; i < textList.Count; i++)
			{
				newStat.wrongPerchoice.Add(0);
			}
			mainTotalWrongNow.text = newStat.totalWrong.ToString();
			newStat.totalLimitTime = current.limitTime;
			newStat.totalWords = textList.Count;
			newStat.totalChoice = current.totalChoice;
			newStat.useTimeLimit = current.isLimitTime;
			Pause_SaveSessionButton.interactable = true;
		}

		public GameObject choicePrototype;
		public int currentValidChoice;
		void SpawnChoice(int num, string customText)
		{
			GameObject go = Instantiate(choicePrototype);
			go.transform.SetParent(mainPageChoiceParent);
			go.transform.localScale = Vector3.one;
			go.name = "CHSpecific" + num;
			Button btn = go.GetComponent<Button>();
			Text txt = go.transform.FindChild("ChoiceText").GetComponent<Text>();
			txt.text = customText;
			if (Console.log.useHint || currentProgress == 0)
			{
				if (num == currentValidChoice && currentProgress >= 0)
				{
					txt.text = string.Format(">> {0}", txt.text);
				}
			}
			btn.onClick.AddListener(delegate { ChooseChoice(num); });
		}

		void SpawnNewChoices()
		{
			KillChild(mainPageChoiceParent);
			currentValidChoice = Random.Range(0, current.totalChoice);
			List<string> newchoiceL = new List<string>(choiceList);
			newchoiceL.RemoveAt(GetRealChoice(newchoiceL));
			for (int i = 0; i < current.totalChoice; i++)
			{
				if (i == currentValidChoice)
				{
					SpawnChoice(i, textList[currentProgress].Text);
				}
				else
				{
					int rndch = Random.Range(0, newchoiceL.Count);
					SpawnChoice(i, newchoiceL[rndch]);
					newchoiceL.RemoveAt(rndch);
				}
			}
			settingPage.RefreshTheme();
		}

		int GetRealChoice(List<string> ncl)
		{
			int real = 0;
			foreach (string s in ncl)
			{
				if (s == textList[currentProgress].Text)
				{
					return ncl.IndexOf(s);
				}
			}
			return real;
		}
		public UnityEngine.UI.Extensions.UILineRenderer endGraphWPC;
		public Text endStatDisplay;
		public void ChooseChoice(int choice)
		{
			if (choice < 0)
			{
				//Command choice
				if (choice == -1)
				{
					currentProgress += 1;
					SpawnNewChoices();
				}
				else if (choice == -5)
				{
					//Remove all choice
					KillChild(mainPageChoiceParent);
					//Goto end page
					endPage.SetActive(true);
					end_primaryButton.Select();
					//Update ending page
					List<Vector2> graphList = new List<Vector2>();
					for (int i = 0; i < newStat.wrongPerchoice.Count; i++)
					{
						float xsize = (250 / newStat.totalWords) * i;
						float ysize = (120 / newStat.totalChoice) * newStat.wrongPerchoice[i];
						graphList.Add(new Vector2(xsize, ysize));
					}
					endGraphWPC.Points = graphList;
					endGraphWPC.SetAllDirty();
					string timeTotal = "N/A";
					string timeUsed = "N/A";
					if (current.isLimitTime)
					{
						timeUsed = newStat.totalTime.ToStringAsTime();
						timeTotal = newStat.totalLimitTime.ToStringAsTime();
					}
					endStatDisplay.text = string.Format(langManager.GetTextValue("ToonWK.end.detail"), newStat.totalWords, newStat.totalWrong, timeUsed, timeTotal);
					EndRound();
				}
			}
			//Check choice number range
			else if (choice > -1)
			{
				if (Console.log.neverWrong)
				{
					choice = currentValidChoice;
				}
				//Normal
				//
				//Check if right or wrong
				if (choice == currentValidChoice)
				{
					//It right
					//Update text display
					mainDisplayText.text += textList[currentProgress].Text + textList[currentProgress].Command.Translate();
					//Consider to move next
					if (currentProgress + 1 > textList.Count - 1)
					{
						//It can't go anymore
						//Spawn Ending choice;
						KillChild(mainPageChoiceParent);
						SpawnChoice(-5, langManager.GetTextValue("ToonWK.main.end"));
					}
					else
					{
						//It still can go. Move next
						currentProgress += 1;
						SpawnNewChoices();
					}
				}
				else
				{
					if (newStat.currentMode == challageMode.Easy)
					{
						//Move to next
						newStat.totalWrong += 1;
						newStat.wrongPerchoice[currentProgress] += 1;
						mainTotalWrongNow.text = newStat.totalWrong.ToString();
						mainWrongAnim.Play();
						newStat.totalTime += 1;
						//UpdateDisplay
						if (!current.showWrongContent)
						{
							string qcCount = "";
							for (int i = 0; i < textList[currentProgress].Text.Length; i++)
							{
								qcCount += "?";
							}
							mainDisplayText.text += string.Format("<color=red>{0}</color>", qcCount);
						}
						else
						{
							mainDisplayText.text += string.Format("<color=red>{0}</color>", textList[currentProgress].Text);
						}
						mainDisplayText.text += textList[currentProgress].Command.Translate();
						//Move next
						if (currentProgress + 1 > textList.Count - 1)
						{
							//It can't go anymore
							//Spawn Ending choice;
							KillChild(mainPageChoiceParent);
							SpawnChoice(-5, langManager.GetTextValue("ToonWK.main.end"));
						}
						else
						{
							//It still can go. Move next
							currentProgress += 1;
							SpawnNewChoices();
						}
					}
					else if (newStat.currentMode == challageMode.Normal)
					{
						//Kill itself.
						Destroy(GameObject.Find("CHSpecific" + choice));
						//It wrong
						newStat.totalWrong += 1;
						newStat.wrongPerchoice[currentProgress] += 1;
						mainTotalWrongNow.text = newStat.totalWrong.ToString();
						mainWrongAnim.Play();
						newStat.totalTime += 1;
					}
					else if (newStat.currentMode == challageMode.Hard)
					{
						ResetRound();
					}
				}
				float f = System.Convert.ToSingle(currentProgress) / System.Convert.ToSingle(textList.Count - 1);
				mainPageProgressbar.fillAmount = f;
			}
		}
		public Animation mainWrongAnim;

		public void ConfirmResetMatch()
		{
			if (currentProgress < 0)
			{
				ResetRound();
			}
			else
			{
				MsgBox.Show.Dialog("Reset match", "Are you sure?", delegate { ResetRound(); Pause = false; }, delegate { Pause = false; });
			}
		}

		public void EndRound()
		{
			if (!Console.log.wasCheating)
			{
				current.stat.Add(newStat);
			}
			ResetRound();
		}

		
		#endregion
		
		#region Session
		public void LoadFile(SessionInfo info)
		{
			textList = info.lastTextList;
			choiceList = info.choiceList;
			SetPage(page.main);
			mainPageTitleText.text = info.LastTitle;
			//Restore info
			//ResetRound 
			newStat = info.current;
			mainPageProgressbar.fillAmount = info.progressFillAmount;
			currentProgress = info.currentAt;
			KillChild(mainPageChoiceParent);
			for (int i = 0; i < info.lastChoices.Count; i++)
			{
				SpawnChoice(i, info.lastChoices[i]);
			}
			currentValidChoice = info.lastCorrect;
			for (int i = 0; i < currentProgress; i++)
			{
				if (newStat.wrongPerchoice[i] > 0)
				{
					if (current.showWrongContent)
					{
						mainDisplayText.text += "<color=red>" + textList[i] + "</color>" + textList[i].Command.Translate();
					}
					else
					{
						mainDisplayText.text += "<color=red>" + textList[i].Text.ObscureText() + "</color>" + textList[i].Command.Translate();
					}
				}
				else if (newStat.wrongPerchoice[i] == 0)
				{
					mainDisplayText.text += textList[i] + textList[i].Command.Translate();
				}
			}
			if (newStat.useTimeLimit) { mainTimerDisplay.text = string.Format("{0}\r\n{1}", newStat.totalTime.ToStringAsTime(), newStat.totalLimitTime.ToStringAsTime()); }
			else { mainTimerDisplay.text = "--:--\r\n--:--"; }
			mainTotalWrongNow.text = newStat.totalWrong.ToString();
		}
		public Button Pause_SaveSessionButton;
		public void SaveSession()
		{
			if (currentProgress < 0 || currentProgress > textList.Count - 1)
			{
				return;
			}
			Pause_SaveSessionButton.interactable = false;
			Sessions.Add(new SessionInfo(n));
			SessionInfo.Save(Sessions, Application.persistentDataPath + "/sessions.txt");
		}
		public RectTransform sessionParent;
		public GameObject noSessionPrototype;
		public GameObject sessionPrototype;
		bool _sdm;
		public bool sessionDeleteMode
		{
			get
			{
				return _sdm;
			}
			set
			{
				LoadSessionList();
				_sdm = value;
			}
		} //false=Open | true=Delete

		public void LoadSessionList()
		{
			LoadSessionList(sessionDeleteMode);
		}

		public void LoadSessionList(bool mode)
		{
			Sessions.Clear();
			Sessions = SessionInfo.Load(Application.persistentDataPath + "/sessions.txt");
			KillChild(sessionParent);
			foreach (SessionInfo sif in Sessions)
			{
				SpawnSessionList(sif, mode);
			}
			if (Sessions.Count < 1)
			{
				GameObject go = Instantiate(noSessionPrototype);
				go.transform.SetParent(sessionParent);
				go.transform.localScale = Vector3.one;
				sessionParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 90);
				settingPage.RefreshTheme();
			}
			else if (Sessions.Count > 0)
			{
				sessionParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sessionParent.childCount * 85);
				settingPage.RefreshTheme();
			}
		}

		public void SpawnSessionList(SessionInfo info, bool mode)
		{
			GameObject go = Instantiate(sessionPrototype);
			go.transform.SetParent(sessionParent);
			go.transform.localScale = Vector3.one;
			Image lastProgress = go.transform.FindChild("LastProgress").GetComponent<Image>();
			Image totalWrong = lastProgress.transform.FindChild("WrongChoice").GetComponent<Image>();
			Text txt = go.transform.FindChild("text").GetComponent<Text>();
			Button loadses = go.GetComponent<Button>();
			lastProgress.fillAmount = info.progressFillAmount;
			int wrongCount = info.current.totalWrong;
			totalWrong.fillAmount = wrongCount / info.current.totalWords;
			txt.text = string.Format("<size=38>{0}</size>\n{1}", info.LastTitle, info.current.dateandTime);
			if (info.current.currentMode == challageMode.Easy)
			{
				lastProgress.color = new Color(0, 0.6f, 0);
			}
			else if (info.current.currentMode == challageMode.Normal)
			{
				lastProgress.color = new Color(0, 0, 0.6f);
			}
			else if (info.current.currentMode == challageMode.Hard)
			{
				lastProgress.color = new Color(0.4f, 0, 0);
			}
			if (!mode)
			{
				loadses.onClick.AddListener(delegate { LoadFile(info); Sessions.RemoveAt(Sessions.IndexOf(info)); Pause_SaveSessionButton.interactable = true; });
			}
			else
			{
				loadses.onClick.AddListener(delegate { Sessions.RemoveAt(Sessions.IndexOf(info)); LoadSessionList(); });
			}
		}
		#endregion

		#region Other
		public void RebuildTextDisplay()
		{
			if (newStat == null || currentPage != page.main || currentPage != page.set)
			{
				return;
			}
			mainDisplayText.text = "";
			for (int i = 0; i < currentProgress; i++)
			{
				if (newStat.wrongPerchoice[i] >= 1)
				{
					//Wrong
					if (!current.showWrongContent)
					{
						mainDisplayText.text += textList[i].Text.ObscureText(true);
					}
					else
					{
						mainDisplayText.text += string.Format("<color=#{1}>{0}</color>", textList[i].Text, ColorUtility.ToHtmlStringRGB(AppTheme.current.Accent));
					}
				}
				else
				{
					//Not wrong
					mainDisplayText.text += textList[i].Text;
				}
				mainDisplayText.text += textList[i].Command.Translate();
			}
		}
		#endregion*/
	}
}
