using Microsoft.AspNetCore.Mvc;
using WebApiTasks.Models;

namespace WebApiTasks.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> tasks = new()
    {
        new TaskItem
        {
            Id = 1,
            Title = "Изучить C#",
            Description = "Изучить основы ASP.NET Core Web API",
            IsCompleted = false
        },

        new TaskItem
        {
            Id = 2,
            Title = "Сделать лабораторную",
            Description = "Реализовать REST API",
            IsCompleted = false
        }
    };

    [HttpGet]
    public ActionResult<List<TaskItem>> GetTasks()
    {
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public ActionResult<TaskItem> GetTask(int id)
    {
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task == null) {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public ActionResult<TaskItem> CreateTask(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title)) {
            return BadRequest();
        }

        if (tasks.Count > 0) {
            task.Id = tasks.Max(t => t.Id) + 1;
        }
        else {
            task.Id = 1;
        }

        tasks.Add(task);

        return CreatedAtAction(
            nameof(GetTask),
            new { id = task.Id },
            task
        );
    }

    [HttpPut("{id}")]
    public ActionResult<TaskItem> UpdateTask(int id, TaskItem updatedTask)
    {
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task == null) {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(updatedTask.Title)) {
            return BadRequest();
        }

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.IsCompleted = updatedTask.IsCompleted;

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteTask(int id)
    {
        var task = tasks.FirstOrDefault(t => t.Id == id);

        if (task == null) {
            return NotFound();
        }

        tasks.Remove(task);

        return Ok();
    }
}