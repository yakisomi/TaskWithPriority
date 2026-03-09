using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskWithPriorityApp
{
    public class TaskManagerWithPriority
    {
        public List<TaskWithPriority> Tasks { get; private set; }

        public TaskManagerWithPriority()
        {
            Tasks = new List<TaskWithPriority>();
            LoadTasks();
        }

        public void AddTask(TaskWithPriority task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Tasks.Add(task);
            SaveTasks();
        }

        public void RemoveTask(TaskWithPriority task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Tasks.Remove(task);
            SaveTasks();
        }

        public void ToggleTaskCompletion(TaskWithPriority task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.IsCompleted = !task.IsCompleted;
            SaveTasks();
        }

        public List<TaskWithPriority> SortTasksByPriority()
        {
            return Tasks.OrderByDescending(t => t.Priority).ToList();
        }

        private void SaveTasks()
        {
            var lines = Tasks.Select(t =>
                $"{t.Description}|{(int)t.Priority}|{t.IsCompleted}|{t.Deadline:yyyy-MM-dd HH:mm:ss}");
            File.WriteAllLines("tasks.txt", lines);
        }

        private void LoadTasks()
        {
            if (!File.Exists("tasks.txt")) return;

            foreach (var line in File.ReadAllLines("tasks.txt"))
            {
                var parts = line.Split('|');
                if (parts.Length == 4 &&
                    int.TryParse(parts[1], out int priority) &&
                    bool.TryParse(parts[2], out bool isCompleted) &&
                    DateTime.TryParse(parts[3], out DateTime deadline))
                {
                    var task = new TaskWithPriority(parts[0], (Priority)priority, deadline);
                    task.IsCompleted = isCompleted;
                    Tasks.Add(task);
                }
            }
        }
    }
}