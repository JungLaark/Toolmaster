using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ToolMaster {
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application {

        Mutex myMutex;

        private void Application_Startup(object sender, StartupEventArgs e) {
            bool aIsNewInstance = false;
            myMutex = new Mutex(true, "MyWPFApplication", out aIsNewInstance);
            if (!aIsNewInstance) {
                WinUIMessageBox.Show(messageBoxText: "이미 실행중인 프로그램이 있습니다.", caption: "이미 실행중", button: MessageBoxButton.OK);

                App.Current.Shutdown();
            }

        }

        static App() {

            var splashScreenViewModel = new DXSplashScreenViewModel() {
                Title = "ToolMaster",
                Subtitle = "유지보수 백오피스 프로그램",
                Logo = new Uri("./Resources/ateciot2.png", UriKind.Relative),
                Copyright = "로딩중...",
                Status = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Assembly.GetName().Version.ToString()
                
            };
            SplashScreenManager.Create(() => new SplashScreen1(), splashScreenViewModel).ShowOnStartup();
            Thread.Sleep(5000);
        }
    }
}
