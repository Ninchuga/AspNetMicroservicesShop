using Xunit;

namespace Shopping.IntegrationTests.Utility.Ordering
{
    [CollectionDefinition("Ordering collection")]
    public class OrderingCollectionFixture : ICollectionFixture<OrderingFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces to the test classes
        // Those test classes that use this collection will share the setup from OrderingFixture
        // Only one time setup will be created before any tests for all of the test classes that use this collection
        // and will not be cleaned up until all test classes in the collection have finished running.
    }
}
