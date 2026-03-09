using System;

namespace TaskWithPriorityApp
{
    public enum Priority
    {
        Низкий,
        Средний,
        Высокий
    }

    public class TaskWithPriority
    {
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime Deadline { get; set; }

        public TaskWithPriority(string description, Priority priority, DateTime deadline)
        {
            Description = description;
            Priority = priority;
            IsCompleted = false;
            Deadline = deadline;
        }
    }
}