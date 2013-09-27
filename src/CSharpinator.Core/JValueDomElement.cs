using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CSharpinator
{
    public class JValueDomElement : IDomElement
    {
        private readonly JValue _jValue;
        private readonly string _name;
        private readonly IFactory _factory;

        public JValueDomElement(JValue jValue, string name, IFactory factory)
        {
            _jValue = jValue;
            _name = name;
            _factory = factory;
        }

        public bool HasElements
        {
            get { throw new NotImplementedException(); }
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