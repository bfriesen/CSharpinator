using System;
using System.Linq;

namespace CSharpinator
{
    public class DomPath
    {
        private readonly string _fullPath;
        private readonly int _typeNameDepth;

        private readonly IdentifierName _typeName;
        private readonly IdentifierName _elementName;
        private readonly int _hashCode;

        public DomPath(string fullPath, int typeNameDepth)
        {
            if (fullPath == null)
            {
                throw new NullReferenceException("fullPath");
            }

            _fullPath = fullPath;
            _typeNameDepth = typeNameDepth;

            var pathElements = fullPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();

            _typeName = new IdentifierName(string.Join("_", pathElements.Take(typeNameDepth + 1).Reverse()));
            _elementName = new IdentifierName(pathElements.First());

            unchecked
            {
                _hashCode = (fullPath.GetHashCode() * 397) ^ typeNameDepth;
            }
        }

        public IdentifierName TypeName { get { return _typeName; } }
        public IdentifierName ElementName { get {  return _elementName; } }
        public string FullPath { get { return _fullPath; } }
        public int TypeNameDepth { get { return _typeNameDepth; } }

        public override bool Equals(object other)
        {
            var otherDomPath = other as DomPath;
            if (otherDomPath == null)
            {
                return false;
            }

            return Equals(otherDomPath);
        }

        protected bool Equals(DomPath other)
        {
            return _hashCode == other._hashCode;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _hashCode;
            }
        }
    }
}