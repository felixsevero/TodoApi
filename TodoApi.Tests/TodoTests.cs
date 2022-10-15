using System.Net;
using System.Net.Http.Json;

namespace TodoApi.Tests;

public class TodoTests {

	[Fact]
	public async Task DeleteTodos() {
		await using var application = new TodoApplication();

		var client = application.CreateClient();
		var response = await client.PostAsJsonAsync("/todoitems", new TodoItemDto { Name = "I want to do this thing tomorrow" });

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		var todos = await client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");

		var todo = Assert.Single(todos);
		Assert.Equal("I want to do this thing tomorrow", todo.Name);
		Assert.False(todo.IsComplete);

		response = await client.DeleteAsync($"/todoitems/{todo.Id}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		response = await client.GetAsync($"/todoitems/{todo.Id}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetTodos() {
		await using var application = new TodoApplication();

		var client = application.CreateClient();
		var todos = await client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");

		Assert.Empty(todos);
	}

	[Fact]
	public async Task PostTodos() {
		await using var application = new TodoApplication();

		var client = application.CreateClient();
		var response = await client.PostAsJsonAsync("/todoitems", new TodoItemDto { Name = "I want to do this thing tomorrow" });

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		var todos = await client.GetFromJsonAsync<List<TodoItemDto>>("/todoitems");

		var todo = Assert.Single(todos);
		Assert.Equal("I want to do this thing tomorrow", todo.Name);
		Assert.False(todo.IsComplete);
	}

}
