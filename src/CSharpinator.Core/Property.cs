using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Concurrent;

namespace CSharpinator
{
    [DebuggerDisplay("{DomPath.FullPath}")]
    public class Property
    {
        private readonly Lazy<PropertyDefinitionSet> _defaultPropertyDefinitionSet;
        private Func<List<PropertyDefinition>> _createDefaultPropertyDefinitionSet;

        private readonly ConcurrentDictionary<string, PropertyDefinitionSet> _extraPropertyDefinitionSets = new ConcurrentDictionary<string, PropertyDefinitionSet>();

        private readonly DomPath _domPath;
        private readonly IFactory _factory;

        public Property(DomPath domPath, bool isNonEmpty, IFactory factory)
        {
            HasHadNonEmptyValue |= isNonEmpty;
            _defaultPropertyDefinitionSet = new Lazy<PropertyDefinitionSet>(CreateDefaultPropertyDefinitionSet);
            _domPath = domPath;
            _factory = factory;
        }

        public DomPath DomPath { get { return _domPath; } }
        public bool HasHadNonEmptyValue { get; set; }

        public PropertyDefinitionSet DefaultPropertyDefinitionSet
        {
            get { return _defaultPropertyDefinitionSet.Value; }
        }

        public IEnumerable<PropertyDefinitionSet> ExtraPropertyDefinitionSets
        {
            get { return _extraPropertyDefinitionSets.Values; }
        }

        public bool ExtraPropertyDefinitionSetExists(string name)
        {
            return _extraPropertyDefinitionSets.ContainsKey(name);
        }

        public PropertyDefinitionSet GetOrAddExtraPropertyDefinitionSet(string name)
        {
            return _extraPropertyDefinitionSets.GetOrAdd(
                name,
                n => new PropertyDefinitionSet
                {
                    Name = n,
                    PropertyDefinitions = new List<PropertyDefinition>()
                });
        }

        public PropertyDefinitionSet AddOrUpdateExtraPropertyDefinitionSet(PropertyDefinitionSet set)
        {
            return _extraPropertyDefinitionSets.AddOrUpdate(
                set.Name,
                n => set,
                (n, s) =>
                {
                    s.PropertyDefinitions.MergeWith(set.PropertyDefinitions);

                    if (set.Order < s.Order)
                    {
                        s.Order = set.Order;
                    }

                    return s;
                });
        }

        public IEnumerable<PropertyDefinition> PotentialPropertyDefinitions
        {
            get
            {
                return
                    _extraPropertyDefinitionSets.Values.Concat(new[] { DefaultPropertyDefinitionSet })
                        .OrderBy(x => x.Order)
                        .Where(x => x.IsEnabled)
                        .SelectMany(x => x.PropertyDefinitions);
            }
        }

        public PropertyDefinition SelectedPropertyDefinition
        {
            get                               
            {
                var selectedPropertyDefinition = PotentialPropertyDefinitions.FirstOrDefault(x =>
                {
                    if (!x.IsEnabled || !x.IsLegal)
                    {
                        return false;
                    }

                    var bclClass = x.Class as IBclClass;
                    return bclClass == null || !bclClass.IsNullable || HasHadNonEmptyValue || bclClass.TypeAlias == "string";
                });

                return selectedPropertyDefinition;
            }
        }

        public string GeneratePropertyCode(Case classCase, Case propertyCase, DocumentType documentType)
        {
            return SelectedPropertyDefinition.GeneratePropertyCode(classCase, propertyCase, documentType);
        }

        public void MakeNullable()
        {
            foreach (var potentialPropertyDefinition in PotentialPropertyDefinitions)
            {
                var bclClass = potentialPropertyDefinition.Class.AsBclClass();
                if (bclClass != null && !bclClass.IsNullable)
                {
                    potentialPropertyDefinition.IsEnabled = false;
                }
            }
        }

        public void InitializeDefaultPropertyDefinitionSet(Action<List<PropertyDefinition>> initializeAction)
        {
            var propertyDefinitions = new List<PropertyDefinition>();
            _createDefaultPropertyDefinitionSet = () =>
            {
                initializeAction(propertyDefinitions);
                return propertyDefinitions;
            };
        }

        private PropertyDefinitionSet CreateDefaultPropertyDefinitionSet()
        {
            if (_createDefaultPropertyDefinitionSet == null)
            {
                throw new InvalidOperationException("The property's InitializeDefaultPropertyDefinitionSet must be called before accessing its DefaultPropertyDefinitionSet property.");
            }

            return new PropertyDefinitionSet { PropertyDefinitions = _createDefaultPropertyDefinitionSet(), Name = "__default", IsEnabled = true };
        }
    }
}
