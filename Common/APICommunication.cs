using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RestSharp.Authenticators;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using ToolMaster.Models;
using DevExpress.Xpf.WindowsUI;
using System.Security.Policy;
using DevExpress.Xpf.Editors.Themes;
using NPOI.SS.Formula.Functions;

namespace ToolMaster.Common {
    public class APICommunication {

        Logger logger = null;
        RestResponse restResponse = null;
        RestClient restClient = null;
        RestRequest restRequest = null;
        JToken jToken = null;
        List<Store> listStore = null;
        List<Gateway> listGateway = null;
        List<TagInfo> listTagInfo = null;
        string token = null;
        string emsToken = null;
        string authToken = null;

        public APICommunication() {
            logger = LogManager.GetCurrentClassLogger();
        }

        #region Function
        /// <summary>
        /// for get token, have to login.
        /// </summary>
        public void login(string ipAddr) {
            try {
                string loginJsonText = " {"
                        + " \"header\": {"
                        + " \"token\": null"
                        + " },"
                        + " \"body\": {"
                        + " \"username\": \"" + Setting.Default.EsnId + "\","
                        + " \"password\": \"" + Setting.Default.EsnPassword + "\" "
                        + " }"
                        + " }";

                restResponse = requestRest(loginJsonText, "/user/login", "/user/login", ipAddr);

                if (restResponse != null && restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);

                    jObject.TryGetValue("body", out jToken);
                    this.token = jToken.Value<String>("token");
                    this.authToken = jToken.Value<String>("x_auth_token");

                    logger.Info("LOGIN SUCCESS!");

                } else {
                    logger.Error("FAILED LOGIN! ");
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
            }
        }

        /// <summary>
        /// for get token, have to ems login.
        /// </summary>
        public void emsLogin(string ipAddr) {
            try {
                string loginJsonText = " {"
                        + " \"user_id\": \"" + "develop" + "\","
                        + " \"user_pw\": \"" + "esl" + "\" "
                        + " }";

                restResponse = requestRest(loginJsonText, "/esl/service/v2/login", "emsLogin", ipAddr);

                if (restResponse != null) {
                    if (restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                        JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);

                        jObject.TryGetValue("data", out jToken);
                        //this.emsToken = jToken.Value<String>("token");
                        //var temp = jToken[0];

                        logger.Info(ipAddr + " LOGIN SUCCESS!");

                        foreach (var item in jToken.Children()) {
                            var itemProperty = item.Children<JProperty>();
                            var element = itemProperty.FirstOrDefault(x => x.Name == "token");
                            if (element != null) {
                                this.emsToken = element.Value.ToString();
                            }
                        }
                    } else {
                        logger.Error(ipAddr + " FAILED LOGIN! " + restResponse.StatusCode);
                    }
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
            }
        }

        #region Store

