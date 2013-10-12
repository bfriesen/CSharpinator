using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CSharpinator
{
    public class Repository : IRepository
    {
        private DocumentType _documentType;
        private readonly ConcurrentDictionary<DomPath, UserDefinedClass> _classes = new ConcurrentDictionary<DomPath, UserDefinedClass>();
        private readonly HashSet<string> _dateTimeFormats = new HashSet<string>();
        private readonly HashSet<string> _references = new HashSet<string>();
        private readonly HashSet<string> _usings = new HashSet<string>();

        public Repository(IEnumerable<string> dateTimeFormats, string jsonRootClassElement)
        {
            foreach (var dateTimeFormat in dateTimeFormats)
            {
                _dateTimeFormats.Add(dateTimeFormat);
            }

            JsonRootElementName = jsonRootClassElement;
        }

        public IEnumerable<UserDefinedClass> GetAll()
        {
            return _classes.Values.OrderBy(x => x.Order);
        }

        public UserDefinedClass GetOrAdd(DomPath path)
        {
            return _classes.GetOrAdd(
                path,
                x => new UserDefinedClass(x));
        }

        public UserDefinedClass GetOrAdd(DomPath path, out bool isNew)
        {
            var isNewClass = false;
            var @class = _classes.GetOrAdd(
                path,
                x =>
                {
                    isNewClass = true;
                    return new UserDefinedClass(x);
                });
            isNew = isNewClass;
            return @class;
        }

        public void LoadFromMetadata(Metadata metadata, IFactory factory)
        {
            foreach (var classProxy in metadata.Classes)
            {
                var domPath = classProxy.GetDomPath(factory);
                var userDefinedClass = GetOrAdd(domPath);
                userDefinedClass.CustomName = classProxy.CustomName;

                foreach (var propertyProxy in classProxy.Properties)
                {
                    // We're sending 'true' for the isParentClassNew parameter, since we don't want to mark anything as nullable.
                    userDefinedClass.AddProperty(propertyProxy.ToProperty(this, factory), true, true);
                }
            }

            _documentType = metadata.DocumentType;

            if (metadata.References != null)
            {
                foreach (var reference in metadata.References)
                {
                    _references.Add(reference);
                }
            }
            
            if (metadata.Usings != null)
            {
                foreach (var @using in metadata.Usings)
                {
                    _usings.Add(@using);
                }
            }

            if (metadata.DateTimeFormats != null)
            {
                foreach (var dateTimeFormat in metadata.DateTimeFormats)
                {
                    _dateTimeFormats.Add(dateTimeFormat);
                }
            }
        }

        public Metadata CreateMetadata()
        {
            var metadata = new Metadata
            {
                DocumentType = _documentType,
                Classes = GetAll().Select(UserDefinedClassProxy.FromUserDefinedClass).ToList(),
                DateTimeFormats = _dateTimeFormats,
                References = _references,
                Usings = _usings
            };

            return metadata;
        }

        public DocumentType DocumentType { get { return _documentType; } }

        public IEnumerable<string> References { get { return _references; } }

        public IEnumerable<string> Usings { get { return _usings; } }

        public IEnumerable<string> DateTimeFormats { get { return _dateTimeFormats; } }

        public string JsonRootElementName { get; set; }

        public void SetDocumentType(DocumentType documentType)
        {
            _documentType = documentType;
        }
    }
}
