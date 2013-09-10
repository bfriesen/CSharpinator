﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpifier
{
    public class XmlDomText : IDomElement
    {
        private readonly string _value;

        public XmlDomText(string value)
        {
            _value = value;
        }

        public bool HasElements
        {
            get { return false; }
        }

        public string Value
        {
            get { return _value; }
        }

        public string Name
        {
            get { return "Value"; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                yield break;
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = new Property(XName.Get(Name));

            property.AppendPotentialPropertyDefinitions(
                BclClass.GetLegalClassesFromValue(_value)
                    .Select(bclClass =>
                        new PropertyDefinition(bclClass, Name)
                        {
                            Attributes = new List<AttributeProxy> { AttributeProxy.XmlText() }
                        }));

            return property;
        }
    }
}