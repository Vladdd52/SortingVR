using System;

namespace SortingVR.Core
{
    /// <summary>
    /// Pure C# comparison of two values. Counts comparisons and reports each one,
    /// so counters, logs and visuals can subscribe without knowing about Unity.
    /// </summary>
    public sealed class ValueComparer
    {
        /// <summary>Raised after every comparison: (left, right, result).</summary>
        public event Action<int, int, int> Compared;

        public int ComparisonCount { get; private set; }

        /// <summary>
        /// Returns 1 if left is larger, -1 if right is larger, 0 if they are equal.
        /// </summary>
        public int Compare(int left, int right)
        {
            int result = left > right ? 1 : (left < right ? -1 : 0);
            ComparisonCount++;
            Compared?.Invoke(left, right, result);
            return result;
        }

        public void ResetCount()
        {
            ComparisonCount = 0;
        }
    }
}
