using Microsoft.AspNetCore.Mvc;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Services;

namespace StdApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;
    private readonly ILogger<TodoController> _logger;

    public TodoController(ITodoService todoService, ILogger<TodoController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var todos = await _todoService.GetAllAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemDto>> GetById(Guid id)
    {
        var todo = await _todoService.GetByIdAsync(id);
        if (todo == null)
        {
            _logger.LogWarning("Todo item with ID {TodoId} not found", id);
            return NotFound();
        }

        return Ok(todo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItemDto>> Create([FromBody] CreateTodoItemDto createDto)
    {
        try
        {
            var todo = await _todoService.CreateAsync(createDto);
            _logger.LogInformation("Created todo item with ID {TodoId}", todo.Id);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid todo item creation request");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItemDto>> Update(Guid id, [FromBody] UpdateTodoItemDto updateDto)
    {
        try
        {
            var todo = await _todoService.UpdateAsync(id, updateDto);
            if (todo == null)
            {
                _logger.LogWarning("Todo item with ID {TodoId} not found for update", id);
                return NotFound();
            }

            _logger.LogInformation("Updated todo item with ID {TodoId}", id);
            return Ok(todo);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid todo item update request for ID {TodoId}", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _todoService.DeleteAsync(id);
        if (!result)
        {
            _logger.LogWarning("Todo item with ID {TodoId} not found for deletion", id);
            return NotFound();
        }

        _logger.LogInformation("Deleted todo item with ID {TodoId}", id);
        return NoContent();
    }
}
