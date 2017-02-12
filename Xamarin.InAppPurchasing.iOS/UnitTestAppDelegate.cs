using Foundation;
using UIKit;
using MonoTouch.NUnit.UI;

namespace Xamarin.InAppPurchasing.iOS
{
    [Register("UnitTestAppDelegate")]
    public partial class UnitTestAppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        TouchRunner runner;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            runner = new TouchRunner(window);
            runner.Add(System.Reflection.Assembly.GetExecutingAssembly());
            window.RootViewController = new UINavigationController(runner.GetViewController());
            window.MakeKeyAndVisible();
            return true;
        }
    }
}
