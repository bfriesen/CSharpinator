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
            _name = name;
            _factory = factory;
        }

        public bool HasElements
        {
            get { return _jArray.All(x => x.Type == JTokenType.Object); }
        }

        public IEnumerable<IDomElement> Elements
        {
            get
            {
                return 
                    HasElements
                        ? _jArray.Select(item => _factory.CreateJsonDomElement(item, _name))
                        : Enumerable.Empty<IDomElement>();
            }
        }

        public bool ActsAsRootElement 
        {
            get { return true; }
        }

        public Property CreateProperty(IClassRepository classRepository)
        {
            var property = _factory.CreateProperty(_jArray.GetDomPath(_factory), _jArray.Count > 0);

            var bestFitType = GetBestFitType();

            // If JTokenType.None, then there weren't any elements.
            // If JTokenType.Null, then no JTokenType is able to contain the values of all the other elements.
            // Else, we have a JTokenType that can contain all the values of the other elements.

            // The 'else' condition is the happy path. We'll want to create a property custom tailored for
            // bestFitType. When (or if) it gets merged, the merge mechanism will ensure that we end up with
            // the best type for the property. This should be relatively easy to implement.

            // As for the sad paths, which won't be as easy to implement...

            // There's a strong possibility that we'll need to be able to determine whether a property should
            // even be added to its parent class. If there were no elements in the json array, should we
            // do anything with that array? I don't think so. And what about if no single type exists that
            // could hold all the elements? Should we do anything with *that* array? Or throw an exception?
            // Questions to ponder...

            switch (bestFitType)
            {
                case JTokenType.Null:
                case JTokenType.None:
                {
                    throw new NotImplementedException();
                }
                case JTokenType.Object:
                {
                    property.InitializeDefaultPropertyDefinitionSet(
                        propertyDefinitions =>
                        propertyDefinitions.Append(
                            _factory.CreatePropertyDefinition(
                                ListClass.FromClass(classRepository.GetOrAdd(_jArray.GetDomPath(_factory))),
                                _name,
                                true,
                                true,
                                AttributeProxy.DataMember(_name))));
                    break;
                }
                case JTokenType.Array:
                {
                    throw new NotImplementedException();
                }
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                {
                    property.InitializeDefaultPropertyDefinitionSet(propertyDefinitions => {});

                    foreach (var item in _jArray.OfType<JValue>())
                    {
                        var mergeProperty = _factory.CreateProperty(_jArray.GetDomPath(_factory), _jArray.Count > 0);

                        mergeProperty.InitializeDefaultPropertyDefinitionSet(
                            propertyDefinitions =>
                            propertyDefinitions.Append(
                                _factory.GetAllBclClasses()
                                        .Select(
                                            bclClass =>
                                            _factory.CreatePropertyDefinition(
                                                ListClass.FromClass(bclClass),
                                                _name,
                                                bclClass.IsLegalObjectValue(item.Value),
                                                true,
                                                AttributeProxy.DataMember(_name)))));

                        property.MergeWith(mergeProperty);
                    }
                    break;
                }
                default:
                {
                    throw new InvalidOperationException("Unsupported item in json array: " + bestFitType);
                }
            }

            return property;
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