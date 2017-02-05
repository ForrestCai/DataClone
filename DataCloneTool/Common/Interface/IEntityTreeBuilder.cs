using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public interface IEntityTreeBuilder
    {
        List<EntityNode> BuildEntityTrees(EntityConfig entityConfig);
    }
}
