using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToC_Lab1
{
    public class StateMachine
    {
        private string _currentState;
        private readonly List<string> _states = new List<string>();

        public string CurrentState => _currentState;
        public List<string> States => _states;

        public StateMachine()
        {
            _currentState = "S0"; // Начальное состояние
            _states.Add(_currentState);
        }

        public void Transition(char symbol)
        {
            switch (_currentState)
            {
                case "S0": // Начальное состояние
                    if (IsRussianUpper(symbol))
                    {
                        _currentState = "S1_temp"; // Временное состояние для определения контекста
                    }
                    else if (symbol == ' ')
                    {
                        _currentState = "S0"; // Пропускаем пробелы в начале
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S1_temp": // Временное состояние после первой заглавной буквы
                    if (symbol == '.')
                    {
                        _currentState = "S7"; // Начало обработки инициалов
                    }
                    else if (IsRussianLower(symbol) || symbol == '-')
                    {
                        _currentState = "S1"; // Начало обработки фамилии
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S1": // Обработка фамилии
                    if (IsRussianLower(symbol))
                    {
                        _currentState = "S1"; // Продолжение фамилии
                    }
                    else if (symbol == '-')
                    {
                        _currentState = "S1A"; // Переход к обработке второй части фамилии
                    }
                    else if (symbol == ' ')
                    {
                        _currentState = "S2"; // Переход к пробелу после фамилии
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S1A": // Обработка второй части фамилии (после дефиса)
                    if (IsRussianUpper(symbol))
                    {
                        _currentState = "S1"; // Переход к обработке второй части фамилии
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S2": // Пробел после фамилии
                    if (symbol == ' ')
                    {
                        _currentState = "S2"; // Продолжение пробелов
                    }
                    else if (IsRussianUpper(symbol))
                    {
                        _currentState = "S3"; // Переход к первому инициалу
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S3": // Первый инициал
                    if (symbol == '.')
                    {
                        _currentState = "S4"; // Переход к пробелу между инициалами
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S4": // Пробел между инициалами
                    if (symbol == ' ')
                    {
                        _currentState = "S4"; // Продолжение пробелов
                    }
                    else if (IsRussianUpper(symbol))
                    {
                        _currentState = "S5"; // Переход ко второму инициалу
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S5": // Второй инициал
                    if (symbol == '.')
                    {
                        _currentState = "S6"; // Успешное завершение (фамилия + инициалы)
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S6": // Конечное состояние (фамилия + инициалы)
                    _currentState = "SE"; // После S6 только ошибка
                    break;

                case "S7": // Обработка первого инициала (инициалы перед фамилией)
                    if (symbol == '.')
                    {
                        _currentState = "S8"; // Переход к пробелу между инициалами
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S8": // Пробел между инициалами (инициалы перед фамилией)
                    if (symbol == ' ')
                    {
                        _currentState = "S8"; // Продолжение пробелов
                    }
                    else if (IsRussianUpper(symbol))
                    {
                        _currentState = "S9"; // Переход ко второму инициалу
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S9": // Второй инициал (инициалы перед фамилией)
                    if (symbol == '.')
                    {
                        _currentState = "S10"; // Переход к пробелу перед фамилией
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S10": // Пробел перед фамилией (инициалы перед фамилией)
                    if (symbol == ' ')
                    {
                        _currentState = "S10"; // Продолжение пробелов
                    }
                    else if (IsRussianUpper(symbol))
                    {
                        _currentState = "S11"; // Переход к обработке фамилии
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S11": // Обработка фамилии (инициалы перед фамилией)
                    if (IsRussianLower(symbol))
                    {
                        _currentState = "S11"; // Продолжение фамилии
                    }
                    else if (symbol == '-')
                    {
                        _currentState = "S11A"; // Переход к обработке второй части фамилии
                    }
                    else if (symbol == ' ' || symbol == '\0')
                    {
                        _currentState = "S12"; // Успешное завершение (инициалы + фамилия)
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S11A": // Обработка второй части фамилии (инициалы перед фамилией)
                    if (IsRussianUpper(symbol))
                    {
                        _currentState = "S11"; // Переход к обработке второй части фамилии
                    }
                    else
                    {
                        _currentState = "SE"; // Ошибка
                    }
                    break;

                case "S12": // Конечное состояние (инициалы + фамилия)
                    _currentState = "SE"; // После S12 только ошибка
                    break;

                case "SE": // Состояние ошибки
                           // Остаемся в состоянии ошибки
                    break;
            }
            _states.Add(_currentState);
        }

        public (bool IsValid, List<string> States) Process(string input)
        {
            foreach (char c in input)
                Transition(c);

            // Успешное завершение: S6 (фамилия + инициалы) или S12 (инициалы + фамилия)
            bool isValid = _currentState == "S6" || _currentState == "S12";
            return (isValid, _states);
        }

        private bool IsRussianUpper(char c)
        {
            return "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ".Contains(c);
        }

        private bool IsRussianLower(char c)
        {
            return "абвгдеёжзийклмнопрстуфхцчшщъыьэюя".Contains(c);
        }
    }
}
