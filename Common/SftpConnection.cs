using NLog;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolMaster.Common {
    public class SftpConnection {

        string address { get; set; }
        int port { get; set; }
        string id { get; set; }
        string password { get; set; }
        Logger logger = null;

        public SftpConnection(string address, int port, string id, string password) {
            this.address = address;
            this.port = port;
            this.id = id;
            this.password = password;

            logger = LogManager.GetCurrentClassLogger();

        }

        public bool downloadFileSftp(string localDir, string remoteDir) {
            try {

                using (SftpClient sftpClient = new SftpClient(address, port, id, password)) {
                    sftpClient.KeepAliveInterval = TimeSpan.FromSeconds(60);
                    sftpClient.ConnectionInfo.Timeout = TimeSpan.FromMinutes(180);
                    sftpClient.OperationTimeout = TimeSpan.FromMinutes(180);
                    sftpClient.Connect();

                    if (sftpClient.IsConnected) {
                        using (Stream stream = File.OpenWrite(remoteDir)) {
                            sftpClient.DownloadFile(localDir, stream);
                        }
                    }
                }
                return true;
            }catch(Exception ex) {

                return false;
            }
        }
    }
}
