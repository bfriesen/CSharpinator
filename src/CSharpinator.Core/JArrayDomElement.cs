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
            get { return false; }
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
            var bestFitType = GetBestFitType();

            // If JTokenType.None, then there weren't any elements.
            // If JTokenType.Null, then no JTokenType is able to contain the values of all the other elements.
            // Else, we have a JTokenType that can contain all the values of the other elements.

            throw new NotImplementedException();
        }

        public DomPath GetDomPath(IFactory factory)
        {
            return _jArray.GetDomPath(factory);
        }

        private JTokenType GetBestFitType()
        {
            if (_jArray.Count == 0)
            {
                return JTokenType.None;
            }

            var bestFitType =
                _jArray
                    .Select(candidate => new[] { candidate }.Concat(_jArray.Where(test => candidate != test)).ToList()).ToList()
                    .Where(list => list.All(test => CanContain(list.First().Type, test.Type)))
                    .SelectMany(x => x)
                    .Select(x => (JTokenType?)x.Type)
                    .FirstOrDefault();

            return bestFitType.HasValue ? bestFitType.Value : JTokenType.Null;
        }

        private static bool CanContain(JTokenType candidate, JTokenType test)
        {
            switch (candidate)
            {
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Undefined:
                case JTokenType.Null:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                    return false;
                case JTokenType.Object:
                case JTokenType.Array:
                case JTokenType.Integer:
                case JTokenType.Boolean:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.Date:
                    return test == JTokenType.Null || test == candidate;
                case JTokenType.Float:
                    return test == JTokenType.Null || test == JTokenType.Float || test == JTokenType.Integer;
                case JTokenType.String:
                    // TODO: Fix this - it is probably slightly wrong.
                    return test == JTokenType.Null || test == JTokenType.String || test == JTokenType.Date || test == JTokenType.Guid || test == JTokenType.TimeSpan;
                default:
                    throw new ArgumentOutOfRangeException("candidate");
            }
        }
    }
}