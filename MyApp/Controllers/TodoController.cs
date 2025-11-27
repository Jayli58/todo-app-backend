using Microsoft.AspNetCore.Mvc;
using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;
using MyApp.Services;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos(string userId, TodoStatus? status)
        {
            IEnumerable<TodoItem> todos = await _todoService.GetTodosAsync(userId, status);
            return Ok(todos);
        }

        [HttpGet("{userId}/{todoId}")]
        public async Task<ActionResult<TodoItem>> GetTodo(string userId, string todoId)
        {
            TodoItem todo = await _todoService.GetTodoAsync(userId, todoId);
            if (todo == null) return NotFound();
            return Ok(todo);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> AddTodo([FromBody] TodoItem todo)
        {
            TodoItem insertedTodo = await _todoService.CreateTodoAsync(todo);
            return Ok(insertedTodo);
        }

        [HttpPut("{userId}/{todoId}")]
        public async Task<ActionResult<TodoItem>> UpdateTodo(string userId, string todoId, [FromBody] UpdateTodoRequest request)
        {
            TodoItem todo = await _todoService.UpdateTodoAsync(userId, todoId, request);
            if (todo == null) return NotFound();
            return Ok(todo);
        }

        [HttpDelete("{userId}/{todoId}")]
        public async Task<ActionResult<bool>> DeleteTodo(string userId, string todoId)
        {
            bool flag = await _todoService.DeleteTodoAsync(userId, todoId);
            return Ok(flag);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> SearchTodos(string userId, string? query)
        {
            IEnumerable<TodoItem> todos = await _todoService.SearchTodosAsync(userId, query);
            return Ok(todos);
        }
    }
}
