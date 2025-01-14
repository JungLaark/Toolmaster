using DevExpress.Utils.CommonDialogs.Internal;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.WindowsUI;
using DevExpress.Xpo.DB.Helpers;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
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
using System.Windows.Threading;
using ToolMaster.Common;
using ToolMaster.Models;

namespace ToolMaster.UserControls {
    public partial class UCRequestTest : UserControl {
        APICommunication apiCommunication = null;
        DBPostgreSQL dbPostgresSql = null;
        SshConnection sshConnection = null;
        Store store = null;
        SelectedTable selectedTable = null;
        List<Store> listStore = null;
        Logger logger = null;
        bool txtVisiable = true;

        public UCRequestTest() {
            InitializeComponent();
            logger = LogManager.GetCurrentClassLogger();
            apiCommunication = new APICommunication();
        }
        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            txtIpAddress.Text = "192.168.27.77";
            txtIpAddress.Focus();
            //btnConfirm.Click += btnConfirm_Click;
            //btnConfirm_Click(sender, e);
            //gridTableList_MouseLeftButtonUp(sender, e);
            //gridTableList.SelectedItem = 5;
        }


        #endregion

        #region Function
        public void init() {

            Thread.Sleep(1000);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                apiCommunication.login(this.txtIpAddress.Text.Trim());
                //this.mainGrid.ItemsSource = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());
                //listStore = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());

                /*Thread를 생성하고 API 호출을 해야한다.*/
                /*상품 리스트가 있다고 가정하고*/

                //apiCommunication.postDeleteTag(tag.device_id, store.ip_addr, "/esl/met/v2/delete");



                waitIndicator.Visibility = Visibility.Hidden;
            }));
        }

        #endregion
        private void mainGrid_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e) {

        }

        private void mainGrid_SelectionChanged(object sender, DevExpress.Xpf.Grid.GridSelectionChangedEventArgs e) {

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e) {
            //startLoading();
            //apiTest();
            //apiCommunication.login(this.txtIpAddress.Text.Trim());
            /*
                            for (int i=0; i<100; i++) {
                            new Thread(() => {
                                apiTest();
                            }).Start();
                            }*/
            //apiTest();

            Random random = new Random();
            string price = "";

            for(int i=0; i<10000; i++) {
               
                price = random.Next(10000, 30000).ToString();
                /*                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate () {
                                    apiTest(price);
                                });*/

                //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => apiTest(price)));


                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                    apiCommunication.emsLogin("192.168.27.77");
                    apiTest(price);

                }));

            }


        }

        private void apiTest(string price) {
            logger.Info("api요청 시작");
            apiCommunication.requestTest(txtItemCode.Text.Trim(), price.Trim(),"192.168.27.77", "/esl/merchandise/v2/edit");
            logger.Info("api요청 끝");

        }

        private async Task startLoading() {
            this.waitIndicator.Visibility = Visibility.Visible;

            await Task.Run(() =>
                init()
            );
        }

        private void txtIpAddress_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            txtIpAddress.SelectAll();
        }

        private void mainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {


        }

        private void btnItemCode_Click(object sender, RoutedEventArgs e) {

        }
    }
}
