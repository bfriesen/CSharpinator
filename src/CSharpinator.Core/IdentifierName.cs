using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpinator
{
    [DebuggerDisplay("{Raw}")]
    public class IdentifierName
    {
        private static readonly PluralizationService _pluralizationService = PluralizationService.CreateService(new CultureInfo("en"));

        private readonly string _rawIdentifierName;
        private readonly IList<string> _words;

        private readonly Lazy<string> _pascalCase;
        private readonly Lazy<string> _pascalCasePlural;
        private readonly Lazy<string> _camelCase;
        private readonly Lazy<string> _camelCasePlural;
        private readonly Lazy<string> _snakeCase;
        private readonly Lazy<string> _snakeCasePlural;

        public IdentifierName(string rawIdentifierName)
        {
            _rawIdentifierName = rawIdentifierName;

            _words =
                Regex.Split(
                    Regex.Replace(
                        Regex.Replace(
                            rawIdentifierName.Replace(
                                ' ',
                                '_'),
                            "([A-Z])([A-Z]+)(?=$|[A-Z_0-9])",
                            match => match.Groups[1].Value + match.Groups[2].Value.ToLowerInvariant()),
                        @"([A-Z])",
                        match => "_" + match.Value.ToLowerInvariant()),
                    @"(?:_+(?=[a-zA-Z0-9]))|(?:^_$)")
                .Select(x => x.Trim().Trim('_'))
                .Where(x => !string.IsNullOrEmpty(x) && x.Length > 0)
                .ToList();

            _pascalCase = new Lazy<string>(
                () =>
                    _words
                        .Select((x, i) => i == _words.Count - 1 ? _pluralizationService.Singularize(x) : x)
                        .Aggregate(
                        "",
                        (acc, n) =>
                            acc
                            + char.ToUpper(n[0])
                            + (n.Length > 1 ? n.Substring(1) : "")));

            _pascalCasePlural = new Lazy<string>(
                () =>
                    _words
                        .Select((x, i) => i == _words.Count - 1 ? _pluralizationService.Pluralize(x) : x)
                        .Aggregate(
                        "",
                        (acc, n) =>
                        acc
                        + char.ToUpper(n[0])
                        + (n.Length > 1 ? n.Substring(1) : "")));

            _camelCase = new Lazy<string>(
                () =>
                _words.First()
                + _words.Skip(1)
                    .Select((x, i) => i == _words.Count - 2 ? _pluralizationService.Singularize(x) : x)
                    .Aggregate(
                    "",
                    (acc, n) =>
                    acc
                    + char.ToUpper(n[0])
                    + (n.Length > 1 ? n.Substring(1) : "")));

            _camelCasePlural = new Lazy<string>(
                () =>
                _words.First()
                + _words.Skip(1)
                    .Select((x, i) => i == _words.Count - 2 ? _pluralizationService.Pluralize(x) : x)
                    .Aggregate(
                    "",
                    (acc, n) =>
                    acc
                    + char.ToUpper(n[0])
                    + (n.Length > 1 ? n.Substring(1) : "")));

            _snakeCase = new Lazy<string>(
                () =>
                string.Join("_", _words.Select((x, i) => i == _words.Count - 1 ? _pluralizationService.Singularize(x) : x)));

            _snakeCasePlural = new Lazy<string>(
                () =>
                string.Join("_", _words.Select((x, i) => i == _words.Count - 1 ? _pluralizationService.Pluralize(x) : x)));
        }

        public string Raw
        {
            get { return _rawIdentifierName; }
        }

        public string PascalCase
        {
            get { return _pascalCase.Value; }
        }

        public string PascalCasePlural
        {
            get { return _pascalCasePlural.Value; }
        }

        // ReSharper disable InconsistentNaming
        public string camelCase
        {
            get { return _camelCase.Value; }
        }

        public string camelCasePlural
        {
            get { return _camelCasePlural.Value; }
        }

        public string snake_case
        {
            get { return _snakeCase.Value; }
        }

        public string snake_case_plural
        {
            get { return _snakeCasePlural.Value; }
        }
        // ReSharper restore InconsistentNaming

        public string FormatAs(Case propertyCase, bool isPlural = false)
        {
            switch (propertyCase)
            {
                case Case.PascalCase:
                    return isPlural ? PascalCasePlural : PascalCase;
                case Case.camelCase:
                    return isPlural ? camelCasePlural : camelCase;
                case Case.snake_case:
                    return isPlural ? snake_case_plural : snake_case;
                default:
                    throw new InvalidOperationException("Invalid value for Case: " + (int)propertyCase);
            }
        }
    }
}
