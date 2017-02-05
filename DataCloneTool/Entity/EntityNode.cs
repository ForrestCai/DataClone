using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public class EntityNode
    {
        public Entity CurrentEntity { get; set; }

        public List<EntityNode> ParentEntityNodes { get; set;}

        public List<EntityNode> ChildEntityNodes { get; set; }
    }
}
