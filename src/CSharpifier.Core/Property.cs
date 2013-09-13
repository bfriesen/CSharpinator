using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CSharpifier
{
    [DebuggerDisplay("{Name.Raw}")]
    public class Property
    {
        private readonly List<PropertyDefinition> _potentialPropertyDefinitions = new List<PropertyDefinition>();

        public Property(string id, bool isNonEmpty)
        {
            Id = id;
            HasHadNonEmptyValue |= isNonEmpty;
        }

        public string Id { get; set; }
        public bool HasHadNonEmptyValue { get; set; }

        public IEnumerable<PropertyDefinition> PotentialPropertyDefinitions
        {
            get { return _potentialPropertyDefinitions; }
        }

        public PropertyDefinition SelectedPropertyDefinition
        {
            get
            {
                return _potentialPropertyDefinitions.First(x =>
                {
                    if (!x.IsEnabled || !x.IsLegal)
                    {
                        return false;
                    }

                    var bclClass = x.Class as BclClass;
                    return bclClass == null || !bclClass.IsNullable || HasHadNonEmptyValue;
                });
            }
        }

        public void PrependPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
        {
            InsertPotentialPropertyDefinition(0, potentialPropertyDefinition);
        }

        public void AppendPotentialPropertyDefinition(PropertyDefinition potentialPropertyDefinition)
        {
            InsertPotentialPropertyDefinition(_potentialPropertyDefinitions.Count, potentialPropertyDefinition);
        }

        private void InsertPotentialPropertyDefinition(int index, PropertyDefinition potentialPropertyDefinition)
        {
            var matchingPropertyDefinition = _potentialPropertyDefinitions.FirstOrDefault(x => Equals(x.Class, potentialPropertyDefinition.Class));
            if (matchingPropertyDefinition != null)
            {
                Merge(potentialPropertyDefinition, matchingPropertyDefinition);
                return;
            }

            _potentialPropertyDefinitions.Insert(index, potentialPropertyDefinition);
        }

        private static void Merge(PropertyDefinition source, PropertyDefinition destination)
        {
            // If the the new property definition is illegal, its match should be illegal.
            // But not the other way around - if the new one is legal, and its match is
            // illegal, *don't* make the match legal. Note that we never want to change
            // IsEnabled - it's set purely by the user.
            if (!source.IsLegal)
            {
                destination.IsLegal = false;
            }
        }

        public void AppendPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions)
            {
                AppendPotentialPropertyDefinition(otherPotentialPropertyDefinition);
            }
        }

        public void PrependPotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions.Reverse())
            {
                PrependPotentialPropertyDefinition(otherPotentialPropertyDefinition);
            }
        }

        public void MergePotentialPropertyDefinitions(IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            var thisList = _potentialPropertyDefinitions;
            var otherList = otherPotentialPropertyDefinitions.ToList();

            var matchingIndexes = new List<Tuple<int, int>>();

            for (var i = 0; i < thisList.Count; i++)
            {
                for (var j = 0; j < otherList.Count; j++)
                {
                    if (Equals(thisList[i].Class, otherList[j].Class))
                    {
                        matchingIndexes.Add(Tuple.Create(i, j));
                        Merge(otherList[j], thisList[i]);
                    }
                }
            }

            if (matchingIndexes.Count == 0)
            {
                thisList.AddRange(otherList);
                return;
            }

            for (var i = matchingIndexes.Count - 1; i >= 0; i--)
            {
                var upperBound =
                    i == matchingIndexes.Count - 1
                        ? otherList.Count
                        : matchingIndexes[i + 1].Item2;

                int insertPoint;
                if (i == matchingIndexes.Count - 1)
                {
                    insertPoint =
                        matchingIndexes[i].Item1 == thisList.Count - 1
                            ? thisList.Count
                            : matchingIndexes[i].Item1;
                }
                else
                {
                    insertPoint = matchingIndexes[i + 1].Item1;
                }

                for (var j = upperBound - 1; j > matchingIndexes[i].Item2; j--)
                {
                    if (matchingIndexes.All(x => x.Item2 != j))
                    {
                        thisList.Insert(insertPoint, otherList[j]);
                    }
                }
            }

            if (matchingIndexes[0].Item2 != 0)
            {
                for (var j = matchingIndexes[0].Item2 - 1; j >= 0; j--)
                {
                    if (matchingIndexes.All(x => x.Item2 != j))
                    {
                        thisList.Insert(matchingIndexes[0].Item1, otherList[j]);
                    }
                }
            }
        }

        public string GeneratePropertyCode(Case classCase, Case propertyCase)
        {
            return SelectedPropertyDefinition.GeneratePropertyCode(classCase, propertyCase);
        }
    }
}
