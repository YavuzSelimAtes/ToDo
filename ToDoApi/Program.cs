using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;
using ToDoApi.Helpers;
using Hangfire;
using ToDoApi.Services; 
using Hangfire.PostgreSql;
using System.Globalization; 

var builder = WebApplication.CreateBuilder(args);
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

var __builder = new Npgsql.NpgsqlConnectionStringBuilder(cs);
Console.WriteLine($"[DB DEBUG] Host={__builder.Host}; Port={__builder.Port}; Database={__builder.Database}; Username={__builder.Username}; SSL={__builder.SslMode}");



builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => 
    {
        options.UseNpgsqlConnection(cs);
    }));

builder.Services.AddDbContext<ToDoContext>(options =>
    options.UseNpgsql(cs));

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("open", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHangfireServer();
builder.Services.AddScoped<TaskStateService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoContext>();
    db.Database.Migrate();
}

app.UseHangfireDashboard("/Hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllConnectionsFilter() }
});

RecurringJob.AddOrUpdate<TaskStateService>(
    "daily-task-processor", 
    service => service.ProcessDailyTasks(),
    Cron.Daily(3, 0));

app.UseCors("open");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Todo API is running");

app.MapGet("/api/users/{userId}/todos", async (
    int userId,
    string? category,
    ToDoContext db,
    DateTime? startDate,
    DateTime? endDate,
    int? year,
    int? week,
    int? month) =>
{
    var query = db.ToDoItems.AsQueryable().Where(t => t.UserId == userId && t.IsTemplate == false);

    if (string.IsNullOrEmpty(category))
    {
        return Results.Ok(new List<ToDoItem>());
    }

    query = query.Where(t => t.Category == category);

    if (category == "Günlük")
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value.ToUniversalTime() && t.CreatedAt <= endDate.Value.ToUniversalTime());
        }
    }
    else if (category == "Haftalık")
    {
        if (year.HasValue && week.HasValue)
        {
            var monday = ISOWeek.ToDateTime(year.Value, week.Value, DayOfWeek.Monday);
            var startOfDayUtc = new DateTime(monday.Year, monday.Month, monday.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDayUtc = startOfDayUtc.AddDays(1);

            query = query.Where(t => t.CreatedAt >= startOfDayUtc && t.CreatedAt < endOfDayUtc);
        }
    }
    else if (category == "Aylık")
    {
        if (year.HasValue && month.HasValue)
        {
            var firstDay = new DateTime(year.Value, month.Value, 1);
            var startOfDayUtc = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDayUtc = startOfDayUtc.AddDays(1);

            query = query.Where(t => t.CreatedAt >= startOfDayUtc && t.CreatedAt < endOfDayUtc);
        }
    }

    var tasks = await query.OrderBy(t => t.CreatedAt)
                           .Select(t => new TaskDto
                           {
                               Id = t.Id,
                               Title = t.Title,
                               Category = t.Category,
                               State = t.State,
                               CreatedAt = t.CreatedAt,
                               UserId = t.UserId
                           }).ToListAsync();

    return Results.Ok(tasks);
});

app.MapGet("/api/users/{userId}", async (int userId, ToDoContext db) =>
{
    var user = await db.Users.FindAsync(userId);

    if (user is null)
    {
        return Results.NotFound("Kullanıcı bulunamadı.");
    }

    var userDto = new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Score = user.Score,
        DailyTasks = user.DailyTasks,
        WeeklyTasks = user.WeeklyTasks,
        MonthlyTasks = user.MonthlyTasks,
        DailyTasksCompleted = user.DailyTasksCompleted,
        WeeklyTasksCompleted = user.WeeklyTasksCompleted,
        MonthlyTasksCompleted = user.MonthlyTasksCompleted
    };

    return Results.Ok(userDto);
});

app.MapPost("/api/users/{userId}/todos", async (int userId, CreateTaskDto dto, ToDoContext db) =>
{
    var user = await db.Users.FindAsync(userId);
    if (user is null)
    {
        return Results.NotFound("Kullanıcı bulunamadı.");
    }

    switch (dto.Category)
    {
        case "Günlük":
            user.DailyTasks++;
            break;
        case "Haftalık":
            user.WeeklyTasks++;
            break;
        case "Aylık":
            user.MonthlyTasks++;
            break;
    }

    var templateTask = new ToDoItem
    {
        Title = dto.Title,
        Category = dto.Category,
        CreatedAt = DateTime.UtcNow,
        State = State.Kapali,
        IsTemplate = true,
        UserId = userId,
        RepeatDayOfWeek = null,
        RepeatDayOfMonth = null
    };
    db.ToDoItems.Add(templateTask);
    await db.SaveChangesAsync();

    
    var now = dto.CreatedAt.ToUniversalTime();
    DateTime instanceDate = now.Date;

    if (dto.Category == "Haftalık")
    {
        int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        instanceDate = now.AddDays(-1 * diff).Date;
    }
    else if (dto.Category == "Aylık")
    {
        instanceDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    var firstInstance = new ToDoItem
    {
        Title = dto.Title,
        Category = dto.Category,
        CreatedAt = instanceDate,
        State = State.Kapali,
        IsTemplate = false,
        ParentTaskId = templateTask.Id,
        UserId = userId
    };
    db.ToDoItems.Add(firstInstance);
    await db.SaveChangesAsync();

    if (dto.Category == "Haftalık")
    {
        templateTask.RepeatDayOfWeek = (int)instanceDate.DayOfWeek;
    }
    else if (dto.Category == "Aylık")
    {
        templateTask.RepeatDayOfMonth = instanceDate.Day;
    }
    await db.SaveChangesAsync();

    var userDto = new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Score = user.Score,
        DailyTasks = user.DailyTasks,
        WeeklyTasks = user.WeeklyTasks,
        MonthlyTasks = user.MonthlyTasks,
        DailyTasksCompleted = user.DailyTasksCompleted,
        WeeklyTasksCompleted = user.WeeklyTasksCompleted,
        MonthlyTasksCompleted = user.MonthlyTasksCompleted
    };

    return Results.Ok(userDto); 
});


