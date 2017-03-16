using Android.App;
using Android.Content;
using Android.OS;
using Com.Android.Vending.Billing;
using System;
using System.Threading.Tasks;

namespace Xamarin.InAppPurchasing.Droid
{
    public class BillingConnection : Java.Lang.Object, IServiceConnection
    {
        private TaskCompletionSource<bool> _connectSource;

        public IInAppBillingService Service
        {
            get;
            private set;
        }

        public async Task Connect()
        {
            var intent = new Intent(BillingConstants.BillingIntent);
            intent.SetPackage(BillingConstants.BillingPackageName);
            var context = Application.Context;
            int intentServicesCount = context.PackageManager.QueryIntentServices(intent, 0).Count;
            if (intentServicesCount != 0)
            {
                var source = 
                    _connectSource = new TaskCompletionSource<bool>();
                context.BindService(intent, this, Bind.AutoCreate);
                await source.Task;
            }
            else
            {
                throw new Exception($"Service {BillingConstants.BillingIntent} not found!");
            }
        }

        public void Disconnect()
        {
            Application.Context.UnbindService(this);
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            try
            {
                Service = IInAppBillingServiceStub.AsInterface(service);

                int response = Service.IsBillingSupported(BillingConstants.ApiVersion, Application.Context.PackageName, BillingConstants.ItemTypeInApp);
                var source = _connectSource;
                if (response == BillingConstants.ResultOk)
                {
                    if (source != null)
                        source.TrySetResult(true);
                }
                else
                {
                    if (source != null)
                        source.TrySetException(new Exception("Billing not supported!"));
                }
            }
            catch (Exception exc)
            {
                var source = _connectSource;
                if (source != null)
                    source.TrySetException(exc);
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Service = null;
        }
    }
}