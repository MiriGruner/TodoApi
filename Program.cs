using System.Text;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

//var connectionString = "Server=bujkixbbtsdssxnmdyuu-mysql.services.clever-cloud.com;User=uftyv7t4ctoqb2rx;Password=tC6pwbqLIWAI1c7MSb88;Database=bujkixbbtsdssxnmdyuu;";
// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Get the connection string from app configuration

var connectionString = Environment.GetEnvironmentVariable("ToDoDB");



if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Connection string is missing!");
    return;
}
try
{

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


}
catch (Exception ex)
{
    Console.WriteLine($"שגיאה: {ex.Message}");
}


var app = builder.Build();

app.UseCors();
app.UseAuthentication(); // Enable JWT Authentication
app.UseAuthorization();  // Enable Authorization Middleware

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// Route to Get All Tasks


app.MapGet("/", async (ToDoDbContext dbContext) =>
{
    // Fetch all items from the database
    var tasks = await dbContext.Items.ToListAsync();

    // Check if the list is empty
    if (tasks == null || tasks.Count == 0)
    {
        return Results.NotFound("No items found in the database.");
    }

    // Return the tasks as JSON
    return Results.Json(tasks);
});

// Route to Add New Task
app.MapPost("/", async (ToDoDbContext dbContext, Item newItem) =>
{
    dbContext.Items.Add(newItem);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/{newItem.Id}", newItem);
});

// Route to Update a Task
app.MapPut("/{id}", async (int id, ToDoDbContext dbContext, Item updatedItem) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound("Item not found.");
    }

    if (!string.IsNullOrEmpty(updatedItem.Name))
        item.Name = updatedItem.Name;
        item.IsComplete = updatedItem.IsComplete;
        await dbContext.SaveChangesAsync();
  
    return Results.Json(item);
    
});

// Route to Delete a Task
app.MapDelete("/{id}",   async (int id, ToDoDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound("Item not found.");
    }

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.Ok("Item deleted successfully.");
});

app.Run();