using ToDoApi.Data;
using ToDoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ToDoApi.Services
{
    public class TaskStateService
    {
        private readonly ILogger<TaskStateService> _logger;
        private readonly ToDoContext _db;

        public TaskStateService(ToDoContext db, ILogger<TaskStateService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Bu metot, Hangfire tarafından her gün otomatik olarak çalıştırılacak
        public async Task ProcessDailyTasks()
        {
            _logger.LogInformation("Günlük görev işleme süreci başladı.");
            var today = DateTime.UtcNow.Date;

            // --- 1. YENİ KOPYALARI OLUŞTURMA ---
            await CreateNewInstances(today);

            // --- 2. DÜNKÜ GÖREVLERİ "AÇIK" YAPMA ---
            var yesterday = today.AddDays(-1);
            var tasksToOpen = await _db.ToDoItems
                .Where(t => t.IsTemplate == false && t.State == State.Kapali && t.CreatedAt.Date == yesterday)
                .ToListAsync();

            foreach (var task in tasksToOpen)
            {
                task.State = State.Acik;
            }
            _logger.LogInformation($"{tasksToOpen.Count} adet görev 'Açık' duruma getirildi.");

            // --- 3. DAHA ESKİ "AÇIK" GÖREVLERİ "KAPALI" YAPMA (KİLİTLEME) ---
            var tasksToClose = await _db.ToDoItems
                .Where(t => t.IsTemplate == false && t.State == State.Acik && t.CreatedAt.Date < yesterday)
                .ToListAsync();
                
            foreach (var task in tasksToClose)
            {
                task.State = State.Kapali;
            }
            _logger.LogInformation($"{tasksToClose.Count} adet görev 'Kapalı' duruma getirilerek kilitlendi.");

            await _db.SaveChangesAsync();
            _logger.LogInformation("Günlük görev işleme süreci tamamlandı.");
        }
        
        private async Task CreateNewInstances(DateTime today)
        {
            // Günlük şablonları bul ve kopyalarını oluştur
            var dailyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Günlük").ToListAsync();
            foreach (var template in dailyTemplates)
            {
                var newInstance = CreateInstanceFromTemplate(template, today);
                _db.ToDoItems.Add(newInstance);
            }
             _logger.LogInformation($"{dailyTemplates.Count} adet yeni günlük görev oluşturuldu.");

            // Eğer bugün Pazartesi ise, haftalık şablonların kopyalarını oluştur
            if (today.DayOfWeek == DayOfWeek.Monday)
            {
                var weeklyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Haftalık").ToListAsync();
                foreach (var template in weeklyTemplates)
                {
                    var newInstance = CreateInstanceFromTemplate(template, today);
                    _db.ToDoItems.Add(newInstance);
                }
                _logger.LogInformation($"{weeklyTemplates.Count} adet yeni haftalık görev oluşturuldu.");
            }

            // Eğer bugün ayın 1'i ise, aylık şablonların kopyalarını oluştur
            if (today.Day == 1)
            {
                var monthlyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Aylık").ToListAsync();
                foreach (var template in monthlyTemplates)
                {
                    var newInstance = CreateInstanceFromTemplate(template, today);
                    _db.ToDoItems.Add(newInstance);
                }
                 _logger.LogInformation($"{monthlyTemplates.Count} adet yeni aylık görev oluşturuldu.");
            }
        }
        
        private ToDoItem CreateInstanceFromTemplate(ToDoItem template, DateTime creationDate)
        {
            return new ToDoItem
            {
                Title = template.Title,
                Category = template.Category,
                CreatedAt = creationDate,
                State = State.Kapali,
                IsTemplate = false,
                ParentTaskId = template.Id,
                UserId = template.UserId
            };
        }
    }
}