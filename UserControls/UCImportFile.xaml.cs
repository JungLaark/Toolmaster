using DevExpress.Utils.CommonDialogs.Internal;
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
    public partial class UCImportFile : UserControl {
        APICommunication apiCommunication = null;
        DBPostgreSQL dbPostgresSql = null;
        SshConnection sshConnection = null;
        Store store = null;
        SelectedTable selectedTable = null;
        List<Store> listStore = null;
        Logger logger = null;
        bool txtVisiable = true;

        public UCImportFile() {
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
                //mainGrid.ItemsSource = getDbReplication(listStore);
                mainGrid.ItemsSource = listStore;

                if (mainGrid.ItemsSource != null) {
                    this.txtStoreCount.Text = "총 매장 개수 : " + this.mainGrid.VisibleRowCount.ToString() + "개";
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

         
        }


        /// <summary>
        /// 파일 불러오기 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFileLoad_Click(object sender, RoutedEventArgs e) {
            try {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                List<Tag> listTag = new List<Tag>();

                openFileDialog.Filter = "txt|*.txt";
                openFileDialog.Title = "태그 리스트 txt 파일 선택";

                if(openFileDialog.ShowDialog() == true) {
                    using (StreamReader streamReader = new StreamReader(openFileDialog.FileName, Encoding.UTF8)) {
                        while (!streamReader.EndOfStream) {
                            listTag.Add(new Models.Tag { device_id = streamReader.ReadLine().Trim() });
                        }
                        this.taglistGrid.ItemsSource = listTag;
                        this.txtTagListCount.Text = "태그 개수 : " +  listTag.Count.ToString() + " 개";
                    } 
                }

            }catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "파일 리딩시 오류", button: MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// 태그 리스트 초기화 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResetTagList_Click(object sender, RoutedEventArgs e) {
            this.taglistGrid.ItemsSource = null;
            this.txtTagListCount.Text = null;
        }

        /// <summary>
        /// 태그 삭제 버튼 클릭 이벤트  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteTag_Click(object sender, RoutedEventArgs e) {
            try {
                List<Tag> tags = this.taglistGrid.ItemsSource as List<Tag>;
                Store store = this.mainGrid.SelectedItem as Store;

                if(tags == null || tags.Count <= 0) {
                    WinUIMessageBox.Show(messageBoxText: "태그 리스트가 있는 텍스트 파일을 불러와주세요.", caption: "파일 로드 필요", button: MessageBoxButton.OK);
                    return;
                }

                if(store == null) {
                    WinUIMessageBox.Show(messageBoxText: "태그 삭제를 할 매장을 선택해주세요.", caption: "매장 선택 필요", button: MessageBoxButton.OK);
                    return;
                }

                if (WinUIMessageBox.Show(messageBoxText: tags.Count.ToString() + "개의 태그를 삭제하시겠습니까?", caption: "태그 삭제", button: MessageBoxButton.OKCancel) == MessageBoxResult.OK){

                    if (store != null) {
                        apiCommunication.emsLogin(store.ip_addr);

                        foreach (Tag tag in tags) {
                            apiCommunication.postDeleteTag(tag.device_id, store.ip_addr, "/esl/met/v2/delete");
                            Thread.Sleep(100);
                        }

                        WinUIMessageBox.Show(messageBoxText: tags.Count.ToString() + "개의 태그 삭제가 완료되었습니다. 태그가 삭제되었는지 EMS에서 확인해주세요.", caption: "태그 삭제 완료", button: MessageBoxButton.OK);

                        /*core restart*//*
                        sshConnection = new SshConnection(store.ip_addr);
                        if (sshConnection.connectSsh()) {
                            sshConnection.executeRestartCommand();
                        }*/

                        sshConnection = null;
                        this.taglistGrid.ItemsSource = null;
                    } else {
                        WinUIMessageBox.Show(messageBoxText: "태그 삭제를 할 매장을 선택해주세요.", caption: "매장 선택 필요", button: MessageBoxButton.OK);
                    }
                }
            } catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "태그 삭제시 오류", button: MessageBoxButton.OK);
            }
        }
    }
}
