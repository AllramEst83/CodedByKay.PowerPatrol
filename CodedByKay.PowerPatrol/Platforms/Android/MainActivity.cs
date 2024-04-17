using Android.App;
using Android.Content.PM;

namespace CodedByKay.PowerPatrol
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        //protected override void OnCreate(Bundle? savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    RequestedOrientation = ScreenOrientation.Portrait;

        //    // Define the request code for notification permission request
        //    const int requestNotification = 0;

        //    //Fix the wrnings for this code
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.M) // Check for Marshmallow or above for permission check
        //    {
        //        // Since PostNotifications permission is only available on Android 13 and above, we need to conditionally check it
        //        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13
        //        {
        //            string[] notificationPermission = { Manifest.Permission.PostNotifications };

        //            if (CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
        //            {
        //                RequestPermissions(notificationPermission, requestNotification);
        //            }
        //        }
        //    }
        //}
    }
}
