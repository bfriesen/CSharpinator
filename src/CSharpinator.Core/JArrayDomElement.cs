using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public class JArrayDomElement : IDomElement
    {
        private readonly JArray _jArray;
        private readonly string _name;
        private readonly IFactory _factory;

        public JArrayDomElement(JArray jArray, string name, IFactory factory)
        {
            _jArray = jArray;
            _name = name; // TODO: pluralize name?
            _factory = factory;
        }

        public bool HasElements
        {
            get { return true; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return _jArray.Select(item => _factory.CreateJsonDomElement(item, _name)); // TODO: singularize _name?
            }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            throw new NotImplementedException();
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _jArray.GetDomPath(factory);
        }
    }
}