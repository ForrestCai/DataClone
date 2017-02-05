using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public class EntityTreeBuilder : IEntityTreeBuilder
    {
        public List<EntityNode> BuildEntityTrees(EntityConfig entityConfig)
        {
            var entityNodeRoots = new List<EntityNode>();
            var childEntityList = new List<Entity>();

            foreach (Entity entity in entityConfig.Entities)
            {
                var hasNoGolbalReference = false;

                if (entity.References != null)
                {
                    foreach (var tempReference in entity.References)
                    {
                        var referredEntity = entityConfig.Entities.FirstOrDefault(innerEntity => innerEntity.Name.Equals(tempReference.ReferredEntityName, StringComparison.OrdinalIgnoreCase));
                        if (referredEntity != null && !referredEntity.Global)
                        {
                            hasNoGolbalReference = true;
                            break;
                        }
                    }
                }

                if ((entity.References == null || !hasNoGolbalReference) && string.IsNullOrEmpty(entity.MasterEntity))
                {
                    entityNodeRoots.Add(new EntityNode
                    {
                        CurrentEntity = entity
                    });
                    continue;
                }

                childEntityList.Add(entity);
            }

            while (childEntityList.Count > 0)
            {
                List<Entity> foundEntity = new List<Entity>();
                foreach (var childEntity in childEntityList)
                {
                    var parentEntityNodes = FindParentEntityNode(entityNodeRoots, childEntity);
                    if (parentEntityNodes != null && ((childEntity.References != null && parentEntityNodes.Count == childEntity.References.Length) || 
                        (parentEntityNodes.Any() && !string.IsNullOrEmpty(childEntity.MasterEntity))))
                    {
                        var newEntityNode = new EntityNode()
                        {
                            CurrentEntity = childEntity,
                            ParentEntityNodes = new List<EntityNode>()
                        };

                        foreach (var parentEntityNode in parentEntityNodes)
                        {
                            newEntityNode.ParentEntityNodes.Add(parentEntityNode);

                            if (parentEntityNode.ChildEntityNodes == null)
                            {
                                parentEntityNode.ChildEntityNodes = new List<EntityNode>();
                            }
                            parentEntityNode.ChildEntityNodes.Add(newEntityNode);

                            if (childEntity.References != null)
                            {
                                foreach (var reference in childEntity.References)
                                {
                                    if (reference.ReferredEntityName.Equals(parentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (reference.ReferredColumnName.Equals(parentEntityNode.CurrentEntity.IdentityColumnName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            reference.IsReferredIdentityColumn = true;
                                        }

                                        if (!parentEntityNode.CurrentEntity.ReferredColumns.Any(s => s.Equals(reference.ReferredColumnName, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            parentEntityNode.CurrentEntity.ReferredColumns.Add(reference.ReferredColumnName);
                                        }
                                    }
                                }
                            }
                        }

                        foundEntity.Add(childEntity);
                    }
                }

                foreach (var item in foundEntity)
                {
                    childEntityList.Remove(item);
                }
            }

            var realRootEntityNode = entityNodeRoots.First(node => !node.CurrentEntity.Global);

            // If root has reference, it must refer to the global entity. Then set the global entity as the root's parent entity.
            if (realRootEntityNode.CurrentEntity.References != null && realRootEntityNode.CurrentEntity.References.Any())
            {
                // Currently only support one entity refer to one global entity
                var reference = realRootEntityNode.CurrentEntity.References[0];
                var parentEntityNode = entityNodeRoots.FirstOrDefault(node => node.CurrentEntity.Name.Equals(reference.ReferredEntityName, StringComparison.OrdinalIgnoreCase));

                if (parentEntityNode != null && reference.ReferredColumnName.Equals(parentEntityNode.CurrentEntity.IdentityColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    reference.IsReferredIdentityColumn = true;
                }

                realRootEntityNode.ParentEntityNodes = new List<EntityNode>();
                realRootEntityNode.ParentEntityNodes.Add(parentEntityNode);
            }

            return entityNodeRoots;
        }

        private List<EntityNode> FindParentEntityNode(List<EntityNode> entityNodeRoots, Entity entity)
        {
            List<EntityNode> result = new List<EntityNode>();

            foreach (var entityNode in entityNodeRoots)
            {
                var stack = new Stack<EntityNode>();
                stack.Push(entityNode);

                while (stack.Any())
                {
                    var currentEntityNode = stack.Pop();

                    if (!string.IsNullOrEmpty(entity.MasterEntity))                        
                    {
                        if (entity.MasterEntity.Equals(currentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase) 
                            && !result.Any(innerEntityNode => Object.ReferenceEquals(innerEntityNode, currentEntityNode)))
                        {
                            result.Add(currentEntityNode);   
                        }
                    }
                    
                    if (entity.References != null)
                    {
                        foreach (var reference in entity.References)
                        {
                            if (currentEntityNode.CurrentEntity.Name.Equals(reference.ReferredEntityName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!result.Any(innerEntityNode => Object.ReferenceEquals(innerEntityNode, currentEntityNode)))
                                {
                                    result.Add(currentEntityNode);
                                }
                                break;
                            }
                        }
                    }

                    if (currentEntityNode.ChildEntityNodes != null && currentEntityNode.ChildEntityNodes.Any())
                    {
                        foreach (var child in currentEntityNode.ChildEntityNodes)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }

            if (result.Count > 0) return result;
            return null;
        }
    }
}
