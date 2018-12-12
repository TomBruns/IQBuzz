using System;

using Xunit;

namespace WP.Learning.MongoDB.UnitTests
{
    // https://xunit.github.io/docs/comparisons.html
    public class MongoDBTests
    {
        [Fact(Skip = @"Only needed 1 time")]
        //[Fact]
        public void TestBootstrapSchema()
        {
            MongoDBContext.BootstrapMongoSchema();
        }
    }
}
