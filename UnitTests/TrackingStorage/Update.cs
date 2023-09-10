using System.Collections.Generic;
using TrackingKit_Core.TrackingKit_Core.Factories;
using Xunit;

namespace Tracking.UnitTests.TrackingStorage
{
    public class Update
    {
        [Fact]
        public void SetValue_ValidInput_SetsValueCorrectly()
        {
            /*
            var storage = new TrackerStorage();

            storage.SetValue("testProperty", 1, 1, "value1", new[] { "tag1", "tag2" });

            Assert.Contains("testProperty", storage.Keys);
            */
        }

        [Fact]
        public void SetValue_InvalidPropertyName_ThrowsArgumentException()
        {
            var storage = new InMemoryTrackerStorage();

            Assert.Throws<System.ArgumentException>(() => storage.SetValue("", 1, 1, "value"));
        }

        public void HelloTest()
        {
            Object bob = new();

            var trackedItem = TrackerSystem.GetOrRegister(bob);

            trackedItem.Add("Money", 200, 1, "personal");

            trackedItem.Add("Money", 200, 300, "personal");

            TagFilter w = new();

            w.Set("personal", FilterOption.Include);

            using(var scope = trackedItem.ScopeByTicks(1, 200, w))
            {

                LogFactory.Info(scope.GetOrNext<int>("Money", 1));
            }


        }
    }
}
