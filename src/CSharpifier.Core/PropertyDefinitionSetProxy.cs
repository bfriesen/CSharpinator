﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSharpifier
{
    public class PropertyDefinitionSetProxy
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsEnabled { get; set; }

        [XmlArray]
        [XmlArrayItem("PropertyDefinition")]
        public List<PropertyDefinitionProxy> PropertyDefinitions { get; set; }

        public static PropertyDefinitionSetProxy FromPropertyDefinitionSet(PropertyDefinitionSet propertyDefinitionSet)
        {
            return new PropertyDefinitionSetProxy
            {
                Name = propertyDefinitionSet.Name,
                Order = propertyDefinitionSet.Order,
                IsEnabled = propertyDefinitionSet.IsEnabled,
                PropertyDefinitions = propertyDefinitionSet.PropertyDefinitions.Select(PropertyDefinitionProxy.FromPropertyDefinition).ToList()
            };
        }

        public PropertyDefinitionSet ToPropertyDefinitionSet(IClassRepository classRepository)
        {
            return new PropertyDefinitionSet
            {
                Name = Name,
                Order = Order,
                IsEnabled = IsEnabled,
                PropertyDefinitions = PropertyDefinitions.Select(x => x.ToPropertyDefinition(classRepository)).ToList()
            };
        }
    }
}
