using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public interface IClassRepository
    {
        IEnumerable<UserDefinedClass> GetAll();
        UserDefinedClass GetOrCreate(string typeName);
    }
}
