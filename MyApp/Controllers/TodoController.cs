using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Exceptions;
using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;
using MyApp.Services;

namespace MyApp.Controllers
{
    // Enforce authorization for all endpoints in this controller
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly ICurrentUser _currentUser;

        public TodoController(ITodoService todoService, ICurrentUser currentUser)
        {
            _todoService = todoService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos(TodoStatus? status)
        {
            IEnumerable<TodoItem> todos = await _todoService.GetTodosAsync(_currentUser.UserId, status);
            return Ok(todos);
        }

        [HttpGet("{todoId}")]
        public async Task<ActionResult<TodoItem>> GetTodo(string todoId)
        {
            TodoItem todo = await _todoService.GetTodoAsync(_currentUser.UserId, todoId);
            if (todo == null) throw new TodoNotFoundException();
            return Ok(todo);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> AddTodo([FromBody] CreateTodoRequest request)
        {
            TodoItem insertedTodo = await _todoService.CreateTodoAsync(request);
            return Ok(insertedTodo);
        }

        [HttpPut("{todoId}")]
        public async Task<ActionResult<TodoItem?>> UpdateTodo(string todoId, [FromBody] UpdateTodoRequest request)
        {
            TodoItem? todo = await _todoService.UpdateTodoAsync(_currentUser.UserId, todoId, request);
            if (todo == null) throw new TodoNotFoundException();
            return Ok(todo);
        }

        [HttpDelete("{todoId}")]
        public async Task<ActionResult<bool>> DeleteTodo(string todoId)
        {
            bool flag = await _todoService.DeleteTodoAsync(_currentUser.UserId, todoId);
            return Ok(flag);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> SearchTodos(string? query)
        {
            IEnumerable<TodoItem> todos = await _todoService.SearchTodosAsync(_currentUser.UserId, query);
            return Ok(todos);
        }

        [HttpPut("{todoId}/remainder")]
        public async Task<ActionResult> SetRemainder(string todoId, [FromBody] SetReminderRequest request)
        {
            // validate timestamp
            if (request.RemindAtEpoch <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return BadRequest("Reminder time must be in the future.");

            bool flag = await _todoService.SetRemainderAsync(_currentUser.UserId, todoId, request.RemindAtEpoch);
            if (!flag) throw new TodoNotFoundException();

            return Ok();
        }

    }
}
