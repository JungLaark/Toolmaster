using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.WindowsUI;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    public partial class UCManageEms : UserControl {
        APICommunication apiCommunication = null;
        DBPostgreSQL dbPostgresSql = null;
        SshConnection sshConnection = null;
        Store store = null;
        SelectedTable selectedTable = null;
        List<Store> listStore = null;
        Logger logger = null;
        bool txtVisiable = true;

        public UCManageEms() {
            InitializeComponent();
            logger = LogManager.GetCurrentClassLogger();
            apiCommunication = new APICommunication();
        }
        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            txtIpAddress.Text = Setting.Default.EsnIp.Trim();
            txtIpAddress.Focus();
            //btnConfirm.Click += btnConfirm_Click;
            btnConfirm_Click(sender, e);
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
                listStore = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());

                /*너무 느리단말이지....*/
                mainGrid.ItemsSource = getDbReplication(listStore);

                if (mainGrid.ItemsSource != null) {
                    this.txtStoreCount.Text = "총 매장 개수 " + this.mainGrid.VisibleRowCount.ToString() + "개";
                } else {
                    WinUIMessageBox.Show(messageBoxText: "조회된 데이터가 없습니다. 아이피 및 포트를 확인하세요.", caption: "아이피나 포트 확인", button: MessageBoxButton.OK);
                }

                waitIndicator.Visibility = Visibility.Hidden;
            }));


        }


        #endregion
        private void mainGrid_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e) {

        }

        private void mainGrid_SelectionChanged(object sender, DevExpress.Xpf.Grid.GridSelectionChangedEventArgs e) {

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e) {
            startLoading();
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

            if (mainGrid.SelectedItem == null) {
                WinUIMessageBox.Show(messageBoxText: "매장을 선택해주세요.", caption: "매장 선택", button: MessageBoxButton.OK);
                return;
            }

            store = mainGrid.SelectedItem as Store;

            if (store == null) {
                WinUIMessageBox.Show(messageBoxText: "매장 선택을 다시 한 번 확인해주세요.", caption: "매장 선택", button: MessageBoxButton.OK);
                return;
            }

            String ipAddr = store.ip_addr;

            if (!apiCommunication.pingIp(ipAddr)) {
                WinUIMessageBox.Show(messageBoxText: "아이피 주소를 확인해주세요.", caption: "아이피 확인", button: MessageBoxButton.OK);
                return;
            }
            waitIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 속도 개선의 여지가 없을까? 
        /// </summary>
        /// <param name="listStore"></param>
        /// <returns></returns>
        public List<Store> getDbReplication(List<Store> listStore) {

            if (listStore == null) {
                return null;
            }
            String returnString = string.Empty;

            foreach (Store store in listStore) {
                sshConnection = new SshConnection(store.ip_addr);

                if (sshConnection.connectSsh()) {

                    returnString = sshConnection.executeReplicationCommand();

                    if (!returnString.Equals("Fail")) {
                        store.db_replication = returnString;
                    }
                    waitIndicator.Visibility = Visibility.Hidden;
                }

                sshConnection = null;
            }

            return listStore;
        }
    }
}
