using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolMaster.Models {
    public class TagInfo {
        public int connected { get; set; }
        public int busy { get; set; }
        public int removed { get; set; }
        public int disconnected { get; set; }
        public int success { get; set; }
        public int waiting { get; set; }
        public string type { get; set; }
        public int atGateway { get; set; }
        public int storage { get; set; }
        public int lowBattery { get; set; }
        public int sortId { get; set; }
        public int total { get; set; }
        public int inProgress { get; set; }
        public string storeCode { get; set; }
        public string storeName { get; set; }

    }
}
