using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public class DomVisitor
    {
        private IClassRepository _classRepository;

        public DomVisitor(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public bool ExcludeNamespace { get; set; }

        public void Visit(IDomElement element)
        {
            Visit(element, null);
        }

        private void Visit(IDomElement element, UserDefinedClass currentClass)
        {
            if (element.HasElements) // if element has child elements
            {
                if (currentClass != null) // if this is the root element
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property);
                }

                currentClass = _classRepository.GetOrCreate(element.Name);

                foreach (var childElement in element.Elements)
                {
                    Visit(childElement, currentClass);
                }
            }
            else // if element has no child elements
            {
                if (currentClass == null) // if this is the root element
                {
                    currentClass = _classRepository.GetOrCreate(element.Name);
                }
                else // if this is not the root element
                {
                    var property = element.CreateProperty(_classRepository);
                    currentClass.AddProperty(property);
                }
            }
        }
    }
}
