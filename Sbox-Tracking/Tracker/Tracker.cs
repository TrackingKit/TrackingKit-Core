using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Tracking
{
    public partial class Tracker : ITracker
    {
        // Linked list 
        private readonly Dictionary<TrackerKey, object> Values = new Dictionary<TrackerKey, object>();

        public HashSet<string> CurrentBuildTags { get; set; } = new HashSet<string>();

        public void EndGroup(params string[] idents) => idents?.ToList().ForEach(ident => CurrentBuildTags.Remove(ident));

        public void StartGroup(params string[] idents) => idents?.ToList().ForEach(ident => CurrentBuildTags.Add(ident));

        public bool KeyExistsInTracker(string propertyName) => Values.Keys.Where(x => x.PropertyName == propertyName).Count() != 0;

        protected (int min, int max) RecordedRange => (Values.Min(x => x.Key.Tick), Values.Max(x => x.Key.Tick));

        public bool IsScoped => Filter?.IsScoped ?? false;

        public void Set(string propertyName, object value, params string[] idents)
        {

            var latestRecordedVersion = Values.Keys
                .Where(x => x.Tick == Time.Tick && x.PropertyName == propertyName)
                .Select(key => key.Version)
                .DefaultIfEmpty(0) // Provide 0 as the default value if the collection is empty
                .Max();

            TrackerKey key = new()
            {
                PropertyName = propertyName,
                Version = latestRecordedVersion + 1,
                Tags = idents.AsEnumerable().Concat(CurrentBuildTags).ToArray()
            };

            Values.Add(key, value);
        }


    }

}