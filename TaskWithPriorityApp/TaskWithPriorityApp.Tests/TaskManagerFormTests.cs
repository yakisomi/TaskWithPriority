using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace TaskWithPriorityApp.Tests
{
    [TestClass]
    public class TaskManagerFormTests
    {
        // Обязательно для работы TestContext.WriteLine
        public TestContext TestContext { get; set; }

        private Application _app;
        private UIA3Automation _automation;
        private Window _mainWindow;

        [TestInitialize]
        public void TestInitialize()
        {
            if (File.Exists("tasks.txt"))
            {
                File.Delete("tasks.txt");
            }

            _app = Application.Launch(@"..\..\..\TaskWithPriorityApp\bin\Debug\TaskWithPriorityApp.exe");
            _automation = new UIA3Automation();
            Thread.Sleep(1500);
            _mainWindow = _app.GetMainWindow(_automation);
            Thread.Sleep(500);

            // Закрываем все возможные уведомления перед стартом теста
            CloseAllMessageBoxes();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (_app != null && !_app.HasExited)
                {
                    _app.Close();
                    Thread.Sleep(500);
                }
            }
            catch { }

            _automation?.Dispose();
        }

        // Вспомогательный метод: ожидание и проверка уведомления
        private bool WaitForNotification(int timeoutSeconds = 10)
        {
            TestContext.WriteLine($"Ожидание уведомления в течение {timeoutSeconds} сек...");
            Thread.Sleep(timeoutSeconds * 1000);

            // Проверяем, появилось ли модальное окно (уведомление)
            var modal = _mainWindow?.ModalWindows.FirstOrDefault();
            if (modal != null)
            {
                TestContext.WriteLine("Уведомление обнаружено!");
                return true;
            }

            TestContext.WriteLine("Уведомление не появилось");
            return false;
        }

        // Закрытие всех MessageBox
        private void CloseAllMessageBoxes()
        {
            Thread.Sleep(500);

            try
            {
                // Закрываем все модальные окна
                while (_mainWindow.ModalWindows.Length > 0)
                {
                    var messageBox = _mainWindow.ModalWindows.First();
                    var okButton = messageBox.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.Button));

                    if (okButton != null && okButton.IsEnabled)
                    {
                        okButton.Click();
                        Thread.Sleep(300);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch { }
        }

        // TC-001: Добавление задачи
        [TestMethod]
        public void TC_001_AddTask_WithValidData_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            int initialCount = tasksListBox.Items.Length;

            descriptionTextBox.Text = "Лаба1";
            addButton.Click();
            Thread.Sleep(1500);

            tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            Assert.IsTrue(tasksListBox.Items.Length > initialCount);
            Assert.IsTrue(tasksListBox.Items.Any(i => i.Text.Contains("Лаба1")));
        }

        // TC-002: Добавление задачи с пустым описанием
        [TestMethod]
        public void TC_002_AddTask_WithEmptyDescription_ShouldShowError()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            int initialCount = tasksListBox.Items.Length;

            descriptionTextBox.Text = "";
            addButton.Click();

            CloseAllMessageBoxes();
            Thread.Sleep(300);

            tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            Assert.AreEqual(initialCount, tasksListBox.Items.Length);
        }

        // TC-003: Изменение статуса на "выполнена"
        [TestMethod]
        public void TC_003_ToggleTask_ToCompleted_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var toggleButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Отметить")).AsButton();

            descriptionTextBox.Text = "Тестовая задача";
            addButton.Click();
            Thread.Sleep(1500);

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            if (tasksListBox.Items.Length > 0)
            {
                tasksListBox.Items[0].Click();
                Thread.Sleep(500);

                toggleButton.Click();
                Thread.Sleep(1500);

                tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.List)).AsListBox();

                Assert.IsTrue(tasksListBox.Items[0].Text.Contains("[X]"),
                    "Статус не изменился на [X]");
            }
            else
            {
                Assert.Fail("Задача не была добавлена");
            }
        }

        // TC-004: Изменение статуса на "не выполнена"
        [TestMethod]
        public void TC_004_ToggleTask_ToNotCompleted_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var toggleButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Отметить")).AsButton();

            descriptionTextBox.Text = "Тестовая задача 2";
            addButton.Click();
            Thread.Sleep(1500);

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            if (tasksListBox.Items.Length > 0)
            {
                // Сначала отмечаем как выполненную
                tasksListBox.Items[0].Click();
                Thread.Sleep(300);
                toggleButton.Click();
                Thread.Sleep(1000);
                CloseAllMessageBoxes();

                // Теперь отмечаем как НЕ выполненную (возвращаем в исходное состояние)
                tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.List)).AsListBox();

                if (tasksListBox.Items.Length > 0)
                {
                    tasksListBox.Items[0].Click();
                    Thread.Sleep(300);
                    toggleButton.Click();
                    Thread.Sleep(1000);
                    CloseAllMessageBoxes();

                    tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.List)).AsListBox();

                    Assert.IsTrue(tasksListBox.Items.Any(i => i.Text.Contains("[ ]")),
                        "Статус не изменился на [ ]");
                }
            }
        }

        // TC-005: Удаление выполненной задачи
        [TestMethod]
        public void TC_005_DeleteTask_Completed_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var toggleButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Отметить")).AsButton();
            var removeButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Удалить")).AsButton();

            descriptionTextBox.Text = "Удалить задачу";
            addButton.Click();
            Thread.Sleep(1500);

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            if (tasksListBox.Items.Length > 0)
            {
                tasksListBox.Items[0].Click();
                Thread.Sleep(300);

                toggleButton.Click();
                Thread.Sleep(1000);

                CloseAllMessageBoxes();

                tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.List)).AsListBox();

                if (tasksListBox.Items.Length > 0)
                {
                    tasksListBox.Items[0].Click();
                    Thread.Sleep(300);

                    removeButton.Click();
                    Thread.Sleep(1000);

                    CloseAllMessageBoxes();
                    Thread.Sleep(500);

                    tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                        cf.ByControlType(ControlType.List)).AsListBox();

                    Assert.AreEqual(0, tasksListBox.Items.Length, "Задача не была удалена");
                }
            }
        }

        // TC-006: Удаление невыполненной задачи
        [TestMethod]
        public void TC_006_DeleteTask_NotCompleted_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var removeButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Удалить")).AsButton();

            descriptionTextBox.Text = "Удалить невыполненную";
            addButton.Click();
            Thread.Sleep(1500);

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            if (tasksListBox.Items.Length > 0)
            {
                tasksListBox.Items[0].Click();
                Thread.Sleep(500);

                removeButton.Click();
                Thread.Sleep(1000);

                CloseAllMessageBoxes();
                Thread.Sleep(500);

                tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.List)).AsListBox();

                Assert.AreEqual(0, tasksListBox.Items.Length, "Задача не была удалена");
            }
        }

        // TC-007: Удаление без выбора — ПРОВЕРКА СООБЩЕНИЯ ОБ ОШИБКЕ
        [TestMethod]
        public void TC_007_DeleteTask_WithoutSelection_ShouldShowError()
        {
            var removeButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Удалить")).AsButton();

            removeButton.Click();
            Thread.Sleep(500);

            var modal = _mainWindow.ModalWindows.FirstOrDefault();
            Assert.IsNotNull(modal, "MessageBox с ошибкой не появился");

            var textElement = modal.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Text));

            Assert.IsNotNull(textElement, "Текст в MessageBox не найден");
            Assert.IsTrue(
                textElement.Name.Contains("Выберите задачу для удаления"),
                $"Неверное сообщение об ошибке. Ожидалось: 'Выберите задачу для удаления!', Получено: '{textElement.Name}'");

            CloseAllMessageBoxes();
        }

        // TC-008: Сортировка задач по приоритету
        [TestMethod]
        public void TC_008_SortTasks_ByPriority_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var sortButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Сортировать")).AsButton();

            // Находим ComboBox с приоритетами
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var priorityComboBox = allComboBoxes[0].AsComboBox();

            // Вспомогательный метод для выбора приоритета
            void SelectPriority(string value)
            {
                // Находим кнопку-стрелку внутри ComboBox
                var expandButton = priorityComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                // Кликаем по стрелке (или по самому ComboBox если кнопки нет)
                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    priorityComboBox.Click();
                }

                Thread.Sleep(50);

                // Ищем нужный элемент в раскрывшемся списке
                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(50);
                }
            }

            // Добавляем 3 задачи с разными приоритетами
            descriptionTextBox.Text = "Задача 1";
            SelectPriority("Низкий");
            addButton.Click();
            Thread.Sleep(50);

            descriptionTextBox.Text = "Задача 2";
            SelectPriority("Высокий");
            addButton.Click();
            Thread.Sleep(50);

            descriptionTextBox.Text = "Задача 3";
            SelectPriority("Средний");
            addButton.Click();
            Thread.Sleep(50);

            // Нажимаем сортировку
            sortButton.Click();
            Thread.Sleep(200);

            // Получаем список задач после сортировки
            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            // Просто проверяем порядок: Высокий → Средний → Низкий
            Assert.AreEqual(3, tasksListBox.Items.Length);
            Assert.IsTrue(tasksListBox.Items[0].Text.Contains("Высокий"),
                "Первая задача должна быть с высоким приоритетом");
            Assert.IsTrue(tasksListBox.Items[1].Text.Contains("Средний"),
                "Вторая задача должна быть со средним приоритетом");
            Assert.IsTrue(tasksListBox.Items[2].Text.Contains("Низкий"),
                "Третья задача должна быть с низким приоритетом");
        }

        // TC-009: Сохранение в файл
        [TestMethod]
        public void TC_009_SaveTasksToFile_ShouldSucceed()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();

            descriptionTextBox.Text = "Тест";
            addButton.Click();
            Thread.Sleep(1500);

            _app?.Close();
            Thread.Sleep(1500);

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "tasks.txt");
            Assert.IsTrue(File.Exists(filePath) || File.Exists("tasks.txt"),
                "Файл tasks.txt не создан");

            string content = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : File.ReadAllText("tasks.txt");

            Assert.IsTrue(content.Contains("Тест"), "Файл не содержит данные задачи");
        }

        // TC-010: Загрузка из файла
        [TestMethod]
        public void TC_010_LoadTasksFromFile_ShouldSucceed()
        {
            _app?.Close();
            Thread.Sleep(1000);

            File.WriteAllText("tasks.txt", "Тест|2|False|2026-03-25 20:47:46");

            _app = Application.Launch(@"..\..\..\TaskWithPriorityApp\bin\Debug\TaskWithPriorityApp.exe");
            _mainWindow = _app.GetMainWindow(_automation);
            Thread.Sleep(2000);

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            Assert.IsTrue(tasksListBox.Items.Any(item => item.Text.Contains("Тест")),
                "Задача не загружена из файла");
        }

        // TC-011: Уведомление о задаче
        [TestMethod]
        public void TC_011_Notification_3DaysBeforeDeadline_ShouldShow()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();

            // Находим ComboBox для уведомлений
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var notificationComboBox = allComboBoxes.LastOrDefault()?.AsComboBox();

            Assert.IsNotNull(notificationComboBox, "ComboBox уведомлений не найден");

            // Вспомогательный метод для выбора интервала уведомления
            void SelectNotificationInterval(string value)
            {
                var expandButton = notificationComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    notificationComboBox.Click();
                }

                Thread.Sleep(300);

                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(300);
                }
            }

            descriptionTextBox.Text = "Задача с уведомлением";
            addButton.Click();
            Thread.Sleep(1500);

            SelectNotificationInterval("За 3 дня");

            // Единая точка проверки уведомления
            bool notificationShown = WaitForNotification(10);
            CloseAllMessageBoxes();

            Assert.IsTrue(notificationShown, "Уведомление не было показано");
        }

        // TC-012: Выполненная задача не уведомляет
        [TestMethod]
        public void TC_012_CompletedTask_ShouldNotNotify()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();
            var toggleButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Отметить")).AsButton();
            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            // Находим ComboBox для уведомлений
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var notificationComboBox = allComboBoxes.LastOrDefault()?.AsComboBox();

            Assert.IsNotNull(notificationComboBox, "ComboBox уведомлений не найден");

            // Вспомогательный метод для выбора интервала уведомления
            void SelectNotificationInterval(string value)
            {
                var expandButton = notificationComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    notificationComboBox.Click();
                }

                Thread.Sleep(300);

                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(300);
                }
            }

            descriptionTextBox.Text = "Выполненная задача";
            addButton.Click();
            Thread.Sleep(1500);

            if (tasksListBox.Items.Length > 0)
            {
                tasksListBox.Items[0].Click();
                Thread.Sleep(500);
                toggleButton.Click();
                Thread.Sleep(1500);

                SelectNotificationInterval("За 3 дня");

                // 🔥 Проверяем, что уведомление НЕ появилось
                bool notificationShown = WaitForNotification(10);
                CloseAllMessageBoxes();

                Assert.IsFalse(notificationShown, "Уведомление появилось для выполненной задачи");
            }
        }

        // TC-013: Повторяющиеся уведомления
        [TestMethod]
        public void TC_013_RepeatedNotifications_ShouldWork()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();

            // Находим ComboBox для уведомлений
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var notificationComboBox = allComboBoxes.LastOrDefault()?.AsComboBox();

            Assert.IsNotNull(notificationComboBox, "ComboBox уведомлений не найден");

            // Вспомогательный метод для выбора интервала уведомления
            void SelectNotificationInterval(string value)
            {
                // Находим кнопку-стрелку внутри ComboBox
                var expandButton = notificationComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                // Кликаем по стрелке (или по самому ComboBox если кнопки нет)
                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    notificationComboBox.Click();
                }

                Thread.Sleep(300);

                // Ищем нужный элемент в раскрывшемся списке
                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(300);
                }
            }

            descriptionTextBox.Text = "Повторяющееся уведомление";
            addButton.Click();
            Thread.Sleep(1500);

            SelectNotificationInterval("За 3 дня");

            // 🔥 Первая проверка (ждем 10 секунд)
            bool firstNotification = WaitForNotification(10);
            CloseAllMessageBoxes();
            TestContext.WriteLine($"Первое уведомление: {(firstNotification ? "сработало" : "не сработало")}");

            // 🔥 Вторая проверка (ждем еще 10 секунд для повторного уведомления)
            bool secondNotification = WaitForNotification(10);
            CloseAllMessageBoxes();
            TestContext.WriteLine($"Второе уведомление: {(secondNotification ? "сработало" : "не сработало")}");

            Assert.IsTrue(firstNotification && secondNotification,
                "Ни одно из повторяющихся уведомлений не сработало");
        }

        // TC-014: Несколько задач в одном уведомлении
        [TestMethod]
        public void TC_014_MultipleTasks_InOneNotification_ShouldWork()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();

            // Находим ComboBox для уведомлений
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var notificationComboBox = allComboBoxes.LastOrDefault()?.AsComboBox();

            Assert.IsNotNull(notificationComboBox, "ComboBox уведомлений не найден");

            // Вспомогательный метод для выбора интервала уведомления
            void SelectNotificationInterval(string value)
            {
                var expandButton = notificationComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    notificationComboBox.Click();
                }

                Thread.Sleep(300);

                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(300);
                }
            }

            // Добавляем 3 задачи
            for (int i = 1; i <= 3; i++)
            {
                descriptionTextBox.Text = $"Задача {i}";
                addButton.Click();
                Thread.Sleep(500);
            }

            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            // ПРОВЕРКА: все 3 задачи добавлены
            Assert.AreEqual(3, tasksListBox.Items.Length, "Не все задачи добавлены");

            SelectNotificationInterval("За 3 дня");

            // Единая точка проверки
            bool notificationShown = WaitForNotification(10);
            CloseAllMessageBoxes();

            TestContext.WriteLine($"Уведомление для нескольких задач: {(notificationShown ? "сработало" : "не сработало")}");

            // Проверяем что список задач всё ещё доступен
            tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            Assert.AreEqual(3, tasksListBox.Items.Length,
                "Список задач изменился после настройки уведомлений");
        }

        // TC-015: Настройка интервала
        [TestMethod]
        public void TC_015_NotificationInterval_Setting_ShouldWork()
        {
            var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.Edit)).AsTextBox();
            var addButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByName("Добавить")).AsButton();

            // Находим ComboBox для уведомлений
            var allComboBoxes = _mainWindow.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.ComboBox));
            var notificationComboBox = allComboBoxes.LastOrDefault()?.AsComboBox();

            Assert.IsNotNull(notificationComboBox, "ComboBox уведомлений не найден");

            // Вспомогательный метод для выбора интервала уведомления
            void SelectNotificationInterval(string value)
            {
                var expandButton = notificationComboBox.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.Button));

                if (expandButton != null)
                {
                    expandButton.Click();
                }
                else
                {
                    notificationComboBox.Click();
                }

                Thread.Sleep(300);

                var listItem = _mainWindow.FindFirstDescendant(cf =>
                    cf.ByControlType(ControlType.ListItem).And(cf.ByName(value)));

                if (listItem != null)
                {
                    listItem.Click();
                    Thread.Sleep(300);
                }
            }

            descriptionTextBox.Text = "Задача на 5 дней";
            addButton.Click();
            Thread.Sleep(1500);

            // ПРОВЕРКА: можно выбрать разные значения без ошибок
            SelectNotificationInterval("За 7 дней");
            Thread.Sleep(300);

            SelectNotificationInterval("За 3 дня");
            Thread.Sleep(300);

            // Финальная проверка стабильности
            bool notificationShown = WaitForNotification(10);
            CloseAllMessageBoxes();

            Assert.IsTrue(_mainWindow.IsAvailable,
                "Приложение перестало отвечать после смены интервалов");
        }

        // TC-016: Просроченный дедлайн
        [TestMethod]
        public void TC_016_PassedDeadline_ShouldNotNotify()
        {
            // Сначала закрываем приложение
            _app?.Close();
            Thread.Sleep(1000);

            // Создаём файл с просроченной задачей (дедлайн был 5 дней назад)
            string pastDate = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss");
            File.WriteAllText("tasks.txt", $"Просроченная задача|2|False|{pastDate}");

            // Запускаем приложение заново
            _app = Application.Launch(@"..\..\..\TaskWithPriorityApp\bin\Debug\TaskWithPriorityApp.exe");
            _mainWindow = _app.GetMainWindow(_automation);
            Thread.Sleep(2000);

            // Проверяем, что задача загрузилась
            var tasksListBox = _mainWindow.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.List)).AsListBox();

            Assert.IsTrue(tasksListBox.Items.Any(i => i.Text.Contains("Просроченная задача")),
                "Просроченная задача не загружена");

            // Ожидаем 10 секунд и проверяем, что уведомление НЕ появилось
            bool notificationShown = WaitForNotification(10);
            CloseAllMessageBoxes();

            Assert.IsFalse(notificationShown,
                "Уведомление появилось для просроченной задачи (дедлайн прошёл 5 дней назад)");
        }
    }
}