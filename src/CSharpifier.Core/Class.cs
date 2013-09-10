using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public abstract class Class
    {
        public abstract string GeneratePropertyCode(string propertyName, Case classCase);
    }
}
