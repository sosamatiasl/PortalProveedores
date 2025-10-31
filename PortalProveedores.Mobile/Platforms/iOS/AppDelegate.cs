using Foundation;
using UIKit;
using ZXing.Net.Maui;

namespace PortalProveedores.Mobile
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Se debe agregar esta línea de código de inicialización si el error persiste en iOS/Simulador:
            // ZXing.Net.Maui.Platform.iOS.Platform.Init(); 

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
