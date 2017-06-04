using JustRemember.Helpers;
using JustRemember.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember.ViewModels
{
    public class AppConfigViewModel : Observable
    {
        public AppConfigModel config;

        public async void Initialie()
        {
            config = await config.Load();
        }
    }
}
