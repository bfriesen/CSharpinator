﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpifier
{
    public class Property
    {
        private readonly List<PropertyDefinition> _potentialPropertyDefinitions = new List<PropertyDefinition>();

        public Property(XName propertyName)
        {
            Name = new IdentifierName(propertyName.ToString());
        }

        public IdentifierName Name { get; set; }

        public IEnumerable<PropertyDefinition> PotentialPropertyDefinitions
        {
            get { return _potentialPropertyDefinitions; }
        }

        public void PrependPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
        {
            if (_potentialPropertyDefinitions.Any(x => x.Class == potentialPropertyDefinition.Class))
            {
                return;
            }

            InsertPotentialPropertyDefinition(0, potentialPropertyDefinition);
        }

        public void AppendPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
        {
            InsertPotentialPropertyDefinition(_potentialPropertyDefinitions.Count, potentialPropertyDefinition);
        }

        private void InsertPotentialPropertyDefinition(int index, PropertyDefinition potentialPropertyDefinition)
        {
            if (_potentialPropertyDefinitions.Any(x => Equals(x.Class, potentialPropertyDefinition.Class)))
            {
                return;
            }

            _potentialPropertyDefinitions.Insert(index, potentialPropertyDefinition);
        }

        public void AppendPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions)
            {
                AppendPotentialPropertyDefinition(otherPotentialPropertyDefinition);
            }
        }

        public void PrependPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions.Reverse())
            {
                PrependPotentialPropertyDefinition(otherPotentialPropertyDefinition);
            }
        }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return PotentialPropertyDefinitions.First().GeneratePropertyCode(classCase, propertyCase);
        }
    }
}
