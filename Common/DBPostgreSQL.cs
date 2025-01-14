using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ToolMaster.Models;

namespace ToolMaster.Common {
    class DBPostgreSQL {
        Logger logger = null;
        string connString = string.Empty;
        string ipAddr = string.Empty;
        string userName = string.Empty;
        string dbName = string.Empty;
        string port = string.Empty;
        string password = string.Empty;

        public DBPostgreSQL(string ipAddr, string userName, string dbName, string port, string password) {
            logger = LogManager.GetCurrentClassLogger();
            this.ipAddr = ipAddr;
            this.userName = userName;
            this.dbName = dbName;
            this.port = port;
            this.password = password;
            this.connString = String.Format(
                              "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                              ipAddr,
                              userName,
                              dbName,
                              port,
                              password);
        }
        /// <summary>
        /// get table list in ems
        /// </summary>
        /// <returns></returns>
        public List<SelectedTable> listSelectedTable() {
            try {

                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    List<SelectedTable> listSelectedTable = new List<SelectedTable>();

                    conn.Open();

                    using (var command = new Npgsql.NpgsqlCommand()) {
                        command.Connection = conn;
                        command.CommandText = "SELECT * " +
                                              "FROM pg_catalog.pg_tables " +
                                              "WHERE schemaname = 'public' ";
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                listSelectedTable.Add(new SelectedTable() { tablename = reader[1].ToString() });
                            }
                        }
                    }
                    return listSelectedTable;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }

        }

        public List<NHProduct> listProduct(string productName) {
            try {

                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    List<NHProduct> listNhProduct = new List<NHProduct>();

                    conn.Open();

                    using (var command = new Npgsql.NpgsqlCommand()) {
                        command.Connection = conn;
                        command.CommandText = "SELECT b.name, a.device_id, a.mcode, b.price " +
                                              "FROM g_met_matching a " +
                                              "LEFT OUTER JOIN g_merchandise b " +
                                              "ON a.mcode = b.barcode " +
                                              "WHERE b.name = '" + productName + "' ";
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                listNhProduct.Add(new NHProduct() {
                                    name = reader[0].ToString(),
                                    deviceId = reader[1].ToString(),
                                    mCode = reader[2].ToString(),
                                    price = reader[3].ToString()
                                });
                            }
                        }
                    }
                    return listNhProduct;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }

        }
        /// <summary>
        /// get seleted table's columns and rows
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable selectTable(string tableName) {

            try {
                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    DataTable dataTable = new DataTable();
                    conn.Open();

                    using (var command = new Npgsql.NpgsqlCommand()) {
                        command.Connection = conn;
                        command.CommandText = "SELECT * "
                                            + "FROM information_schema.columns "
                                            + "WHERE table_catalog = 'esl' "
                                             + "AND table_schema = 'public' "
                                            + "AND table_name = '" + tableName.Trim() + "'";

                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                dataTable.Columns.Add(reader[3].ToString());
                            }
                        }

                        command.CommandText = "SELECT * "
                                            + "FROM " + tableName + "";

                        using (var reader = command.ExecuteReader()) {

                            int k = 0;
                            while (reader.Read()) {
                                DataRow row = dataTable.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++) {
                                    row[dataTable.Columns[i]] = reader[k].ToString();
                                    k++;
                                }
                                dataTable.Rows.Add(row);
                                k = 0;
                            }
                            return dataTable;
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// delete row
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool deleteRow(string tableName, string id) {

            try {
                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand("DELETE FROM " + tableName + " WHERE id = '" + id + "'")) {
                        cmd.Connection = conn;
                        int k = cmd.ExecuteNonQuery();

                        if (k > 0) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// specific table delete with product_key
        /// </summary>
        /// <param name="tempString"></param>
        /// <param name="paramIpAddr"></param>
        /// <returns></returns>
        public int deleteFromCode(string tempString, string paramIpAddr) {
            try {
                string _connString = string.Empty;
                if (paramIpAddr != null) {
                    _connString = String.Format(
                                  "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                                  paramIpAddr,
                                  userName,
                                  dbName,
                                  port,
                                  password);

                    using (var conn = new Npgsql.NpgsqlConnection(_connString)) {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand("DELETE FROM g_usertext WHERE product_key in (" + tempString + ")")) {
                            using (NpgsqlTransaction transaction = conn.BeginTransaction()) {
                                cmd.Connection = conn;
                                cmd.Transaction = transaction;
                                int k = cmd.ExecuteNonQuery();
                                transaction.Commit();
                                return k;
                            }
                        }
                    }
                } else {
                    return 0;
                }

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return 0;
            }
        }
        /// <summary>
        /// g_usertext 삭제 전 데이터 조회
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="paramIpAddr"></param>
        /// <param name="tempString"></param>
        /// <returns></returns>
        public int selectFromCode(string tableName, string paramIpAddr, string tempString) {
            try {
                int count = 0;
                string _connString = string.Empty;
                if (paramIpAddr != null) {
                    _connString = String.Format(
                                  "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                                  paramIpAddr,
                                  userName,
                                  dbName,
                                  port,
                                  password);

                    using (var conn = new Npgsql.NpgsqlConnection(_connString)) {
                        conn.Open();
                        using (var command = new Npgsql.NpgsqlCommand()) {
                            command.Connection = conn;

                            command.CommandText = "SELECT COUNT(*) count "
                                            + " FROM " + tableName + ""
                                            + " WHERE product_key in (" + tempString + ")";

                            using (var reader = command.ExecuteReader()) {
                                if (reader.Read()) {
                                    count = int.Parse(reader[0].ToString());
                                }
                            }
                        }
                    }

                    return count;
                }
                return 0;

            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return 0;
            }
        }


        /// <summary>
        /// truncate multiple table
        /// </summary>
        /// <param name="paramIpAddr"></param>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        public bool truncateTable(string paramIpAddr, string tableNames) {

            try {
                string _connString = string.Empty;
                if (paramIpAddr != null) {/*다수 store*/
                    _connString = String.Format(
                                  "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                                  paramIpAddr,
                                  userName,
                                  dbName,
                                  port,
                                  password);

                    using (var conn = new Npgsql.NpgsqlConnection(_connString)) {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand("TRUNCATE TABLE " + tableNames.TrimEnd(','), conn)) {
                            int k = cmd.ExecuteNonQuery();
                        }
                    }
                } else {/*단일 store*/
                    using (var conn = new Npgsql.NpgsqlConnection(this.connString)) {
                        conn.Open();
                        using (var cmd = new Npgsql.NpgsqlCommand("TRUNCATE TABLE " + tableNames.TrimEnd(','), conn)) {
                            int k = cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }

        public string selectData(string ipAddr, string tableName, string strName, string strCode) {
            try {
                string returnString = String.Empty;
                string connString = String.Format(
                  "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                  ipAddr,
                  userName,
                  dbName,
                  port,
                  password);
                returnString = "[" + strCode + ":" + strName + "]" + System.Environment.NewLine;

                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    conn.Open();
                    using (var command = new Npgsql.NpgsqlCommand()) {
                        command.Connection = conn;
                        command.CommandText = "SELECT product_key "
                                            + "FROM " + tableName
                                            + " WHERE LENGTH(product_key) = 13";
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                returnString += reader[0].ToString() + System.Environment.NewLine;
                            }
                            return returnString;
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return "";
            }
        }

        public bool updateUserTable(string ipAddr) {
            try {
                List<string> listReturnString = new List<string>();
                string connString = String.Format(
                  "Server={0};Username={1};Database={2};Port={3};Password={4};Timeout=5",
                  ipAddr,
                  userName,
                  dbName,
                  port,
                  password);

                using (var conn = new Npgsql.NpgsqlConnection(connString)) {
                    conn.Open();
                    using (var command = new Npgsql.NpgsqlCommand()) {
                        command.Connection = conn;
                        command.CommandText = "SELECT id FROM g_user "
                                            + " WHERE fail_count != '0' ";
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                //returnString += reader[0].ToString() + System.Environment.NewLine;

                                listReturnString.Add(reader[0].ToString());
                            }
                        }


                        if (listReturnString.Count > 0) {

                            foreach(string temp in listReturnString) {
                                using (var updateCommand = new Npgsql.NpgsqlCommand()) {
                                    updateCommand.Connection = conn;
                                    updateCommand.CommandText = "UPDATE g_user SET fail_count = 0 WHERE id = '" + temp + "'";

                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                        } else {
                            return false;
                        }
                    }
                    return true;
                }
            } catch (Exception ex) {
                logger.Error(ex.ToString());
                return false;
            }
        }
    }
}
