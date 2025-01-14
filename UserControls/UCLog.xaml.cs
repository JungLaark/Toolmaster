using DevExpress.Utils.Serializing;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.WindowsUI;
using Microsoft.Win32;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ToolMaster.Properties;

namespace ToolMaster.UserControls {
    /// <summary>
    /// UCManageEntireLog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCLog : UserControl {

        List<HwType> listHwType = null;
        List<LogType> listLogType = null;
        Store store = null;
        LogTypePath logTypePath = null;
        SshConnection sshConnection = null;
        ScpConnection scpConncetion = null;
        SftpConnection sftpConnection = null;
        APICommunication apiCommunication = null;
        Logger logger = null;

        public UCLog() {
            InitializeComponent();
            logger = LogManager.GetCurrentClassLogger();
            apiCommunication = new APICommunication();
            txtIpAddress.Text = Setting.Default.EsnIp.Trim();
            txtIpAddress.Focus();
            
        }

        #region Function
        public void init() {

            LogTypePath logTypePath = new LogTypePath();
            //this.txtOsVersion.Text = "1. IP 주소를 입력해주세요. " 
            //                        + Environment.NewLine
            //                        + "2. H/W와 Log 타입을 선택해주세요."
            //                        + Environment.NewLine
            //                        + "3. Connection 버튼을 클릭해주세요.";

            listHwType = new List<HwType>() {
                new HwType { Name = "Gen2"},
                new HwType { Name = "Gen2+"},
                new HwType { Name = "Gen Server"}
            };

            listLogType = new List<LogType>() {
                new LogType{ Id = 0, Name = "Core", LogPath = logTypePath.coreLogPath},
                new LogType{ Id = 1, Name = "DB", LogPath = logTypePath.dbLogPath},
                new LogType{ Id = 2, Name = "FailOver", LogPath = logTypePath.failOverLogPath},
                new LogType{ Id = 3, Name = "EMS", LogPath = logTypePath.emsLogPath},
                new LogType{ Id = 4, Name = "Gateway", LogPath = logTypePath.gatewayLogPath},
                new LogType{ Id = 5, Name = "NetworkConf.", LogPath = logTypePath.Gen2NetworkConfPath},
                new LogType{ Id = 6, Name = "NetworkConf.", LogPath = logTypePath.GenServerNetworkConfPath},
                new LogType{ Id = 7, Name = "Gateway_Log", LogPath = logTypePath.gatewaySendLogPath}
            };

            Thread.Sleep(1000);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                apiCommunication.login(this.txtIpAddress.Text.Trim());
                this.mainGrid.ItemsSource = apiCommunication.postStoreList(this.txtIpAddress.Text.Trim());

                if(this.mainGrid.ItemsSource == null) {
                    WinUIMessageBox.Show(messageBoxText: "조회된 데이터가 없습니다. 아이피 및 포트를 확인하세요.", caption: "아이피나 포트 확인", button: MessageBoxButton.OK);
                    waitIndicator.Visibility = Visibility.Hidden;
                    return;
                }
                //if (mainGrid.ItemsSource != null) {
                //    this.txtStoreCount.Text = this.mainGrid.VisibleRowCount.ToString() + " 건";
                //}

                this.gridHardwareType.ItemsSource = listHwType;
                waitIndicator.Visibility = Visibility.Hidden;
            }));
        }

        public List<LogFile> textToList(string originString) {
            try {
                if (originString == "") {
                    return null;
                }
                string[] listString = originString.Split('\n');
                LogFile logFile = null;
                List<LogFile> listLogFile = new List<LogFile>();

                foreach (string item in listString) {
                    logFile = new LogFile() { fileName = item };
                    listLogFile.Add(logFile);
                }

                return listLogFile;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        #endregion

        #region Event
        private void gridHardwareType_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

            if(mainGrid.SelectedItems.Count <= 0) {
                WinUIMessageBox.Show(messageBoxText: "매장을 선택해주세요.", caption: "매장 선택", button: MessageBoxButton.OK);
                return;
            }

            HwType hwType = null;
            hwType = gridHardwareType.SelectedItem as HwType;

            if (hwType != null) {
                if (hwType.Name.Equals("Gen2")) {
                    this.gridLogType.ItemsSource = from item in listLogType
                                                   where item.Id == 4 || item.Id == 5
                                                   select item;
                } else if (hwType.Name.Equals("Gen2+")) {
                    this.gridLogType.ItemsSource = from item in listLogType
                                                   where item.Id != 6
                                                   select item;

                } else if (hwType.Name.Equals("Gen Server")) {
                    this.gridLogType.ItemsSource = from item in listLogType
                                                   where item.Id != 4 && item.Id != 5
                                                   select item;
                }
            } else {
                WinUIMessageBox.Show(messageBoxText: "H/W를 선택해주세요.", caption: "H/W 선택", button: MessageBoxButton.OK);
            }
        }


        private void gridLogType_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            string ipAddr = string.Empty;
            store = null;

            if (mainGrid.SelectedItem == null) {
                WinUIMessageBox.Show(messageBoxText: "매장을 선택해주세요.", caption: "매장 선택", button: MessageBoxButton.OK);
                return;
            }

            store = mainGrid.SelectedItem as Store;
            ipAddr = store.ip_addr;

            if (this.gridLogType.SelectedItem == null || !apiCommunication.pingIp(ipAddr)) {
                WinUIMessageBox.Show(messageBoxText: "아이피 주소를 확인해주세요.", caption: "아이피 확인", button: MessageBoxButton.OK);
                return;
            }
            sshConnection = new SshConnection(ipAddr);
            LogType logType = this.gridLogType.SelectedItem as LogType;

            waitIndicator.Visibility = Visibility.Visible;

            if (logType != null) {

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate {
                    if (sshConnection.connectSsh()) {
                        /*OS Version 가지고 오기*/
                        //this.txtOsVersion.Text = sshConnection.executeCommand("cat /proc/version");
                        /*Log File List 가지고 오기*/
                        if (logType.Name.Equals("NetworkConf.")) {
                            this.gridLogFile.ItemsSource = textToList(sshConnection.executeCommand("ls " + logType.LogPath.Substring(0, 13)));
                        } else {
                            this.gridLogFile.ItemsSource = textToList(sshConnection.executeCommand("ls " + logType.LogPath));
                        }
                    } else {
                        WinUIMessageBox.Show(messageBoxText: "연결할 수 없습니다. SSH 포트를 확인해주세요.", caption: "포트 확인", button: MessageBoxButton.OK);
                    }

                    waitIndicator.Visibility = Visibility.Hidden;
                }));
            }
        }

        private void gridLogFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }


        /// <summary>
        /// download 버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownload_Click(object sender, RoutedEventArgs e) {
            Stopwatch stopWatch = null;
            
            this.waitIndicator.Visibility = Visibility.Visible;

            float time = 0;
            float fileSize = 0;
            store = null;
            store = mainGrid.SelectedItem as Store;

            LogFile logFile = null;
            LogType logType = null;

            logType = this.gridLogType.SelectedItem as LogType;
            logFile = this.gridLogFile.SelectedItem as LogFile;

            string address = store.ip_addr.ToString();
            string fileName = string.Empty;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "zip|*.zip";
            saveFileDialog.Title = "로그 파일 저장";
            saveFileDialog.FileName = logFile.fileName + ".zip";

            scpConncetion = new ScpConnection(address);

            if (logType != null && logFile != null && saveFileDialog.ShowDialog() == true) {

                stopWatch = Stopwatch.StartNew();
                fileName = saveFileDialog.FileName;

                if (logType.LogPath.Contains("ip.conf") || logType.LogPath.Contains("/etc/network/interfaces")) {
                    //this.txtLog.Text = sshConnection.executeCommand("cat " + logType.LogPath);
                } else {
                    logger.Info(sshConnection.executeCommand("zip " + logType.LogPath + logFile.fileName + ".zip " + logType.LogPath + logFile.fileName));
                    fileSize = scpConncetion.downloadTextFile(scpConncetion.connectScp(), logType.LogPath + logFile.fileName + ".zip", fileName);
                    
                    if (fileSize > 0) {
                        stopWatch.Stop();
                        time = stopWatch.ElapsedMilliseconds;
                        WinUIMessageBox.Show(messageBoxText: "경로 : " + fileName + ", 크기 : " + Math.Round(fileSize/1000000, 3).ToString() + "MB의 파일 다운로드가 " + (time/1000) + "초에 완료되었습니다.", caption: "파일 다운로드 완료", button: MessageBoxButton.OK);
                        logger.Info("경로 : " + fileName + " 에 파일 다운로드가 완료되었습니다.");
                        logger.Info(sshConnection.executeCommand("rm " + logType.LogPath + logFile.fileName + ".zip"));
                    } else {
                        /*기존에 있는 압축 파일 삭제해야 함.*/
                        logger.Info(sshConnection.executeCommand("rm " + logType.LogPath + logFile.fileName + ".zip"));
                        WinUIMessageBox.Show(messageBoxText: "경로 : " + fileName + "에 파일 다운로드가 실패하였습니다. 로그를 확인하세요.", caption: "파일 다운로드 실패", button: MessageBoxButton.OK);
                    }
                }
            }
            
            this.waitIndicator.Visibility = Visibility.Hidden;
        }
        #endregion

        private void mainGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void mainGrid_SelectionChanged(object sender, DevExpress.Xpf.Grid.GridSelectionChangedEventArgs e) {

        }

        private void mainGrid_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e) {

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            btnConfirm_Click(sender, e);
        }

        private void btnOpenCMD_Click(object sender, RoutedEventArgs e) {
            String command = String.Empty;
            store = null;
            String ipAddr = String.Empty;
            try {
                store = mainGrid.SelectedItem as Store;
                ipAddr = store.ip_addr;
                //command = "ssh " + Setting.Default.SshId + "@" + ipAddr;
                command = Setting.Default.SshId + "@" + ipAddr + " -pw " + Setting.Default.SshPw;
                MessageBox.Show(command);
                //System.Diagnostics.Process.Start("cmd.exe", command);
                System.Diagnostics.Process.Start("putty.exe", command);
            }catch(Exception ex) {
                WinUIMessageBox.Show(messageBoxText: "cmd 창 여는데 에러가 생겼습니다.", caption: "파일 다운로드 실패", button: MessageBoxButton.OK);
            }
        }
    }
}
