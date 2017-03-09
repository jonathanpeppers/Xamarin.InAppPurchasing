using Android.App;
using Android.Content;
using Android.OS;
using System.Reflection;
using Xunit.Runners.UI;

namespace Xamarin.InAppPurchasing.Droid
{
    [Activity(Label = "Xamarin.InAppPurchasing.Droid", MainLauncher = true, Theme = "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            AddTestAssembly(Assembly.GetExecutingAssembly());

            base.OnCreate(savedInstanceState);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            PurchaseServiceTests.PurchaseService?.HandleActivityResult(requestCode, resultCode, data);
        }
    }
}

