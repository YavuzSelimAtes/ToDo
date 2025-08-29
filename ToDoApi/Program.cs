using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;
using ToDoApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseCors("open");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Todo API is running");

// --- YENİ TODOITEM ENDPOINT'LERİ ---

// Bir kullanıcıya ait görevleri getir (kategoriye göre filtrelenebilir)
app.MapGet("/api/users/{userId}/todos", async (int userId, [FromQuery] string? category, ToDoContext db) =>
{
    var query = db.ToDoItems.Where(t => t.UserId == userId);

    // Eğer bir kategori belirtilmişse, sadece o kategorideki görevleri filtrele
    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(t => t.Category == category);
    }

    return await query.AsNoTracking().ToListAsync();
});

// Bir kullanıcı için yeni bir görev oluştur
app.MapPost("/api/users/{userId}/todos", async (int userId, [FromBody] ToDoItem newTodo, ToDoContext db) =>
{
    if (string.IsNullOrWhiteSpace(newTodo.Title) || string.IsNullOrWhiteSpace(newTodo.Category))
    {
        return Results.BadRequest("Başlık ve kategori alanları boş olamaz.");
    }

    var todoToAdd = new ToDoItem
    {
        Title = newTodo.Title,
        Category = newTodo.Category,
        IsCompleted = false,
        UserId = userId
    };
    db.ToDoItems.Add(todoToAdd);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{todoToAdd.Id}", todoToAdd);
});

app.MapDelete("/api/todos/{id:int}", async (int id, [FromBody] DeleteTaskDto dto, ToDoContext db) =>
{
    // Önce şifresini kontrol edeceğimiz kullanıcıyı bulalım
    var user = await db.Users.FindAsync(dto.UserId);
    if (user is null)
    {
        return Results.NotFound("Kullanıcı bulunamadı.");
    }

    // Gönderilen şifreyi hash'le ve veritabanındakiyle karşılaştır
    var hashed = HashPassword.Password(dto.Password);
    if (user.PasswordHash != hashed)
    {
        return Results.Unauthorized(); // Şifre yanlışsa yetkisiz hatası ver
    }

    // Şifre doğruysa, silinmek istenen görevi bul
    var task = await db.ToDoItems.FindAsync(id);
    if (task is null)
    {
        return Results.NotFound("Görev bulunamadı.");
    }

    // Görevin, şifresini doğruladığımız kullanıcıya ait olduğundan emin ol (ekstra güvenlik)
    if (task.UserId != dto.UserId)
    {
        return Results.Forbid();
    }

    // Her şey yolundaysa görevi sil
    db.ToDoItems.Remove(task);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

// AuthController gibi diğer controller'ları etkinleştirir
app.MapControllers();

app.Run();
