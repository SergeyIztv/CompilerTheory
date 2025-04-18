using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CompilerTheory.processor;
using CompilerTheory.utility;

namespace CompilerTheory
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentFilePath;
        private UndoStack _undoStack;
        private RedoStack _redoStack;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Регулярное выражение для поиска ФИО (фамилия и инициалы)
        private string pattern = @"([А-ЯЁ][а-яё]{1,}(-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))\s*[А-ЯЁ]\.\s*[А-ЯЁ]\.|[А-ЯЁ]\.\s*[А-ЯЁ]\.\s*([А-ЯЁ][а-яё]{1,}(?:-[А-ЯЁ][а-яё]{1,})?(?:ов|ова|ин|ина|ий|ая|ой))";
        public UndoStack UndoStack
        {
            get => _undoStack;
            private set
            {
                _undoStack = value;
                OnPropertyChanged(nameof(UndoStack));
            }
        }

        public RedoStack RedoStack
        {
            get => _redoStack;
            private set
            {
                _redoStack = value;
                OnPropertyChanged(nameof(RedoStack));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            currentFilePath = string.Empty;
            _undoStack = new UndoStack();
            _redoStack = new RedoStack();
            DataContext = this; 
            // Устанавливаем обработчик события изменения текста
            TextEditor.PreviewKeyDown += TextEditor_PreviewKeyDown;
            //this.PreviewKeyDown += MainWindow_PreviewKeyDown; // Добавляем обработчик для Ctrl+Z, Ctrl+Y
        }

        // Обработчик нажатия клавиш
        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата клавиша Enter или Tab
            if (e.Key == Key.Enter || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Space)
            {
                // Добавляем текущее состояние текста в стек отмены
                _undoStack.Push(TextEditor.Text);
                _redoStack.Clear(); // Очищаем стек повторов при изменении текста
            }
        }

        private void NewFile(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Хотите сохранить файл перед созданием нового файла?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            TextEditor.Clear();
            currentFilePath = string.Empty;

            // Очищаем стеки Undo и Redo
            _undoStack.Clear();
            _redoStack.Clear();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {
                MessageBoxResult result = MessageBox.Show(
                    "Хотите сохранить файл перед открытием нового файла?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                TextEditor.Text = File.ReadAllText(openFileDialog.FileName);
                currentFilePath = openFileDialog.FileName;

                // Очищаем стеки Undo и Redo
                _undoStack.Clear();
                _redoStack.Clear();
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileAs(sender, e);
            }
            else
            {
                File.WriteAllText(currentFilePath, TextEditor.Text);

                // Очищаем стеки Undo и Redo после сохранения
                _undoStack.Clear();
                _redoStack.Clear();
            }
        }

        private void SaveFileAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = ".txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, TextEditor.Text);
                currentFilePath = saveFileDialog.FileName;
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) || !string.IsNullOrEmpty(TextEditor.Text))
            {

                // Спрашиваем у пользователя, хочет ли он сохранить изменения
                MessageBoxResult result = MessageBox.Show(
                    "У вас есть несохраненные изменения. Хотите сохранить файл перед выходом?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Если пользователь выбрал "Да", то сохраняем файл
                    SaveFile(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // Если пользователь выбрал "Отмена", то отменяем выход
                    return;
                }
            }

            // Закрываем приложение
            Application.Current.Shutdown();
        }

        private void CutText(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.Cut();
        }

        private void CopyText(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        private void PasteText(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.Paste();
        }

        private void DeleteText(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();
            TextEditor.SelectedText = string.Empty;
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            TextEditor.SelectAll();
        }

        private void UndoText(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                _redoStack.Push(TextEditor.Text);
                TextEditor.Text = _undoStack.Pop();
                TextEditor.CaretIndex = Math.Min(caretPos, TextEditor.Text.Length); // Восстанавливаем позицию курсора
            }
        }

        private void RedoText(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                int caretPos = TextEditor.CaretIndex; // Запоминаем позицию курсора
                _undoStack.Push(TextEditor.Text);
                TextEditor.Text = _redoStack.Pop();
                TextEditor.CaretIndex = Math.Min(caretPos, TextEditor.Text.Length); // Восстанавливаем позицию курсора
            }
        }

        private void ShowHelp(object sender, RoutedEventArgs e)
        {
            
            string helpFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Help.html");

            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(helpFilePath) { UseShellExecute = true });
        }

        private void AboutApp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Текстовый редактор на WPF\nРазработан для лабораторной работы.", "О программе");
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ToggleMaximize(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void HighlightedText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Передаем фокус на TextEditor
            TextEditor.Focus();

            // Вызываем метод TextEditor_TextChanged
            TextEditor_TextChanged(sender, null);
        }

        private void TextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
            TextEditor.Visibility = Visibility.Visible;
            HighlightedText.Visibility = Visibility.Collapsed;
            
        }

        private void UpdateLineNumbers()
        {
            // Получаем текст из RichTextBox
            string text = TextEditor.Text;

            // Разделяем текст по строкам
            string[] lines = text.Split('\n');

            // Создаем строку для номеров строк
            StringBuilder lineNumbers = new StringBuilder();

            // Заполняем строку номерами строк
            for (int i = 1; i <= lines.Length; i++)
            {
                lineNumbers.AppendLine(i.ToString());  // Добавляем номер строки с новой строки
            }

            // Устанавливаем номера строк в TextBlock
            LineNumbers.Text = lineNumbers.ToString();
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            _undoStack.Push(TextEditor.Text);
            _redoStack.Clear();

            TextEditor.Clear();
            ErrorOutput.Clear();
        }

        private void FindFIO_RegEx(object sender, RoutedEventArgs e)
{
    // Получаем текст из TextEditor
    string inputText = TextEditor.Text;

     Regex regex = new Regex(pattern);
    MatchCollection matches = regex.Matches(inputText);

    // Очищаем ErrorOutput перед выводом новых данных
    ErrorOutput.Clear();
    HighlightedText.Inlines.Clear();

    List<FoundFIO> foundFIOs = new List<FoundFIO>();

    if (matches.Count > 0)
    {
        // Разделяем текст на строки
        string[] lines = inputText.Split('\n');
        StringBuilder result = new StringBuilder();
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            if (!match.Success) continue;

            int position = match.Index;
            int endPosition = match.Index + match.Length;
            int lineNumber = 0;
            int positionInLine = 0;
            int currentLength = 0;

            // Вычисляем номер строки и позицию в строке
            for (int i = 0; i < lines.Length; i++)
            {
                if (currentLength + lines[i].Length + 1 > match.Index)
                {
                    lineNumber = i + 1;
                    positionInLine = match.Index - currentLength;
                    break;
                }
                currentLength += lines[i].Length + 1;
            }

            // Сохраняем данные в список
            foundFIOs.Add(new FoundFIO(match.Value, positionInLine, lineNumber, endPosition));

            // Подсветка найденного текста
            HighlightedText.Inlines.Add(new Run(inputText.Substring(lastIndex, match.Index - lastIndex)));

            HighlightedText.Inlines.Add(new Run(match.Value)
            {
                Background = Brushes.Yellow,
                Foreground = Brushes.Black
            });

            lastIndex = match.Index + match.Length;

            // Формируем строку результата
            string matchInfo = $"Найдено: {match.Value} (Строка: {lineNumber}, Позиция начала: {positionInLine}, Позиция конца: {endPosition})";
            result.AppendLine(matchInfo);

            // Выводим результат в ErrorOutput
            ErrorOutput.AppendText(matchInfo + Environment.NewLine);
        }

        // Добавляем оставшийся текст
        HighlightedText.Inlines.Add(new Run(inputText.Substring(lastIndex)));

        // Показываем TextBlock с подсветкой
        HighlightedText.Visibility = Visibility.Visible;
        TextEditor.Visibility = Visibility.Collapsed;

        // Предлагаем пользователю сохранить результат в файл
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Text Files (*.txt)|*.txt",
            DefaultExt = ".txt",
            Title = "Сохранить результаты поиска"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            File.WriteAllText(saveFileDialog.FileName, result.ToString());
            MessageBox.Show("Результаты поиска успешно сохранены!", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        }
        else
        {
            ErrorOutput.AppendText("Не найдено совпадений!" + Environment.NewLine);
        }

        // Выводим список найденных данных в консоль (для теста)
        foreach (var fio in foundFIOs)
        {
            Console.WriteLine(fio);
        }
    }
        
        private void ParseText(object sender, RoutedEventArgs routedEventArgs)
        {
            // Получаем текст из TextEditor
            string inputText = TextEditor.Text;
            ErrorOutput.Clear(); // Очищаем вывод
            var processor = new CppProcessor();
            
            var errors = processor.Process(inputText);
            
            if (errors.Count == 0)
            {
                ErrorOutput.Text = "Ошибки отсутствуют!";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var error in errors)
            {
                
                // Определяем позицию в тексте
                int lineNumber = 1;
                int currentIndex = 0;
                int lastNewLineIndex = -1;

                while (currentIndex < error.Position && currentIndex < inputText.Length)
                {
                    if (inputText[currentIndex] == '\n')
                    {
                        lineNumber++;
                        lastNewLineIndex = currentIndex;
                    }
                    currentIndex++;
                }

                int linePosition = error.Position - lastNewLineIndex - 1;

                // Формируем сообщение
                string foundInfo = $"Обнаружено: {error.FoundTokenValue}";
                if (!string.IsNullOrEmpty(error.FoundTokenValue))
                    foundInfo = $"Обнаружено: '{error.FoundTokenValue}' ";

                sb.AppendLine($"Строка {lineNumber}, позиция {linePosition}: {error.Message}{foundInfo}");
            }
            ErrorOutput.Text = sb.ToString();
        }
        private void FindFIO_FiniteStateMachine(object sender, RoutedEventArgs e)
        {
            // Получаем текст из TextEditor
            string inputText = TextEditor.Text;
            ErrorOutput.Clear(); // Очищаем вывод

            // Создаем конечный автомат
            var machine = new StateMachine();

            // Обрабатываем текст посимвольно
            for (int i = 0; i < inputText.Length; i++)
            {
                char currentChar = inputText[i];
                machine.Transition(currentChar);

                // Если достигнуто состояние ошибки, прерываем обработку
                if (machine.CurrentState == "SE")
                {
                    break;
                }
            }

            // Получаем результат обработки
            bool isValid = machine.CurrentState == "S6"; // Успешное завершение
            string status = isValid ? "КОРРЕКТНО" : "НЕКОРРЕКТНО";
            string statesHistory = string.Join(" → ", machine.States);

            // Выводим результат в ErrorOutput
            ErrorOutput.AppendText($"Результат обработки: {status}\n");
            ErrorOutput.AppendText($"Состояния КА: {statesHistory}\n");
        }

        private void GetRegularExpression(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = pattern;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }


    // Классы для хранения стека операций Undo и Redo
    public class UndoStack : INotifyPropertyChanged
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Push(string text)
        {
            _stack.Push(text);
            OnPropertyChanged(nameof(Count));
        }

        public string Pop()
        {
            var result = _stack.Count > 0 ? _stack.Pop() : null;
            OnPropertyChanged(nameof(Count));
            return result;
        }

        public void Clear()
        {
            _stack.Clear();
            OnPropertyChanged(nameof(Count));
        }

        public int Count => _stack.Count;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FoundFIO
    {
        public string FIO { get; set; } // Найденная строка (ФИО)
        public int Position { get; set; } // Позиция в тексте
        public int LineNumber { get; set; } // Номер строки в файле
        public int EndPosition { get; set; } // Позиция конца вхождения

        public FoundFIO(string fio, int position, int lineNumber, int endPosition)
        {
            FIO = fio;
            Position = position;
            LineNumber = lineNumber;
            EndPosition = endPosition;
        }

        public override string ToString()
        {
            return $"ФИО: {FIO}, Строка: {LineNumber}, Начало: {Position}, Конец: {EndPosition}";
        }
    }

    
    public class RedoStack : INotifyPropertyChanged
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Push(string text)
        {
            _stack.Push(text);
            OnPropertyChanged(nameof(Count));
        }

        public string Pop()
        {
            var result = _stack.Count > 0 ? _stack.Pop() : null;
            OnPropertyChanged(nameof(Count));
            return result;
        }

        public void Clear()
        {
            _stack.Clear();
            OnPropertyChanged(nameof(Count));
        }

        public int Count => _stack.Count;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
