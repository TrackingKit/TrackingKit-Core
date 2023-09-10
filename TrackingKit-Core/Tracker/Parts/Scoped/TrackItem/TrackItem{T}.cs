using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking;

namespace TrackingKit_Core
{
    /*
    public class Example
    {
        [TrackItem] public int Hi { get; set; }

        [TrackItem] public string Hello { get; set; }
    }

    public enum ScopedGeneratedEnumExample
    {
        Hi,
        Hello
    }

    public class ScopedGeneratedExample : BaseGeneratedTrackScope<ScopedGeneratedEnumExample>
    {
        public TrackItem<int> Hi => new TrackItem<int>(settings, storage, ScopedGeneratedEnumExample.Hi);

        public TrackItem<string> Hello => new TrackItem<int>(settings, storage, ScopedGeneratedEnumExample.Hello);
    }

    public class BaseGeneratedTrackScope<TKey>
    {
        protected ITrackerStorage<TKey> Storage;

        protected ScopedSettings Settings;

        public BaseGeneratedTrackScope(ScopedSettings settings, ITrackerStorage<TKey> storage, TKey key)
        {
            Settings = settings;
            Storage = storage;
        }

    }
    */

    public class TrackSpecificItem<TKey, TOutputPropertyValue> : BaseTrackItem<TKey>
        where TKey : IEquatable<TKey>
    {
        public TrackSpecificItem(ScopedSettings settings, ITrackerStorage<TKey> storage, TKey key) : base(settings, storage, key)
        {

        }

        public TOutputPropertyValue Get()
        {
            Storage.TryGetValue<TKey>()

            return default;
        }

        public TOutputPropertyValue GetOrDefault()
        {
            return default;
        }

        public IEnumerable<(TKey Key, TOutputPropertyValue Data)> GetDetailed()
        {
            return default;
        }

        public IEnumerable<(TKey Key, TOutputPropertyValue Data)> GetDetailedOrDefault()
        {
            return default;
        }


    }

    public class TrackRangeItem<TKey, TOutputPropertyValue> : BaseTrackItem<TKey>
        where TKey : IEquatable<TKey>
    {
        public TrackRangeItem(ScopedSettings settings, ITrackerStorage<TKey> storage) : base(settings, storage)
        {
        }
    }

    /// <summary>
    /// Allows to get specific information about a 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class BaseTrackItem<TKey>
        where TKey : IEquatable<TKey>
    {
        protected readonly TKey Key;

        protected readonly ITrackerStorage<TKey> Storage;

        protected readonly ScopedSettings Settings;

        public BaseTrackItem(ScopedSettings settings, ITrackerStorage<TKey> storage, TKey key)
        {
            Settings = settings;
            Storage = storage;
            Key = key;
        }



        // TODO: Fix.
        public virtual int Count { get; }

    }
}
