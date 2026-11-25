using System;
using System.Collections.Generic;

namespace OpamLab09
{
    public class Student
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public int Score { get; set; }

        public Student(string name, string group, int score)
        {
            Name = name;
            Group = group;
            Score = score;
        }

        public override string ToString() => $"{Name} ({Group}, {Score})";
    }

    public static class Validation
    {
        // Перевірка 1: масив впорядкований згідно з cmp
        public static bool IsSorted<T>(T[] a, Func<T, T, int> cmp)
        {
            for (int i = 1; i < a.Length; i++)
            {
                if (cmp(a[i - 1], a[i]) > 0) return false;
            }
            return true;
        }

        // Перевірка 2: вихідний масив є точною перестановкою вхідного (мультимножина елементів збігається)
        public static bool IsPermutationOf<T>(T[] sorted, T[] original) where T : notnull
        {
            if (sorted.Length != original.Length) return false;

            var counts = new Dictionary<T, int>(original.Length);
            foreach (var item in original)
            {
                counts.TryGetValue(item, out int c);
                counts[item] = c + 1;
            }

            foreach (var item in sorted)
            {
                if (!counts.TryGetValue(item, out int c) || c == 0) return false;
                counts[item] = c - 1;
            }

            return true;
        }

        // Генерація 4 типів вхідних масивів
        public static int[] GenerateRandom(int n, int seed, int K)
        {
            var rng = new Random(seed);
            int[] arr = new int[n];
            int maxVal = 20 * K;
            for (int i = 0; i < n; i++) arr[i] = rng.Next(0, maxVal + 1);
            return arr;
        }

        public static int[] MakeSorted(int[] randomBase)
        {
            int[] copy = (int[])randomBase.Clone();
            Array.Sort(copy);
            return copy;
        }

        public static int[] MakeReversed(int[] randomBase)
        {
            int[] copy = MakeSorted(randomBase);
            Array.Reverse(copy);
            return copy;
        }

        public static int[] MakeNearlySorted(int[] randomBase, int seed, int K)
        {
            int[] copy = MakeSorted(randomBase);
            if (copy.Length <= 1) return copy;
            var rng = new Random(seed + 555);
            for (int i = 0; i < K; i++)
            {
                int idx1 = rng.Next(0, copy.Length);
                int idx2 = rng.Next(0, copy.Length);
                (copy[idx1], copy[idx2]) = (copy[idx2], copy[idx1]);
            }
            return copy;
        }
    }
}