using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolMaster.Models {
    public class Store {
        public string str_code { get; set; }
        public string str_name { get; set; }
        public string str_type { get; set; }
        public string ip_addr { get; set; }
        public bool is_checked { get; set; }
        public string biztp_code { get; set; }
        public string active_device_type { get; set; }
        public string core_status { get; set; }
        public string slave_status { get; set; }
        public string gateway_status { get; set; }
        public string network_status { get; set; }
        public string sync_status { get; set; }
        public int gw_tot_cnt { get; set; }
        public int gw_success_cnt { get; set; }
        public int gw_fail_cnt { get; set; }
        public string reg_date { get; set; }
        public string update_time { get; set; }
        public string db_replication { get; set; }
        public string mac_addr { get; set; }

        
        

    }
}
