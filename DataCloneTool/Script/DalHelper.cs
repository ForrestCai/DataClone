using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;

namespace DataCloneTool
{
    public class ScriptDalHelper : IScriptDalHelper
    {
        public string GetColumNameAndTypeList(string tableName)
        {
            string result = string.Empty;
            using (var con = new SqlConnection(Setting.SourceConString))
            {
                con.Open();
                using (var com = new SqlCommand("GetColumNameAndTypeList", con))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    var parameter = new SqlParameter();
                    parameter.ParameterName = "@tableName";
                    parameter.SqlDbType = SqlDbType.VarChar;
                    parameter.SqlValue = tableName;
                    com.Parameters.Add(parameter);
                    using(var reader = com.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = reader.GetString(0);
                        }                        
                    }
                }
            }

            return result;
        }

        public List<Dictionary<string, string>> ExecuteSelectString(string selectString, Dictionary<string, string> columnNameAndTypeDic)
        {
            var result = new List<Dictionary<string, string>>();
            using (var con = new SqlConnection(Setting.SourceConString))
            {
                con.Open();
                using (var com = new SqlCommand(selectString, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, string>();
                            for (int index = 0; index < columnNameAndTypeDic.Count; index++)
                            {
                                string value = null;
                                if (reader.IsDBNull(index))
                                {
                                    value = "null";
                                }
                                else
                                {
                                    switch (columnNameAndTypeDic.Values.ToArray<string>()[index])
                                    {
                                        case "smallint":
                                            value = reader.GetInt16(index).ToString();
                                            break;
                                        case "int":
                                            value = reader.GetInt32(index).ToString();
                                            break;
                                        case "char":
                                        case "varchar":
                                        case "nvarchar":
                                            value = reader.GetString(index).RaplaceQuote().AppendQuote();
                                            break;
                                        case "datetime":
                                            value = reader.GetDateTime(index).ToString("yyyy-MM-dd HH:mm:ss").RaplaceQuote().AppendQuote();
                                            break;
                                        case "bit":
                                            value = reader.GetBoolean(index) ? "1":"0";
                                            break;
                                        case "money":
                                            value = reader.GetDecimal(index) .ToString();
                                            break;
                                        case "float":
                                            value = reader.GetDouble(index).ToString();
                                            break;
                                    }
                                }
                                row.Add(columnNameAndTypeDic.Keys.ToArray<string>()[index], value);
                            }
                            result.Add(row);
                        }
                    }
                }
            }

            return result;
        }
    }
}
