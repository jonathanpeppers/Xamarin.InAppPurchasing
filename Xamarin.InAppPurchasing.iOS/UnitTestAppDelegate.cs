using System.Reflection;
using Foundation;
using UIKit;
using Xunit.Runner;
using Xunit.Sdk;

namespace Xamarin.InAppPurchasing.iOS
{
    [Register("UnitTestAppDelegate")]
    public partial class UnitTestAppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}
