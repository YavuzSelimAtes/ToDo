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
            //var today = DateTime.UtcNow.Date; // <<-- ORİJİNAL KOD (Normal çalışma için bu satır aktif olmalı)
            var today = DateTime.UtcNow.Date.AddDays(2);    // YARINI test etmek için bu satırı kullanın
            //var today = DateTime.UtcNow.Date.AddDays(14);    // 1 HAFTA SONRASINI test etmek için bu satırı kullanın
            //var today = DateTime.UtcNow.Date.AddMonths(1);  // 1 AY SONRASINI test etmek için bu satırı kullanın

            await CreateNewInstances(today);

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

            var tasksToOpen = await _db.ToDoItems
                .Where(t => !t.IsTemplate && t.State == State.Kapali && shouldBeOpenIds.Contains(t.Id))
                .ToListAsync();

            if (tasksToOpen.Any())
            {
                foreach (var task in tasksToOpen)
                {
                    task.State = State.Acik;
                }
            }

            var tasksToFail = await _db.ToDoItems
                .Where(t => !t.IsTemplate && t.State == State.Acik && !shouldBeOpenIds.Contains(t.Id))
                .Include(t => t.User)
                .ToListAsync();

            if (tasksToFail.Any())
            {
                foreach (var task in tasksToFail)
                {
                    task.State = State.Basarisiz;

                    if (task.User != null)
                    {
                        switch (task.Category)
                        {
                            case "Günlük":
                                task.User.DailyTasksFailed++;
                                task.User.Score -= 3;
                                break;
                            case "Haftalık":
                                task.User.WeeklyTasksFailed++;
                                task.User.Score -= 9;
                                break;
                            case "Aylık":
                                task.User.MonthlyTasksFailed++;
                                task.User.Score -= 30;
                                break;
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

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

            var dailyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Günlük").ToListAsync();
            foreach (var template in dailyTemplates)
            {
                await CreateIfNotExists(template, today);
            }

            var weeklyTemplates = await _db.ToDoItems.Where(t => t.IsTemplate && t.Category == "Haftalık").ToListAsync();
            foreach (var template in weeklyTemplates)
            {
                if (template.RepeatDayOfWeek.HasValue)
                {
                    var repeatDay = (DayOfWeek)template.RepeatDayOfWeek.Value;

                    int diff = ((int)today.DayOfWeek - (int)repeatDay + 7) % 7;
                    var thisWeeksRepeatDate = today.AddDays(-diff).Date;

                    await CreateIfNotExists(template, thisWeeksRepeatDate);
                }
            }

            var monthlyTemplates = await _db.ToDoItems
                .Where(t => t.IsTemplate && t.Category == "Aylık")
                .ToListAsync();

            foreach (var template in monthlyTemplates)
            {
                if (!template.RepeatDayOfMonth.HasValue) continue;

                int repeatDay = template.RepeatDayOfMonth.Value;
                int lastDayOfCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);

                int targetDay = repeatDay > lastDayOfCurrentMonth ? lastDayOfCurrentMonth : repeatDay;
                var thisMonthsRepeatDate = new DateTime(today.Year, today.Month, targetDay, 0, 0, 0, DateTimeKind.Utc);

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

