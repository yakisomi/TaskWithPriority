using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TaskWithPriorityApp;

namespace TaskWithPriorityApp.Tests
{
    [TestClass]
    public class TaskManagerWithPriorityTests
    {
        // Очищает тестовый файл перед каждым тестом
        [TestInitialize]
        public void SetUp()
        {
            if (File.Exists("tasks.txt"))
                File.Delete("tasks.txt");
        }

        // Удаляет тестовый файл после каждого теста
        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists("tasks.txt"))
                File.Delete("tasks.txt");
        }

        // Constructor создаёт пустой список задач
        [TestMethod]
        public void Constructor_InitializesEmptyList()
        {
            var manager = new TaskManagerWithPriority();

            Assert.IsNotNull(manager.Tasks);
            Assert.AreEqual(0, manager.Tasks.Count);
        }

        // AddTask успешно добавляет задачу в коллекцию
        [TestMethod]
        public void AddTask_AddsSuccessfully()
        {
            var manager = new TaskManagerWithPriority();
            var task = new TaskWithPriority("Новая задача", Priority.Средний, DateTime.Now);

            manager.AddTask(task);

            Assert.AreEqual(1, manager.Tasks.Count);
            CollectionAssert.Contains(manager.Tasks, task);
        }

        // AddTask выбрасывает ArgumentNullException при передаче null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddTask_Null_ThrowsException()
        {
            var manager = new TaskManagerWithPriority();

            manager.AddTask(null);
        }

        // RemoveTask успешно удаляет задачу из коллекции
        [TestMethod]
        public void RemoveTask_RemovesSuccessfully()
        {
            var manager = new TaskManagerWithPriority();
            var task = new TaskWithPriority("Удалить", Priority.Низкий, DateTime.Now);
            manager.AddTask(task);

            manager.RemoveTask(task);

            Assert.AreEqual(0, manager.Tasks.Count);
        }

        // RemoveTask выбрасывает ArgumentNullException при передаче null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveTask_Null_ThrowsException()
        {
            var manager = new TaskManagerWithPriority();

            manager.RemoveTask(null);
        }

        // ToggleTaskCompletion переключает статус задачи с false на true
        [TestMethod]
        public void ToggleTaskCompletion_TogglesToTrue()
        {
            var manager = new TaskManagerWithPriority();
            var task = new TaskWithPriority("Переключить", Priority.Высокий, DateTime.Now);
            manager.AddTask(task);
            Assert.IsFalse(task.IsCompleted);

            manager.ToggleTaskCompletion(task);

            Assert.IsTrue(task.IsCompleted);
        }

        // ToggleTaskCompletion переключает статус задачи с true на false
        [TestMethod]
        public void ToggleTaskCompletion_TogglesToFalse()
        {
            var manager = new TaskManagerWithPriority();
            var task = new TaskWithPriority("Переключить", Priority.Высокий, DateTime.Now);
            manager.AddTask(task);
            manager.ToggleTaskCompletion(task);
            Assert.IsTrue(task.IsCompleted);

            manager.ToggleTaskCompletion(task);

            Assert.IsFalse(task.IsCompleted);
        }

        // ToggleTaskCompletion выбрасывает ArgumentNullException при передаче null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToggleTaskCompletion_Null_ThrowsException()
        {
            var manager = new TaskManagerWithPriority();

            manager.ToggleTaskCompletion(null);
        }

        // SortTasksByPriority сортирует задачи по убыванию приоритета
        [TestMethod]
        public void SortTasksByPriority_SortsCorrectly()
        {
            var manager = new TaskManagerWithPriority();
            manager.AddTask(new TaskWithPriority("Low", Priority.Низкий, DateTime.Now));
            manager.AddTask(new TaskWithPriority("High", Priority.Высокий, DateTime.Now));
            manager.AddTask(new TaskWithPriority("Medium", Priority.Средний, DateTime.Now));

            var sorted = manager.SortTasksByPriority();

            Assert.AreEqual(3, sorted.Count);
            Assert.AreEqual(Priority.Высокий, sorted[0].Priority);
            Assert.AreEqual(Priority.Средний, sorted[1].Priority);
            Assert.AreEqual(Priority.Низкий, sorted[2].Priority);
        }
    }
}