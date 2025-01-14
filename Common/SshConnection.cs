using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.WindowsUI;
using System.Windows;
using NLog;
using Renci.SshNet;

namespace ToolMaster.Common {
    class SshConnection {

        SshClient sshClient = null;
        string ipAddr = string.Empty;
        Logger logger = null;

        public SshConnection(string ipAddr) {
            this.ipAddr = ipAddr;
            logger = LogManager.GetCurrentClassLogger();
        }

        public bool connectSsh() {
            try {
                string returnString = string.Empty;

                if (sshClient == null) {
                    sshClient = new SshClient(
                        this.ipAddr,
                        int.Parse(Setting.Default.SshPort),
                        Setting.Default.SshId,
                        Setting.Default.SshPw
                        );

                    sshClient.Connect();
                    this.sshClient = sshClient;
                    logger.Info("SUCCESS CONNECTION TO " + this.ipAddr);
                    return true;
                } else {
                    logger.Error("FAIL CONNECTION. sshClient is not null " + this.ipAddr);
                    return false;
                }
            } catch (Exception ex) {
                WinUIMessageBox.Show(messageBoxText: "연결할 수 없습니다. SSH 포트나 비밀번호를 확인해주세요." + ex.ToString() , caption: "포트 확인", button: MessageBoxButton.OK);
                logger.Error("connectSsh func. : " + ex.ToString());
                sshDisconnect(ref sshClient);

                return false;
            }
        }

        /// <summary>
        /// execute restart command in linux
        /// </summary>
        /// <returns></returns>
        public string executeRestartCommand() {
            try {
                string returnString = string.Empty;
                if(sshClient != null) {
                    returnString = sshClient.CreateCommand(string.Format(@"/etc/init.d/ecore restart")).Execute();
                }
                return returnString;
            }catch(Exception ex) {
                logger.Error(ex.ToString());
                return "Fail";
            }
        }

        public string executeReplicationCommand() {
            try {
                string returnString = string.Empty;

                if (sshClient != null) {
                    int k = 0;
                    string[] response = sshClient.CreateCommand(string.Format(@"/esl/common/bin/repl_status.sh")).Execute().Split('\n');
                    /*여기서 자르자*/
                    returnString = returnString.ToString();

                    for(int i = 0; i<response.Length; i++) {
                        if (response[i].Contains("---")) {
                            k = i + 1;
                            break;
                        }
                    }

                    if (response[k].Contains("rows")) {
                        return "활성";
                    } else {
                        if (Convert.ToDouble(response[k]) >= 0) {
                            return "활성";
                        } else {
                            return "비활성";
                        }
                    }
                }
                return "Fail";
            }catch(Exception ex) {
                logger.Error(ex.ToString());
                return "Fail";
            }
        }

        /// <summary>
        /// execute file copy in linux
        /// </summary>
        /// <returns></returns>
        public bool executeCopyCommand() {
            try {
                if(sshClient != null) {
                    string fullPath = Setting.Default.remotePath + Setting.Default.textFileName;
                    sshClient.CreateCommand(string.Format(@"cp " + fullPath +  " " + fullPath.Substring(0, fullPath.Length - 4) + "_`date +%Y-%m-%d_%H:%M:%S`.txt")).Execute();
                    logger.Info("[EXECUTE COMMAND] : " + @"cp " + fullPath + " " + fullPath.Substring(0, fullPath.Length - 4) + "_`date +%Y-%m-%d_%H:%M:%S`.txt");
                }
                return true;
            }catch(Exception ex) {
                logger.Error(ex.ToString());
                return false;
            } 
        }

        public string executeCommand(string commString) {
            try {
                if (sshClient != null) {
                    string returnString = string.Empty;
                    returnString = sshClient.CreateCommand(string.Format(commString)).Execute();
                    logger.Info("[EXECUTE COMMAND] : " + commString);
                    return returnString;
                } else {
                    return "";
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return "";
            }
        }



        public void sshDisconnect(ref SshClient sshClient) {
            try {
                if (sshClient != null) {
                    if (sshClient.IsConnected) {
                        sshClient.Disconnect();
                    }
                    sshClient = null;
                }
            } catch (System.Exception ex) {
                logger.Error(ex.ToString());
                sshClient = null;
            }
        }
    }
}
