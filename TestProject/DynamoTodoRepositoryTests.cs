using MyApp.Data.Repos;
using MyApp.Models.Enum;

namespace TestProject
{
    public class DynamoTodoRepositoryTests
    {
        [Fact]
        public void BuildStatusFilter_WithStatus_ReturnsStatusExpression()
        {
            var result = DynamoTodoRepository.BuildStatusFilter(TodoStatus.Complete);

            Assert.Equal("StatusCode = :status", result.FilterExpression);
            Assert.True(result.Values.ContainsKey(":status"));
            Assert.Equal(
                ((int)TodoStatus.Complete).ToString(),
                result.Values[":status"].N);
        }

        [Fact]
        public void BuildStatusFilter_NoStatus_ReturnsDefaultExpression()
        {
            var result = DynamoTodoRepository.BuildStatusFilter(null);

            Assert.Equal(
                "StatusCode = :incomplete OR StatusCode = :complete",
                result.FilterExpression);
            Assert.Equal(
                ((int)TodoStatus.Incomplete).ToString(),
                result.Values[":incomplete"].N);
            Assert.Equal(
                ((int)TodoStatus.Complete).ToString(),
                result.Values[":complete"].N);
        }
    }
}
