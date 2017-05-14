using JustRemember_.Helpers;
using JustRemember_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustRemember_.ViewModels
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
