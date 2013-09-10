﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpifier
{
    public class ListClass : Class
    {
        private static readonly ConcurrentDictionary<Class, ListClass> _classes
            = new ConcurrentDictionary<Class, ListClass>();
        private readonly Class _class;

        private ListClass(Class @class)
        {
            _class = @class;
        }

        public static ListClass FromClass(Class @class)
        {
            return _classes.GetOrAdd(@class, x => new ListClass(x));
        }

        public Class Class
        {
            get { return _class; }
        }

        public override string GeneratePropertyCode(string propertyName, Case classCase)
        {
            var typeName =
                _class is UserDefinedClass
                    ? ((UserDefinedClass)_class).TypeName.FormatAs(classCase)
                    : ((BclClass)_class).TypeName;
            return string.Format("public List<{0}> {1} {{ get; set; }}", typeName, propertyName);
        }

        public override bool Equals(object other)
        {
            var otherListClass = other as ListClass;
            if (otherListClass == null)
            {
                return false;
            }

            return Equals(_class, otherListClass._class);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = "ListClass".GetHashCode();
                result = (result * 397) ^ (_class.GetHashCode());
                return result;
            }
        }
    }

}
