using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TaskWithPriorityApp;

namespace TaskWithPriorityApp.Tests
{
    [TestClass]
    public class TaskWithPriorityTests
    {

        // Constructor корректно устанавливает все свойства задачи
        [TestMethod]
        public void Constructor_SetsPropertiesCorrectly()
        {

            var description = "Тестовая задача";
            var priority = Priority.Высокий;
            var deadline = new DateTime(2024, 12, 31);

            var task = new TaskWithPriority(description, priority, deadline);

            Assert.AreEqual(description, task.Description);
            Assert.AreEqual(priority, task.Priority);
            Assert.AreEqual(deadline, task.Deadline);
            Assert.IsFalse(task.IsCompleted);
        }

        // Constructor разрешает пустую строку в описании
        [TestMethod]
        public void Constructor_WithEmptyDescription_Allowed()
        {

            var task = new TaskWithPriority("", Priority.Средний, DateTime.Now);

            Assert.AreEqual("", task.Description);
        }

        // Constructor разрешает null в описании
        [TestMethod]
        public void Constructor_WithNullDescription_Allowed()
        {
            var task = new TaskWithPriority(null, Priority.Низкий, DateTime.Now);

            Assert.IsNull(task.Description);
        }

        // IsCompleted по умолчанию имеет значение false
        [TestMethod]
        public void IsCompleted_DefaultValueIsFalse()
        {
            var task = new TaskWithPriority("Задача", Priority.Средний, DateTime.Now);

            Assert.IsFalse(task.IsCompleted);
        }

        // IsCompleted можно изменить на true
        [TestMethod]
        public void IsCompleted_CanBeSetToTrue()
        {
            var task = new TaskWithPriority("Задача", Priority.Средний, DateTime.Now);

            task.IsCompleted = true;

            Assert.IsTrue(task.IsCompleted);
        }

        // IsCompleted можно изменить на false
        [TestMethod]
        public void IsCompleted_CanBeSetToFalse()
        {
            var task = new TaskWithPriority("Задача", Priority.Средний, DateTime.Now);
            task.IsCompleted = true;

            task.IsCompleted = false;

            Assert.IsFalse(task.IsCompleted);
        }

        //Priority можно изменить после создания задачи
        [TestMethod]
        public void Priority_CanBeChanged()
        {
            var task = new TaskWithPriority("Задача", Priority.Низкий, DateTime.Now);

            task.Priority = Priority.Высокий;

            Assert.AreEqual(Priority.Высокий, task.Priority);
        }

        // Description можно изменить после создания задачи
        [TestMethod]
        public void Description_CanBeChanged()
        {
            var task = new TaskWithPriority("Старое описание", Priority.Средний, DateTime.Now);

            task.Description = "Новое описание";

            Assert.AreEqual("Новое описание", task.Description);
        }

        // Deadline можно изменить после создания задачи
        [TestMethod]
        public void Deadline_CanBeChanged()
        {
            var task = new TaskWithPriority("Задача", Priority.Средний, DateTime.Now);
            var newDeadline = new DateTime(2025, 1, 1);

            task.Deadline = newDeadline;

            Assert.AreEqual(newDeadline, task.Deadline);
        }
    }
}