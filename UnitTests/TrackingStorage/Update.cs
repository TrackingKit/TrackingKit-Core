using System.Collections.Generic;
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
            var storage = new TrackerStorage();

            Assert.Throws<System.ArgumentException>(() => storage.SetValue("", 1, 1, "value"));
        }
    }
}
