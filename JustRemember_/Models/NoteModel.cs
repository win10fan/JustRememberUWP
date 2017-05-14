﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace JustRemember_.Models
{
    public struct NoteModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string FirstLine
        {
            get
            {
                //result = Foo.Split(new String[] { "\r\n" }, StringSplitOptions.None);
                var pack = Content.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return pack[0];
            }
        }

        public static async Task<ObservableCollection<NoteModel>> GetNotesAsync()
        {
            if (!Directory.Exists(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\"))
            {
                StorageFolder fol = ApplicationData.Current.RoamingFolder;
                await fol.CreateFolderAsync("Notes");
            }
            if (Directory.GetFiles(ApplicationData.Current.RoamingFolder.Path + "\\Notes\\").Length < 1)
            {
                return new ObservableCollection<NoteModel>();
            }
            StorageFolder folder = ApplicationData.Current.RoamingFolder;
            var noteFolder = await folder.GetFolderAsync("Notes");
            var noteFiles = await noteFolder.GetFilesAsync();
            ObservableCollection<NoteModel> allNote = new ObservableCollection<NoteModel>();
            if (noteFiles != null)
            {
                foreach (var file in noteFiles)
                {
                    NoteModel n3 = new NoteModel();
                    n3.Title = file.DisplayName;
                    n3.Content = await FileIO.ReadTextAsync(file);
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
    }
}
