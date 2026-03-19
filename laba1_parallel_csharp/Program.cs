using System.Diagnostics;
using System.Numerics;

namespace laba1_parallel_csharp;

internal sealed class SumWorker
{
    private readonly int _id;
    private readonly int _step;
    private readonly int _workTimeMs;
    private readonly Thread _thread;
    private volatile bool _stopRequested;

    public BigInteger Sum { get; private set; } = BigInteger.Zero;
    public long Count { get; private set; }

    public int Id => _id;
    public int Step => _step;
    public int WorkTimeMs => _workTimeMs;

    public SumWorker(int id, int step, int workTimeMs)
    {
        if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step));
        if (workTimeMs <= 0) throw new ArgumentOutOfRangeException(nameof(workTimeMs));

        _id = id;
        _step = step;
        _workTimeMs = workTimeMs;
        _thread = new Thread(Run)
        {
            IsBackground = false,
            Name = $"SumWorker-{id}"
        };
    }

    public void Start() => _thread.Start();

    public void RequestStop() => _stopRequested = true;

    public void Join() => _thread.Join();

    private void Run()
    {
        BigInteger currentValue = BigInteger.Zero;

        while (!_stopRequested)
        {
            Sum += currentValue;
            currentValue += _step;
            Count++;
        }

        Console.WriteLine(
            $"[Потік {_id}] завершив роботу. Крок: {_step} | Доданків: {Count} | Сума: {Sum}");
    }
}

internal sealed class Controller
{
    private readonly SumWorker[] _workers;
    private readonly Thread _thread;

    public Controller(SumWorker[] workers)
    {
        _workers = workers;
        _thread = new Thread(Run)
        {
            IsBackground = false,
            Name = "Controller"
        };
    }

    public void Start() => _thread.Start();

    public void Join() => _thread.Join();

    private void Run()
    {
        var sw = Stopwatch.StartNew();
        var stopSent = new bool[_workers.Length];
        var active = _workers.Length;

        while (active > 0)
        {
            for (int i = 0; i < _workers.Length; i++)
            {
                if (stopSent[i])
                {
                    continue;
                }

                if (sw.ElapsedMilliseconds >= _workers[i].WorkTimeMs)
                {
                    _workers[i].RequestStop();
                    stopSent[i] = true;
                    active--;
                }
            }

            Thread.Sleep(1);
        }
    }
}

internal static class Program
{
    private const int MinThreads = 1;
    private const int MaxThreads = 32;
    private const int MaxStep = 1_000_000;
    private const int MaxWorkTime = 60_000;

    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Помилка. Введіть ціле число в межах від {min} до {max}.");
        }
    }

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Лабораторна робота №1");
        Console.WriteLine("Тема: Засоби створення багатопоточних програм");
        Console.WriteLine();

        int threadCount = ReadInt($"Кількість потоків ({MinThreads}-{MaxThreads}): ", MinThreads, MaxThreads);
        Console.WriteLine();

        var workers = new SumWorker[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            Console.WriteLine($"Налаштування потоку {i + 1}:");
            int step = ReadInt($"  Крок ({MinThreads}-{MaxStep}): ", MinThreads, MaxStep);
            int workTime = ReadInt($"  Час роботи в мс ({MinThreads}-{MaxWorkTime}): ", MinThreads, MaxWorkTime);
            workers[i] = new SumWorker(i + 1, step, workTime);
            Console.WriteLine();
        }

        foreach (var worker in workers)
        {
            worker.Start();
        }

        var controller = new Controller(workers);
        controller.Start();

        foreach (var worker in workers)
        {
            worker.Join();
        }

        controller.Join();

        Console.WriteLine();
        Console.WriteLine("Усі потоки завершили роботу.");
    }
}
