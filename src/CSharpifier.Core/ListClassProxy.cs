using System.Xml.Serialization;

namespace CSharpifier
{
    [XmlRoot("ListClass")]
    public class ListClassProxy : ClassProxy
    {
        public ClassProxy Class { get; set; }

        public static ListClassProxy FromListClass(ListClass listClass)
        {
            return new ListClassProxy
            {
                Class = FromClass(listClass.Class)
            };
        }

        public static ListClass ToListClass(ListClassProxy listClassProxy, IClassRepository classRepository)
        {
            return ListClass.FromClass(listClassProxy.Class.ToClass(classRepository));
        }
    }
}
