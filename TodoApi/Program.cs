using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.IncludeFields = true);

var app = builder.Build();

app.MapGet("/", () => new TodoWithFields { Name = "Walk dog", IsComplete = false });

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.Select(t => new TodoItemDto(t)).ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) => await db.Todos.Where(t => t.IsComplete).Select(t => new TodoItemDto(t)).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(new TodoItemDto(todo)) : Results.NotFound());

app.MapPost("/todoitems", async (TodoItemDto todoItemDto, TodoDb db) => {
	var todoItem = new Todo {
		IsComplete = todoItemDto.IsComplete,
		Name = todoItemDto.Name
	};
	db.Todos.Add(todoItem);
	await db.SaveChangesAsync();
	return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDto(todoItem));
});

app.MapPut("/todoitems/{id}", async (int id, TodoItemDto todoItemDto, TodoDb db) => {
	var todo = await db.Todos.FindAsync(id);
	if(todo is null)
		return Results.NotFound();
	todo.IsComplete = todoItemDto.IsComplete;
	todo.Name = todoItemDto.Name;
	await db.SaveChangesAsync();
	return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) => {
	if(await db.Todos.FindAsync(id) is Todo todo) {
		db.Todos.Remove(todo);
		await db.SaveChangesAsync();
		return Results.Ok(new TodoItemDto(todo));
	}
	return Results.NotFound();
});

app.Run();

public class Todo {
	public int Id { get; set; }
	public string? Name { get; set; }
	public bool IsComplete { get; set; }
	public string? Secret { get; set; }
}

public class TodoDb: DbContext {
	public TodoDb(DbContextOptions<TodoDb> options) : base(options) {
	}
	public DbSet<Todo> Todos => Set<Todo>();
}

public class TodoItemDto {
	public int Id { get; set; }
	public string? Name { get; set; }
	public bool IsComplete { get; set; }
	public TodoItemDto() {
	}
	public TodoItemDto(Todo todoItem) => (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}

class TodoWithFields {
	public string? Name;
	public bool IsComplete;
}

public partial class Program {
}
