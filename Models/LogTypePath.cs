using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolMaster.Models {
     public class LogTypePath {
        //public static string coreLogPath = "/esl/core/data/";
        //public static string dbLogPath = "/esl/db/pg_log/";
        //public static string failOverLogPath = "/esl/log/logger/";
        //public static string emsLogPath = "/esl/web/logs/";
        //public static string gatewayLogPath = "/usr/local/esl/log/";
        //public static string Gen2NetworkConfPath = "/conf/ip.conf";
        //public static string GenServerNetworkConfPath = "/etc/network/interface";

        public string coreLogPath { get { return "/esl/core/data/"; } }
        public string dbLogPath { get { return "/esl/db/pg_log/"; } }
        public string failOverLogPath { get { return "/esl/log/logger/"; } }
        public string emsLogPath { get { return "/esl/web/logs/"; } }
        public string gatewayLogPath { get { return "/usr/local/esl/log/"; } }
        public string Gen2NetworkConfPath { get { return "/conf/ip.conf"; } }
        public string GenServerNetworkConfPath { get { return "/etc/network/interfaces";} }

        public string gatewaySendLogPath { get { return "/esl/core/eslFTP_HOME/Gateway_Log"; } }
    }
}