static ToDoItem CreateInstance(ToDoItem template, DateTime creationDate, ToDoUser user)
{
    return new ToDoItem
    {
        Title = template.Title,
        Category = template.Category,
        CreatedAt = creationDate,
        State = State.Kapali,
        IsTemplate = false,
        ParentTaskId = template.Id,
        UserId = template.UserId,
        User = user // <-- YENİ EKLENDİ: İlişkiyi net bir şekilde kuruyoruz.
    };
}

app.MapPut("/api/todos/{id:int}", async (int id, TaskDto updatedTask, ToDoContext db) =>
{
    var task = await db.ToDoItems.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound("Görev bulunamadı.");
    }

     if (task.State == State.Acik && updatedTask.State == State.Isaretli)
    {
        var user = await db.Users.FindAsync(task.UserId);
        if (user != null)
        {
            switch (task.Category)
            {
                case "Günlük":
                    user.Score += 2;
                    user.DailyTasksCompleted++;
                    break;
                case "Haftalık":
                    user.Score += 7;
                    user.WeeklyTasksCompleted++;
                    break;
                case "Aylık":
                    user.Score += 25;
                    user.MonthlyTasksCompleted++;
                    break;
            }
        }
    }

    task.Title = updatedTask.Title;
    task.State = updatedTask.State;

    await db.SaveChangesAsync();

    var ownerUser = await db.Users.FindAsync(task.UserId);
    var userDto = new UserDto
    {
        Id = ownerUser.Id,
        Username = ownerUser.Username,
        Score = ownerUser.Score,
        DailyTasks = ownerUser.DailyTasks,
        WeeklyTasks = ownerUser.WeeklyTasks,
        MonthlyTasks = ownerUser.MonthlyTasks,
        DailyTasksCompleted = ownerUser.DailyTasksCompleted,
        WeeklyTasksCompleted = ownerUser.WeeklyTasksCompleted,
        MonthlyTasksCompleted = ownerUser.MonthlyTasksCompleted
    };

    return Results.Ok(userDto);
});

app.MapDelete("/api/todos/template/{instanceId:int}", async (int instanceId, [FromBody] DeleteTaskDto dto, ToDoContext db) =>
{
    // 1. Şifre ve Kullanıcı Doğrulaması
    var user = await db.Users.FindAsync(dto.UserId);
    if (user is null) return Results.NotFound("Kullanıcı bulunamadı.");

    var hashed = HashPassword.Password(dto.Password);
    if (user.PasswordHash != hashed) return Results.Unauthorized();

    var taskInstance = await db.ToDoItems.FindAsync(instanceId);
    if (taskInstance is null) return Results.NotFound("Görev bulunamadı.");
    if (taskInstance.UserId != dto.UserId) return Results.Forbid();

    // 2. Ana Şablonun ID'sini Bul
    int? templateIdToDelete = taskInstance.IsTemplate ? taskInstance.Id : taskInstance.ParentTaskId;
    if (templateIdToDelete is null)
    {
        db.ToDoItems.Remove(taskInstance);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

     var templateTask = await db.ToDoItems.FindAsync(templateIdToDelete);
    if (templateTask != null)
    {
        switch (templateTask.Category)
        {
            case "Günlük":
                if (user.DailyTasks > 0) user.DailyTasks--;
                break;
            case "Haftalık":
                if (user.WeeklyTasks > 0) user.WeeklyTasks--;
                break;
            case "Aylık":
                if (user.MonthlyTasks > 0) user.MonthlyTasks--;
                break;
        }
    }

    var today = DateTime.UtcNow.Date;

    var tasksToDelete = await db.ToDoItems
        .Where(t => 
            t.Id == templateIdToDelete || // 1. Ana şablonun kendisi
            (t.ParentTaskId == templateIdToDelete && t.CreatedAt.Date >= today) || // 2. Bugünden itibaren oluşturulmuş kopyalar
            t.Id == instanceId // 3. (YENİ EKLENDİ) Üzerine tıklanan kopyanın kendisi
        )
        .ToListAsync();

    if (tasksToDelete.Any())
    {
        db.ToDoItems.RemoveRange(tasksToDelete);
        await db.SaveChangesAsync();
    }

    var userDto = new UserDto
    {
        Id = user.Id,
        Username = user.Username,
        Score = user.Score,
        DailyTasks = user.DailyTasks,
        WeeklyTasks = user.WeeklyTasks,
        MonthlyTasks = user.MonthlyTasks,
        DailyTasksCompleted = user.DailyTasksCompleted,
        WeeklyTasksCompleted = user.WeeklyTasksCompleted,
        MonthlyTasksCompleted = user.MonthlyTasksCompleted
    };
    return Results.Ok(userDto); 
});

app.MapControllers();

app.Run();
