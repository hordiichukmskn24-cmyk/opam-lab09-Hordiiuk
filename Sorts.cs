using System;

namespace OpamLab09
{
    public static class Sorts
    {
        // =========================================================================
        // ЗАВДАННЯ 1: Прості сортування (таблиця A)
        // =========================================================================

        // a = 0: Сортування вибором
        public static void SelectionSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIdx = i;
                for (int j = i + 1; j < n; j++)
                {
                    comparisons++;
                    if (cmp(a[j], a[minIdx]) < 0)
                    {
                        minIdx = j;
                    }
                }
                if (minIdx != i)
                {
                    T tmp = a[i];
                    a[i] = a[minIdx];
                    a[minIdx] = tmp;
                    moves += 2;
                }
            }
        }

        // a = 1: Сортування вставками
        public static void InsertionSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            InsertionSortRange(a, 0, a.Length - 1, cmp, ref comparisons, ref moves);
        }

        public static void InsertionSortRange<T>(T[] a, int lo, int hi, Func<T, T, int> cmp, ref int comparisons, ref int moves)
        {
            for (int i = lo + 1; i <= hi; i++)
            {
                T key = a[i];
                int j = i - 1;
                bool shifted = false;

                while (j >= lo)
                {
                    comparisons++;
                    if (cmp(a[j], key) > 0)
                    {
                        a[j + 1] = a[j];
                        moves++;
                        shifted = true;
                        j--;
                    }
                    else
                    {
                        break;
                    }
                }

                if (shifted)
                {
                    a[j + 1] = key;
                    moves++;
                }
            }
        }

        // a = 2: Сортування бульбашкою з прапорцем
        public static void BubbleSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;
            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;
                for (int j = 0; j < n - 1 - i; j++)
                {
                    comparisons++;
                    if (cmp(a[j], a[j + 1]) > 0)
                    {
                        T tmp = a[j];
                        a[j] = a[j + 1];
                        a[j + 1] = tmp;
                        moves += 2;
                        swapped = true;
                    }
                }
                if (!swapped) break;
            }
        }

        // =========================================================================
        // ЗАВДАННЯ 2: Швидкі сортування O(n log n) (таблиця B)
        // =========================================================================

        // b = 0: Сортування злиттям, рекурсивне (буфер виділяється один раз зовні)
        public static void MergeSortRecursive<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            if (a.Length <= 1) return;
            T[] aux = new T[a.Length];
            MergeSortRec(a, aux, 0, a.Length - 1, cmp, ref comparisons, ref moves);
        }

        private static void MergeSortRec<T>(T[] a, T[] aux, int lo, int hi, Func<T, T, int> cmp, ref int comparisons, ref int moves)
        {
            if (lo >= hi) return;
            int mid = lo + (hi - lo) / 2;
            MergeSortRec(a, aux, lo, mid, cmp, ref comparisons, ref moves);
            MergeSortRec(a, aux, mid + 1, hi, cmp, ref comparisons, ref moves);
            Merge(a, aux, lo, mid, hi, cmp, ref comparisons, ref moves);
        }

        // b = 1: Сортування злиттям знизу вгору (без рекурсії)
        public static void MergeSortBottomUp<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;
            if (n <= 1) return;
            T[] aux = new T[n];

            for (int sz = 1; sz < n; sz *= 2)
            {
                for (int lo = 0; lo < n - sz; lo += 2 * sz)
                {
                    int mid = lo + sz - 1;
                    int hi = Math.Min(lo + 2 * sz - 1, n - 1);
                    Merge(a, aux, lo, mid, hi, cmp, ref comparisons, ref moves);
                }
            }
        }

        private static void Merge<T>(T[] a, T[] aux, int lo, int mid, int hi, Func<T, T, int> cmp, ref int comparisons, ref int moves)
        {
            for (int k = lo; k <= hi; k++)
            {
                aux[k] = a[k];
                moves++;
            }

            int i = lo, j = mid + 1;
            for (int k = lo; k <= hi; k++)
            {
                if (i > mid)
                {
                    a[k] = aux[j++];
                    moves++;
                }
                else if (j > hi)
                {
                    a[k] = aux[i++];
                    moves++;
                }
                else
                {
                    comparisons++;
                    if (cmp(aux[i], aux[j]) <= 0)
                    {
                        a[k] = aux[i++];
                    }
                    else
                    {
                        a[k] = aux[j++];
                    }
                    moves++;
                }
            }
        }

        // b = 2: Швидке сортування, опорний — середній елемент ділянки (розбиття Хоара)
        public static void QuickSortMiddle<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            if (a.Length <= 1) return;
            QuickSortMidRec(a, 0, a.Length - 1, cmp, ref comparisons, ref moves);
        }

        private static void QuickSortMidRec<T>(T[] a, int lo, int hi, Func<T, T, int> cmp, ref int comparisons, ref int moves)
        {
            if (lo >= hi) return;
            T pivot = a[lo + (hi - lo) / 2];
            int i = lo, j = hi;

            while (i <= j)
            {
                while (true)
                {
                    comparisons++;
                    if (cmp(a[i], pivot) < 0) i++;
                    else break;
                }
                while (true)
                {
                    comparisons++;
                    if (cmp(a[j], pivot) > 0) j--;
                    else break;
                }
                if (i <= j)
                {
                    if (i != j)
                    {
                        T tmp = a[i];
                        a[i] = a[j];
                        a[j] = tmp;
                        moves += 2;
                    }
                    i++;
                    j--;
                }
            }

            if (lo < j) QuickSortMidRec(a, lo, j, cmp, ref comparisons, ref moves);
            if (i < hi) QuickSortMidRec(a, i, hi, cmp, ref comparisons, ref moves);
        }

        // b = 3: Швидке сортування, опорний — медіана трьох (окрема функція з підрахунком порівнянь)
        public static void QuickSortMedianOfThree<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            if (a.Length <= 1) return;
            QuickSortMed3Rec(a, 0, a.Length - 1, cmp, ref comparisons, ref moves, 0);
        }

        // Окрема функція обчислення медіани трьох (перший, середній, останній)
        private static T MedianOfThree<T>(T first, T middle, T last, Func<T, T, int> cmp, ref int comparisons)
        {
            comparisons++;
            if (cmp(first, middle) < 0)
            {
                comparisons++;
                if (cmp(middle, last) < 0) return middle;
                comparisons++;
                return cmp(first, last) < 0 ? last : first;
            }
            else
            {
                comparisons++;
                if (cmp(first, last) < 0) return first;
                comparisons++;
                return cmp(middle, last) < 0 ? last : middle;
            }
        }

        private static void QuickSortMed3Rec<T>(T[] a, int lo, int hi, Func<T, T, int> cmp, ref int comparisons, ref int moves, int insertionThreshold)
        {
            if (lo >= hi) return;

            if (insertionThreshold > 0 && (hi - lo + 1) <= insertionThreshold)
            {
                InsertionSortRange(a, lo, hi, cmp, ref comparisons, ref moves);
                return;
            }

            int mid = lo + (hi - lo) / 2;
            T pivot = MedianOfThree(a[lo], a[mid], a[hi], cmp, ref comparisons);

            int i = lo, j = hi;
            while (i <= j)
            {
                while (true)
                {
                    comparisons++;
                    if (cmp(a[i], pivot) < 0) i++;
                    else break;
                }
                while (true)
                {
                    comparisons++;
                    if (cmp(a[j], pivot) > 0) j--;
                    else break;
                }
                if (i <= j)
                {
                    if (i != j)
                    {
                        T tmp = a[i];
                        a[i] = a[j];
                        a[j] = tmp;
                        moves += 2;
                    }
                    i++;
                    j--;
                }
            }

            if (lo < j) QuickSortMed3Rec(a, lo, j, cmp, ref comparisons, ref moves, insertionThreshold);
            if (i < hi) QuickSortMed3Rec(a, i, hi, cmp, ref comparisons, ref moves, insertionThreshold);
        }

        // =========================================================================
        // ЗАВДАННЯ 3: Додаткові варіанти (таблиця C)
        // =========================================================================

        // c = 0: Сортування підрахунком (для невід'ємних цілих ключів)
        public static void CountingSort<T>(T[] a, Func<T, int> keySelector, int maxKey, out int comparisons, out int moves)
        {
            comparisons = 0; // Без порівнянь елементів між собою
            moves = 0;
            int n = a.Length;
            if (n <= 1) return;

            int[] count = new int[maxKey + 1];
            T[] output = new T[n];

            for (int i = 0; i < n; i++) count[keySelector(a[i])]++;
            for (int k = 1; k <= maxKey; k++) count[k] += count[k - 1];

            for (int i = n - 1; i >= 0; i--)
            {
                int key = keySelector(a[i]);
                output[--count[key]] = a[i];
                moves++;
            }

            for (int i = 0; i < n; i++)
            {
                a[i] = output[i];
                moves++;
            }
        }

        // c = 1: Сортування вставками з бінарним пошуком позиції
        public static void BinaryInsertionSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;

            for (int i = 1; i < n; i++)
            {
                T key = a[i];
                int lo = 0, hi = i;

                while (lo < hi)
                {
                    int mid = lo + (hi - lo) / 2;
                    comparisons++;
                    if (cmp(a[mid], key) <= 0) lo = mid + 1;
                    else hi = mid;
                }

                if (lo < i)
                {
                    for (int j = i; j > lo; j--)
                    {
                        a[j] = a[j - 1];
                        moves++;
                    }
                    a[lo] = key;
                    moves++;
                }
            }
        }

        // c = 2: Гібрид (алгоритм із завдання 2 + вставки для ділянок <= threshold)
        public static void HybridSort<T>(T[] a, Func<T, T, int> cmp, int threshold, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            if (a.Length <= 1) return;
            QuickSortMed3Rec(a, 0, a.Length - 1, cmp, ref comparisons, ref moves, threshold);
        }

        // c = 3: Сортування Шелла (послідовність Кнута: 1, 4, 13, 40, ...)
        public static void ShellSort<T>(T[] a, Func<T, T, int> cmp, out int comparisons, out int moves)
        {
            comparisons = 0;
            moves = 0;
            int n = a.Length;
            int gap = 1;
            while (gap < n / 3) gap = 3 * gap + 1;

            while (gap >= 1)
            {
                for (int i = gap; i < n; i++)
                {
                    T temp = a[i];
                    int j = i;
                    bool shifted = false;

                    while (j >= gap)
                    {
                        comparisons++;
                        if (cmp(a[j - gap], temp) > 0)
                        {
                            a[j] = a[j - gap];
                            moves++;
                            shifted = true;
                            j -= gap;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (shifted)
                    {
                        a[j] = temp;
                        moves++;
                    }
                }
                gap /= 3;
            }
        }
    }
}