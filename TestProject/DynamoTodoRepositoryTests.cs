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
        public void BuildStatusKeyCondition_WithStatus_ReturnsStatusExpression()
        {
            var result = DynamoTodoRepository.BuildStatusKeyCondition(TodoStatus.Complete);

            Assert.Equal(
                "UserId = :userId AND StatusTodoId BETWEEN :statusStart AND :statusEnd",
                result.KeyConditionExpression);
            Assert.True(result.Values.ContainsKey(":statusStart"));
            Assert.True(result.Values.ContainsKey(":statusEnd"));
            Assert.Equal(
                ((int)TodoStatus.Complete).ToString("D1") + "#",
                result.Values[":statusStart"].S);
            Assert.Equal(
                ((int)TodoStatus.Complete).ToString("D1") + "#~",
                result.Values[":statusEnd"].S);
        }

        [Fact]
        public void BuildStatusKeyCondition_NoStatus_ReturnsDefaultExpression()
        {
            var result = DynamoTodoRepository.BuildStatusKeyCondition(null);

            Assert.Equal(
                "UserId = :userId AND StatusTodoId BETWEEN :statusStart AND :statusEnd",
                result.KeyConditionExpression);
            Assert.Equal(
                ((int)TodoStatus.Incomplete).ToString("D1") + "#",
                result.Values[":statusStart"].S);
            Assert.Equal(
                ((int)TodoStatus.Complete).ToString("D1") + "#~",
                result.Values[":statusEnd"].S);
        }

        [Fact]
        // Test that the repository returns nextToken for empty pages
        public async Task SearchTodosPageAsync_EmptyFilteredPages_ReturnsNextToken()
        {
            var clientMock = new Mock<IAmazonDynamoDB>();
            var contextMock = new Mock<IDynamoDBContext>();

            var lastEvaluatedKey = new Dictionary<string, AttributeValue>
            {
                ["UserId"] = new AttributeValue { S = "U1" },
                ["TodoId"] = new AttributeValue { S = "T1" }
            };

            // Setup the mock to return an empty page with a continuation token
            // such case would only happen when search result is empty but there are more pages to be fetched
            clientMock
                .Setup(c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>(),
                    LastEvaluatedKey = lastEvaluatedKey
                });

            var repo = new DynamoTodoRepository(clientMock.Object, contextMock.Object);

            var result = await repo.SearchTodosPageAsync("U1", "milk", 10, null);

            var expectedToken = MyApp.Data.Dynamo.DynamoQueryHelper.EncodePaginationToken(lastEvaluatedKey);

            Assert.Empty(result.Items);
            Assert.Equal(expectedToken, result.NextToken);
            clientMock.Verify(
                c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
