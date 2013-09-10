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
        void AddOrUpdate(UserDefinedClass @class);
        UserDefinedClass GetOrCreate(string typeName);
    }
}
