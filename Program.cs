using System;
using System.Diagnostics;
using System.Text;

namespace OpamLab09
{
    internal class Program
    {
        delegate void SortDelegate<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves);

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            // =========================================================================
            // ВКАЖІТЬ ВАШІ ПАРАМЕТРИ ВАРІАНТА ТУТ:
            int N = 7;   // Номер у списку групи
            int K = 19;  // Номер першої літери імені в українській абетці (1..33)
            // =========================================================================

            int a = N % 3;
            int b = N % 4;
            int c = N % 5;

            Console.WriteLine($"N = {N}, K = {K}, a = {a}, b = {b}, c = {c}");
            Console.WriteLine(new string('=', 80));

            Func<int, int, int> intCmp = (x, y) => x.CompareTo(y);

            // Вибір алгоритмів за параметрами a, b, c
            SortDelegate<int> simpleSort = a == 0 ? Sorts.SelectionSort : a == 1 ? Sorts.InsertionSort : Sorts.BubbleSort;
            string simpleName = a == 0 ? "Вибором (a=0)" : a == 1 ? "Вставками (a=1)" : "Бульбашкою (a=2)";

            SortDelegate<int> fastSort = b == 0 ? Sorts.MergeSortRecursive
                                       : b == 1 ? Sorts.MergeSortBottomUp
                                       : b == 2 ? Sorts.QuickSortMiddle
                                                : Sorts.QuickSortMedianOfThree;
            string fastName = b == 0 ? "Злиттям рекурс. (b=0)"
                            : b == 1 ? "Злиттям знизу вгору (b=1)"
                            : b == 2 ? "Швидке середній (b=2)"
                                     : "Швидке медіана 3 (b=3)";

            int defaultHybridThreshold = 16;
            SortDelegate<int> extraSort = c == 0 ? (int[] arr, Func<int, int, int> _, out int cmps, out int mvs) => Sorts.CountingSort(arr, x => x, 20 * K, out cmps, out mvs)
                                        : c == 1 ? Sorts.BinaryInsertionSort
                                        : c == 2 ? (int[] arr, Func<int, int, int> cmp, out int cmps, out int mvs) => Sorts.HybridSort(arr, cmp, defaultHybridThreshold, out cmps, out mvs)
                                                 : Sorts.ShellSort;
            string extraName = c == 0 ? "Підрахунком (c=0)"
                             : c == 1 ? "Бін. вставками (c=1)"
                             : c == 2 ? $"Гібрид T={defaultHybridThreshold} (c=2)"
                                      : "Шелла (c=3)";

            // --- ЗАВДАННЯ 1: Масив для трасування ---
            int traceLen = (K % 5) + 6;
            int[] fullBase = Validation.GenerateRandom(100, N, K);
            int[] traceArr = new int[traceLen];
            Array.Copy(fullBase, traceArr, traceLen);
            Console.WriteLine($"\n--- ЗАВДАННЯ 1: Масив для трасування (довжина K%5 + 6 = {traceLen}) ---");
            Console.WriteLine("Вихідний: [" + string.Join(", ", traceArr) + "]");
            PrintInsertionTrace((int[])traceArr.Clone());

            // --- ЗАВДАННЯ 2: Тест виродження на однакових елементах ---
            Console.WriteLine("\n--- ЗАВДАННЯ 2: Виродження на масиві однакових елементів (n = 1 000) ---");
            int[] allEqual = new int[1000];
            Array.Fill(allEqual, K);
            fastSort((int[])allEqual.Clone(), intCmp, out int eqCmp, out int eqMov);
            Console.WriteLine($"{fastName}: порівнянь = {eqCmp:N0}, переставлянь = {eqMov:N0} (очікувано ~n^2/2 = 500 000 для швидкого сортування)");

            // --- ЗАВДАННЯ 3: Додатковий експеримент для свого c ---
            Console.WriteLine($"\n--- ЗАВДАННЯ 3: Додатковий варіант ({extraName}) ---");
            if (c == 2)
            {
                int[] thresholds = { 1, 4, 16, 2 * K + 3 };
                int[] test100k = Validation.GenerateRandom(100_000, N, K);
                Console.WriteLine("Пороги для гібрида на n = 100 000 (Release, мін. з 3 запусків):");
                foreach (int t in thresholds)
                {
                    double ms = MeasureMinMs(() =>
                    {
                        int[] copy = (int[])test100k.Clone();
                        Sorts.HybridSort(copy, intCmp, t, out _, out _);
                    });
                    Console.WriteLine($"  Поріг {t,3}: {ms:F3} мс");
                }
            }

