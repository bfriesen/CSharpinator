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

        public void Visit(IDomElement element, bool metaExists)
        {
            bool isNew;
            _classRepository.GetOrAdd(element.Name, out isNew);
            Visit(element, null, isNew, true, metaExists);
        }

        private void Visit(IDomElement element, UserDefinedClass currentClass, bool isNew, bool isRoot, bool metaExists)
        {
            if (element.HasElements) // if element has child elements
            {
                if (!isRoot) // if this is not the root element
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property, isNew, metaExists);
                }

                currentClass = _classRepository.GetOrAdd(element.Name, out isNew);

                foreach (var childElement in element.Elements)
                {
                    Visit(childElement, currentClass, isNew, false, metaExists);
                }

                foreach (var orphanedProperty in
                        currentClass.Properties.Where(
                            property => metaExists && element.Elements.All(childElement => property.Id != childElement.Name)))
                {
                    // If there's a property that exists, but isn't present in our element's children, it should be nullable.
                    orphanedProperty.MakeNullable();
                }
            }
            else // if element has no child elements
            {
                if (isRoot) // if this is the root element
                {
                    // Make sure a class exists for the root element, no matter what.
                    _classRepository.GetOrAdd(element.Name);
                }
                else
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property, isNew, metaExists);
                }
                
                if (metaExists)
                {
                    // If we're refining, and this element has no children, there is a
                    // possibility that it had previous contained children. Make those
                    // children nullable.
                    currentClass = _classRepository.GetOrAdd(element.Name);
                    foreach (var property in currentClass.Properties)
                    {
                        property.MakeNullable();
                    }
                }
            }
        }
    }
}
