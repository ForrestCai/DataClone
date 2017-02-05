using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public interface IScriptDalHelper
    {
        string GetColumNameAndTypeList(string tableName);
        List<Dictionary<string, string>> ExecuteSelectString(string selectString, Dictionary<string, string> columnNameAndTypeDic);
    }
}
