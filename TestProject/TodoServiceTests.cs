using Moq;
using MyApp.Data.Repos;
using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;
using MyApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _repoMock;
        private readonly TodoService _service;

        public TodoServiceTests()
        {
            _repoMock = new Mock<ITodoRepository>();
            _service = new TodoService(_repoMock.Object);
        }

        [Fact]
        public async Task CreateTodoAsync_ShouldAssignUlid_AndCallRepo()
        {
            var inputTodo = new TodoItem { UserId = "U1" };

            _repoMock
                .Setup(r => r.AddTodoAsync(It.IsAny<TodoItem>()))
                .ReturnsAsync((TodoItem t) => t);

            var result = await _service.CreateTodoAsync(inputTodo);

            Assert.NotNull(result.TodoId);             // Ulid generated
            Assert.Equal("U1", result.UserId);
            _repoMock.Verify(r => r.AddTodoAsync(It.IsAny<TodoItem>()), Times.Once);
        }

        [Fact]
        public async Task SearchTodosAsync_ShouldReturnMatchingItems()
        {
            var todos = new List<TodoItem>
            {
                new TodoItem { Title = "Buy milk" },
                new TodoItem { Title = "Study C#" },
                new TodoItem { Content = "Milk Tea recipe" }
            };

            _repoMock.Setup(r => r.GetAllTodosAsync("U1", null))
                     .ReturnsAsync(todos);

            var results = await _service.SearchTodosAsync("U1", "milk");

            Assert.Equal(2, results.Count());
            Assert.Contains(results, t => t.Title == "Buy milk");
            Assert.Contains(results, t => t.Content == "Milk Tea recipe");
        }

        [Fact]
        public async Task UpdateTodoAsync_ShouldModifyFields_AndCallRepo()
        {
            var existing = new TodoItem
            {
                UserId = "U1",
                TodoId = "T1",
                Title = "Old",
                Content = "Old content",
                StatusCode = TodoStatus.Incomplete
            };

            var request = new UpdateTodoRequest
            {
                Title = "New Title",
                Content = "Updated content",
                StatusCode = TodoStatus.Complete,
                RemindTimestamp = 123456
            };

            _repoMock.Setup(r => r.GetTodoAsync("U1", "T1"))
                     .ReturnsAsync(existing);

            _repoMock.Setup(r => r.UpdateTodoAsync(It.IsAny<TodoItem>()))
                     // Just return completed task without any value
                     .Returns(Task.CompletedTask);

            var result = await _service.UpdateTodoAsync("U1", "T1", request);

            Assert.Equal("New Title", result.Title);
            Assert.Equal("Updated content", result.Content);
            Assert.Equal(TodoStatus.Complete, result.StatusCode);
            Assert.Equal(123456, result.RemindTimestamp);

            _repoMock.Verify(r => r.UpdateTodoAsync(existing), Times.Once);
        }
    }
}
