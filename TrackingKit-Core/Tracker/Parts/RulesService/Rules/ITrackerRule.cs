using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking.Rules
{
    public interface ITrackerRule<TKey>
        where TKey : IEquatable<TKey>
    {

        bool? ShouldAdd(TKey propertyName, object obj);

        bool? ShouldDelete(ITimeUnit timeUnit, int version, object obj, IReadOnlyCollection<string> tags) => null;


    }
}
