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
    public partial class UCManageEmsDB : UserControl {
        APICommunication apiCommunication = null;
        DBPostgreSQL dbPostgresSql = null;
        SshConnection sshConnection = null;
        Store store = null;
        SelectedTable selectedTable = null;
        List<Store> listStore = null;
        Logger logger = null;
        bool txtVisiable = true;
        public UCManageEmsDB() {
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

        //[Obsolete("Use the DataControlBase.SelectionMode property instead")]
        //[Browsable(false)]
        //public TableViewSelectMode MultiSelectMode { get; set; }


        /// <summary>
        /// store list row click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            store = null;
            store = new Store();
            store = mainGrid.SelectedItem as Store;

            if (store != null && apiCommunication.pingIp(store.ip_addr) ) {
                dbPostgresSql = new DBPostgreSQL(store.ip_addr, Setting.Default.dbUserId, Setting.Default.dbName, Setting.Default.dbPort, Setting.Default.dbPassword);
                gridTableList.ItemsSource = dbPostgresSql.listSelectedTable();
                this.txtTableCount.Text = gridTableList.VisibleRowCount.ToString() + " 건";
                this.txtSelectedStoreUrl.Text = store.ip_addr.ToString();
            }
        }

        /// <summary>
        /// table list row click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridTableList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            this.Dispatcher.Invoke((Action)(() => {
                selectedTable = new SelectedTable();
                selectedTable = gridTableList.SelectedItem as SelectedTable;

                if (selectedTable != null) {

                    this.txtTableName.Text = selectedTable.tablename;
                    if(gridTableList.SelectedItems.Count <= 1) {
                        gridSelectedTable.ItemsSource = dbPostgresSql.selectTable(selectedTable.tablename);
                    } else {
                        gridSelectedTable.ItemsSource = null;
                    }
                    this.txtRowCount.Text = gridSelectedTable.VisibleRowCount.ToString() + " 건";
                }
            }));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            sshConnection = new SshConnection(txtSelectedStoreUrl.Text.Trim());

            if (gridTableList.SelectedItems.Count <= 0 || mainGrid.SelectedItems.Count <= 0) {
                WinUIMessageBox.Show(messageBoxText: "매장과 테이블을 선택해주세요.", caption: "매장 및 테이블 선택 필요", button: MessageBoxButton.OK);
                return;
            }

            this.txtPassword.Visibility = Visibility.Visible;
            this.labelPassword.Visibility = Visibility.Visible;

            string tableNames = string.Empty;
            string tableName = this.txtTableName.Text;
            bool result = false;

            btnDelete.Content = "삭제 비밀번호 입력 및 확인";

            if(txtPassword.Text.Trim() == Setting.Default.deleteRowPW) {
                if(gridTableList.SelectedItems.Count == 1 && gridSelectedTable.SelectedItems.Count > 0) {
                    if (WinUIMessageBox.Show(messageBoxText: tableName + " 의 데이터 " + gridSelectedTable.SelectedItems.Count + "개를 삭제하시겠습니까? 데이터 삭제에 유의하세요.", caption: "데이터 삭제", button: MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                        waitIndicator.Visibility = Visibility.Visible;
                        foreach (DataRowView row in gridSelectedTable.SelectedItems) {
                            DataRow dataRow = row.Row as DataRow;
                            if (dataRow != null) {
                                string id = dataRow["id"].ToString();
                                if (id != "") {
                                    result = dbPostgresSql.deleteRow(tableName, id);
                                }
                            }
                        }

                        /*core restart*/
                        if (result) {
                            WinUIMessageBox.Show(messageBoxText: "데이터 삭제가 완료되었습니다. Core가 재시작됩니다.", caption: "삭제 완료", button: MessageBoxButton.OK);
                            if (sshConnection.connectSsh()) {
                                sshConnection.executeRestartCommand();
                                logger.Info(txtSelectedStoreUrl.Text.Trim() + " CORE RESTART!");
                            }
                            this.gridSelectedTable.ItemsSource = null;
                        }
                        waitIndicator.Visibility = Visibility.Hidden;
                    }
                }else if(gridTableList.SelectedItems.Count > 1) {
                    if (WinUIMessageBox.Show(messageBoxText: "선택한 " +gridTableList.SelectedItems.Count.ToString()+"개의 테이블의 데이터를 전체 삭제하시겠습니까? 데이터 삭제에 유의하세요.", caption: "선택한 테이블의 데이터 전체 삭제", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                        foreach (var data in gridTableList.SelectedItems) {
                            SelectedTable selectedTable = data as SelectedTable;
                            if (selectedTable != null) {
                                tableNames += selectedTable.tablename + ",";
                            }
                        }

                        result = dbPostgresSql.truncateTable(null, tableNames);
                        WinUIMessageBox.Show(messageBoxText: "선택한 테이블의 데이터가 전체 삭제되었습니다. Core를 재시작합니다.", caption: "Core 재시작", button: MessageBoxButton.OK);
                        if (result && sshConnection.connectSsh()) {
                            logger.Info(txtSelectedStoreUrl.Text.Trim() + " CORE RESTART!");
                            sshConnection.executeRestartCommand();
                        }
                    }
                } else {
                    WinUIMessageBox.Show(messageBoxText: "테이블이나 조회된 데이터가 선택되지 않았습니다. 확인해주세요.", caption: "선택한 데이터 확인", button: MessageBoxButton.OK);

                }
            } else if(txtPassword.Text == ""){
             
            } else {
                WinUIMessageBox.Show(messageBoxText: "데이터 삭제용 비밀번호가 틀렸습니다. 확인해주세요. HINT : 홈플러스 SSH 비밀번호", caption: "데이터 삭제용 비밀번호 확인", button: MessageBoxButton.OK);

            }

            //this.txtPassword.Visibility = Visibility.Hidden;
            //this.labelPassword.Visibility = Visibility.Hidden;

            //btnDelete.Content = "삭제";

        }
        /// <summary>
        /// truncate button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTruncate_Click(object sender, RoutedEventArgs e) {
            bool result = false;
            string tableNames = string.Empty;

            sshConnection = new SshConnection(txtSelectedStoreUrl.Text.Trim());
            logger.Info(gridTableList.SelectedItems.Count.ToString() + " Truncate Process.");
            //MessageBox.Show(gridTableList.SelectedItems.Count.ToString() + " Truncate Process.");
            /*이거 뭐 문제가 있다. check 한 리스트를 가지고 와야할 거같은데/ */
            MessageBox.Show(gridTableList.SelectedItems.Count.ToString());
            if (gridTableList.SelectedItems.Count > 0) {
                if (WinUIMessageBox.Show(messageBoxText: "선택한 테이블의 데이터를 모두 삭제하시겠습니까? 데이터 삭제에 유의하세요.", caption: "선택한 테이블 삭제", button: MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    foreach (var data in gridTableList.SelectedItems) {
                        SelectedTable selectedTable = data as SelectedTable;
                        if (selectedTable != null) {
                            tableNames += selectedTable.tablename + ",";
                        }
                    }

                    result = dbPostgresSql.truncateTable(null, tableNames);
                    WinUIMessageBox.Show(messageBoxText: "선택한 테이블의 데이터가 삭제되었습니다. Core를 재시작합니다.", caption: "Core 재시작", button: MessageBoxButton.OK);
                    if (result && sshConnection.connectSsh()) {
                        logger.Info(txtSelectedStoreUrl.Text.Trim() + " CORE RESTART!");
                        sshConnection.executeRestartCommand();
                    }
                }

            } else {
                WinUIMessageBox.Show(messageBoxText: "테이블을 선택해주세요.", caption: "테이블 선택", button: MessageBoxButton.OK);

            }
        }

        /// <summary>
        /// truncate multiple table multiple store button event
        /// 다수 매장 별 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void btnTruncateAllStore_Click(object sender, RoutedEventArgs e) {
        //    listStore = new List<Store>();
        //    this.txtSeletedInfomaion.Text = string.Empty;
        //    bool result = false;
        //    string storeNames = string.Empty;
        //    string tableNames = string.Empty;
        //    if (mainGrid.SelectedItems.Count < 0) {
        //        //MessageBox.Show("Please select store's row more 1.");
        //        WinUIMessageBox.Show(messageBoxText: "1개 이상의 매장을 선택해주세요.", caption: "다수 매장 선택", button: MessageBoxButton.OK);
        //        return;
        //    }

        //    if (gridTableList.SelectedItems.Count < 0) {
        //        WinUIMessageBox.Show(messageBoxText: "1개의 테이블을 선택해주세요.", caption: "테이블 선택", button: MessageBoxButton.OK);
        //        return;
        //    }

        //    if (WinUIMessageBox.Show(messageBoxText: "선택한 매장과 테이블의 데이터를 삭제하시겠습니까?", caption: "선택한 매장의 테이블 데이터 삭제", button: MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
        //        foreach (var row in mainGrid.SelectedItems) {
        //            Store store = row as Store;
        //            if (store != null) {
        //                listStore.Add(store);
        //                storeNames += store.str_name + ",";
        //            }
        //        }

        //        foreach (var data in gridTableList.SelectedItems) {
        //            SelectedTable selectedTable = data as SelectedTable;
        //            if (selectedTable != null) {
        //                tableNames += selectedTable.tablename + ",";
        //            }
        //        }
        //        this.txtSeletedInfomaion.Text = "[Store] : " + storeNames + ", [Table] : " + tableNames;
        //        logger.Info("Will be truncate " + "[Store : " + listStore.Count.ToString() + "], [Table : " + tableNames + "]");
        //        result = truncateMultipleStore(listStore, tableNames);

        //        if (result) {
        //            logger.Info("had truncated[Store : " + listStore.Count.ToString() + "], [Table : " + gridTableList.SelectedItems.Count.ToString() + "]");
        //            WinUIMessageBox.Show(messageBoxText: this.txtSeletedInfomaion.Text + "를 삭제했습니다. 총 Store : " + listStore.Count.ToString() + " 개, Table : " + gridTableList.SelectedItems.Count.ToString() + "개 에서의 데이터가 삭제 되었습니다.", caption: "삭제 완료", button: MessageBoxButton.OK);
        //        }
        //    }


        //}

        #endregion

        #region Function
        public void init() {

            Thread.Sleep(1000);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                apiCommunication.login(this.txtIpAddress.Text.Trim());
                this.mainGrid.ItemsSource = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());
                if (mainGrid.ItemsSource != null) {
                    this.txtStoreCount.Text = this.mainGrid.VisibleRowCount.ToString() + " 건";
                } else {
                    WinUIMessageBox.Show(messageBoxText: "조회된 데이터가 없습니다. 아이피 및 포트를 확인하세요.", caption: "아이피나 포트 확인", button: MessageBoxButton.OK);
                }

                waitIndicator.Visibility = Visibility.Hidden;
            }));


        }

        /// <summary>
        /// truncate multiple store multiple table
        /// </summary>
        /// <param name="listStore"></param>
        /// <param name="listTable"></param>
        /// <returns></returns>
        public bool truncateMultipleStore(List<Store> listStore, string tableNames) {
            if (listStore != null && tableNames != null) {
                foreach (Store store in listStore) {
                    if (apiCommunication.pingIp(store.ip_addr)) {
                        dbPostgresSql.truncateTable(store.ip_addr, tableNames);
                        
                        Thread.Sleep(100);

                        if(sshConnection == null) {
                            sshConnection = new SshConnection(store.ip_addr.Trim());
                            sshConnection.connectSsh();
                        }
                        sshConnection.executeRestartCommand();
                        logger.Info(store.ip_addr + " CORE RESTART!");
                    }
                }
                return true;
            } else {
                return false;
            }
        }


        #endregion

        private void btnSelectTable_Click(object sender, RoutedEventArgs e) {
            /*1. store 다 가지고 오기*/
            List<Store> listStore = mainGrid.ItemsSource as List<Store>;
            //string tableName = this.gridTableList.SelectedItem.ToString();
            SelectedTable table = this.gridTableList.SelectedItem as SelectedTable;
            FileControl fileControl = new FileControl();

            if (listStore == null || table == null) {
                MessageBox.Show("Please Select Store or Table!");
                return;
            }

            foreach (var item in listStore) {
                Store store = item as Store;
                if (store != null && apiCommunication.pingIp(store.ip_addr)) {
                    fileControl.makeTextFile(Setting.Default.EsnIp + ".txt", AppDomain.CurrentDomain.BaseDirectory.Trim());
                    fileControl.appendTextFile(Setting.Default.EsnIp + ".txt", AppDomain.CurrentDomain.BaseDirectory.Trim(), dbPostgresSql.selectData(store.ip_addr, table.tablename, store.str_name, store.str_code));
                    MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory.Trim() + Setting.Default.EsnIp + ".txt" + "생성완료");
                }
            }
        }

        private void btnDeleteFromCode_Click(object sender, RoutedEventArgs e) {

            /*1. 팝업을 띄워주든, text 박스를 보여주든*/
            if (txtVisiable) {
                this.txtItemCode.Visibility = Visibility.Visible;
                this.txtVisiable = false;

            } else {
                this.txtItemCode.Visibility = Visibility.Hidden;
                this.txtVisiable = true;
            }

            if (txtItemCode.Text != "") {
                int count = 0;
                int resultCount = 0;
                string tempString = "'" + txtItemCode.Text.Trim().Replace("\r\n", "','") + "'";
                string storeName = string.Empty;

                foreach (var item in this.mainGrid.SelectedItems) {
                    Store store = item as Store;
                    if (store != null && apiCommunication.pingIp(store.ip_addr)) {
                        storeName = store.str_name;
                        count += this.dbPostgresSql.selectFromCode("g_usertext", store.ip_addr, tempString);
                    }
                }

                if (MessageBox.Show(storeName + " 등 " + this.mainGrid.SelectedItems.Count.ToString() + " 개의 매장에서 총 " + count.ToString() + " 건의 데이터가 g_usertext에서 조회되었습니다. 삭제 진행하시겠습니까?", "조회", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel) {
                    this.txtItemCode.Text = string.Empty;
                    return;
                }

                foreach (var item in this.mainGrid.SelectedItems) {
                    Store store = item as Store;
                    if (store != null && apiCommunication.pingIp(store.ip_addr)) {
                        resultCount += this.dbPostgresSql.deleteFromCode(tempString, store.ip_addr);
                    }
                }

                MessageBox.Show("총 " + count.ToString() + " 건의 데이터 중 " + resultCount.ToString() + " 건이 삭제되었습니다.", "삭제 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                //MessageBox.Show("SKU_TCODE == 1 인 데이터 개수 : " + resultSkuTCodeCount.ToString() + "건");
                this.txtItemCode.Text = string.Empty;
            } else {
                MessageBox.Show("product_key 가 textbox 에 입력되지 않았습니다.");
                this.txtItemCode.Text = string.Empty;
            }
        }

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


        private void btnLoginFailInit_Click(object sender, RoutedEventArgs e) {

            //List<Store> listStore = mainGrid.ItemsSource as List<Store>;

            //foreach(Store store in listStore) {
            //    if (apiCommunication.pingIp(store.ip_addr)) {
            //        dbPostgresSql.updateUserTable(store.ip_addr, "g_user");
            //    }
            //}
            Store store = mainGrid.SelectedItem as Store;

            
            if (store == null) {
                WinUIMessageBox.Show(messageBoxText: "로그인 실패 횟수 초기화를 할 매장을 선택해주세요.", caption: "하나의 매장 선택", button: MessageBoxButton.OK);
                return;
            }

            if(dbPostgresSql.updateUserTable(store.ip_addr)) {
                WinUIMessageBox.Show(messageBoxText: "로그인 실패 횟수 초기화가 완료되었습니다.", caption: "초기화 완료", button: MessageBoxButton.OK);

            } else {
                WinUIMessageBox.Show(messageBoxText: "로그인 실패된 계정이 없습니다. 확인해주세요.", caption: "확인", button: MessageBoxButton.OK);

            }
        }

        //private void txtIpAddress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
        //    MessageBox.Show("찍혔다");
        //    txtIpAddress.SelectAll();
        //}

        //private void txtIpAddress_MouseDown(object sender, MouseButtonEventArgs e) {
        //    /*왜 안먹힐까....*/
        //    logger.Info("찍혔다.");
        //    WinUIMessageBox.Show(messageBoxText: "1개 이상의 매장을 선택해주세요.", caption: "다수 매장 선택", button: MessageBoxButton.OK);
        //    MessageBox.Show("clicked");
        //    txtIpAddress.SelectAll();
        //}
    }
}
