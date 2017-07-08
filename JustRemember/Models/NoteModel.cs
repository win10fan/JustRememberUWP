﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember.Models
{
    public class NoteModel
    {
        public string Title { get; set; }
        public string Content { get; set; }

		public NoteModel()
		{
			Title = "Untitled";
			Content = "";
		}

		[JsonIgnore]
		public string FirstLine
		{
			get
			{
				if (Mode == noteMode.qa)
				{
					if (lines[lines.Length - 1].StartsWith("A="))
						return lines[1];
					return lines[1].Substring(0,lines[1].Length - 3);
				}
				else if (Mode == noteMode.volc)
				{
					return lines[1];
				}
				return lines[0];
			}
		}

		[JsonIgnore]
		string[] lines
		{
			get
			{
				return Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			}
		}

		[JsonIgnore]
		public noteMode Mode
		{
			get
			{
				if (lines[0] == "#MODE=EXAM")
				{
					return noteMode.qa;
				}
				else if (lines[0] == "#MODE=VOLC")
				{
					return noteMode.volc;
				}
				return noteMode.none;
			}
		}

        public static async Task<ObservableCollection<NoteModel>> GetNotesAsync()
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\"))
            {
                await folder.CreateFolderAsync("Notes");
            }
            if (Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\").Length < 1)
            {
                return new ObservableCollection<NoteModel>();
            }
            var noteFolder = await folder.GetFolderAsync("Notes");
            var noteFiles = await noteFolder.GetFilesAsync();
            ObservableCollection<NoteModel> allNote = new ObservableCollection<NoteModel>();
            if (noteFiles != null)
            {
                foreach (var file in noteFiles)
                {
					NoteModel n3 = new NoteModel()
					{
						Title = file.DisplayName,
						Content = await FileIO.ReadTextAsync(file)
					};
					allNote.Add(n3);
                }
            }
            return allNote;
        }

        public static async Task DeleteNote(string name)
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            var noteFolder = await folder.GetFolderAsync("Notes");
            var noteFiles = await noteFolder.GetFileAsync($"{name}.txt");
            if (noteFiles != null)
            {
                await noteFiles.DeleteAsync();
            }
        }

		public static async Task SaveNote(NoteModel note)
		{
			var folder = (StorageFolder) await ApplicationData.Current.RoamingFolder.TryGetItemAsync("Notes");
			if (folder != null)
			{
				//Folder exist
				var nf = (StorageFile)await folder.TryGetItemAsync(note.Title);
				if (nf == null)
				{
					//No file
					nf = await folder.CreateFileAsync(note.Title);
				}
				else
				{
					await nf.DeleteAsync();
					if (note.Title.EndsWith(".txt"))
					{
						nf = await folder.CreateFileAsync(note.Title);
					}
					else
					{
						nf = await folder.CreateFileAsync($"{note.Title}.txt");
					}
				}
				await FileIO.WriteTextAsync(nf, note.Content);
			}
		}

		public static async Task<NoteModel> GetOneNoteButNotMicrosoftOneNoteButOneOfANoteWithParticularPath(string path)
		{
			StorageFile fol = await StorageFile.GetFileFromPathAsync(path);
			string displayName = fol.DisplayName;
			if (displayName.EndsWith(".txt"))
			{
				displayName = displayName.Replace(".txt", "");
			}
			while (char.IsNumber(displayName[0]))
			{
				displayName = displayName.Remove(0, 1);
			}
			NoteModel datNote = new NoteModel()
			{
				Title = displayName,
				Content = await FileIO.ReadTextAsync(fol)
			};
			return datNote;
		}
	}

	public enum noteMode
	{
		none,
		qa,
		volc
	}
}