// Лабораторна робота: Багатопоточні програми на C#
// Тема: Засоби створення багатопоточних програм

using System;
using System.Threading;

namespace ConsoleApp1
{
    // Клас для роботи потоку обчислення суми послідовності
    class SumThread
    {
        private int threadId;           // Порядковий номер потоку
        private int step;               // Крок послідовності
        private int workTime;           // Час роботи потоку в мс
        private volatile bool shouldStop; // Прапорець зупинки потоку
        private long sum;               // Знайдена сума
        private int count;              // Кількість використаних елементів
        private Thread thread;          // Посилання на потік

        public SumThread(int id, int step, int workTime)
        {
            this.threadId = id;
            this.step = step;
            this.workTime = workTime;
            this.shouldStop = false;
            this.sum = 0;
            this.count = 0;
            this.thread = new Thread(Run);
        }

        // Запуск потоку
        public void Start()
        {
            thread.Start();
        }

        // Запуск потоку з автоматичною зупинкою через заданий час
        public void StartWithTimer()
        {
            thread.Start();
            // Створюємо окремий потік-таймер для зупинки
            Thread timerThread = new Thread(() =>
            {
                Thread.Sleep(workTime);
                Stop();
            });
            timerThread.IsBackground = true;
            timerThread.Start();
        }

        // Зупинка потоку
        public void Stop()
        {
            shouldStop = true;
        }

        // Очікування завершення потоку
        public void Join()
        {
            thread.Join();
        }

        // Основний метод роботи потоку
        private void Run()
        {
            long currentValue = 0;

            while (!shouldStop)
            {
                sum += currentValue;
                currentValue += step;
                count++;
            }

            // Виведення результатів після завершення потоку
            Console.WriteLine($"Потік #{threadId}: Сума = {sum}, Кількість елементів = {count}, Крок = {step}, Час роботи = {workTime} мс");
        }
    }

    // Головний клас програми
    class Program
    {
        private const int MAX_THREADS = 32;  // Максимальна кількість потоків
        private const int MIN_VALUE = 1;     // Мінімальне значення для введення
        private const int MAX_STEP = 1000;   // Максимальний крок
        private const int MAX_TIME = 60000;  // Максимальний час (60 секунд)

        // Метод для отримання коректного числа від користувача
        static int GetValidNumber(string prompt, int min, int max, int defaultValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                
                // Перевірка на порожній ввід при автоматизованому тестуванні
                if (string.IsNullOrEmpty(input))
                {
                    return defaultValue;
                }
                
                if (int.TryParse(input, out int value))
                {
                    if (value >= min && value <= max)
                    {
                        return value;
                    }
                    else
                    {
                        Console.WriteLine($"  ❌ Помилка! Число має бути від {min} до {max}. Спробуйте ще раз.");
                    }
                }
                else
                {
                    Console.WriteLine("  ❌ Помилка! Введіть коректне ціле число. Спробуйте ще раз.");
                }
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("   Багатопоточна програма обчислення сум послідовностей");
            Console.WriteLine("   Кожен потік має власний крок та час роботи");
            Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

            // Введення кількості потоків з валідацією
            Console.WriteLine($"📌 Обмеження: від {MIN_VALUE} до {MAX_THREADS} потоків\n");
            int threadCount = GetValidNumber(
                $"Введіть кількість потоків (1-{MAX_THREADS}): ",
                MIN_VALUE,
                MAX_THREADS,
                4  // Значення за замовчуванням
            );

            // Створення масиву потоків
            SumThread[] threads = new SumThread[threadCount];
            
            Console.WriteLine("\n" + new string('─', 60));
            Console.WriteLine($"Налаштування параметрів для {threadCount} поток(ів):");
            Console.WriteLine(new string('─', 60) + "\n");

            // Введення параметрів для кожного потоку окремо
            for (int i = 0; i < threadCount; i++)
            {
                Console.WriteLine($"╔═══ Потік #{i + 1} ═══╗");
                
                // Введення кроку для цього потоку з валідацією
                int step = GetValidNumber(
                    $"  Крок послідовності (1-{MAX_STEP}): ",
                    MIN_VALUE,
                    MAX_STEP,
                    i + 1  // За замовчуванням
                );

                // Введення часу роботи для цього потоку з валідацією
                int workTime = GetValidNumber(
                    $"  Час роботи в мс (1-{MAX_TIME}): ",
                    MIN_VALUE,
                    MAX_TIME,
                    100 * (i + 1)  // За замовчуванням
                );

                // Створення потоку з індивідуальними параметрами
                threads[i] = new SumThread(i + 1, step, workTime);
                Console.WriteLine($"  ✓ Потік #{i + 1}: крок={step}, час={workTime} мс");
                Console.WriteLine("╚" + new string('═', 20) + "╝\n");
            }

            // Запуск всіх потоків
            Console.WriteLine(new string('═', 60));
            Console.WriteLine("🚀 Запуск потоків...");
            Console.WriteLine(new string('═', 60) + "\n");
            
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].StartWithTimer();
                Console.WriteLine($"  ✓ Потік #{i + 1} запущено з індивідуальним таймером");
            }

            Console.WriteLine("\n" + new string('─', 60));
            Console.WriteLine($"⏳ Всі {threadCount} поток(ів) працюють паралельно...");
            Console.WriteLine("   Очікування завершення...");
            Console.WriteLine(new string('─', 60) + "\n");

            // Очікування завершення всіх потоків
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine("✅ Всі потоки успішно завершили роботу!");
            Console.WriteLine(new string('═', 60));
            
            // Перевірка чи доступний консольний ввід
            try
            {
                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine("\n💡 Натисніть будь-яку клавішу для виходу...");
                    Console.ReadKey();
                }
            }
            catch
            {
                // Ігноруємо помилку при автоматизованому тестуванні
            }
        }
    }
}
