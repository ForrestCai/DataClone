using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public class ScriptEngine : IScriptEngine
    {
        private IEntityTreeBuilder _entityTreeBuilder;
        private IScriptHelper _scriptHelper;
        private IScriptDalHelper _dalHelper;

        public ScriptEngine(IEntityTreeBuilder entityTreeBuilder, IScriptHelper scriptHelper, IScriptDalHelper dalHelper)
        {
            if (entityTreeBuilder == null) throw new ArgumentNullException("entityTreeBuilder");
            if (scriptHelper == null) throw new ArgumentNullException("scriptHelper");
            if (dalHelper == null) throw new ArgumentNullException("dalHelper");

            _entityTreeBuilder = entityTreeBuilder;
            _scriptHelper = scriptHelper;
            _dalHelper = dalHelper;
        }

        public string BuildScript(EntityConfig entityConfig, string rootEntityFileterValue)
        {
            var entityRootNodes = _entityTreeBuilder.BuildEntityTrees(entityConfig);
            // Only support one root
            var realRootNode = entityRootNodes.First(entityNode => !entityNode.CurrentEntity.Global);
            realRootNode.CurrentEntity.Filter = int.Parse(rootEntityFileterValue);
            var scriptSB = new StringBuilder();
            scriptSB.Append(BuildScript(realRootNode, null, realRootNode.ParentEntityNodes == null ? null : realRootNode.ParentEntityNodes[0].CurrentEntity));
            return scriptSB.ToString();

        }

        private string BuildScript(EntityNode entityNode, Dictionary<string, string> sourceParentEntityRow, Entity parentEntity)
        {
            var scriptSB = new StringBuilder();

            var currentEntity = entityNode.CurrentEntity;
            var columnNameAndTypeDic = _scriptHelper.GetColumnNameAndTypeDic(currentEntity);
            var sourceRows = _dalHelper.ExecuteSelectString(_scriptHelper.GetSelectString(columnNameAndTypeDic, currentEntity, parentEntity != null ? parentEntity.Name : null, sourceParentEntityRow), columnNameAndTypeDic);

            if (sourceRows.Count > 0 && (entityNode.CurrentEntity.Global || (entityNode.ChildEntityNodes != null && entityNode.ChildEntityNodes.Count > 0 && !currentEntity.IdentityColumnVariableDeclared)))
            {
                scriptSB.AppendLine("declare " + currentEntity.IdentityColumnVariable + " as int");
                currentEntity.IdentityColumnVariableDeclared = true;
            }

            var currentIdentityValueInserted = false;

            if (sourceRows.Count > 0)
            {
                if (!string.IsNullOrEmpty(currentEntity.MasterEntity))
                {
                    var parentEntityNode = entityNode.ParentEntityNodes.First(parentNode => parentNode.CurrentEntity.Name.Equals(currentEntity.MasterEntity, StringComparison.OrdinalIgnoreCase));
                    if (parentEntityNode.ChildEntityNodes.Count != entityNode.ChildEntityNodes.Count)
                    {
                        _scriptHelper.CloneChilds(parentEntityNode, entityNode);
                    }
                }
            }

            foreach (var sourceRow in sourceRows)
            {
                if (entityNode.ParentEntityNodes != null && entityNode.ParentEntityNodes.Any(parentEntityNode => parentEntityNode.CurrentEntity.Global))
                {
                    // Currently only support one entity refer to one global entity
                    var globalParentNode = entityNode.ParentEntityNodes.First(parentEntityNode => parentEntityNode.CurrentEntity.Global);
                    var globalReference = entityNode.CurrentEntity.References.First(reference => reference.ReferredEntityName.Equals(globalParentNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase));
                    var globalKeyValue = sourceRow[globalReference.OwnColumnName];
                    if (globalParentNode.CurrentEntity.GlobalEntityKeyList == null || !globalParentNode.CurrentEntity.GlobalEntityKeyList.Contains(globalKeyValue))
                    {
                        scriptSB.AppendLine(HandleGlobalEntity(globalParentNode, globalKeyValue));
                        if (globalParentNode.CurrentEntity.GlobalEntityKeyList == null)
                            globalParentNode.CurrentEntity.GlobalEntityKeyList = new List<string>();
                        globalParentNode.CurrentEntity.GlobalEntityKeyList.Add(globalKeyValue);
                    }
                }

                currentIdentityValueInserted = false;

                var insertString = _scriptHelper.GetInsertString(columnNameAndTypeDic, entityNode, sourceRow, parentEntity);

                scriptSB.AppendLine(insertString);

                if (entityNode.ChildEntityNodes != null && entityNode.ChildEntityNodes.Any())
                {
                    foreach (var childEntityNode in entityNode.ChildEntityNodes)
                    {
                        if (childEntityNode.CurrentEntity.References != null)
                        {
                            Reference[] noGlobalReference;
                            // Currently only support one entity refer to one global entity
                            if (childEntityNode.ParentEntityNodes.Any(parentEntityNode => parentEntityNode.CurrentEntity.Global))
                            {
                                var globalParentNode = childEntityNode.ParentEntityNodes.First(parentEntityNode => parentEntityNode.CurrentEntity.Global);
                                noGlobalReference = childEntityNode.CurrentEntity.References.Where(reference => !reference.ReferredEntityName.Equals(globalParentNode.CurrentEntity.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
                            }
                            else
                            {
                                noGlobalReference = childEntityNode.CurrentEntity.References;
                            }

                            if ((noGlobalReference.Count(reference => !reference.Processed && reference.IsReferredIdentityColumn) > 1 ||
                                noGlobalReference.Any(reference => reference.Processed && reference.IsReferredIdentityColumn &&
                                    reference.ReferredEntityName.Equals(currentEntity.Name, StringComparison.OrdinalIgnoreCase))))
                            {
                                if (!currentEntity.IdentityValueMapTableDeclared)
                                {
                                    scriptSB.AppendLine(string.Format("Declare {0} As Table (OldId int, NewId int)", currentEntity.IdentityValueMapTable));
                                    currentEntity.IdentityValueMapTableDeclared = true;
                                }

                                var currentReference = childEntityNode.CurrentEntity.References.First(reference => reference.IsReferredIdentityColumn && reference.ReferredEntityName.Equals(currentEntity.Name, StringComparison.OrdinalIgnoreCase));

                                if (!currentIdentityValueInserted)
                                {
                                    var referredColumn = currentReference.ReferredColumnName;
                                    var sourceValue = sourceRow[referredColumn];

                                    scriptSB.AppendLine(string.Format("Insert Into {0} (OldId, NewId) Values ({1}, {2})", currentEntity.IdentityValueMapTable,
                                        sourceValue.ToString(), currentEntity.IdentityColumnVariable));

                                    currentReference.Processed = true;
                                    currentIdentityValueInserted = true;
                                }
                                continue;
                            }
                        }

                        scriptSB.Append(BuildScript(childEntityNode, sourceRow, currentEntity));
                    }
                }
            }

            return scriptSB.ToString();
        }

        private string HandleGlobalEntity(EntityNode node, string keyValue)
        {
            var scriptSB = new StringBuilder();
            var currentEntity = node.CurrentEntity;

            var columnNameAndTypeDic = _scriptHelper.GetColumnNameAndTypeDic(currentEntity);
            var sourceRow = _dalHelper.ExecuteSelectString(_scriptHelper.GetSelectString(columnNameAndTypeDic, currentEntity, null, null, keyValue), columnNameAndTypeDic);

            if (!currentEntity.IdentityColumnVariableDeclared)
            {
                scriptSB.AppendLine("declare " + currentEntity.IdentityColumnVariable + " as int");
                currentEntity.IdentityColumnVariableDeclared = true;
            }

            var currentIdentityValueInserted = false;

            var insertString = _scriptHelper.GetInsertString(columnNameAndTypeDic, node, sourceRow[0], null);

            scriptSB.AppendLine(insertString);

            foreach (var childEntityNode in node.ChildEntityNodes)
            {
                if (childEntityNode.CurrentEntity.References != null)
                {
                    if (!currentEntity.IdentityValueMapTableDeclared)
                    {
                        scriptSB.AppendLine(string.Format("Declare {0} As Table (OldId int, NewId int)", currentEntity.IdentityValueMapTable));
                        currentEntity.IdentityValueMapTableDeclared = true;
                    }

                    var currentReference = childEntityNode.CurrentEntity.References.First(reference => reference.IsReferredIdentityColumn && reference.ReferredEntityName.Equals(currentEntity.Name, StringComparison.OrdinalIgnoreCase));
                    currentReference.Processed = true;

                    if (!currentIdentityValueInserted)
                    {
                        var referredColumn = currentReference.ReferredColumnName;
                        var sourceValue = keyValue;

                        scriptSB.AppendLine(string.Format("Insert Into {0} (OldId, NewId) Values ({1}, {2})", currentEntity.IdentityValueMapTable,
                            sourceValue.ToString(), currentEntity.IdentityColumnVariable));
                        currentIdentityValueInserted = true;
                    }
                }
            }

            return scriptSB.ToString();
        }
    }
}
