using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public interface IScriptHelper
    {
        Dictionary<string, string> GetColumnNameAndTypeDic(Entity entity);

        string GetSelectString(Dictionary<string, string> columnNameAndTypeDic, Entity entity, 
            string currentParentEntityName, Dictionary<string, string> parentEntityRow, string globalPrimaryKeyValue = "");

        string GetInsertString(Dictionary<string, string> columnNameAndTypeDic, EntityNode entityNode,
            Dictionary<string, string> sourceRow, Entity currentParentEntity);

        void CloneChilds(EntityNode parentEntityNode, EntityNode childEntityNode);
    }
}
