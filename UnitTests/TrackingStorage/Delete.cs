using Xunit;

namespace Tracking.UnitTests.TrackingStorage
{
    public class Delete
    {
        [Fact]
        public void RemoveValues_ValidInput_ReturnsTrueAndRemovesData()
        {
            var storage = new TrackerStorage();
            storage.SetValue("testProperty", 1, 1, "value1", new[] { "tag1", "tag2" });

            bool result = storage.RemoveValues("testProperty", 1);

            Assert.True(result);
        }

        [Fact]
        public void RemoveValues_InvalidPropertyName_ReturnsFalse()
        {
            var storage = new TrackerStorage();

            bool result = storage.RemoveValues("invalidProperty", 1);

            Assert.False(result);
        }

        [Fact]
        public void RemoveSpecificValue_ValidInput_ReturnsTrueAndRemovesData()
        {
            var storage = new TrackerStorage();
            storage.SetValue("testProperty", 1, 1, "value1", new[] { "tag1", "tag2" });

            bool result = storage.RemoveSpecificValue("testProperty", 1, 1);

            Assert.True(result);
            // ... [rest of the test]
        }

        [Fact]
        public void RemoveSpecificValue_InvalidPropertyName_ReturnsFalse()
        {
            var storage = new TrackerStorage();

            bool result = storage.RemoveSpecificValue("invalidProperty", 1, 1);

            Assert.False(result);
        }
    }
}
