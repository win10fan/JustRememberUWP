using JustRemember_.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.Models
{
    public class SavedSessionModel
    {
        public ObservableCollection<SessionModel> save { get; set; }

        public SavedSessionModel()
        {
            save = new ObservableCollection<SessionModel>();
        }

        public async Task<ObservableCollection<SessionModel>> Load()
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
            {
                await folder.CreateFolderAsync("Sessions");
            }
            if (Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\").Length < 1)
            {
                return new ObservableCollection<SessionModel>();
            }
            var sessionFolder = await folder.GetFolderAsync("Sessions");
            var sessions = await sessionFolder.GetFilesAsync();
            ObservableCollection<SessionModel> allsession = new ObservableCollection<SessionModel>();
            if (sessions != null)
            {
                foreach (var file in sessions)
                {
                    SessionModel ss = new SessionModel();
                    string content = await FileIO.ReadTextAsync(file);
                    ss = await Json.ToObjectAsync<SessionModel>(content);
                    allsession.Add(ss);
                }
            }
            return allsession;
        }

        public async Task Save()
        {
            if (save.Count < 1)
            {
                return;
            }
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
            {
                await folder.CreateFolderAsync("Sessions");
            }
            var sessionFolder = await folder.GetFolderAsync("Sessions");
            foreach (var ss in save)
            {
                string content = await Json.StringifyAsync(ss);
                StorageFile file = await sessionFolder.CreateFileAsync(ss.GeneratedName);
                await FileIO.WriteTextAsync(file, content);
            }
        }

        public static async Task AddNew(SessionModel ss)
        {
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Sessions\\"))
            {
                await folder.CreateFolderAsync("Sessions");
            }
            var sessionFolder = await folder.GetFolderAsync("Sessions");
            string content = await Json.StringifyAsync(ss);
            StorageFile file = await sessionFolder.CreateFileAsync(ss.GeneratedName);
            await FileIO.WriteTextAsync(file, content);
        }
    }
}