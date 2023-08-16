using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    public static class NullableExtensions
    {
        public static TResult? ShortCircuit<T, TResult>(this IEnumerable<T> source, Func<T, TResult?> function) 
            where TResult : struct
        {
            foreach (T element in source)
            {
                var result = function(element);
                if (result.HasValue)
                {
                    return result;
                }
            }

            return null;
        }


    }
}
