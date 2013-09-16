using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpifier
{
    public static class PropertyDefinitionExtensions
    {
        public static void Prepend(this List<PropertyDefinition> potentialPropertyDefinitions, PropertyDefinition potentialPropertyDefinition)
        {
            potentialPropertyDefinitions.InsertPotentialPropertyDefinition(0, potentialPropertyDefinition);
        }

        public static void Append(this List<PropertyDefinition> potentialPropertyDefinitions, PropertyDefinition potentialPropertyDefinition)
        {
            potentialPropertyDefinitions.InsertPotentialPropertyDefinition(potentialPropertyDefinitions.Count, potentialPropertyDefinition);
        }

        private static void InsertPotentialPropertyDefinition(this List<PropertyDefinition> potentialPropertyDefinitions, int index, PropertyDefinition potentialPropertyDefinition)
        {
            var matchingPropertyDefinition = potentialPropertyDefinitions.FirstOrDefault(x => Equals(x.Class, potentialPropertyDefinition.Class));
            if (matchingPropertyDefinition != null)
            {
                Merge(potentialPropertyDefinition, matchingPropertyDefinition);
                return;
            }

            potentialPropertyDefinitions.Insert(index, potentialPropertyDefinition);
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

            if (!source.IsEnabled)
            {
                destination.IsEnabled = false;
            }
        }

        public static void Append(this List<PropertyDefinition> potentialPropertyDefinitions, IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions)
            {
                potentialPropertyDefinitions.Append(otherPotentialPropertyDefinition);
            }
        }

        public static void Prepend(this List<PropertyDefinition> potentialPropertyDefinitions, IEnumerable<PropertyDefinition> otherPotentialPropertyDefinitions)
        {
            foreach (var otherPotentialPropertyDefinition in otherPotentialPropertyDefinitions.Reverse())
            {
                potentialPropertyDefinitions.Prepend(otherPotentialPropertyDefinition);
            }
        }

        public static void MergeWith(this Property propertyToMergeInto, Property propertyToMergeFrom)
        {
            propertyToMergeInto.HasHadNonEmptyValue |= propertyToMergeFrom.HasHadNonEmptyValue;
            propertyToMergeInto.DefaultPropertyDefinitionSet.PropertyDefinitions.MergeWith(propertyToMergeFrom.DefaultPropertyDefinitionSet.PropertyDefinitions);

            foreach (var extraPropertyDefinitionSet in propertyToMergeFrom.ExtraPropertyDefinitionSets)
            {
                propertyToMergeInto.AddOrUpdateExtraPropertyDefinitionSet(extraPropertyDefinitionSet);
            }

            foreach (var orphanedSet in propertyToMergeInto.ExtraPropertyDefinitionSets.Where(intoSet => propertyToMergeFrom.ExtraPropertyDefinitionSets.All(fromSet => intoSet.Name != fromSet.Name)))
            {
                orphanedSet.IsEnabled = false;
            }
        }

        public static void MergeWith(this List<PropertyDefinition> listToMergeInto, IEnumerable<PropertyDefinition> listToMergeFrom)
        {
            var thisList = listToMergeInto;
            var otherList = listToMergeFrom.ToList();

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
    }
}