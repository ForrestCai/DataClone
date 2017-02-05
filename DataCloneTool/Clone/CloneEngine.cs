using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public class CloneEngine
    {
        private IEntityTreeBuilder _entityTreeBuilder;

        public CloneEngine(IEntityTreeBuilder entityTreeBuilder)
        {
            if (entityTreeBuilder == null) throw new ArgumentNullException("entityTreeBuilder");

            _entityTreeBuilder = entityTreeBuilder;
        }

        public void Clone(EntityConfig entityConfig)
        {
            var entityNodeRoots = _entityTreeBuilder.BuildEntityTrees(entityConfig);

            DoClone(entityNodeRoots);
        }

        private void DoClone(List<EntityNode> entityNodeRoots)
        {
            foreach (var entityNodeRoot in entityNodeRoots)
            {
                // Clone current entity
                var currentEntity = entityNodeRoot.CurrentEntity;
                var columnNameAndTypeDic = CloneHelper.GetColumnNameAndTypeDic(currentEntity);
                var sourceRows = CloneDalHelper.ExecuteSelectString(CloneHelper.GetSelectString(columnNameAndTypeDic, currentEntity), columnNameAndTypeDic);

                var targetReferredColumnTypes = new Dictionary<string, string>();
                foreach (var referredColumn in currentEntity.ReferredColumns)
                {
                    targetReferredColumnTypes.Add(referredColumn, columnNameAndTypeDic[referredColumn]);
                }

                foreach (var sourceRow in sourceRows)
                {
                    var insertString = CloneHelper.GetInsertString(columnNameAndTypeDic, currentEntity, sourceRow, string.Empty, null);
                    var newTargetReferredColumnValues = CloneDalHelper.ExecuteInsertString(insertString, targetReferredColumnTypes);

                    if (entityNodeRoot.ChildEntityNodes != null && entityNodeRoot.ChildEntityNodes.Any())
                    {
                        foreach (var childEntityNode in entityNodeRoot.ChildEntityNodes)
                        {
                            if (childEntityNode.CurrentEntity.References.Count() - childEntityNode.CurrentEntity.ReferencesColumnValueMapping.Count > 1
                                 || childEntityNode.CurrentEntity.ReferencesColumnValueMapping.ContainsKey(currentEntity.Name))
                            {
                                if (!childEntityNode.CurrentEntity.ReferencesColumnValueMapping.ContainsKey(currentEntity.Name))
                                {
                                    childEntityNode.CurrentEntity.ReferencesColumnValueMapping.Add(currentEntity.Name, new Dictionary<object, object>());
                                }

                                var referredColumn = childEntityNode.CurrentEntity.References.First(reference => reference.ReferredEntityName.Equals(currentEntity.Name, StringComparison.OrdinalIgnoreCase)).ReferredColumnName;
                                var sourceValue = sourceRow[referredColumn];
                                var targetValue = newTargetReferredColumnValues[referredColumn];

                                if (!childEntityNode.CurrentEntity.ReferencesColumnValueMapping[currentEntity.Name].ContainsKey(sourceValue))
                                {
                                    childEntityNode.CurrentEntity.ReferencesColumnValueMapping[currentEntity.Name].Add(sourceValue, targetValue);
                                }

                                continue;
                            }

                            CloneEntity(childEntityNode, sourceRow, currentEntity.Name, newTargetReferredColumnValues);
                        }
                    }
                }
            }
        }

        private void CloneEntity(EntityNode entityNode, Dictionary<string, object> sourceParentEntityRow,
            string parentEntityName, Dictionary<string, object> targetReferredColumnValues)
        {
            var currentEntity = entityNode.CurrentEntity;
            var columnNameAndTypeDic = CloneHelper.GetColumnNameAndTypeDic(currentEntity);
            var sourceRows = CloneDalHelper.ExecuteSelectString(CloneHelper.GetSelectString(columnNameAndTypeDic, currentEntity, parentEntityName, sourceParentEntityRow), columnNameAndTypeDic);

            var targetReferredColumnTypes = new Dictionary<string, string>();
            foreach (var referredColumn in currentEntity.ReferredColumns)
            {
                targetReferredColumnTypes.Add(referredColumn, columnNameAndTypeDic[referredColumn]);
            }

            foreach (var sourceRow in sourceRows)
            {
                var insertString = CloneHelper.GetInsertString(columnNameAndTypeDic, currentEntity, sourceRow, parentEntityName, targetReferredColumnValues);
                var newTargetReferredColumnValues = CloneDalHelper.ExecuteInsertString(insertString, targetReferredColumnTypes);

                if (entityNode.ChildEntityNodes != null && entityNode.ChildEntityNodes.Any())
                {
                    foreach (var childEntityNode in entityNode.ChildEntityNodes)
                    {
                        if (childEntityNode.CurrentEntity.References.Count() - childEntityNode.CurrentEntity.ReferencesColumnValueMapping.Count > 1
                            || childEntityNode.CurrentEntity.ReferencesColumnValueMapping.ContainsKey(currentEntity.Name))
                        {
                            if (!childEntityNode.CurrentEntity.ReferencesColumnValueMapping.ContainsKey(currentEntity.Name))
                            {
                                childEntityNode.CurrentEntity.ReferencesColumnValueMapping.Add(currentEntity.Name, new Dictionary<object, object>());
                            }

                            var referredColumn = childEntityNode.CurrentEntity.References.First(reference => reference.ReferredEntityName.Equals(currentEntity.Name, StringComparison.OrdinalIgnoreCase)).ReferredColumnName;
                            var sourceValue = sourceRow[referredColumn];
                            var targetValue = newTargetReferredColumnValues[referredColumn];

                            if (!childEntityNode.CurrentEntity.ReferencesColumnValueMapping[currentEntity.Name].ContainsKey(sourceValue))
                            {
                                childEntityNode.CurrentEntity.ReferencesColumnValueMapping[currentEntity.Name].Add(sourceValue, targetValue);
                            }

                            continue;
                        }

                        CloneEntity(childEntityNode, sourceRow, currentEntity.Name, newTargetReferredColumnValues);
                    }
                }
            }
        }
    }
}
