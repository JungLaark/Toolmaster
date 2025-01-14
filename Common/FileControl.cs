using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace ToolMaster.Common {
    class FileControl {
        Logger logger = null;
        public string filePath { get; set; }
        public string fileName { get; set; }
        public FileControl(string filePath, string fileName) {
            this.filePath = filePath;
            this.fileName = fileName;
            logger = LogManager.GetCurrentClassLogger();
        }

        public FileControl() {
            logger = LogManager.GetCurrentClassLogger();
        }

        public void makeTextFile(string fileName, string filePath) {
            try {
                /*1. 해당 경로에 text 파일 만들어놓고 */
                /*2. 생성된 것이 확인되면 */
                /*3. 거기다 append*/
                    
                if(!File.Exists(filePath + fileName)) {
                    using (File.Create(filePath + fileName)) {
                    }
                } else {
                    logger.Error("Already exist file.");
                }
            }catch(Exception ex) {
                logger.Error(ex.ToString());
            }

        }

        public bool appendTextFile(string fileName, string filePath, string appendText) {
            try {

                if(File.Exists(filePath + fileName)) {
                    using (FileStream fs = new FileStream(filePath+fileName, FileMode.Append, FileAccess.Write)) {
                        using (StreamWriter sw = new StreamWriter(fs)) {
                            sw.WriteLine(appendText);
                        }
                    }
                    return true;
                } else {
                    logger.Error("Already exist file.");
                    return false;
                }
                    
            }catch(Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }
    }
}
