using DevExpress.Xpf.WindowsUI;
using DevExpress.XtraPrinting.Native.LayoutAdjustment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToolMaster.UserControls {
    /// <summary>
    /// UCLandingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCLandingPage : UserControl {
        public UCLandingPage() {
            InitializeComponent();
            this.txtId.Text = "";
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {

            Setting.Default.isLogin = false;
            string id = this.txtId.Text.Trim();
            string password = this.txtPassword.Text.Trim() ;

            if(id == "" || password == "") {
                WinUIMessageBox.Show(messageBoxText: "아이디나 비밀번호를 입력해주세요.", caption: "로그인", button: MessageBoxButton.OK);
                Setting.Default.isLogin = false;
                return;
            }

            if(Setting.Default.userId_lab.Equals(id) && Setting.Default.password_lab.Equals(password)) {
                MainWindow.GetInstance.btnManageEms.IsEnabled = true;
                MainWindow.GetInstance.btnManageGateway.IsEnabled = true;
                MainWindow.GetInstance.btnLog.IsEnabled = true;

                MainWindow.GetInstance.frameMainWindow.Content = null;
                MainWindow.GetInstance.frameMainWindow.Content = new UCManageEmsDB();

                Setting.Default.isLogin = true;

            } else if(Setting.Default.userId_service.Equals(id) && Setting.Default.password_service.Equals(password)) {
                MainWindow.GetInstance.btnManageEms.IsEnabled = false;
                MainWindow.GetInstance.btnManageGateway.IsEnabled = false;
                MainWindow.GetInstance.btnLog.IsEnabled = true;

                MainWindow.GetInstance.frameMainWindow.Content = null;
                MainWindow.GetInstance.frameMainWindow.Content = new UCLog();

                Setting.Default.isLogin = true;
            }
            //MainWindow.GetInstance.btnMain.IsEnabled = false;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e) {
            MainWindow.GetInstance.frameMainWindow.Content = null;
            MainWindow.GetInstance.frameMainWindow.Content = new UCSettings();
        }
    }
}
