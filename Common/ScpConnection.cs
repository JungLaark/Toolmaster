using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolMaster.Models;

namespace ToolMaster.Common {
    class ScpConnection {

        ScpClient scpClient = null;
        string ipAddr = string.Empty;
        Logger logger = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddr">ESN IP</param>
        public ScpConnection(string ipAddr) {
            logger = LogManager.GetCurrentClassLogger();
            this.ipAddr = ipAddr;
            scpClient = new ScpClient(ipAddr,
                      int.Parse(Setting.Default.SshPort),
                      Setting.Default.SshId,
                      Setting.Default.SshPw
                       );
            logger.Info("ScpConnection: " + ipAddr + ", " + Setting.Default.SshPort);
        }

        public ScpClient connectScp() {
            try {
                if (scpClient != null) {

                    scpClient.Connect();
                    return scpClient;
                } else {
                    return null;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        public long downloadTextFile(ScpClient scpClient, string remotePath, string localPath) {
            try {

                long fileSize = 0;

                if(scpClient == null) {
                    logger.Error("scpClient is null.");
                    return 0;
                }
                //directoryInfo = null;
                //directoryInfo = new DirectoryInfo(localPath);

                using (Stream stream = File.OpenWrite(localPath)) {
                    scpClient.Download(remotePath, stream);
                    fileSize = stream.Length;
                }

                //scpClient.Download(remotePath, directoryInfo);
                //뭐지? 이해가 안감..... 경로에 왜 
                //logger.Info("COMPLETE DOWNLOAD [remote] : " + remotePath);
                //logger.Info("[local] : " + Setting.Default.localPath);

                return fileSize;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return 0;
            }
        }

        public string readFile() {
            try {
                string localPath = Setting.Default.localPath + Setting.Default.textFileName;
                string fileName = Setting.Default.textFileName;
                string result = string.Empty;

                if (File.Exists(localPath)) {
                    using (StreamReader streamReader = new StreamReader(localPath, Encoding.Default, true)) {
                        result = streamReader.ReadToEnd();
                    }
                    logger.Info("COMPLETE FILE READ " + localPath);
                }

                File.Delete(localPath);
                logger.Info("COMPLETE FILE DELETE " + localPath);

                return result;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        public List<StoreTextFile> deserializeText(string originText) {
            try {
                StoreTextFile storeTextFile = null;
                List<StoreTextFile> listStoreTextFile = new List<StoreTextFile>();
                JArray jArray = JsonConvert.DeserializeObject<JArray>(originText);

                foreach (JObject jObject in jArray) {
                    storeTextFile = new StoreTextFile() {
                        esn_url = jObject["esn_url"].ToString() == "" ? "" : jObject["esn_url"].ToString(),
                        str_code = jObject["str_code"].ToString() == "" ? "" : jObject["str_code"].ToString(),
                        str_name = jObject["str_name"].ToString() == "" ? "" : jObject["str_name"].ToString()
                    };
                    listStoreTextFile.Add(storeTextFile);
                }

                return listStoreTextFile;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listStore"></param>
        /// <returns></returns>
        public bool textFileSave(List<Store> listStore) {
            try {

                if (listStore == null) {
                    return false;
                }

                string fileName = Setting.Default.localPath + Setting.Default.textFileName;
                List<StoreTextFile> listStoreTextFile = new List<StoreTextFile>();
                JsonSerializer jsonSerializer = new JsonSerializer();
                jsonSerializer.Formatting = Formatting.Indented;
                jsonSerializer.NullValueHandling = NullValueHandling.Ignore;    

                foreach (Store store in listStore) {
                    StoreTextFile storeTextFile = new StoreTextFile() {
                        esn_url = store.ip_addr,
                        str_code = store.str_code,
                        str_name = store.str_name,
                    };
                    listStoreTextFile.Add(storeTextFile);
                }

                /*Create text file in local path*/
                if(listStoreTextFile != null) {
                    using (StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create), Encoding.Default)) {
                        using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                            jsonSerializer.Serialize(jsonWriter, listStoreTextFile);
                        }
                    }
                    logger.Info("COMPLETE FILE SAVE " + fileName);
                }

                return true;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 로컬에 저장된 파일을 보낸다. 그냥 원격지에 파일 저장하면 될텐데..뭐 로컬에도 있으면 
        /// </summary>
        /// <returns>string: 저장된 remote path</returns>
        public string sendFileToRemote() {
            try {
                string fileName = Setting.Default.localPath + Setting.Default.textFileName;
                string remotePath = Setting.Default.remotePath + Setting.Default.textFileName;
                if (this.scpClient != null && this.scpClient.IsConnected) {
                    this.scpClient.Upload(new FileInfo(fileName), remotePath);
                }
                return remotePath;
                
            }catch(Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        public bool disconnScp(ScpClient scpCient) {
            try {
                if (scpClient != null) {
                    if (scpClient.IsConnected) {
                        scpClient.Disconnect();
                    }

                    scpClient = null;
                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }
    }
}
