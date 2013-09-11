using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpifier
{
    public class IdentifierName
    {
        private readonly string _rawIdentifierName;
        private readonly IList<string> _words;

        private readonly Lazy<string> _pascalCase;
        private readonly Lazy<string> _camelCase;
        private readonly Lazy<string> _snakeCase;

        public IdentifierName(string rawIdentifierName)
        {
            _rawIdentifierName = rawIdentifierName;

            _words =
                Regex.Split(
                    Regex.Replace(
                        rawIdentifierName.Replace(' ', '_'),
                        @"([A-Z])",
                        match => "_" + match.Value.ToLowerInvariant()),
                    @"(?:_(?=[a-zA-Z]))|(?:^_$)")
                .Select(x => x.Trim().Trim('_'))
                .Where(x => !string.IsNullOrEmpty(x) && x.Length > 0)
                .ToList();

            _pascalCase = new Lazy<string>(
                () =>
                _words.Aggregate(
                    "",
                    (acc, n) =>
                        acc
                        + char.ToUpper(n[0]).ToString()
                        + (n.Length > 1 ? n.Substring(1) : "")));

            _camelCase = new Lazy<string>(
                () =>
                _words.First()
                + _words.Skip(1).Aggregate(
                    "",
                    (acc, n) =>
                    acc
                    + char.ToUpper(n[0]).ToString()
                    + (n.Length > 1 ? n.Substring(1) : "")));

            _snakeCase = new Lazy<string>(
                () =>
                string.Join("_", _words));
        }

        public string Raw
        {
            get { return _rawIdentifierName; }
        }

        public string PascalCase
        {
            get { return _pascalCase.Value; }
        }

        public string camelCase
        {
            get { return _camelCase.Value; }
        }

        public string snake_case
        {
            get { return _snakeCase.Value; }
        }

        public string FormatAs(Case propertyCase)
        {
            switch (propertyCase)
            {
                case Case.PascalCase:
                    return PascalCase;
                case Case.camelCase:
                    return camelCase;
                case Case.snake_case:
                    return snake_case;
                default:
                    throw new InvalidOperationException("Invalid value for Case: " + (int)propertyCase);
            }
        }
    }
}
