using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Moq;
using MyApp.Data.Repos;
using System.Collections.Generic;
using System.Threading;

namespace TestProject
{
    public class DynamoTodoRepositoryTests
    {
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
            // this would only happen when search result is empty but there are more pages to be fetched
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

        [Fact]
        public async Task SearchTodosPageAsync_NormalizesQuery_ForCaseInsensitiveSearch()
        {
            var clientMock = new Mock<IAmazonDynamoDB>();
            var contextMock = new Mock<IDynamoDBContext>();
            QueryRequest? capturedRequest = null;

            clientMock
                .Setup(c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<QueryRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new QueryResponse
                {
                    Items = new List<Dictionary<string, AttributeValue>>(),
                    LastEvaluatedKey = null
                });

            var repo = new DynamoTodoRepository(clientMock.Object, contextMock.Object);

            await repo.SearchTodosPageAsync("U1", "  BaL ", 10, null);

            Assert.NotNull(capturedRequest);
            Assert.Equal(
                "contains(TitleLower, :queryLower) OR contains(ContentLower, :queryLower)",
                capturedRequest!.FilterExpression);
            Assert.Equal(
                "bal",
                capturedRequest.ExpressionAttributeValues[":queryLower"].S);
        }
    }
}
