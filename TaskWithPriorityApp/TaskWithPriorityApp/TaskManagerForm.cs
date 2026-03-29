using System;
using System.Drawing;
using System.Windows.Forms;

namespace TaskWithPriorityApp
{
    public class TaskManagerForm : Form
    {
        private readonly TaskManagerWithPriority _taskManager;
        private readonly TextBox _descriptionTextBox;
        private readonly ComboBox _priorityComboBox;
        private readonly DateTimePicker _deadlinePicker;
        private readonly Button _addButton;
        private readonly Button _removeButton;
        private readonly Button _toggleButton;
        private readonly Button _sortButton;
        private readonly ListBox _tasksListBox;

        public TaskManagerForm()
        {
            Text = "Управление задачами с приоритетом (Вариант 9)";
            Width = 600;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;

            _taskManager = new TaskManagerWithPriority();

            _descriptionTextBox = new TextBox
            {
                Location = new Point(10, 10),
                Width = 200,
                Text = "Описание"
            };

            _priorityComboBox = new ComboBox
            {
                Location = new Point(220, 10),
                Width = 100,
                Items = { "Низкий", "Средний", "Высокий" },
                SelectedIndex = 1
            };

            _deadlinePicker = new DateTimePicker
            {
                Location = new Point(330, 10)
            };

            _addButton = new Button
            {
                Location = new Point(10, 40),
                Width = 100,
                Text = "Добавить"
            };
            _addButton.Click += OnAddClick;

            _removeButton = new Button
            {
                Location = new Point(120, 40),
                Width = 100,
                Text = "Удалить"
            };
            _removeButton.Click += OnRemoveClick;

            _toggleButton = new Button
            {
                Location = new Point(220, 40),
                Width = 100,
                Text = "Отметить"
            };
            _toggleButton.Click += OnToggleClick;

            _sortButton = new Button
            {
                Location = new Point(330, 40),
                Width = 100,
                Text = "Сортировать"
            };
            _sortButton.Click += OnSortClick;

            _tasksListBox = new ListBox
            {
                Location = new Point(10, 80),
                Width = 560,
                Height = 350
            };

            Controls.AddRange(new Control[]
            {
                _descriptionTextBox, _priorityComboBox, _deadlinePicker,
                _addButton, _removeButton, _toggleButton, _sortButton, _tasksListBox
            });

            UpdateTasksList();
        }

        private void OnAddClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_descriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание задачи!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var priority = (Priority)Enum.Parse(typeof(Priority), _priorityComboBox.SelectedItem.ToString());
                _taskManager.AddTask(new TaskWithPriority(_descriptionTextBox.Text, priority, _deadlinePicker.Value));
                _descriptionTextBox.Clear();
                UpdateTasksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRemoveClick(object sender, EventArgs e)
        {
            if (_tasksListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите задачу для удаления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var desc = GetDescriptionFromSelectedItem();
                var task = _taskManager.Tasks.Find(t => t.Description == desc);
                if (task != null)
                {
                    _taskManager.RemoveTask(task);
                    UpdateTasksList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnToggleClick(object sender, EventArgs e)
        {
            if (_tasksListBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите задачу!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var desc = GetDescriptionFromSelectedItem();
                var task = _taskManager.Tasks.Find(t => t.Description == desc);
                if (task != null)
                {
                    _taskManager.ToggleTaskCompletion(task);
                    UpdateTasksList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            _tasksListBox.Items.Clear();
            foreach (var task in _taskManager.SortTasksByPriority())
            {
                _tasksListBox.Items.Add(FormatTask(task));
            }
        }

        private void UpdateTasksList()
        {
            _tasksListBox.Items.Clear();
            foreach (var task in _taskManager.Tasks)
                _tasksListBox.Items.Add(FormatTask(task));
        }

        private string FormatTask(TaskWithPriority t) =>
            $"{(t.IsCompleted ? "[X]" : "[ ]")} {t.Description} (Приоритет: {t.Priority})";


        private string GetDescriptionFromSelectedItem()
        {
            var selected = _tasksListBox.SelectedItem.ToString();

            if (selected.StartsWith("["))
            {
                var bracketEnd = selected.IndexOf(']');
                if (bracketEnd >= 0 && selected.Length > bracketEnd + 2)
                {
                    selected = selected.Substring(bracketEnd + 2);
                }
            }

            var priorityIndex = selected.IndexOf(" (Приоритет:");
            if (priorityIndex > 0)
            {
                selected = selected.Substring(0, priorityIndex);
            }

            return selected;
        }
    }
}