        /// <summary>
        /// get store list 
        /// </summary>
        public List<Store> postStoreList(string ipAddr) {

            try {
                RestResponse restResponse;
                listStore = new List<Store>();
                Store store = null;

                string jsonText = "{"
                                + " \"header\": {"
                                + " \"token\" : \"" + this.token + "\" "
                                + " }, "
                                + " \"body\": { "
                                + " \"limit\": 0, "
                                + " \"offset\": 0 "
                                + " }"
                                + " }";

                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, "/store/list", "/store/list", ipAddr);


                if (restResponse != null && restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    jToken = null;
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("body", out jToken);

                    foreach (JToken item in jToken) {
                        store = new Store() {
                            str_code = item.Value<string>("str_code"),
                            str_name = item.Value<string>("str_name"),
                            ip_addr = item.Value<string>("ip_addr"),
                            biztp_code = item.Value<string>("biztp_code"),
                            is_checked = false,
                            active_device_type = item.Value<string>("active_device_type"),
                            core_status= item.Value<string>("core_status"),
                            gateway_status= item.Value<string>("gateway_status"),
                            gw_fail_cnt= item.Value<int>("gw_fail_cnt"),
                            gw_success_cnt= item.Value<int>("gw_success_cnt"),
                            gw_tot_cnt= item.Value<int>("gw_tot_cnt"),
                            network_status= item.Value<string>("network_state"),
                            reg_date = item.Value<string>("reg_date"),
                            slave_status= item.Value<string>("slave_state"),
                            sync_status= item.Value<string>("sync_status"),
                            update_time = item.Value<string>("update_time"),
                            str_type = item.Value<string>("str_type")

                        };
                        listStore.Add(store);
                    }

                    logger.Info("SUCCESS GET STORE LIST!");

                    return listStore;
                } else {
                    return null;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Store 다중 삭제 
        /// </summary>
        /// <param name="listStore"></param>
        /// <returns></returns>
        public bool deleteStore(List<Store> listStore) {
            try {
               bool flag = false;

                foreach (Store store in listStore) {
                    string jsonText = "{"
                       + " \"header\": {"
                       + " \"token\" : \"" + this.token + "\" "
                       + " }, "
                       + " \"body\": { "
                       + " \"str_code\": \"" + store.str_code.Trim() + "\" "
                       + " }"
                       + " }";

                    string url = "http://" + Setting.Default.EsnIp + ":8081";
                    restClient = new RestClient(url);
                    restRequest = new RestRequest("/store/delete", Method.POST);
                    restRequest.AddHeader("x-Auth-Token", this.authToken);
                    restRequest.AddParameter("application/json", jsonText, ParameterType.RequestBody);
                    restRequest.RequestFormat = RestSharp.DataFormat.Json;

                    var response = restClient.Execute(restRequest, Method.POST);

                    if (response.StatusCode == HttpStatusCode.OK) {
                        logger.Info("매장 코드 : " + store.str_code + " 매장 명 : " + store.str_name + " 이/가 삭제되었습니다.");
                        flag = true;
                    } else {
                        logger.Error("매장 코드 : " + store.str_code + " 매장 명 : " + store.str_name + " 이/가 삭제 시 오류가 발생했습니다.");
                        flag = false;
                    }
                }
                return flag;

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// update store name 
        /// </summary>
        /// <param name="storeName"></param>
        public bool updateStore(Store store, string storeName) {
            try {
                bool flag = false;

                    string jsonText = "{"
                       + " \"header\": {"
                       + " \"token\" : \"" + this.token + "\" "
                       + " }, "
                       + " \"body\": { "
                       + " \"str_code\": \"" + store.str_code + "\" ,"
                       + " \"biztp_code\": \"" + store.biztp_code + "\" ,"
                       + " \"shop_code_array\": [\"1000\"] ,"
                       + " \"str_name\": \"" + storeName + "\" ,"
                       + " \"str_type\": \"" + store.str_type + "\" ,"
                       + " \"reg_date\": \"" + store.reg_date + "\" "
                       + " }"
                       + " }";

                    string url = "http://" + Setting.Default.EsnIp + ":8081";
                    restClient = new RestClient(url);
                    restRequest = new RestRequest("/store/biz_update", Method.POST);
                    restRequest.AddHeader("x-Auth-Token", this.authToken);
                    restRequest.AddParameter("application/json", jsonText, ParameterType.RequestBody);
                    restRequest.RequestFormat = RestSharp.DataFormat.Json;

                    var response = restClient.Execute(restRequest, Method.POST);

                    if (response.StatusCode == HttpStatusCode.OK) {
                        //logger.Info("매장 코드 : " + store.str_code + " 매장 명 : " + store.str_name + " 이/가 삭제되었습니다.");
                        flag = true;
                    } else {
                        //logger.Error("매장 코드 : " + store.str_code + " 매장 명 : " + store.str_name + " 이/가 삭제 시 오류가 발생했습니다.");
                        flag = false;
                    }
                return flag;

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Gateway

        /// <summary>
        /// Get Gateway list!
        /// </summary>
        /// <param name="idAddr"></param>
        /// <returns></returns>
        public List<Gateway> postGatewayList(string ipAddr, string path) {
            try {
                RestResponse restResponse;
                Gateway gateway = null;
                listGateway = new List<Gateway>();

                string jsonText = "{}";
                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "gateway", ipAddr);

                if (restResponse != null && restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    jToken = null;
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("data", out jToken);

                    if (jObject != null) {
                        foreach (JToken item in jToken) {
                            gateway = new Gateway() {
                                aliveInterval = item.Value<string>("aliveInterval"),
                                beaconChannel = item.Value<string>("beaconChannel"),
                                beaconPower = item.Value<string>("beaconPower"),
                                commonChannel = item.Value<string>("commonChannel"),
                                commonChannel2 = item.Value<string>("commonChannel2"),
                                commonChannelRF = item.Value<string>("commonChannelRF"),
                                commonChannelRF2 = item.Value<string>("commonChannel2"),
                                dataChannel = item.Value<string>("dataChannel"),
                                dataChannel2 = item.Value<string>("dataChannel2"),
                                dataChannelRF = item.Value<string>("dataChannelRF"),
                                dataChannelRF2 = item.Value<string>("dataChannelRF2"),
                                device_id = item.Value<string>("device_id"),
                                fwupdate_state = item.Value<string>("fwupdate_state"),
                                gw_type = item.Value<string>("gw_type"),
                                invalid_tag_count = item.Value<string>("invalid_tag_count"),
                                ip = item.Value<string>("ip"),
                                name = item.Value<string>("name"),
                                no = item.Value<string>("no"),
                                normal_tag_count = item.Value<string>("normal_tag_count"),
                                operation_mode = item.Value<string>("operation_mode"),
                                removed_tag_count = item.Value<string>("removed_tag_count"),
                                state = item.Value<string>("state"),
                                storage_box = item.Value<string>("storage_box"),
                                version = item.Value<string>("version")
                            };
                            listGateway.Add(gateway);

                        }
                    } else {
                        logger.Info("조회된 데이터가 없습니다.");
                        return null;
                    }

                    logger.Info(ipAddr + " SUCCESS GET STORE LIST!");

                    return listGateway;
                } else {
                    return listGateway;
                }
            } catch (Exception ex) {
                logger.Error(ipAddr + " " + ex.ToString());
                return listGateway;
            }
        }

        /// <summary>
        /// Get Gateway Info!
        /// </summary>
        /// <param name="idAddr"></param>
        /// <returns></returns>
        public GatewayInfo postGatewayInfo(string ipAddr, string deviceId, string path) {
            try {
                RestResponse restResponse;
                GatewayInfo gatewayInfo = null;

                string jsonText = " {"
                       + " \"device_id\": \"" + deviceId + "\"," 
                       + " }";

                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "gateway", ipAddr);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    //jArray = new JArray();
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("data", out jToken);

                    if (jToken != null) {
                        gatewayInfo = new GatewayInfo() {

                            aliveInterval = jToken[0].Value<string>("aliveInterval"),
                            beaconChannel = jToken[0].Value<string>("beaconChannel"),
                            beaconPower = jToken[0].Value<string>("beaconPower"),
                            channelMode = jToken[0].Value<string>("channelMode"),
                            channelThreshold = jToken[0].Value<string>("channelThreshold"),
                            commonChannel = jToken[0].Value<string>("commonChannel"),
                            commonChannel2 = jToken[0].Value<string>("commonChannel2"),
                            commonChannelRF = jToken[0].Value<string>("commonChannelRF"),
                            commonChannelRF2 = jToken[0].Value<string>("commonChannelRF2"),
                            commonChannelSPI = jToken[0].Value<string>("commonChannelSPI"),
                            dataChannel = jToken[0].Value<string>("dataChannel"),
                            dataChannel2 = jToken[0].Value<string>("dataChannel2"),
                            dataChannelRF = jToken[0].Value<string>("dataChannelRF"),
                            dataChannelRF2 = jToken[0].Value<string>("dataChannelRF2"),
                            dataChannelSPI = jToken[0].Value<string>("dataChannelSPI"),
                            device_id = jToken[0].Value<string>("device_id"),
                            downloadSpeed = jToken[0].Value<string>("downloadSpeed"),
                            ip = jToken[0].Value<string>("ip"),
                            maxTagCount = jToken[0].Value<string>("maxTagCount"),
                            result = jToken[0].Value<string>("result"),
                            retryFirst = jToken[0].Value<string>("retryFirst"),
                            retryNext = jToken[0].Value<string>("retryNext"),
                            rfSecurity = jToken[0].Value<string>("rfSecurity"),
                            scanDuration = jToken[0].Value<string>("scanDuration"),
                            timeoutGateway = jToken[0].Value<string>("timeoutGateway"),
                            timeoutHeT = jToken[0].Value<string>("timeoutHeT"),
                            timeoutInvalidTag = jToken[0].Value<string>("timeoutInvalidTag"),
                            timeoutMeT = jToken[0].Value<string>("timeoutMeT"),
                        };
                    } else {
                        logger.Info("조회된 데이터가 없습니다.");
                        return null;
                    }

                    logger.Info(ipAddr + " SUCCESS GET GATEWAY LIST!");

                    return gatewayInfo;
                } else {
                    return gatewayInfo;
                }
            } catch (Exception ex) {
                logger.Error(ipAddr + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Set Gateway!
        /// </summary>
        /// <param name="idAddr"></param>
        /// <returns></returns>
        public bool setGatewayInfo(GatewayInfo paramGatewayInfo, string path, string alive, string maxTag) {
            try {
                RestResponse restResponse;
                GatewayInfo gatewayInfo = null;

                if(paramGatewayInfo == null) {
                    logger.Error(path + " paramGatewayInfo return value null");
                    MessageBox.Show("paramGatewayInfo is null");
                    return false;
                }

                string jsonText = " {"
                                + " \"device_id\": \"" + paramGatewayInfo.device_id + "\","
                                + " \"aliveInterval\": \"" + alive + "\", "
                                + " \"beaconChannel\": \"" + paramGatewayInfo.beaconChannel + "\" ,"
                                + " \"beaconPower\": \"" + paramGatewayInfo.beaconPower + "\" ,"
                                + " \"channelMode\": \"" + paramGatewayInfo.channelMode + "\" ,"
                                + " \"channelThreshold\": \"" + paramGatewayInfo.channelThreshold + "\" ,"
                                + " \"commonChannel\": \"" + paramGatewayInfo.commonChannel + "\" ,"
                                + " \"commonChannel2\": \"" + paramGatewayInfo.commonChannel2 + "\" ,"
                                + " \"commonChannelRF\": \"" + paramGatewayInfo.commonChannelRF + "\" ,"
                                + " \"commonChannelRF2\": \""  + paramGatewayInfo.commonChannelRF2 + "\" ,"
                                + " \"commonChannelSPI\": \"" + paramGatewayInfo.commonChannelSPI + "\" ,"
                                + " \"dataChannel\": \"" + paramGatewayInfo.dataChannel  +     "\" ,"
                                + " \"dataChannel2\": \"" + paramGatewayInfo.dataChannel2 + "\" ,"
                                + " \"dataChannelRF\": \"" + paramGatewayInfo.dataChannelRF + "\" ,"
                                + " \"dataChannelRF2\": \"" + paramGatewayInfo.dataChannelRF2 + "\" ,"
                                + " \"dataChannelSPI\": \"" + paramGatewayInfo.dataChannelSPI + "\" ,"
                                + " \"downloadSpeed\": \"" + paramGatewayInfo.downloadSpeed + "\" ,"
                                + " \"ip\": \"" + paramGatewayInfo.ip + "\" ,"
                                + " \"maxTagCount\": \"" + maxTag   +  "\" ,"
                                + " \"retryFirst\": \"" + paramGatewayInfo.retryFirst  +    "\","
                                + " \"retryNext\": \"" + paramGatewayInfo.retryNext  +  "\","
                                + " \"rfSecurity\": \"" + paramGatewayInfo.rfSecurity + "\" ,"
                                + " \"scanDuration\": \"" + paramGatewayInfo.scanDuration + "\" ,"
                                + " \"timeoutGateway\": \"" + paramGatewayInfo.timeoutGateway + "\" ,"
                                + " \"timeoutHeT\": \"" + paramGatewayInfo.timeoutHeT + "\" ,"
                                + " \"timeoutMeT\": \"" + paramGatewayInfo.timeoutMeT + "\" ,"
                                + " \"timeoutInvalidTag\": \"" + paramGatewayInfo.timeoutInvalidTag + "\" "
                                + " }";
                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "gateway", paramGatewayInfo.ip);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("data", out jToken);

                    if (jToken != null) {

                    } else {
                        logger.Info("조회된 데이터가 없습니다.");
                        return false;
                    }

                    logger.Info(paramGatewayInfo.device_id + " SUCCESS UPDATE GATEWAY LIST!");
                    
                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                logger.Error(paramGatewayInfo.device_id + " "+ ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 태그 갯수 리스트 조회
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<TagInfo> postTagInfo(string ipAddr, string paramStoreCode, string paramStoreName,string path) {
            try {
                RestResponse restResponse;

                TagInfo tagInfo = null;
                listTagInfo = new List<TagInfo>();

                string jsonText = "{}";
                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "tagInfo", ipAddr);

                if (restResponse != null && restResponse.StatusCode == System.Net.HttpStatusCode.OK) {
                    jToken = null;
                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("data", out jToken);

                    if (jObject != null) {
                        foreach (JToken item in jToken) {
                            tagInfo = new TagInfo() {
                                atGateway = item.Value<int>("At_Gateway"),
                                busy = item.Value<int>("Busy"),
                                connected = item.Value<int>("Connected"),
                                disconnected = item.Value<int>("Disconnected"),
                                inProgress = item.Value<int>("In_Progress"),
                                lowBattery = item.Value<int>("Low_Battery"),
                                removed = item.Value<int>("Removed"),
                                sortId = item.Value<int>("Sort_ID"),
                                storage = item.Value<int>("Storage"),
                                success = item.Value<int>("Success"),
                                total = item.Value<int>("Total"),
                                type = item.Value<string>("Type"),
                                waiting = item.Value<int>("Waiting"),
                                storeCode = paramStoreCode,
                                storeName = paramStoreName,
                            };

                            listTagInfo.Add(tagInfo);
                        }
                    } else {
                        logger.Info("조회된 데이터가 없습니다.");
                        return null;
                    }

                    logger.Info(ipAddr + " SUCCESS GET Tag Count LIST!");

                    return listTagInfo;
                } else {
                    return listTagInfo;
                }
            } catch (Exception ex) {
                logger.Error(ipAddr + " " + ex.ToString());
                return listTagInfo;
            }
        }

        public bool postDeleteTag(string tagId, string ipAddr, string path) {
            try {
                RestResponse restResponse;

                if (tagId == null) {
                    logger.Error(path + " tagId return value null");
                    return false;
                }

                string jsonText = " {"
                                + " \"device_id\": \"" + tagId + "\""
                                + " }";
                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "deleteTag", ipAddr);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK) {

                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("result", out jToken);

                    if (jToken.Equals("success")) {
                        return true;
                    } else {
                        logger.Info("조회된 데이터가 없습니다.");
                        return false;
                    }
                } else {
                    return false;
                }
            } catch (Exception ex) {
                return false;
            }
        }

        public bool requestTest(string skuCode, string price, string ipAddr, string path) {
            try {
                RestResponse restResponse;

                if (skuCode == null) {
                    logger.Error(path + " tagId return value null");
                    return false;
                }

                string jsonText = "{\"change\":{\"str_code\":\"1092\"," +
                    "\"sku_code\":\"" + skuCode + "\"," +
                    "\"cust_sku_nm\":\"\"," +
                    "\"new_uprice\":\"" +price+ "\"," +
                    "\"esl_new_uprice\":\""+price+"\"," +
                    "\"esl_sale_uprice\":\"\"," +
                    "\"esl_unit_price\":\"10g당 295원\"," +
                    "\"low_price_20\":\"\"," +
                    "\"evt_low_price_20\":\"\"," +
                    "\"esl_unit_cap\":\"10\"," +
                    "\"esl_tot_cap\":\"40\"," +
                    "\"clear_disc_rate\":\"\"," +
                    "\"enuri_rate_str\":\"\"," +
                    "\"enuri_amt_str\":\"\"," +
                    "\"promo_yn\":\"N\"," +
                    "\"esl_promo_code\":\"\"," +
                    "\"brand_code\":\"\"," +
                    "\"pl_yn\":\"\"," +
                    "\"src_yn\":\"\"," +
                    "\"cert_eco_tcode\":\"\"," +
                    "\"wine_img1\":\"101_01_014\"," +
                    "\"attr_val_nm\":\"\"," +
                    "\"image_code1\":\"\"," +
                    "\"image_code2\":\"\"," +
                    "\"origin_nm\":\"\"," +
                    "\"pog_no\":\"초콜릿 06 - 03 - 01 - 1\"," +
                    "\"g_code\":\"340\"," +
                    "\"g_nm\":\"과자\"," +
                    "\"m_code\":\"7860\"," +
                    "\"m_nm\":\"초콜릿\"," +
                    "\"d_code\":\"0476\"," +
                    "\"d_nm\":\"수입초콜릿\"," +
                    "\"qr_code_url\":\"\"," +
                    "\"rc_gubun\":\"\"," +
                    "\"bottle_price\":\"\"," +
                    "\"ori_bot_wine\":\"\"," +
                    "\"rc_cvt\":\"\"," +
                    "\"event_start_date\":\"\"," +
                    "\"event_end_date\":\"\"," +
                    "\"event_theme_type_code\":\"\"," +
                    "\"event_theme_type_nm\":\"\"," +
                    "\"event_type_code\":\"\"," +
                    "\"event_type_nm\":\"\"," +
                    "\"evnty_code\":\"\"," +
                    "\"evnty_nm\":\"\"," +
                    "\"pcard_club_id\":\"\"," +
                    "\"pcard_club_nm\":\"\"," +
                    "\"enuri_rate\":\"\"," +
                    "\"enuri_amt\":\"\"," +
                    "\"n1\":\"\"," +
                    "\"isp_cont1\":\"\"," +
                    "\"isp_cont2\":\"\"," +
                    "\"appy_date\":\"20171228\"}," +
                    "\"key\":\"" +skuCode + "\"}";

                restResponse = new RestResponse();
                restResponse = requestRest(jsonText, path, "requestTest", ipAddr);

                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK) {

                    JObject jObject = (JObject)JsonConvert.DeserializeObject(restResponse.Content);
                    jObject.TryGetValue("result", out jToken);

                    if (jToken.Equals("success")) {
                        logger.Info(skuCode + " 상품 정보 업데이트가 완료되었습니다.");
                        return true;
                    } else {
                        logger.Info(jToken);
                        return false;
                    }
                } else {
                    return false;
                }
            } catch (Exception ex) {
                return false;
            }
        }


        #endregion

        /// <summary>
        /// request method  - 공통
        /// </summary>
        /// <param name="jsonText">header + body</param>
        /// <param name="resource">ex) /user/login</param>
        /// <returns></returns>
        public RestResponse requestRest(string jsonText, string resource, string type, string ipAddr) {
            string url = string.Empty;
            try {
                string esnPort = string.Empty;

                if (type.Equals("gateway") || type.Equals("emsLogin") || type.Equals("tagInfo") || type.Equals("deleteTag") || type.Equals("requestTest")) {
                    //esnPort = "8080";//Core
                    esnPort = Setting.Default.corePort;
                } else {
                    //esnPort = "8081"; -> daemon port -> 192.168.30.30 emart
                    //esnPort = "8181"; //daemon port -> 192.168.30.30 hom
                    //eplus 61.33.142.222 nh
                    esnPort = Setting.Default.EsnPort;
                }

                url = "http://" + ipAddr + ":" + esnPort;

                //if (!pingIp(ipAddr)) {
                //    logger.Error("접근 할 수 없는 주소입니다. 입력 주소 : " + ipAddr);
                //    WinUIMessageBox.Show(messageBoxText: "접근 할 수 없는 주소입니다. 입력 주소 : " + ipAddr, caption: "입력 주소 확인", button: MessageBoxButton.OK);
                //    return null;
                //}

                logger.Info(url + " try to request");

                restClient = new RestClient(url);
                restRequest = new RestRequest(resource, Method.POST);

                if (type.Equals("/store/list")) {
                    restRequest.AddHeader("x-Auth-Token", this.authToken);
                } else if (type.Equals("gateway") || type.Equals("tagInfo") || type.Equals("deleteTag") || type.Equals("requestTest")) {
                    restRequest.AddHeader("token", this.emsToken);
                    restClient.Authenticator = new HttpBasicAuthenticator("esl", "esl");
                } else if (type.Equals("emsLogin")) {
                    restClient.Authenticator = new HttpBasicAuthenticator("esl", "esl");
                }

                restRequest.AddParameter("application/json", jsonText, ParameterType.RequestBody);
                restRequest.RequestFormat = RestSharp.DataFormat.Json;

                var response = restClient.Execute(restRequest, Method.POST);
                logger.Info(url + " success response");
                return (RestResponse)response;

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                WinUIMessageBox.Show(messageBoxText: "연결에 실패했습니다. 주소 및 포트를 확인해주세요. " + url, caption: "입력 주소 확인", button: MessageBoxButton.OK);
                return null;
            }
        }

        /// <summary>
        /// 유효한 IP인지 확인.
        /// </summary>
        /// <param name="ipAddr"></param>
        /// <returns></returns>
        public bool pingIp(string ipAddr) {
            try {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();

                options.DontFragment = true;

                string strData = "test";
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(strData);

                System.Net.NetworkInformation.PingReply reply = ping.Send(System.Net.IPAddress.Parse(ipAddr), 120, buffer, options);

                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success) {
                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        #endregion
    }
}
