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
var connectionString = "Host=db;Database=todoapp;Username=postgres;Password=mysecretpassword";

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => 
    {
        options.UseNpgsqlConnection(connectionString);
    }));

builder.Services.AddDbContext<ToDoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseHangfireDashboard();
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
    var query = db.ToDoItems.AsQueryable().Where(t => t.UserId == userId);

    query = query.Where(t => t.IsTemplate == false);

    if (string.IsNullOrEmpty(category))
    {
        return Results.Ok(new List<ToDoItem>());
    }

    query = query.Where(t => t.Category == category);

    if (category == "Günlük")
    {
        if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
        }
        else
        {
             return Results.Ok(new List<ToDoItem>());
        }
    }
    else if (category == "Haftalık")
    {
        if (year.HasValue && week.HasValue)
        {
            var weekStartUnspecified = ISOWeek.ToDateTime(year.Value, week.Value, DayOfWeek.Monday);
            var weekStart = DateTime.SpecifyKind(weekStartUnspecified, DateTimeKind.Utc);
            var weekEnd = weekStart.AddDays(7);
            
            query = query.Where(t => t.CreatedAt >= weekStart && t.CreatedAt < weekEnd);
        }
        else
        {
            return Results.Ok(new List<ToDoItem>());
        }
    }
    else if (category == "Aylık")
    {
        if (year.HasValue && month.HasValue)
        {
            var monthStart = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);
            
            query = query.Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd);
        }
        else
        {
            return Results.Ok(new List<ToDoItem>());
        }
    }

    var tasks = await query.OrderBy(t => t.CreatedAt)
                           .Select(t => new TaskDto {
                               Id = t.Id,
                               Title = t.Title,
                               Category = t.Category,
                               State = t.State,
                               CreatedAt = t.CreatedAt,
                               UserId = t.UserId
                           }).ToListAsync();
    
    return Results.Ok(tasks);
});

app.MapPost("/api/users/{userId}/todos", async (int userId, CreateTaskDto dto, ToDoContext db) =>
{
    var user = await db.Users.FindAsync(userId);
    if (user is null) return Results.NotFound("Kullanıcı bulunamadı.");

    // Tüm kategoriler için bir ana ŞABLON oluşturulur.
    var templateTask = new ToDoItem
    {
        Title = dto.Title,
        Category = dto.Category,
        CreatedAt = DateTime.UtcNow,
        State = State.Kapali, // Şablonların durumu her zaman Kapalı'dır.
        IsTemplate = true,    // Bu bir şablon.
        ParentTaskId = null,
        UserId = userId,
        User = user
    };
    db.ToDoItems.Add(templateTask);

    // SADECE "Günlük" görevler için ilk gün kopyası hemen oluşturulur.
    // Haftalık ve Aylık görevlerin ilk kopyaları Hangfire tarafından oluşturulacak.
    if (dto.Category == "Günlük")
    {
        await db.SaveChangesAsync(); // Şablonun ID'sini almak için kaydet.

        var firstInstance = new ToDoItem
        {
            Title = dto.Title,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow,
            State = State.Kapali, // İlk gün kopyası Kapalı başlar.
            IsTemplate = false,   // Bu bir kopya.
            ParentTaskId = templateTask.Id, // Ana şablonuna bağla.
            UserId = userId,
            User = user
        };
        db.ToDoItems.Add(firstInstance);
    }

    await db.SaveChangesAsync(); // Son değişiklikleri kaydet.

    return Results.Ok(new { message = "Görev şablonu başarıyla oluşturuldu." });
});

// Program.cs dosyasında MapPost'un altına ekle

app.MapPut("/api/todos/{id:int}", async (int id, TaskDto updatedTask, ToDoContext db) =>
{
    var task = await db.ToDoItems.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound("Görev bulunamadı.");
    }

    // Sadece güncellenmesine izin verdiğimiz alanları değiştir
    task.Title = updatedTask.Title;
    task.State = updatedTask.State;
    // Diğer alanları (createdAt, UserId vb.) değiştirmeye izin verme

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/todos/template/{instanceId:int}", async (int instanceId, [FromBody] DeleteTaskDto dto, ToDoContext db) =>
{
    // 1. Şifre ve Kullanıcı Doğrulaması (Değişiklik yok)
    var user = await db.Users.FindAsync(dto.UserId);
    if (user is null) return Results.NotFound("Kullanıcı bulunamadı.");

    var hashed = HashPassword.Password(dto.Password);
    if (user.PasswordHash != hashed) return Results.Unauthorized();

    var taskInstance = await db.ToDoItems.FindAsync(instanceId);
    if (taskInstance is null) return Results.NotFound("Görev bulunamadı.");
    if (taskInstance.UserId != dto.UserId) return Results.Forbid();

    // 2. Ana Şablonun ID'sini Bul (Değişiklik yok)
    int? templateIdToDelete = taskInstance.IsTemplate ? taskInstance.Id : taskInstance.ParentTaskId;
    if (templateIdToDelete is null)
    {
        // Eğer bir şablona bağlı değilse, sadece o tek görevi sil ve çık.
        db.ToDoItems.Remove(taskInstance);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    // --- YENİ SİLME MANTIĞI ---
    // 3. Silinecek Görevleri Belirle
    var today = DateTime.UtcNow.Date;

    // Ana şablonu VE bugüne veya geleceğe ait olan tüm kopyaları bul.
    // Geçmişteki kopyalara (eskiler) dokunma.
    var tasksToDelete = await db.ToDoItems
        .Where(t => 
            t.Id == templateIdToDelete || // Ana şablonun kendisi
            (t.ParentTaskId == templateIdToDelete && t.CreatedAt.Date >= today) // Bugünden itibaren oluşturulmuş kopyalar
        )
        .ToListAsync();

    // 4. Bulunan Görevleri Sil
    if (tasksToDelete.Any())
    {
        db.ToDoItems.RemoveRange(tasksToDelete);
        await db.SaveChangesAsync();
    }

    return Results.NoContent();
});

// AuthController gibi diğer controller'ları etkinleştirir
app.MapControllers();

app.Run();
