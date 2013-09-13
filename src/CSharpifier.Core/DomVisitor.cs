using System.Linq;

namespace CSharpifier
{
    public class DomVisitor
    {
        private readonly IClassRepository _classRepository;

        public DomVisitor(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public bool ExcludeNamespace { get; set; }

        public void Visit(IDomElement element)
        {
            bool isNew;
            _classRepository.GetOrCreate(element.Name, out isNew);
            Visit(element, null, isNew, true);
        }

        private void Visit(IDomElement element, UserDefinedClass currentClass, bool isNew, bool isRoot)
        {
            if (element.HasElements) // if element has child elements
            {
                if (!isRoot) // if this is not the root element
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property, isNew);
                }

                currentClass = _classRepository.GetOrCreate(element.Name, out isNew);

                foreach (var childElement in element.Elements)
                {
                    Visit(childElement, currentClass, isNew, false);
                }

                foreach (var orphanedProperty in
                        currentClass.Properties.Where(
                            property => element.Elements.All(childElement => property.Id != childElement.Name)))
                {
                    // If there's a property that exists, but isn't present in our element's children, it should be nullable.
                    orphanedProperty.MakeNullable();
                }
            }
            else // if element has no child elements
            {
                if (isRoot) // if this is the root element
                {
                    _classRepository.GetOrCreate(element.Name);
                }
                else
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property, isNew);
                }
            }
        }
    }
}
