using MyApp.Common;

namespace TestProject
{
    public class UtilTest
    {
        [Fact]
        public void Test1()
        {
            string ulid = UlidGenerator.NewUlid();
            // ulid should be 26 characters long
            Assert.Equal(26, ulid.Length);
        }
    }
}
