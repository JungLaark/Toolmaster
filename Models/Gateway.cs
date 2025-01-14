using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolMaster.Models {
    public class Gateway {
        public string no { get; set; }
        public string beaconChannel { get; set; }
        public string device_id { get; set; }
        public string fwupdate_state { get; set; }
        public string ip { get; set; }
        public string dataChannelRF { get; set; }
        public string operation_mode { get; set; }
        public string aliveInterval { get; set; }
        public string gw_type { get; set; }
        public string commonChannel2 { get; set; }
        public string version { get; set; }
        public string commonChannelRF2 { get; set; }
        public string beaconPower { get; set; }
        public string dataChannelRF2 { get; set; }
        public string commonChannel { get; set; }
        public string commonChannelRF { get; set; }
        public string normal_tag_count { get; set; }
        public string storage_box { get; set; }
        public string name { get; set; }
        public string removed_tag_count { get; set; }
        public string dataChannel2 { get; set; }
        public string invalid_tag_count { get; set; }
        public string state { get; set; }
        public string dataChannel { get; set; }
    }
}
