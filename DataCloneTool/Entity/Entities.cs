using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataCloneTool
{
    [XmlRoot("EntityConfig")]
    public class EntityConfig
    {
        [XmlArray("Entities")]
        public Entity[] Entities { get; set; }
    }

    [XmlRoot("Entity")]
    public class Entity
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("TableName")]
        public string TableName { get; set; }

        [XmlAttribute("IdentityColumnName")]
        public string IdentityColumnName { get; set; }

        [XmlAttribute("Global")]
        public bool Global { get; set; }

        [XmlAttribute("UniqueColumnName")]
        public string UniqueColumnName { get; set; }

        [XmlAttribute("Filter")]
        public int Filter { get; set; }

        [XmlAttribute("MasterEntity")]
        public string MasterEntity { get; set; }

        [XmlElement("SelectCondition")]
        public string SelectCondition { get; set; }

        [XmlArray("References")]
        public Reference[] References { get; set; }

        [XmlArray("Restrictions")]
        public Restriction[] Restrictions { get; set; }

        // Used for Clone
        [XmlIgnore()]
        public List<string> ReferredColumns = new List<string>();

        [XmlIgnore()]
        public List<string> GlobalEntityKeyList;

        // Used for Clone
        [XmlIgnore()]
        public Dictionary<string, Dictionary<object, object>> ReferencesColumnValueMapping = new Dictionary<string, Dictionary<object, object>>();

        [XmlIgnore()]
        public bool IdentityColumnVariableDeclared { get; set; }

        private string _identityColumnVariable;
        [XmlIgnore()]
        public string IdentityColumnVariable
        {
            get
            {
                if (string.IsNullOrEmpty(_identityColumnVariable))
                {
                    _identityColumnVariable = string.Format("@{0}_{1}", Name, IdentityColumnName);
                }
                return _identityColumnVariable;
            }
        }

        [XmlIgnore()]
        public bool IdentityValueMapTableDeclared { get; set; }

        private string _identityValueMapTable;
        [XmlIgnore()]
        public string IdentityValueMapTable
        {
            get
            {
                if (string.IsNullOrEmpty(_identityValueMapTable))
                {
                    if (!string.IsNullOrEmpty(this.MasterEntity))
                    {
                        _identityValueMapTable = string.Format("@{0}_{1}", this.Name, "IdentityValueMapTable");
                    }
                    else
                    {
                        _identityValueMapTable = string.Format("@{0}_{1}", this.Name.Split(new string[] { "_" }, StringSplitOptions.None)[0], "IdentityValueMapTable");
                    }
                }
                return _identityValueMapTable;
            }
        }

        private int _generation;
        [XmlIgnore()]
        public int Generation
        {
            get
            {
                return _generation;
            }
            private set
            {
                _generation = value;
            }
        }

        public void IncreaseGeneration()
        {
            _generation++;
            if (_generation == 1)
            {
                this.Name = string.Format("{0}_{1}", this.Name, _generation);
            }
            else
            {
                this.Name = string.Format("{0}_{1}", this.Name.Split(new string[] {"_"}, StringSplitOptions.None)[0], _generation);
            }
        }

        public Entity Clone()
        {
            Entity newEntity = new Entity{
                Name = this.Name,
                TableName = this.TableName,
                IdentityColumnName = this.IdentityColumnName,
                Global = this.Global,
                UniqueColumnName = this.UniqueColumnName,
                Filter = Filter,
                MasterEntity = MasterEntity,
                SelectCondition = SelectCondition,
                Generation = this._generation,
                IdentityValueMapTableDeclared = this.IdentityValueMapTableDeclared
            };

            if (this.References != null)
            {
                newEntity.References = new Reference[this.References.Length];
                for (int i = 0; i < this.References.Length; i++)
                {
                    newEntity.References[i] = this.References[i].Clone();
                }
            }

            if (this.Restrictions != null)
            {
                newEntity.Restrictions = new Restriction[this.Restrictions.Length];
                for (int i = 0; i < this.Restrictions.Length; i++)
                {
                    newEntity.Restrictions[i] = this.Restrictions[i].Clone();
                }
            }

            return newEntity;
        }
    }

    [XmlRoot("Reference")]
    public class Reference
    {
        [XmlAttribute("OwnColumnName")]
        public string OwnColumnName { get; set; }

        [XmlAttribute("ReferredEntityName")]
        public string ReferredEntityName { get; set; }

        [XmlAttribute("ReferredColumnName")]
        public string ReferredColumnName { get; set; }

        [XmlIgnore()]
        public bool IsReferredIdentityColumn { get; set; }

        [XmlIgnore()]
        public bool Processed { get; set; }

        public Reference Clone()
        {
            Reference newReference = new Reference { 
                OwnColumnName = this.OwnColumnName,
                ReferredEntityName = this.ReferredEntityName,
                ReferredColumnName = this.ReferredColumnName
            };

            return newReference;
        }
    }

    [XmlRoot("Restriction")]
    public class Restriction
    {
        [XmlAttribute("ColumnName")]
        public string ColumnName { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }

        public Restriction Clone()
        {
            Restriction newRestriction = new Restriction
            {
                ColumnName = this.ColumnName,
                Value = this.Value,
            };

            return newRestriction;
        }
    }
}
