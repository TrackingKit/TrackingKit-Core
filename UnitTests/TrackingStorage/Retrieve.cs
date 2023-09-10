using Xunit;

namespace Tracking.UnitTests.TrackingStorage
{
    public class RetrievalTests
    {
        private void PopulateStorage(InMemoryTrackerStorage storage, int propertiesCount, int ticksCount, int versionsCount, int tagsCount)
        {
            for (int p = 1; p <= propertiesCount; p++)
            {
                for (int t = 1; t <= ticksCount; t++)
                {
                    for (int v = 1; v <= versionsCount; v++)
                    {
                        var tags = Enumerable.Range(1, tagsCount).Select(tag => $"tag{tag}").ToList();
                        storage.SetValue($"property{p}", t, v, $"value{p}-{t}-{v}", tags);
                    }
                }
            }
        }

        [Fact]
        public void TryGetLatestVersion_WithLargeData_ReturnsCorrectVersion()
        {
            var storage = new InMemoryTrackerStorage();
            PopulateStorage(storage, 10, 10, 10, 5);

            bool result = storage.TryGetLatestVersion("property5", 5, out var latestVersion);

            Assert.True(result);
            Assert.Equal(10, latestVersion);
        }

        [Fact]
        public void TryGetVersions_WithLargeData_ReturnsAllVersions()
        {
            var storage = new InMemoryTrackerStorage();
            PopulateStorage(storage, 10, 10, 10, 5);

            bool result = storage.TryGetVersions("property5", 5, out var versions);

            Assert.True(result);
            Assert.Equal(Enumerable.Range(1, 10), versions);
        }

        // ... other tests ...

        [Fact]
        public void Clone_WithLargeData_CreatesDeepCopy()
        {
            var storage = new InMemoryTrackerStorage();
            PopulateStorage(storage, 10, 10, 10, 5);

            var cloned = (InMemoryTrackerStorage)storage.Clone();
            cloned.SetValue("property5", 5, 11, "new-value");

            bool originalResult = storage.TryGetLatestVersion("property5", 5, out var originalVersion);
            bool clonedResult = cloned.TryGetLatestVersion("property5", 5, out var clonedVersion);

            Assert.True(originalResult);
            Assert.True(clonedResult);
            Assert.Equal(10, originalVersion);
            Assert.Equal(11, clonedVersion);
        }
    }
}
