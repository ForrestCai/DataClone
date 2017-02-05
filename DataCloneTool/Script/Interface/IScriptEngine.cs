using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public interface IScriptEngine
    {
        string BuildScript(EntityConfig entityConfig, string rootEntityFileterValue);
    }
}
