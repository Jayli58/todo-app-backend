using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using MyApp.Data.Repos;
using MyApp.Models;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepository _repo;

        public TodoController(ITodoRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos(string userId)
        {
            var todos = await _repo.GetAllTodosAsync(userId);
            return Ok(todos);
        }

        [HttpGet("{userId}/{todoId}")]
        public async Task<IActionResult> GetTodo(string userId, string todoId)
        {
            var todo = await _repo.GetTodoAsync(userId, todoId);
            if (todo == null) return NotFound();
            return Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> AddTodo(TodoItem todo)
        {
            // Generate a new ULID for the TodoId
            todo.TodoId = UlidGenerator.NewUlid();

            await _repo.AddTodoAsync(todo);
            return Ok(todo);
        }

        [HttpPut("{userId}/{todoId}")]
        public async Task<IActionResult> UpdateTodo(string userId, string todoId, [FromBody] TodoItem todo)
        {
            todo.UserId = userId;
            todo.TodoId = todoId;

            await _repo.UpdateTodoAsync(todo);
            return Ok(todo);
        }

        [HttpDelete("{userId}/{todoId}")]
        public async Task<IActionResult> DeleteTodo(string userId, string todoId)
        {
            await _repo.DeleteTodoAsync(userId, todoId);
            return Ok();
        }

    }
}
