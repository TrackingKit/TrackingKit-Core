using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tracking
{
    public static class DictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, IReadOnlyDictionary<TInnerKey, TInnerValue>> ToReadOnlyNestedDictionary<TKey, TInnerKey, TInnerValue>(
            this SortedDictionary<TKey, SortedDictionary<TInnerKey, TInnerValue>> data)
            where TKey : notnull
            where TInnerKey : notnull
        {
            var result = data.ToDictionary(
                outerKeyValuePair => outerKeyValuePair.Key,
                outerKeyValuePair => (IReadOnlyDictionary<TInnerKey, TInnerValue>)
                    new ReadOnlyDictionary<TInnerKey, TInnerValue>(outerKeyValuePair.Value));

            return new ReadOnlyDictionary<TKey, IReadOnlyDictionary<TInnerKey, TInnerValue>>(result);
        }

        public static IReadOnlyDictionary<TKey, IReadOnlyDictionary<TInnerKey, IReadOnlyDictionary<TInnerValue, TInnerMostValue>>>
           ToReadOnlyNestedDictionary<TKey, TInnerKey, TInnerValue, TInnerMostValue>(
           this Dictionary<TKey, SortedDictionary<TInnerKey, SortedDictionary<TInnerValue, TInnerMostValue>>> data)
           where TKey : notnull
           where TInnerKey : notnull
           where TInnerValue : notnull
        {
            var result = data.ToDictionary(
                outerKeyValuePair => outerKeyValuePair.Key,
                outerKeyValuePair => (IReadOnlyDictionary<TInnerKey, IReadOnlyDictionary<TInnerValue, TInnerMostValue>>)
                    outerKeyValuePair.Value.ToDictionary(
                        innerKeyValuePair => innerKeyValuePair.Key,
                        innerKeyValuePair => (IReadOnlyDictionary<TInnerValue, TInnerMostValue>)
                            new ReadOnlyDictionary<TInnerValue, TInnerMostValue>(innerKeyValuePair.Value)
                    )
            );

            return new ReadOnlyDictionary<TKey, IReadOnlyDictionary<TInnerKey, IReadOnlyDictionary<TInnerValue, TInnerMostValue>>>(result);
        }

    }



}
