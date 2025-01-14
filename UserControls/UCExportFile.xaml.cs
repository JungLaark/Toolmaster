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
using Microsoft.Win32;
using System.Runtime.InteropServices.ComTypes;
using NPOI.XSSF.UserModel;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using NPOI.SS.UserModel;
using System.Data;

namespace ToolMaster.UserControls {
    /// <summary>
    /// UCManageGateway.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCExportFile : UserControl {

        APICommunication apiCommunication = null;
        Store store = null;
        List<TagInfo> listTagInfo = null;

        public UCExportFile() {
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

            store = gridStoreList.SelectedItem as Store;

            if (store != null && apiCommunication.pingIp(store.ip_addr)) {
                apiCommunication.emsLogin(store.ip_addr); //"/esl/gateway/v2/query_list"
                //this.gridGatewayList.ItemsSource = apiCommunication.postGatewayList(store.ip_addr, "/esl/gateway/v2/query_list");
            }
        }

        #endregion

        private void btnConfirm_Click(object sender, RoutedEventArgs e) {
            startLoading();
        }

        private void btnExportTagList_Click(object sender, RoutedEventArgs e) {
            // saveExcelFileTask();

            saveExcelFile();
        }

        private async Task startLoading() {
            this.waitIndicator.Visibility = Visibility.Visible;

            await Task.Run(() =>
                init()
            ); 
        }

