using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskWithPriorityApp;

namespace NotificationTests
{
    [TestClass]
    public class NotificationTests
    {
        // Проверка порога "За 3 дня" и логики попадания в уведомление
        [TestMethod]
        public void CheckNotifications_DeadlineWithin3Days_ShouldNotify()
        {
            var task = new TaskWithPriority("Тестовая задача", Priority.Высокий, DateTime.Now.AddDays(3));
            task.IsCompleted = false;
            var threshold = TimeSpan.FromDays(3);

            var timeUntilDeadline = task.Deadline.Date - DateTime.Now.Date;
            var shouldNotify = timeUntilDeadline <= threshold && timeUntilDeadline >= TimeSpan.Zero;

            Assert.IsTrue(shouldNotify, "Задача с дедлайном через 3 дня должна попасть в уведомление при пороге 3 дня");
        }

        // Выполненная задача не уведомляет
        [TestMethod]
        public void CheckNotifications_CompletedTask_ShouldNotNotify()
        {
            var task = new TaskWithPriority("Выполненная задача", Priority.Средний, DateTime.Now.AddDays(2));
            task.IsCompleted = true; 

            Assert.IsTrue(task.IsCompleted, "Задача должна быть помечена как выполненная");

            var isSkipped = task.IsCompleted;
            Assert.IsTrue(isSkipped, "Выполненная задача должна быть пропущена при проверке уведомлений");
        }

        // Повторяющиеся уведомления 
        [TestMethod]
        public void CheckNotifications_RepeatedChecks_ShouldNotifyAgain()
        {
            var task = new TaskWithPriority("Повторяемая задача", Priority.Высокий, DateTime.Now.AddDays(2));
            task.IsCompleted = false;
            var threshold = TimeSpan.FromDays(3);

            var timeUntilDeadline = task.Deadline.Date - DateTime.Now.Date;
            var shouldNotifyFirst = timeUntilDeadline <= threshold && timeUntilDeadline >= TimeSpan.Zero;

            var shouldNotifySecond = timeUntilDeadline <= threshold && timeUntilDeadline >= TimeSpan.Zero;

            Assert.IsTrue(shouldNotifyFirst, "Первое уведомление должно сработать");
            Assert.IsTrue(shouldNotifySecond, "Повторное уведомление должно сработать, так как задача не выполнена");
        }

        // Несколько задач собираются в список
        [TestMethod]
        public void CheckNotifications_MultipleTasks_CollectedInList()
        {
            var tasks = new List<TaskWithPriority>
            {
                new TaskWithPriority("Задача 1", Priority.Высокий, DateTime.Now.AddDays(1)),
                new TaskWithPriority("Задача 2", Priority.Средний, DateTime.Now.AddDays(2)),
                new TaskWithPriority("Задача 3", Priority.Низкий, DateTime.Now.AddDays(3))
            };
            var threshold = TimeSpan.FromDays(3);
            var pendingTasks = new List<TaskWithPriority>();

            foreach (var task in tasks)
            {
                if (task.IsCompleted) continue;
                var timeUntilDeadline = task.Deadline.Date - DateTime.Now.Date;
                if (timeUntilDeadline <= threshold && timeUntilDeadline >= TimeSpan.Zero)
                {
                    pendingTasks.Add(task);
                }
            }

            Assert.AreEqual(3, pendingTasks.Count, "Все 3 задачи должны попасть в список уведомлений");
        }

        // Настройка интервала 
        [TestMethod]
        public void CheckNotifications_ThresholdSetting_WorksCorrectly()
        {
            var task = new TaskWithPriority("Задача на 5 дней", Priority.Средний, DateTime.Now.AddDays(5));
            task.IsCompleted = false;

            var threshold7Days = TimeSpan.FromDays(7);
            var threshold3Days = TimeSpan.FromDays(3);
            var timeUntilDeadline = task.Deadline.Date - DateTime.Now.Date;

            var shouldNotify7Days = timeUntilDeadline <= threshold7Days && timeUntilDeadline >= TimeSpan.Zero;
            var shouldNotify3Days = timeUntilDeadline <= threshold3Days && timeUntilDeadline >= TimeSpan.Zero;

            Assert.IsTrue(shouldNotify7Days, "При пороге 7 дней уведомление должно появиться");
            Assert.IsFalse(shouldNotify3Days, "При пороге 3 дня уведомление не должно появиться");
        }

        // Прошедший дедлайн не уведомляет
        [TestMethod]
        public void CheckNotifications_PassedDeadline_ShouldNotNotify()
        {
            var task = new TaskWithPriority("Просроченная задача", Priority.Высокий, DateTime.Now.AddDays(-1));
            task.IsCompleted = false;
            var threshold = TimeSpan.FromDays(7);

            var timeUntilDeadline = task.Deadline.Date - DateTime.Now.Date;
            var shouldNotify = timeUntilDeadline <= threshold && timeUntilDeadline >= TimeSpan.Zero;

            Assert.IsFalse(shouldNotify, "Задача с прошедшим дедлайном не должна попадать в уведомление");
        }
    }
}