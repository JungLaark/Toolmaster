using ToolMaster.Models;
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
using ToolMaster.Common;
using System.Threading;
using System.Windows.Threading;
using DevExpress.Xpf.WindowsUI;
using System.Runtime.InteropServices;
using System.Net;

namespace ToolMaster.UserControls {
    /// <summary>
    /// UCManageGateway.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCManageGateway : UserControl {

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destinationIPValue, int sourceIPValue, byte[] physicalAddressArray, ref uint physicalAddressArrayLength);

        APICommunication apiCommunication = null;
        Store store = null;

        public UCManageGateway() {
            InitializeComponent();
            apiCommunication = new APICommunication();
            txtIpAddress.Text = Setting.Default.EsnIp.Trim();
            txtIpAddress.Focus();
            
        }

        #region Function
        public void init() {

            Thread.Sleep(3000);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                apiCommunication.login(this.txtIpAddress.Text.Trim());
                this.gridStoreList.ItemsSource = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());

                if(this.gridStoreList.ItemsSource == null) {
                    WinUIMessageBox.Show(messageBoxText: "조회된 데이터가 없습니다. 아이피 및 포트를 확인하세요.", caption: "아이피나 포트 확인", button: MessageBoxButton.OK);
                    waitIndicator.Visibility = Visibility.Hidden;

                    return;
                }
                waitIndicator.Visibility = Visibility.Hidden;
            }));

           
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            txtIpAddress.Focus();
            btnConfirm_Click(sender, e);
        }

        /// <summary>
        /// store list row click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridStoreList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //var dg = sender as DataGrid;
            //var dep = (DependencyObject)e.OriginalSource;

            this.gridGatewayList.ItemsSource = null;

            //while (!(dep is DataGridRow)) {
            //    dep = VisualTreeHelper.GetParent(dep);
            //    if (dep is DataGrid) return;
            //}

            //var row = dep as DataGridRow;
            store = gridStoreList.SelectedItem as Store;

            if (store != null && apiCommunication.pingIp(store.ip_addr)) {
                apiCommunication.emsLogin(store.ip_addr); //"/esl/gateway/v2/query_list"
                this.gridGatewayList.ItemsSource = apiCommunication.postGatewayList(store.ip_addr, "/esl/gateway/v2/query_list");
                //this.txtGatewayCount.Text = gridGatewayList.ItemsSource?.OfType<object>().Count().ToString();
            }
        }

        #endregion

        /// <summary>
        /// gateway list row click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridGatewayList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GatewayInfo gatewayInfo = null;
            Gateway gateway = null;

            try {
                gateway = gridGatewayList.SelectedItem as Gateway;
                Store store = gridStoreList.SelectedItem as Store;

                if (gateway != null) {
                    this.txtGwId.Text = gateway.device_id;
                    this.txtGwState.Text = gateway.state;
                    this.txtGwAddress.Text = gateway.ip;
                    this.txtGwNormalTag.Text = gateway.normal_tag_count;
                    this.txtGwInvalidTag.Text = gateway.invalid_tag_count;
                    this.txtGwRemovedTag.Text = gateway.removed_tag_count;

                    gatewayInfo = apiCommunication.postGatewayInfo(store.ip_addr, gateway.device_id, "/esl/gateway/v2/query_config");
                    if (gatewayInfo != null) {
                        this.txtGwAliveInterval.Text = gatewayInfo.aliveInterval;
                        this.txtGwTagMaxCount.Text = gatewayInfo.maxTagCount;

                        this.txtWakeupCha1.Text = gatewayInfo.commonChannel;
                        this.txtWakeupCha2.Text = gatewayInfo.commonChannel2;
                        this.txtDataCha1.Text = gatewayInfo.dataChannel;
                        this.txtDataCha2.Text = gatewayInfo.dataChannel2;
                        this.txtConnCha.Text = gatewayInfo.beaconChannel;

                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.ToString());
            }

          
        }
        /// <summary>
        /// Gateway 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGwSetting_Click(object sender, RoutedEventArgs e) {
            if(WinUIMessageBox.Show(messageBoxText: "수정한 수치로 적용할까요?", caption: "수치 적용", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                bool result = true;
                GatewayInfo gatewayInfo = apiCommunication.postGatewayInfo(txtGwAddress.Text.Trim(), txtGwId.Text.Trim(), "/esl/gateway/v2/query_config");

                result = apiCommunication.setGatewayInfo(gatewayInfo, "esl/gateway/v2/control_config", txtGwAliveInterval.Text.Trim(), txtGwTagMaxCount.Text.Trim());

                if (result) {
                    WinUIMessageBox.Show(messageBoxText: "설정한 수치로 적용되었습니다.", caption: "수치 적용", button: MessageBoxButton.OK);
                }
            }
           
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e) {
            startLoading();
        }

        private async Task startLoading() {
            this.waitIndicator.Visibility = Visibility.Visible;

            await Task.Run(() =>
                init()
            ); ;


        }

        //public List<Store> getMacAddr(List<Store> listStore) {
        //    if(listStore == null) {
        //        return null;
        //    }

        //    foreach(Store store in listStore) {
        //        IPAddress ip = IPAddress.Parse(store.ip_addr);
        //        byte[] btIPArray = new byte[6];

        //        uint uiIp = (uint)btIPArray.Length;

        //        int iIpValue = BitConverter.ToInt32(ip.GetAddressBytes(), 0);
        //        int iReturnCode = SendARP(iIpValue, 0, btIPArray, ref uiIp);

        //        if(iReturnCode != 0) {
        //            continue;
        //        }

        //        string[] strIp = new string[(int)uiIp];

        //        for(int i=0; i<uiIp; i++) {
        //            strIp[i] = btIPArray[i].ToString("X2");
        //        }

        //        //return string.Join(":", strIp);
        //        store.mac_addr = string.Join(":", strIp);
        //    }

        //    return listStore;
        //}
    }
}
