using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using DevExpress.XtraPrinting.Native.LayoutAdjustment;
using NLog;
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
using ToolMaster.Properties;

namespace ToolMaster.UserControls {
    /// <summary>
    /// UCLandingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCSettings : UserControl {
        Logger logger = null;
        public UCSettings() {
            InitializeComponent();
            logger = LogManager.GetCurrentClassLogger();
            init();
            //this.txtId.Text = "ateciot_lab";
        }

        private void init() {
            this.txtEsnIp.Text = Setting.Default.EsnIp;
            this.txtEsnPort.Text = Setting.Default.EsnPort;
            this.txtEsnId.Text = Setting.Default.EsnId;
            this.txtEsnPw.Text = Setting.Default.EsnPassword;
            this.txtCorePort.Text = Setting.Default.corePort;

            this.txtSshId.Text = Setting.Default.SshId;
            this.txtSshPw.Text = Setting.Default.SshPw;
            this.txtSshPort.Text = Setting.Default.SshPort;

            this.txtDbUserId.Text = Setting.Default.dbUserId;
            this.txtDbName.Text = Setting.Default.dbName;
            this.txtDbPw.Text = Setting.Default.dbPassword;
            this.txtDbPort.Text = Setting.Default.dbPort;
        }

        private bool pingIp(string ipAddr) {
            try {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();

                options.DontFragment = true;

                string strData = "test";
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(strData);

                System.Net.NetworkInformation.PingReply reply = ping.Send(System.Net.IPAddress.Parse(ipAddr), 120, buffer, options);

                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success) {
                    options = null;
                    ping = null;

                    return true;
                } else {
                    options = null;
                    ping = null;

                    return false;
                }

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }

        }


        private void btnOpenLog_Click(object sender, RoutedEventArgs e) {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/logs/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

            if (System.IO.File.Exists(filePath)) {
                System.Diagnostics.Process.Start("NotePad.exe", filePath);
            } else {
                WinUIMessageBox.Show(messageBoxText: "로그 파일이 존재하지 않습니다. 경로를 확인해주세요." + filePath, caption: "로그 파일", button: MessageBoxButton.OK);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {

            if (txtEsnIp.Text == "" || txtEsnPort.Text == "" || txtSshId.Text == "" || txtSshPw.Text == "" || txtSshPort.Text == "" || txtDbUserId.Text == "" || txtDbPw.Text == "" || txtDbName.Text == "" || txtDbPort.Text == "") {
                WinUIMessageBox.Show(messageBoxText: "입력되지 않은 곳이 있습니다. 확인해주세요.", caption: "입력 확인", button: MessageBoxButton.OK);
                return;
            }

            if (!pingIp(this.txtEsnIp.Text)) {
                WinUIMessageBox.Show(messageBoxText: "유효하지 않은 IP입니다. 확인해주세요.", caption: "아이피 확인", button: MessageBoxButton.OK);
                logger.Error(this.txtEsnIp.Text + "은/는 유효하지 않은 IP입니다.");
                return;
            }

            Setting.Default.EsnIp = this.txtEsnIp.Text.Trim();
            Setting.Default.EsnPort = this.txtEsnPort.Text.Trim();
            Setting.Default.EsnId = this.txtEsnId.Text.Trim();
            Setting.Default.EsnPassword = this.txtEsnPw.Text.Trim();
            Setting.Default.corePort = this.txtCorePort.Text.Trim();

            Setting.Default.SshId = this.txtSshId.Text.Trim();
            Setting.Default.SshPw = this.txtSshPw.Text.Trim();
            Setting.Default.SshPort = this.txtSshPort.Text.Trim();

            Setting.Default.dbUserId = this.txtDbUserId.Text.Trim();
            Setting.Default.dbName = this.txtDbName.Text.Trim();
            Setting.Default.dbPassword = this.txtDbPw.Text.Trim();
            Setting.Default.dbPort = this.txtDbPort.Text.Trim();

            Setting.Default.Save();

            WinUIMessageBox.Show(messageBoxText: "설정 저장이 완료되었습니다.", caption: "설정 완료", button: MessageBoxButton.OK);
        }

        //private void btnLogin_Click(object sender, RoutedEventArgs e) {

        //    Setting.Default.isLogin = false;
        //    string id = this.txtId.Text.Trim();
        //    string password = this.txtPassword.Text.Trim() ;

        //    if(id == "" || password == "") {
        //        WinUIMessageBox.Show(messageBoxText: "아이디나 비밀번호를 입력해주세요.", caption: "로그인", button: MessageBoxButton.OK);
        //        Setting.Default.isLogin = false;
        //        return;
        //    }

        //    if(Setting.Default.userId_lab.Equals(id) && Setting.Default.password_lab.Equals(password)) {
        //        MainWindow.GetInstance.btnManageEms.IsEnabled = true;
        //        MainWindow.GetInstance.btnManageGateway.IsEnabled = true;
        //        MainWindow.GetInstance.btnLog.IsEnabled = true;

        //        MainWindow.GetInstance.frameMainWindow.Content = null;
        //        MainWindow.GetInstance.frameMainWindow.Content = new UCManageEmsDB();

        //        Setting.Default.isLogin = true;

        //    } else if(Setting.Default.userId_service.Equals(id) && Setting.Default.password_service.Equals(password)) {
        //        MainWindow.GetInstance.btnManageEms.IsEnabled = false;
        //        MainWindow.GetInstance.btnManageGateway.IsEnabled = false;
        //        MainWindow.GetInstance.btnLog.IsEnabled = true;

        //        MainWindow.GetInstance.frameMainWindow.Content = null;
        //        MainWindow.GetInstance.frameMainWindow.Content = new UCLog();

        //        Setting.Default.isLogin = true;
        //    }

        //}
    }
}
