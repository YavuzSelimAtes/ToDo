using ToDoApi.Data;
using ToDoApi.Models;
using Microsoft.EntityFrameworkCore;
using System;

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

        public async Task ProcessDailyTasks()
        {
            // --- GEÇİCİ DEBUG LOG 1: TRIGGER BAŞLANGICI ---
            _logger.LogWarning("========== HANGFIRE TRIGGER KONTROLÜ BAŞLADI ==========");

            // --- TEST İÇİN TARİH SİMÜLASYONU ---
            // Test yapmak için aşağıdaki satırlardan BİRİNİ seçip diğerlerini yorum satırı yapın.
            // Test bittiğinde ORİJİNAL KOD'u tekrar aktif etmeyi UNUTMAYIN!
            
            //var today = DateTime.UtcNow.Date; // <<-- ORİJİNAL KOD (Normal çalışma için bu satır aktif olmalı)
            //var today = DateTime.UtcNow.Date.AddDays(1);    // YARINI test etmek için bu satırı kullanın
            //var today = DateTime.UtcNow.Date.AddDays(14);    // 1 HAFTA SONRASINI test etmek için bu satırı kullanın
            var today = DateTime.UtcNow.Date.AddMonths(2);  // 1 AY SONRASINI test etmek için bu satırı kullanın

            _logger.LogInformation("Görev durumu işleme süreci başladı. Simüle edilen tarih (UTC): {Today}", today);

            // --- 1. ADIM: YENİ GÖREV KOPYALARINI OLUŞTUR (DEĞİŞİKLİK YOK) ---
            await CreateNewInstances(today);

            // --- 2. ADIM (HATA DÜZELTMESİ): AKTİF OLMASI GEREKEN GÖREVLERİ BELİRLE ---
            var yesterday = today.AddDays(-1);
            var sevenDaysAgo = today.AddDays(-7);
            var oneMonthAgo = today.AddMonths(-1);

            var idsThatShouldBeOpen = await _db.ToDoItems
                .AsNoTracking()
                .Where(t => !t.IsTemplate)
                .Where(t =>
                    (t.Category == "Günlük" && t.CreatedAt.Date == yesterday) ||
                    (t.Category == "Haftalık" && t.CreatedAt.Date <= sevenDaysAgo && t.CreatedAt.Date > sevenDaysAgo.AddDays(-7)) ||
                    (t.Category == "Aylık" && t.CreatedAt.Date <= oneMonthAgo && t.CreatedAt.Date > oneMonthAgo.AddMonths(-1))
                )
                .Select(t => t.Id)
                .ToListAsync();

            var shouldBeOpenIds = new HashSet<int>(idsThatShouldBeOpen);

            // --- GEÇİCİ DEBUG LOG 2: BULUNAN GÖREVLER ---
            if (shouldBeOpenIds.Any())
            {
                _logger.LogInformation("Açık duruma getirilmesi gereken {Count} adet görev ID'si bulundu: {Ids}", shouldBeOpenIds.Count, string.Join(", ", shouldBeOpenIds));
            }
            else
            {
                _logger.LogInformation("Bugün 'Açık' duruma getirilecek herhangi bir görev bulunamadı.");
            }

            // --- 3. ADIM: GÖREVLERİ GÜNCELLE (AÇMA VE KAPATMA) ---
            var tasksToOpen = await _db.ToDoItems
                .Where(t => !t.IsTemplate && t.State == State.Kapali && shouldBeOpenIds.Contains(t.Id))
                .ToListAsync();
                
            if (tasksToOpen.Any())
            {
                foreach (var task in tasksToOpen)
                {
                    task.State = State.Acik;
                }
                _logger.LogInformation($"{tasksToOpen.Count} adet görev 'Açık' duruma getirildi.");
            }

            var tasksToClose = await _db.ToDoItems
                .Where(t => !t.IsTemplate && t.State == State.Acik && !shouldBeOpenIds.Contains(t.Id))
                .ToListAsync();

            if (tasksToClose.Any())
            {
                foreach (var task in tasksToClose)
                {
                    task.State = State.Kapali;
                }
                _logger.LogInformation($"{tasksToClose.Count} adet 'Açık' görev süresi dolduğu için kilitlendi.");
            }

            await _db.SaveChangesAsync();

            // --- GEÇİCİ DEBUG LOG 3: TRIGGER BİTİŞİ ---
            _logger.LogInformation("Görev durumu işleme süreci tamamlandı.");
            _logger.LogWarning("========== HANGFIRE TRIGGER KONTROLÜ BİTTİ ==========");
        }
        
        private async Task CreateNewInstances(DateTime today)
{
    async Task CreateIfNotExists(ToDoItem template, DateTime creationDate)
    {
        var startOfDayUtc = new DateTime(creationDate.Year, creationDate.Month, creationDate.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDayUtc = startOfDayUtc.AddDays(1);

        var instanceExists = await _db.ToDoItems.AnyAsync(t =>
            t.ParentTaskId == template.Id &&
            t.CreatedAt >= startOfDayUtc && t.CreatedAt < endOfDayUtc);

        if (!instanceExists)
        {
            var newInstance = CreateInstanceFromTemplate(template, startOfDayUtc);
            _db.ToDoItems.Add(newInstance);
            _logger.LogInformation("Yeni {Category} görevi oluşturuldu: {Title}", template.Category, template.Title);
        }
    }

    // 🔹 Günlük tekrar
    var dailyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Günlük").ToListAsync();
    foreach (var template in dailyTemplates)
    {
        await CreateIfNotExists(template, today);
    }

    // 🔹 Haftalık tekrar
    var weeklyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Haftalık").ToListAsync();
    foreach (var template in weeklyTemplates)
    {
if (template.RepeatDayOfWeek.HasValue)
{
    var repeatDay = (DayOfWeek)template.RepeatDayOfWeek.Value;

    // Bu haftanın tekrar günü (örn: Pazartesi ise, bu haftanın Pazartesi tarihi)
    int diff = ((int)today.DayOfWeek - (int)repeatDay + 7) % 7;
    var thisWeeksRepeatDate = today.AddDays(-diff).Date;

    await CreateIfNotExists(template, thisWeeksRepeatDate);
}
    }

    // 🔹 Aylık tekrar
var monthlyTemplates = await _db.ToDoItems
    .Where(t => t.IsTemplate && t.Category == "Aylık")
    .ToListAsync();

foreach (var template in monthlyTemplates)
{
    if (!template.RepeatDayOfMonth.HasValue) continue;

    int repeatDay = template.RepeatDayOfMonth.Value;
    int lastDayOfCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);

    // Bu ay için hedef tekrar günü
    int targetDay = repeatDay > lastDayOfCurrentMonth ? lastDayOfCurrentMonth : repeatDay;
    var thisMonthsRepeatDate = new DateTime(today.Year, today.Month, targetDay, 0, 0, 0, DateTimeKind.Utc);

    // Eğer bu ay için henüz instance oluşturulmamışsa, şimdi oluştur
    await CreateIfNotExists(template, thisMonthsRepeatDate);
}


    await _db.SaveChangesAsync();
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

