using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpifier
{
    [DebuggerDisplay("{Id}")]
    public class Property
    {
        private readonly Lazy<List<PropertyDefinition>> _potentialPropertyDefinitions;
        private Func<List<PropertyDefinition>> _createPotentialPropertyDefinitions;

        public Property(string id, bool isNonEmpty)
        {
            Id = id;
            HasHadNonEmptyValue |= isNonEmpty;
            _potentialPropertyDefinitions = new Lazy<List<PropertyDefinition>>(CreatePotentialPropertyDefinitions);
        }

        public string Id { get; set; }
        public bool HasHadNonEmptyValue { get; set; }

        public List<PropertyDefinition> PotentialPropertyDefinitions
        {
            get { return _potentialPropertyDefinitions.Value; }
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
                var bclClass = potentialPropertyDefinition.Class as BclClass;
                if (bclClass != null && !bclClass.IsNullable)
                {
                    potentialPropertyDefinition.IsEnabled = false;
                }
            }
        }

        public void InitializePotentialPropertyDefinitions(Action<List<PropertyDefinition>> initializeAction)
        {
            var potentialPropertyDefinitions = new List<PropertyDefinition>();
            _createPotentialPropertyDefinitions = () =>
            {
                initializeAction(potentialPropertyDefinitions);
                return potentialPropertyDefinitions;
            };
        }

        private List<PropertyDefinition> CreatePotentialPropertyDefinitions()
        {
            if (_createPotentialPropertyDefinitions == null)
            {
                throw new InvalidOperationException("The property's InitializePotentialPropertyDefinitions must be called before accessing its PotentialPropertyDefinition property.");
            }

            return _createPotentialPropertyDefinitions();
        }
    }
}
