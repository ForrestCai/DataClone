using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public static class CloneHelper
    {
        public static Dictionary<string, string> GetColumnNameAndTypeDic(Entity entity)
        {
            var columnNameAndTypeDic = new Dictionary<string, string>();
            var columNameAndTypeStrings = CloneDalHelper.GetColumNameAndTypeList(entity.TableName);
            var columNameAndTypeArray = columNameAndTypeStrings.Split(new string[] { ";" }, StringSplitOptions.None);

            foreach (var columNameAndTypeString in columNameAndTypeArray)
            {
                var columNameAndType = columNameAndTypeString.Split(new string[] { "," }, StringSplitOptions.None);
                if (columNameAndType.Length == 2)
                {
                    columnNameAndTypeDic[columNameAndType[0]] = columNameAndType[1];
                }
            }
            return columnNameAndTypeDic;
        }

        public static string GetSelectString(Dictionary<string, string> columnNameAndTypeDic, Entity entity)
        {
            var selectSB = new StringBuilder("select ");

            foreach (var item in columnNameAndTypeDic)
            {
                selectSB.Append(item.Key + ",");
            }

            selectSB = selectSB.Replace(",", string.Empty, selectSB.Length - 1, 1);

            selectSB.Append(" from " + entity.TableName);

            if (!entity.Global)
            {
                selectSB.Append(" where id = 1 ");
            }

            return selectSB.ToString();
        }

        public static string GetSelectString(Dictionary<string, string> columnNameAndTypeDic, Entity entity, string currentParentEntityName, Dictionary<string, object> parentEntityRow)
        {
            var selectSB = new StringBuilder("select ");

            foreach (var item in columnNameAndTypeDic)
            {
                selectSB.Append(item.Key + ",");
            }

            selectSB = selectSB.Replace(",", string.Empty, selectSB.Length - 1, 1);
            selectSB.Append(" from " + entity.TableName);
            selectSB.Append(" where ");

            var currentParentReference = entity.References.First(reference => reference.ReferredEntityName.Equals(currentParentEntityName, StringComparison.OrdinalIgnoreCase));
            selectSB.Append(currentParentReference.OwnColumnName.ToString() + "=");

            switch (columnNameAndTypeDic[currentParentReference.OwnColumnName])
            {
                case "int":
                    selectSB.Append(parentEntityRow[currentParentReference.ReferredColumnName].ToString());
                    break;
                case "nvarchar":
                    selectSB.Append(string.Format("'{0}'", parentEntityRow[currentParentReference.ReferredColumnName].ToString()));
                    break;
            }

            return selectSB.ToString();
        }

        public static string GetInsertString(Dictionary<string, string> columnNameAndTypeDic, Entity entity,
            Dictionary<string, object> sourceRow, string currentParentEntityName, Dictionary<string, object> targetReferredColumnValues)
        {
            var insertSB = new StringBuilder();

            if (entity.Global)
            {
                insertSB.Append("if not exists(select top 1 1 from " + entity.TableName + " where " + entity.UniqueColumnName + "='" + sourceRow[entity.UniqueColumnName] + "')");

                insertSB.Append(" begin ");
            }

            insertSB.Append("insert into " + entity.TableName + " (");

            // Columns
            foreach (var item in columnNameAndTypeDic)
            {
                if (item.Key.Equals(entity.IdentityColumnName, StringComparison.OrdinalIgnoreCase)) continue;
                insertSB.Append(item.Key + ",");
            }

            insertSB = insertSB.Replace(",", string.Empty, insertSB.Length - 1, 1);
            insertSB = insertSB.Append(") values (");

            // Values
            foreach (var item in columnNameAndTypeDic)
            {
                if (item.Key.Equals(entity.IdentityColumnName, StringComparison.OrdinalIgnoreCase)) continue;
                var value = sourceRow[item.Key];

                if (entity.References != null && entity.References.Any(reference => reference.OwnColumnName.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    var currentColumnReference = entity.References.First(reference => reference.OwnColumnName.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (currentColumnReference.ReferredEntityName.Equals(currentParentEntityName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = targetReferredColumnValues[currentColumnReference.ReferredColumnName];
                    }
                    else
                    {
                        value = entity.ReferencesColumnValueMapping[currentColumnReference.ReferredEntityName][value];
                    }
                }

                switch (item.Value)
                {
                    case "int":
                        value = value.ToString();
                        break;
                    case "nvarchar":
                        value = string.Format("'{0}'", value);
                        break;
                }

                insertSB.Append(value + ",");
            }

            insertSB = insertSB.Replace(",", string.Empty, insertSB.Length - 1, 1);
            insertSB = insertSB.Append(")");

            if (entity.ReferredColumns.Count == 0)
            {
                return insertSB.ToString();
            }

            if (entity.ReferredColumns.Count == 1 && entity.ReferredColumns[0].Equals(entity.IdentityColumnName, StringComparison.OrdinalIgnoreCase))
            {
                insertSB.Append(" select convert(int, scope_identity()) as " + entity.ReferredColumns[0]);

                if (entity.Global)
                {
                    insertSB.Append(" end else begin ");
                    insertSB.Append(" select " + entity.IdentityColumnName + " as " + entity.ReferredColumns[0]);
                    insertSB.Append(" from " + entity.TableName);
                    insertSB.Append(" where " + entity.UniqueColumnName + "='" + sourceRow[entity.UniqueColumnName] + "'");
                    insertSB.Append(" end");
                }

                return insertSB.ToString();
            }

            insertSB.Append(" select ");

            foreach (var referredColumn in entity.ReferredColumns)
            {
                if (referredColumn.Equals(entity.IdentityColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    insertSB.Append("convert(int, scope_identity()) as " + referredColumn);
                }
                else
                {
                    insertSB.Append(referredColumn);
                }

                insertSB.Append(",");
            }

            insertSB = insertSB.Replace(",", string.Empty, insertSB.Length - 1, 1);
            insertSB.Append(" from " + entity.TableName);
            insertSB.Append(" where ");
            insertSB.Append(entity.IdentityColumnName + "= convert(int, scope_identity())");

            if (entity.Global)
            {
                insertSB.Append(" end else begin ");

                insertSB.Append(" select ");

                foreach (var referredColumn in entity.ReferredColumns)
                {
                    insertSB.Append(referredColumn);
                    insertSB.Append(",");
                }

                insertSB = insertSB.Replace(",", string.Empty, insertSB.Length - 1, 1);
                insertSB.Append(" from " + entity.TableName);
                insertSB.Append(" where " + entity.UniqueColumnName + "='" + sourceRow[entity.UniqueColumnName] + "'");
                insertSB.Append(" end");
            }

            return insertSB.ToString();
        }
    }
}
