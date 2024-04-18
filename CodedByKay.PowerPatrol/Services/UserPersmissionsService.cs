using CodedByKay.PowerPatrol.Interfaces;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Font = Microsoft.Maui.Font;
#if ANDROID
using Android.Content;
#endif

namespace CodedByKay.PowerPatrol.Services
{
    public class UserPersmissionsService : IUserPersmissionsService
    {
        private async Task<bool> CheckForPermissionForNetworkState()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.NetworkState>();
            }

            if (status != PermissionStatus.Granted)
            {
                return false;
            }

            return true;
        }
        public async Task<bool> GetPermissionsFromUser(CancellationToken cancellationToken)
        {
            var isNetworkStateGranted = await CheckForPermissionForNetworkState();

            if (isNetworkStateGranted)
            {
                return true;
            }

            return false;
        }
        
    }
}
