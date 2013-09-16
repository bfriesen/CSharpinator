using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Concurrent;

namespace CSharpifier
{
    [DebuggerDisplay("{Id}")]
    public class Property
    {
        private readonly Lazy<PropertyDefinitionSet> _defaultPropertyDefinitionSet;
        private Func<List<PropertyDefinition>> _createDefaultPropertyDefinitionSet;

        private readonly ConcurrentDictionary<string, PropertyDefinitionSet> _extraPropertyDefinitionSets = new ConcurrentDictionary<string, PropertyDefinitionSet>();

        public Property(string id, bool isNonEmpty)
        {
            Id = id;
            HasHadNonEmptyValue |= isNonEmpty;
            _defaultPropertyDefinitionSet = new Lazy<PropertyDefinitionSet>(CreateDefaultPropertyDefinitionSet);
        }

        public string Id { get; set; }
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
                n =>
                {
                    return new PropertyDefinitionSet
                    {
                        Name = n,
                        PropertyDefinitions = new List<PropertyDefinition>()
                    };
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
                        .SelectMany(x => x.PropertyDefinitions);
            }
        }

        public PropertyDefinition SelectedPropertyDefinition
        {
            get                               
            {
                return PotentialPropertyDefinitions.First(x =>
                {
                    if (!x.IsEnabled || !x.IsLegal)
                    {
                        return false;
                    }

                    var bclClass = x.Class as BclClass;
                    return bclClass == null || !bclClass.IsNullable || HasHadNonEmptyValue;
                });
            }
        }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return SelectedPropertyDefinition.GeneratePropertyCode(classCase, propertyCase);
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
