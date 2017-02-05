using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public class ScriptHelper : IScriptHelper
    {
        private IEntityTreeBuilder _entityTreeBuilder;
        private IScriptDalHelper _dalHelper;

        public ScriptHelper(IEntityTreeBuilder entityTreeBuilder, IScriptDalHelper dalHelper)
        {
            if (entityTreeBuilder == null) throw new ArgumentNullException("entityTreeBuilder");
            if (dalHelper == null) throw new ArgumentNullException("dalHelper");

            _entityTreeBuilder = entityTreeBuilder;
            _dalHelper = dalHelper;
        }

        public Dictionary<string, string> GetColumnNameAndTypeDic(Entity entity)
        {
            var columnNameAndTypeDic = new Dictionary<string, string>();
            var columNameAndTypeStrings = _dalHelper.GetColumNameAndTypeList(entity.TableName);
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

        public string GetSelectString(Dictionary<string, string> columnNameAndTypeDic, Entity entity,
            string currentParentEntityName, Dictionary<string, string> parentEntityRow, string globalPrimaryKeyValue = "")
        {
            var selectSB = new StringBuilder("select ");

            foreach (var item in columnNameAndTypeDic)
            {
                selectSB.Append(item.Key + ",");
            }

            selectSB = selectSB.Replace(",", string.Empty, selectSB.Length - 1, 1);
            selectSB.Append(" from " + entity.TableName);


            if (entity.Global && !string.IsNullOrEmpty(globalPrimaryKeyValue))
            {
                selectSB.Append(" where ");
                selectSB.Append(string.Format("{0} = {1}", entity.IdentityColumnName, globalPrimaryKeyValue));
            }
            else
            {
                selectSB.Append(" where ");

                if (entity.Filter > 0)
                {
                    selectSB.Append(entity.IdentityColumnName + " = " + entity.Filter.ToString());
                }
                else if (!string.IsNullOrEmpty(entity.SelectCondition))
                {
                    selectSB.Append(string.Format(entity.SelectCondition, parentEntityRow[entity.IdentityColumnName]));
                }
                else
                {
                    var currentParentReference = entity.References.First(reference => reference.ReferredEntityName.Equals(currentParentEntityName, StringComparison.OrdinalIgnoreCase));
                    selectSB.Append(currentParentReference.OwnColumnName.ToString() + "=");
                    selectSB.Append(parentEntityRow[currentParentReference.ReferredColumnName]);

                    if (entity.Restrictions != null && entity.Restrictions.Length > 0)
                    {
                        foreach (var restricion in entity.Restrictions)
                        {
                            selectSB.Append(string.Format(" and {0} = {1}", restricion.ColumnName, restricion.Value));
                        }
                    }
                }
            }

            return selectSB.ToString();
        }

        public string GetInsertString(Dictionary<string, string> columnNameAndTypeDic, EntityNode entityNode,
            Dictionary<string, string> sourceRow, Entity currentParentEntity)
        {
            var insertSB = new StringBuilder();
            var entity = entityNode.CurrentEntity;

            if (entity.Global)
            {
                insertSB.AppendLine("if not exists(select top 1 1 from " + entity.TableName + " where " + entity.UniqueColumnName + "=" + sourceRow[entity.UniqueColumnName] + ")");

                insertSB.AppendLine("begin");
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

                if (entity.References != null)
                {
                    var currentColumnReference = entity.References.FirstOrDefault(reference => reference.OwnColumnName.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (currentColumnReference != null && currentColumnReference.IsReferredIdentityColumn)
                    {
                        if (currentParentEntity != null && currentColumnReference.ReferredEntityName.Equals(currentParentEntity.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            value = currentParentEntity.IdentityColumnVariable;
                        }
                        else
                        {
                            var currentReferredEntity = entityNode.ParentEntityNodes.FirstOrDefault(parentEntityNode => parentEntityNode.CurrentEntity.Name.Equals(currentColumnReference.ReferredEntityName, StringComparison.OrdinalIgnoreCase));

                            value = string.Format("(select NewId from {0} where OldId = {1})", currentReferredEntity.CurrentEntity.IdentityValueMapTable, value.ToString());
                        }
                    }
                }

                insertSB.Append(value + ",");
            }

            insertSB = insertSB.Replace(",", string.Empty, insertSB.Length - 1, 1);
            insertSB = insertSB.Append(")");

            if (!entity.Global && entity.ReferredColumns.Count == 0)
            {
                return insertSB.ToString();
            }

            if (entity.ReferredColumns.Any(referredColumn => referredColumn.Equals(entity.IdentityColumnName, StringComparison.OrdinalIgnoreCase)))
            {
                insertSB.AppendLine();
                insertSB.Append(string.Format("set {0} = scope_identity()", entity.IdentityColumnVariable));
            }            

            if (entity.Global)
            {
                insertSB.AppendLine();
                insertSB.AppendLine("end");
                insertSB.AppendLine("else");
                insertSB.AppendLine("begin");

                insertSB.Append("select top 1 " + entity.IdentityColumnVariable + " = ");
                insertSB.Append(entity.IdentityColumnName);
                insertSB.Append(" from " + entity.TableName);
                insertSB.AppendLine(" where " + entity.UniqueColumnName + "=" + sourceRow[entity.UniqueColumnName]);
                insertSB.Append("end");
            }

            return insertSB.ToString();
        }

        public void CloneChilds(EntityNode parentEntityNode, EntityNode childEntityNode)
        {
            List<Entity> newEntities = new List<Entity>();

            // for building the trees, add the parentEntityNode and childEnityNode entity to the list
            parentEntityNode.CurrentEntity.MasterEntity = null;
            newEntities.Add(parentEntityNode.CurrentEntity);
            newEntities.Add(childEntityNode.CurrentEntity);
            foreach (var tempChildEntityNode in parentEntityNode.ChildEntityNodes)
            {
                var stack = new Stack<EntityNode>();
                stack.Push(tempChildEntityNode);

                while (stack.Any())
                {
                    var currentEntityNode = stack.Pop();
                    var newEntity = currentEntityNode.CurrentEntity.Clone();
                    newEntity.IncreaseGeneration();

                    if (!newEntities.Any(entity => entity.Name.Equals(newEntity.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!string.IsNullOrEmpty(newEntity.MasterEntity))
                        {
                            if (newEntity.MasterEntity.Equals(parentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                newEntity.MasterEntity = childEntityNode.CurrentEntity.Name;
                            }
                            else
                            {
                                if (newEntity.Generation == 1)
                                {
                                    newEntity.MasterEntity = string.Format("{0}_{1}", newEntity.MasterEntity, newEntity.Generation);
                                }
                                else
                                {
                                    newEntity.MasterEntity = string.Format("{0}_{1}", newEntity.MasterEntity.Split(new string[] { "_" }, StringSplitOptions.None)[0], newEntity.Generation);
                                }
                            }
                        }
                        else
                        {
                            if (newEntity.References != null)
                            {
                                if (newEntity.Generation == childEntityNode.CurrentEntity.Generation)
                                {
                                    continue;
                                }

                                var referringChildEntity = newEntity.References.Any(reference => reference.ReferredEntityName.Equals(childEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase));
                                var globalParentEntity = currentEntityNode.ParentEntityNodes.FirstOrDefault(parentEntity => parentEntity.CurrentEntity.Global);
                                foreach (var reference in newEntity.References)
                                {
                                    if (globalParentEntity != null && globalParentEntity.CurrentEntity.Name.Equals(reference.ReferredEntityName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        continue;
                                    }

                                    if (referringChildEntity)
                                    {
                                        if (reference.ReferredEntityName.Equals(childEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (newEntity.Generation == 1)
                                            {
                                                reference.ReferredEntityName = string.Format("{0}_{1}", reference.ReferredEntityName, newEntity.Generation);
                                            }
                                            else
                                            {
                                                reference.ReferredEntityName = string.Format("{0}_{1}", reference.ReferredEntityName.Split(new string[] { "_" }, StringSplitOptions.None)[0], newEntity.Generation);
                                            }
                                        }
                                        else if (reference.ReferredEntityName.Equals(parentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            reference.ReferredEntityName = childEntityNode.CurrentEntity.Name;
                                        }
                                    }
                                    else
                                    {
                                        if (!reference.ReferredEntityName.Equals(parentEntityNode.CurrentEntity.Name))
                                        {
                                            if (newEntity.Generation == 1)
                                            {
                                                reference.ReferredEntityName = string.Format("{0}_{1}", reference.ReferredEntityName, newEntity.Generation);
                                            }
                                            else
                                            {
                                                reference.ReferredEntityName = string.Format("{0}_{1}", reference.ReferredEntityName.Split(new string[] { "_" }, StringSplitOptions.None)[0], newEntity.Generation);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        newEntities.Add(newEntity);
                    }

                    foreach (var parentEntity in currentEntityNode.ParentEntityNodes)
                    {
                        if (parentEntity.CurrentEntity.Global)
                        {
                            if (!newEntities.Any(entity => entity.Name.Equals(parentEntity.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                // Add global entity.
                                newEntities.Add(parentEntity.CurrentEntity);

                                var globalReference = newEntity.References.FirstOrDefault(reference => reference.ReferredEntityName.Equals(parentEntity.CurrentEntity.Name));
                                if (globalReference != null)
                                {
                                    globalReference.Processed = true;
                                }
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

            var roots = _entityTreeBuilder.BuildEntityTrees(new EntityConfig { Entities = newEntities.ToArray() });
            var tempChilds = roots.First(root => root.CurrentEntity.Name.Equals(parentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase)).ChildEntityNodes;
            var childSelfNode = tempChilds.First(child => object.ReferenceEquals(child.CurrentEntity, childEntityNode.CurrentEntity));

            childSelfNode.ChildEntityNodes.ForEach(child => {
                var wrongParent = child.ParentEntityNodes.FirstOrDefault(parent => object.ReferenceEquals(parent.CurrentEntity, childEntityNode.CurrentEntity));
                if (wrongParent != null)
                {
                    child.ParentEntityNodes.Remove(wrongParent);
                    child.ParentEntityNodes.Add(childEntityNode);
                }
            });

            var realChildNodes = tempChilds.FindAll(child => !object.ReferenceEquals(child.CurrentEntity, childEntityNode.CurrentEntity));
            realChildNodes.AddRange(childSelfNode.ChildEntityNodes);
            realChildNodes.ForEach(child =>
            {
                if (child.CurrentEntity.References != null)
                {
                    var wrongReference = child.CurrentEntity.References.FirstOrDefault(reference =>
                    { return reference.ReferredEntityName.Equals(parentEntityNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase); });

                    if (wrongReference != null)
                    {
                        wrongReference.ReferredEntityName = childEntityNode.CurrentEntity.Name;
                    }
                }

                var wrongParent = child.ParentEntityNodes.FirstOrDefault(parent => object.ReferenceEquals(parent.CurrentEntity, parentEntityNode.CurrentEntity));
                if (wrongParent != null)
                {
                    child.ParentEntityNodes.Remove(wrongParent);
                    child.ParentEntityNodes.Add(childEntityNode);
                }
            });

            childEntityNode.ChildEntityNodes.AddRange(realChildNodes);
        }
    }
}