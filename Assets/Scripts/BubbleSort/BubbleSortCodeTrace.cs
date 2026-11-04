using System;
using SortingVR.Core;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Maps the BubbleSortGuide state to the bubble sort code line the algorithm is at.
    /// Pure C#: listens to guide.Changed and raises Changed only when a line changes.
    /// </summary>
    public sealed class BubbleSortCodeTrace : ICodeTrace, IDisposable
    {
        private readonly BubbleSortGuide guide;

        public BubbleSortCodeTrace(BubbleSortGuide guide)
        {
            this.guide = guide ?? throw new ArgumentNullException(nameof(guide));
            guide.Changed += Update;
            ActiveLine = -1;
            HintLine = -1;
            Compute(out int active, out int hint);
            ActiveLine = active;
            HintLine = hint;
        }

        public int ActiveLine { get; private set; }

        public int HintLine { get; private set; }

        public event Action Changed;

        public void Dispose()
        {
            guide.Changed -= Update;
        }

        private void Update()
        {
            Compute(out int active, out int hint);
            if (active == ActiveLine && hint == HintLine)
            {
                return;
            }

            ActiveLine = active;
            HintLine = hint;
            Changed?.Invoke();
        }

        private void Compute(out int active, out int hint)
        {
            hint = -1;
            switch (guide.Step)
            {
                case GuideStep.Done:
                    // No swaps in the last pass: left by break. Otherwise the outer loop ran out.
                    active = (int)(guide.LastPassSwaps == 0 ? BubbleSortCodeLine.Break : BubbleSortCodeLine.OuterLoop);
                    break;

                case GuideStep.ReturnSwapped:
                    active = (int)BubbleSortCodeLine.Swap;
                    break;

                case GuideStep.ReturnAsIs:
                    // Condition was false: the inner loop moves on to the next pair.
                    active = (int)BubbleSortCodeLine.InnerLoop;
                    break;

                default:
                    active = (int)BubbleSortCodeLine.Compare;
                    if (guide.LeftSlot == 0)
                    {
                        // A pass has just started.
                        hint = (int)BubbleSortCodeLine.ResetSwapped;
                    }
                    break;
            }
        }
    }
}