            // --- ЗАВДАННЯ 4: Таблиці порівнянь / переставлянь на 4 типах входу ---
            Console.WriteLine("\n--- ЗАВДАННЯ 4: Порівняння / Переставляння ---");
            int[] sizes4 = { 100, 1_000, 10_000 };
            PrintTask4Table(simpleName, simpleSort, sizes4, N, K, intCmp);
            PrintTask4Table(fastName, fastSort, sizes4, N, K, intCmp);
            if (c != 4) PrintTask4Table(extraName, extraSort, sizes4, N, K, intCmp);

            // --- ЗАВДАННЯ 5.1: Час виконання проти Array.Sort ---
            Console.WriteLine("\n--- ЗАВДАННЯ 5.1: Час на випадковому вході (мс, мін. з 3 запусків) ---");
            #if DEBUG
            Console.WriteLine("УВАГА: Запущено в DEBUG! Запустіть: dotnet run -c Release");
            #endif
            Console.WriteLine($"{"n",10} | {simpleName,18} | {fastName,24} | {extraName,20} | {"Array.Sort",12}");
            Console.WriteLine(new string('-', 95));

            // Прогрівочний запуск JIT
            int[] warm = Validation.GenerateRandom(1000, N, K);
            simpleSort((int[])warm.Clone(), intCmp, out _, out _);
            fastSort((int[])warm.Clone(), intCmp, out _, out _);
            extraSort((int[])warm.Clone(), intCmp, out _, out _);
            Array.Sort((int[])warm.Clone());

            int[] sizes5 = { 1_000, 10_000, 100_000, 1_000_000 };
            foreach (int n in sizes5)
            {
                int[] baseArr = Validation.GenerateRandom(n, N, K);

                string simpleStr = n <= 100_000
                    ? $"{MeasureMinMs(() => { var cp = (int[])baseArr.Clone(); simpleSort(cp, intCmp, out _, out _); }):F3}"
                    : "—";
                double fastMs = MeasureMinMs(() => { var cp = (int[])baseArr.Clone(); fastSort(cp, intCmp, out _, out _); });
                double extraMs = MeasureMinMs(() => { var cp = (int[])baseArr.Clone(); extraSort(cp, intCmp, out _, out _); });
                double stdMs = MeasureMinMs(() => { var cp = (int[])baseArr.Clone(); Array.Sort(cp); });

                Console.WriteLine($"{n,10:N0} | {simpleStr,18} | {fastMs,24:F3} | {extraMs,20:F3} | {stdMs,12:F3}");
            }

            // Пошук межі прийнятності (1000 мс) для простого сортування
            Console.WriteLine("\n--- ЗАВДАННЯ 5.2: Пошук межі прийнятності (> 1000 мс) для простого сортування ---");
            int limitN = 20_000;
            while (true)
            {
                int[] testArr = Validation.GenerateRandom(limitN, N, K);
                double ms = MeasureMinMs(() => { var cp = (int[])testArr.Clone(); simpleSort(cp, intCmp, out _, out _); });
                Console.WriteLine($"  n = {limitN,7:N0} -> {ms:F1} мс");
                if (ms > 1000.0) break;
                limitN += 20_000;
            }
            Console.WriteLine($"Межа прийнятності (вперше > 1 с): приблизно n = {limitN:N0}");

            // --- ЗАВДАННЯ 5.3: Перевірка стабільності на Student[] ---
            Console.WriteLine("\n--- ЗАВДАННЯ 5.3: Перевірка стабільності на Student[] (вхід впорядкований за Name) ---");
            Student[] baseStudents =
            {
                new Student("Андрієнко", "КН-3/1", 85),
                new Student("Бойко", "КН-3/2", 75),
                new Student("Василенко", "КН-3/1", 85),
                new Student("Гриценко", "КН-3/2", 90),
                new Student("Дмитренко", "КН-3/1", 75),
                new Student("Євтушенко", "КН-3/2", 85),
                new Student("Захарченко", "КН-3/1", 75),
                new Student("Іваненко", "КН-3/2", 90)
            };
            Func<Student, Student, int> scoreCmp = (s1, s2) => s1.Score.CompareTo(s2.Score);

            CheckStability(simpleName, baseStudents, arr =>
            {
                if (a == 0) Sorts.SelectionSort(arr, scoreCmp, out _, out _);
                else if (a == 1) Sorts.InsertionSort(arr, scoreCmp, out _, out _);
                else Sorts.BubbleSort(arr, scoreCmp, out _, out _);
            });

            CheckStability(fastName, baseStudents, arr =>
            {
                if (b == 0) Sorts.MergeSortRecursive(arr, scoreCmp, out _, out _);
                else if (b == 1) Sorts.MergeSortBottomUp(arr, scoreCmp, out _, out _);
                else if (b == 2) Sorts.QuickSortMiddle(arr, scoreCmp, out _, out _);
                else Sorts.QuickSortMedianOfThree(arr, scoreCmp, out _, out _);
            });

