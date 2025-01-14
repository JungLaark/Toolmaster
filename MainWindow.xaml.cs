using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using ToolMaster.UserControls;

namespace ToolMaster {
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : DevExpress.Xpf.Core.ThemedWindow {

        private static volatile MainWindow instance = null;

        public MainWindow() {
            InitializeComponent();
            instance = this;
        }

        public static MainWindow GetInstance {
            set { instance = value; }
            get { return instance; }
        }

        private void btnMain_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCLandingPage();
        }

        private void btnLog_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCLog();
        }

        private void btnManageGateway_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCManageGateway();
        }

        private void btnManageEms_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCManageEmsDB();
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e) {
            SplashScreenManager.CloseAll();
        }

        private void mainWindow_Loaded_1(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            //this.frameMainWindow.Content = new UCLandingPage();
            //this.frameMainWindow.Content = new UCLog();
            //this.frameMainWindow.Content = new UCRequestTest();
            this.frameMainWindow.Content = new UCImportFile();
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCSettings();
        }

        private void btnEms_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCManageEms();
        }

        private void btnExportFile_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCExportFile();
        }

        private void btnImportFile_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCImportFile();
        }

        private void btnRequestTest_Click(object sender, RoutedEventArgs e) {
            this.frameMainWindow.Content = null;
            this.frameMainWindow.Content = new UCRequestTest();
        }
    }
}
