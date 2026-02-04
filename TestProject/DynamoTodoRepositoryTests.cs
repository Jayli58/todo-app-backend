using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Moq;
using MyApp.Data.Repos;
using MyApp.Models.Enum;
using System.Collections.Generic;
using System.Threading;

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

        [Fact]
        // Test that the repository returns null for nextToken when there are no matching items
        public async Task SearchTodosPageAsync_EmptyFilteredPages_ReturnsNullToken()
        {
            var clientMock = new Mock<IAmazonDynamoDB>();
            var contextMock = new Mock<IDynamoDBContext>();

            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new AttributeValue { S = "U1" },
                ["TodoId"] = new AttributeValue { S = "T1" }
            };

            // Setup the mock to return two empty pages
            clientMock
                .SetupSequence(c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>(),
                    LastEvaluatedKey = lastEvaluatedKey
                })
                .ReturnsAsync(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>(),
                    LastEvaluatedKey = null
                });

            var repo = new DynamoTodoRepository(clientMock.Object, contextMock.Object);

            var result = await repo.SearchTodosPageAsync("U1", "milk", 10, null);

            Assert.Empty(result.Items);
            Assert.Null(result.NextToken);
            clientMock.Verify(
                c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}
