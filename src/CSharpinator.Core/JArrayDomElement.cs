using System;
using System.Collections.Generic;
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
            _name = name;
            _factory = factory;
        }

        public bool HasElements
        {
            get { throw new NotImplementedException(); }
        }

        public string Value
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<IDomElement> Elements
        {
            get { throw new NotImplementedException(); }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            throw new NotImplementedException();
        }

        public DomPath GetDomPath(IFactory factory)
        {
            throw new NotImplementedException();
        }
    }
}