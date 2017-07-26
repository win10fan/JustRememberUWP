using System;
using System.Threading.Tasks;

using JustRemember.Activation;

using Microsoft.Services.Store.Engagement;

using Windows.ApplicationModel.Activation;

namespace JustRemember.Services
{
    internal class StoreNotificationsService : ActivationHandler<ToastNotificationActivatedEventArgs>
    {
        public async Task InitializeAsync()
        {
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

        protected override async Task HandleInternalAsync(ToastNotificationActivatedEventArgs args)
        {
            var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(toastActivationArgs.Argument);

            // Use the originalArgs variable to access the original arguments
            // that were passed to the app.

            await Task.CompletedTask;
        }
    }
}