      /*  private async Task saveExcelFileTask() {
            this.waitIndicator.Visibility = Visibility.Visible;

            await Task.Run(() => {
                saveExcelFile();
            });
        }*/
        private void saveExcelFile() {
            try {
                this.waitIndicator.Visibility = Visibility.Visible;
                List<TagInfo> listInfo;
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "xlsx|*.xlsx";
                saveFileDialog.Title = "매장 별 태그 개수를 파일로 저장";
                saveFileDialog.FileName = "Hyper 매장 별 태그 개수 " + System.DateTime.Now.ToString("yyyy년MM월dd일 HH시mm분ss초");

                if (saveFileDialog.ShowDialog() == true) {
                    listInfo = getTagCountList();

                    if (listInfo != null && listInfo.Count > 0) {
                        makeExcelFile(convertListToDataTable(listInfo), saveFileDialog.FileName);
                    } else {
                        WinUIMessageBox.Show(messageBoxText: "Excel 파일 생성 중 오류가 발생했습니다.", caption: "파일 생성 완료", button: MessageBoxButton.OK);
                    }
                } else {
                    return;
                }

                this.waitIndicator.Visibility = Visibility.Hidden;
            } catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "오류 발생", button: MessageBoxButton.OK);
            }
        }

        private DataTable convertListToDataTable(List<TagInfo> originList) {
            try {

                List<TagInfo> tempList = null;
                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("매장 코드", typeof(string));
                dataTable.Columns.Add("매장명", typeof(string));
                dataTable.Columns.Add("태그 현황", typeof(string));
                dataTable.Columns.Add("2.9 냉동", typeof(string));
                dataTable.Columns.Add("2.9R", typeof(string));
                dataTable.Columns.Add("3.7R", typeof(string));
                dataTable.Columns.Add("12.5R", typeof(string));

                foreach(TagInfo tagInfo in originList) {
                    DataRow row1 = dataTable.NewRow();

                    if(tempList != null && tempList.Find(value => value.storeCode == tagInfo.storeCode) != null) {
                        continue;
                    }

                    /*점포 별로 리스트 가지고 옴*/
                    tempList = originList.FindAll(p => p.storeCode == tagInfo.storeCode);
                   
                    /*사용중 row*/
                    row1["매장 코드"] = tagInfo.storeCode;
                    row1["매장명"] = tagInfo.storeName;

                    row1["태그 현황"] = "사용중";
                    row1["2.9 냉동"] = (from value in tempList
                                     where value.type == "2.9"
                                    select value.connected).FirstOrDefault().ToString();
                    row1["2.9R"] = (from value in tempList
                                  where value.type == "2.9R"
                                 select value.connected).FirstOrDefault().ToString();
                    row1["3.7R"] = (from value in tempList
                                  where value.type == "3.7R"
                                 select value.connected).FirstOrDefault().ToString();
                    row1["12.5R"] = (from value in tempList
                                   where value.type == "12.5R"
                                  select value.connected).FirstOrDefault().ToString();

                    dataTable.Rows.Add(row1);

                    /*미접속 row*/
                    DataRow row2 = dataTable.NewRow();

                    row2["매장 코드"] = "";
                    row2["매장명"] = "";

                    row2["태그 현황"] = "미접속";
                    row2["2.9 냉동"] = (from value in tempList
                                     where value.type == "2.9"
                                    select value.disconnected).FirstOrDefault().ToString();
                    row2["2.9R"] = (from value in tempList
                                  where value.type == "2.9R"
                                 select value.disconnected).FirstOrDefault().ToString();
                    row2["3.7R"] = (from value in tempList
                                  where value.type == "3.7R"
                                 select value.disconnected).FirstOrDefault().ToString();
                    row2["12.5R"] = (from value in tempList
                                   where value.type == "12.5R"
                                  select value.disconnected).FirstOrDefault().ToString();

                    dataTable.Rows.Add(row2);

                }

                return dataTable;

            } catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "오류 발생", button: MessageBoxButton.OK);

                return null;
            }
        }

        private List<TagInfo> getTagCountList() {
            try {
                listTagInfo = new List<TagInfo>();

                foreach (Store store in this.gridStoreList.ItemsSource as List<Store>) {
                    apiCommunication.emsLogin(store.ip_addr);
                    listTagInfo.AddRange(apiCommunication.postTagInfo(store.ip_addr, store.str_code, store.str_name,"/esl/system/v2/tagStatus"));
                }

                return listTagInfo;

            }catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "오류 발생", button: MessageBoxButton.OK);

                return null;
            }
        }

        /// <summary>
        /// 읽은 데이터 Excel 파일 화
        /// </summary>
        /// <param name="listTagInfo"></param>
        private void makeExcelFile(DataTable dt, string filePath) {

            if(dt == null) {
                return;
            }

            XSSFWorkbook workBook;

            try {
                using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)) {
                    
                    workBook = new XSSFWorkbook();
                    //workBook = new XSSFWorkbook(fileStream); EOF in header 라는 오류 뜸.
                    ISheet sheet = workBook.CreateSheet("태그 별 현황 개수");

                    XSSFCellStyle cellStyle = (XSSFCellStyle)workBook.CreateCellStyle();
                    cellStyle.FillForegroundColor = IndexedColors.LightYellow.Index;

                    /*이렇게 하면 안됨. 견본임*/
                    /*Column*/
                    /*                    sheet.CreateRow(0).CreateCell(0).SetCellValue("No.");
                                        sheet.CreateRow(0).CreateCell(1).SetCellValue("코드");
                                        sheet.CreateRow(0).CreateCell(2).SetCellValue("매장명");
                                        sheet.CreateRow(0).CreateCell(3).SetCellValue("태그 현황");
                                        sheet.CreateRow(0).CreateCell(4).SetCellValue("2.9 냉동");
                                        sheet.CreateRow(0).CreateCell(5).SetCellValue("2.9R");
                                        sheet.CreateRow(0).CreateCell(6).SetCellValue("3.7R");
                                        sheet.CreateRow(0).CreateCell(7).SetCellValue("12.5R");
                    */

                    for (int i=0; i<=dt.Rows.Count; i++) {

                        IRow row = sheet.CreateRow(i);

                        for(int j=0; j<8; j++) {

                            ICell cell = row.CreateCell(j);

                            if(i == 0) {
                                cell.CellStyle = cellStyle;
                                if (j == 0) {
                                    cell.SetCellValue("No.");
                                } else if (j == 1) {
                                    cell.SetCellValue("매장 코드");
                                } else if(j == 2) {
                                    cell.SetCellValue("매장명");
                                } else if (j == 3) {
                                    cell.SetCellValue("태그 현황");
                                } else if (j == 4) {
                                    cell.SetCellValue("2.9 냉동");
                                } else if (j == 5) {
                                    cell.SetCellValue("2.9R");
                                } else if (j == 6) {
                                    cell.SetCellValue("3.7R");
                                } else if (j == 7) {
                                    cell.SetCellValue("12.5R");
                                }
                            } else { /* i>0 */
                                cell.CellStyle = null;
                                if (j == 0) {
                                    if(i%2 != 0) {
                                        if (i == 1) {
                                            cell.SetCellValue(1);
                                        } else {
                                            cell.SetCellValue((i+1)/2);
                                        }
                                    }
                                } else { /*i==1부터 j==0*/
                                    cell.SetCellValue(dt.Rows[i-1].ItemArray[j-1].ToString());

                                    /*이렇게 하면 안됨. 견본임*/
                                    //cell.SetCellValue(dt.Rows[i]["매장명"].ToString());
                                    //cell.SetCellValue(dt.Rows[i]["태그 현황"].ToString());
                                    //cell.SetCellValue(dt.Rows[i]["2.9 냉동"].ToString());
                                    //cell.SetCellValue(dt.Rows[i]["2.9R"].ToString());
                                    //cell.SetCellValue(dt.Rows[i]["3.7R"].ToString());
                                    //cell.SetCellValue(dt.Rows[i]["12.5R"].ToString());
                                }
                            }
                        }
                        /*이렇게 하면 안됨 견본임*/
                        /*sheet.CreateRow(i + 1).CreateCell(0).SetCellValue(i + 1);
                        sheet.CreateRow(i + 1).CreateCell(1).SetCellValue(dt.Rows[0][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(2).SetCellValue(dt.Rows[1][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(3).SetCellValue(dt.Rows[2][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(4).SetCellValue(dt.Rows[3][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(5).SetCellValue(dt.Rows[4][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(6).SetCellValue(dt.Rows[5][i].ToString());
                        sheet.CreateRow(i + 1).CreateCell(7).SetCellValue(dt.Rows[6][i].ToString());*/
                        
                    }
                    workBook.Write(fileStream);
                }

                WinUIMessageBox.Show(messageBoxText: "엑셀 파일 생성이 완료되었습니다. 경로 : " + filePath, caption: "파일 생성 완료", button: MessageBoxButton.OK);


            } catch (Exception ex) {
                WinUIMessageBox.Show(messageBoxText: ex.ToString(), caption: "오류 발생", button: MessageBoxButton.OK);
            }
        }
    }
}
