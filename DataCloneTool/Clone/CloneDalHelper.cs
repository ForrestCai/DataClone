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
    public static class CloneDalHelper
    {
        public static string GetColumNameAndTypeList(string tableName)
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

        public static List<Dictionary<string, object>> ExecuteSelectString(string selectString, Dictionary<string, string> columnNameAndTypeDic)
        {
            var result = new List<Dictionary<string, object>>();
            using (var con = new SqlConnection(Setting.SourceConString))
            {
                con.Open();
                using (var com = new SqlCommand(selectString, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int index = 0; index < columnNameAndTypeDic.Count; index++)
                            {
                                object value = null;
                                if (reader.IsDBNull(index))
                                {
                                    value = null;
                                }
                                else
                                {
                                    switch (columnNameAndTypeDic.Values.ToArray<string>()[index])
                                    {
                                        case "int":
                                            value = reader.GetInt32(index).ToString();
                                            break;
                                        case "varchar":
                                        case "nvarchar":
                                            value = reader.GetString(index).RaplaceQuote();
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

        public static Dictionary<string, object> ExecuteInsertString(string insertString, Dictionary<string, string> targetReferredColumnTypes)
        {
            var result = new Dictionary<string, object>();
            using (var con = new SqlConnection(Setting.TargeConString))
            {
                con.Open();
                using (var com = new SqlCommand(insertString, con))
                {
                    if (targetReferredColumnTypes.Count == 0)
                    {
                        com.ExecuteNonQuery();
                    }
                    else
                    {
                        using (var reader = com.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (int index = 0; index < targetReferredColumnTypes.Count; index++)
                                {
                                    object value = null;
                                    switch (targetReferredColumnTypes.Values.ToArray<string>()[index])
                                    {
                                        case "int":
                                            value = reader.GetInt32(index);
                                            break;
                                        case "varchar":
                                            value = reader.GetString(index);
                                            break;
                                        case "nvarchar":
                                            value = reader.GetString(index);
                                            break;
                                    }
                                    result.Add(targetReferredColumnTypes.Keys.ToArray<string>()[index], value);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