            CheckStability("Array.Sort", baseStudents, arr => Array.Sort(arr, (x, y) => scoreCmp(x, y)));

            // --- ПЕРЕВІРКА КРАЙОВИХ ВИПАДКІВ (IsSorted + IsPermutationOf) ---
            Console.WriteLine("\n--- ПЕРЕВІРКА КРАЙОВИХ ВИПАДКІВ ---");
            int[][] edgeCases =
            {
                Array.Empty<int>(),
                new[] { 42 },
                new[] { 10, 20 },
                new[] { 20, 10 },
                new[] { 7, 7, 7, 7, 7, 7 },
                new[] { 9, 3, 9, 3, 9, 3, 9, 3 }
            };
            string[] edgeNames =
            {
                "порожній масив",
                "один елемент",
                "два елементи в порядку",
                "два елементи навпаки",
                "усі елементи однакові",
                "лише два різних значення"
            };

            for (int i = 0; i < edgeCases.Length; i++)
            {
                bool ok1 = VerifySort(simpleSort, edgeCases[i], intCmp);
                bool ok2 = VerifySort(fastSort, edgeCases[i], intCmp);
                bool ok3 = VerifySort(extraSort, edgeCases[i], intCmp);
                Console.WriteLine($"{edgeNames[i],-28} | Простий: {(ok1 ? "OK" : "FAIL")} | Швидкий: {(ok2 ? "OK" : "FAIL")} | Додатковий: {(ok3 ? "OK" : "FAIL")}");
            }
        }

        static void PrintInsertionTrace(int[] a)
        {
            for (int i = 1; i < a.Length; i++)
            {
                int key = a[i];
                int j = i - 1;
                while (j >= 0 && a[j] > key)
                {
                    a[j + 1] = a[j];
                    j--;
                }
                a[j + 1] = key;
                Console.WriteLine($"  Прохід i={i} (вставлено {key,3}): [{string.Join(", ", a)}]");
            }
        }

        static void PrintTask4Table(string title, SortDelegate<int> sort, int[] sizes, int N, int K, Func<int, int, int> cmp)
        {
            Console.WriteLine($"\nАлгоритм: {title}");
            Console.WriteLine($"{"n",8} | {"Випадковий",22} | {"Відсортований",22} | {"Навпаки",22} | {"Майже",22}");
            Console.WriteLine(new string('-', 106));

            foreach (int n in sizes)
            {
                int[] rnd = Validation.GenerateRandom(n, N, K);
                int[] srt = Validation.MakeSorted(rnd);
                int[] rev = Validation.MakeReversed(rnd);
                int[] nr = Validation.MakeNearlySorted(rnd, N, K);

                string c1 = RunAndFormat(sort, rnd, cmp);
                string c2 = RunAndFormat(sort, srt, cmp);
                string c3 = RunAndFormat(sort, rev, cmp);
                string c4 = RunAndFormat(sort, nr, cmp);

                Console.WriteLine($"{n,8:N0} | {c1,22} | {c2,22} | {c3,22} | {c4,22}");
            }
        }

        static string RunAndFormat(SortDelegate<int> sort, int[] input, Func<int, int, int> cmp)
        {
            int[] copy = (int[])input.Clone();
            sort(copy, cmp, out int cmps, out int mvs);
            if (!Validation.IsSorted(copy, cmp) || !Validation.IsPermutationOf(copy, input))
                return "ERR!";
            return $"{cmps:N0} / {mvs:N0}";
        }

        static double MeasureMinMs(Action action)
        {
            double best = double.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds < best) best = sw.Elapsed.TotalMilliseconds;
            }
            return best;
        }

        static void CheckStability(string name, Student[] original, Action<Student[]> sortAction)
        {
            Student[] copy = (Student[])original.Clone();
            sortAction(copy);

            string counterExample = "немає (стабільний)";
            bool stable = true;
            for (int i = 1; i < copy.Length; i++)
            {
                if (copy[i - 1].Score == copy[i].Score && string.Compare(copy[i - 1].Name, copy[i].Name, StringComparison.Ordinal) > 0)
                {
                    stable = false;
                    counterExample = $"{copy[i - 1].Name} та {copy[i].Name} (бал {copy[i].Score})";
                    break;
                }
            }
            Console.WriteLine($"{name,-26} | Фактично: {(stable ? "Стабільний" : "Нестабільний"),-12} | Контрприклад: {counterExample}");
        }

        static bool VerifySort(SortDelegate<int> sort, int[] input, Func<int, int, int> cmp)
        {
            int[] copy = (int[])input.Clone();
            sort(copy, cmp, out _, out _);
            return Validation.IsSorted(copy, cmp) && Validation.IsPermutationOf(copy, input);
        }
    }
}