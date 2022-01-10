using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("SqliteConnectionString") ?? "Data Source=Tasks.db";

builder.Services.AddSqlite<TaskContext>(connectionString);

var app = builder.Build();

await EnsureDb(app.Services, app.Logger);

app.MapGet("/tasks", async (TaskContext context) =>
{
    var tasks = await context.Tasks.Take(20).ToListAsync();

    return Results.Ok(tasks);
});

app.MapGet("/tasks/{id}", async (int id, TaskContext context) =>
{
    var task = await context.Tasks.FirstOrDefaultAsync(m => m.Id == id);

    return task is null ? Results.NotFound() : Results.Ok(task);
});

app.MapPost("/tasks", async (TaskEntity entity, TaskContext context) =>
{
    if (entity is not { })
    {
        return Results.BadRequest();
    }
    
    entity.CreatedAt = DateTime.UtcNow;

    context.Tasks.Add(entity);
    await context.SaveChangesAsync();

    return Results.Created($"/tasks/{entity.Id}", entity);
});

app.MapDelete("/tasks/{id}", async (int id, TaskContext db) =>
{
    var entity = await db.Tasks.FirstOrDefaultAsync(m => m.Id == id);

    if (entity is not { })
    {
        return Results.NotFound();
    }

    db.Tasks.Remove(entity);
    await db.SaveChangesAsync();

    return Results.Ok(entity);

});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();

async Task EnsureDb(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Ensuring database" + " '{connectionString}'", connectionString);

    using var db = services.CreateScope().ServiceProvider.GetRequiredService<TaskContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.MigrateAsync();
}

class TaskEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
}