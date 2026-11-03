using System;
using SortingVR.Core;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Pure C# bubble sort tutor. It keeps the algorithm state (pass, current pair, swaps)
    /// and, given a snapshot of the rack and the scale, decides what the player must do next
    /// or which mistake to point out. Items are identified by integer ids; Empty means nothing.
    /// Event driven: call Evaluate whenever the rack or the scale changes.
    /// </summary>
    public sealed class BubbleSortGuide
    {
        public const int Empty = -1;

        private readonly int[] valueById;
        private readonly int[] order;
        private readonly ValueComparer comparer = new ValueComparer();

        /// <summary>Raised when anything the player should see has changed.</summary>
        public event Action Changed;

        /// <summary>Raised when a pass ends: (pass number, swaps made in it).</summary>
        public event Action<int, int> PassCompleted;

        /// <summary>Raised once, when the row is sorted.</summary>
        public event Action Completed;

        public BubbleSortGuide(int[] initialOrder, int[] valueById)
        {
            if (initialOrder == null) throw new ArgumentNullException(nameof(initialOrder));
            if (valueById == null) throw new ArgumentNullException(nameof(valueById));

            order = (int[])initialOrder.Clone();
            this.valueById = (int[])valueById.Clone();

            Pass = 1;
            SortedFrom = order.Length;
            IssueSlot = Empty;
            Step = order.Length < 2 ? GuideStep.Done : GuideStep.PlaceOnScale;
            if (Step == GuideStep.Done)
            {
                SortedFrom = 0;
            }
        }

        public int SlotCount => order.Length;

        /// <summary>Current pass, starting at 1.</summary>
        public int Pass { get; private set; }

        /// <summary>Left slot of the pair being compared.</summary>
        public int LeftSlot { get; private set; }

        public int RightSlot => LeftSlot + 1;

        /// <summary>Slots with an index at or above this are in their final place.</summary>
        public int SortedFrom { get; private set; }

        public GuideStep Step { get; private set; }

        public GuideIssue Issue { get; private set; }

        /// <summary>The slot the current issue is about, or Empty.</summary>
        public int IssueSlot { get; private set; }

        public int ComparisonCount => comparer.ComparisonCount;

        public int SwapCount { get; private set; }

        public int SwapsThisPass { get; private set; }

        /// <summary>Swaps made in the last finished pass (0 before the first one ends).</summary>
        public int LastPassSwaps { get; private set; }

        public bool IsDone => Step == GuideStep.Done;

        /// <summary>The id of the item that is committed to a slot by the algorithm so far.</summary>
        public int ItemInSlot(int slot) => order[slot];

        /// <summary>
        /// Updates the guide from the current state of the world.
        /// slots[i] is the item id in rack slot i (or Empty); leftPan / rightPan are the ids on the scale.
        /// </summary>
        public void Evaluate(int[] slots, int leftPan, int rightPan)
        {
            if (slots == null || slots.Length != order.Length)
            {
                throw new ArgumentException("Snapshot size must match the slot count.", nameof(slots));
            }

            if (IsDone)
            {
                return;
            }

            GuideStep oldStep = Step;
            GuideIssue oldIssue = Issue;
            int oldIssueSlot = IssueSlot;
            int oldLeft = LeftSlot;
            int oldPass = Pass;

            GuideIssue issue = GuideIssue.None;
            int issueSlot = Empty;

            int a = order[LeftSlot];
            int b = order[RightSlot];

            // Only the current pair may be on the scale.
            if ((leftPan != Empty && leftPan != a && leftPan != b) ||
                (rightPan != Empty && rightPan != a && rightPan != b))
            {
                issue = GuideIssue.WrongTubeOnScale;
            }

            // Every other slot must keep its tube.
            if (issue == GuideIssue.None)
            {
                for (int k = 0; k < order.Length; k++)
                {
                    if (k != LeftSlot && k != RightSlot && slots[k] != order[k])
                    {
                        issue = GuideIssue.OtherSlotTouched;
                        issueSlot = k;
                        break;
                    }
                }
            }

            int leftNow = slots[LeftSlot];
            int rightNow = slots[RightSlot];

            if (Step == GuideStep.PlaceOnScale)
            {
                if (leftPan == a && rightPan == b)
                {
                    int result = comparer.Compare(valueById[a], valueById[b]);
                    Step = result > 0 ? GuideStep.ReturnSwapped : GuideStep.ReturnAsIs;
                }
                else if (issue == GuideIssue.None && leftPan == b && rightPan == a)
                {
                    issue = GuideIssue.PansMixedUp;
                }
                else if (issue == GuideIssue.None &&
                         ((leftNow != Empty && leftNow != a) || (rightNow != Empty && rightNow != b)))
                {
                    issue = GuideIssue.CompareFirst;
                }
            }
            else
            {
                bool swap = Step == GuideStep.ReturnSwapped;
                int expectLeft = swap ? b : a;
                int expectRight = swap ? a : b;

                if (leftNow == expectLeft && rightNow == expectRight)
                {
                    if (issue == GuideIssue.None)
                    {
                        CommitPair(swap);
                    }
                }
                else if (issue == GuideIssue.None)
                {
                    if (leftNow != Empty && leftNow != expectLeft)
                    {
                        issue = GuideIssue.WrongReturn;
                        issueSlot = LeftSlot;
                    }
                    else if (rightNow != Empty && rightNow != expectRight)
                    {
                        issue = GuideIssue.WrongReturn;
                        issueSlot = RightSlot;
                    }
                }
            }

            Issue = issue;
            IssueSlot = issueSlot;

            if (Step != oldStep || Issue != oldIssue || IssueSlot != oldIssueSlot ||
                LeftSlot != oldLeft || Pass != oldPass)
            {
                Changed?.Invoke();
            }
        }

        private void CommitPair(bool swap)
        {
            if (swap)
            {
                int t = order[LeftSlot];
                order[LeftSlot] = order[RightSlot];
                order[RightSlot] = t;
                SwapCount++;
                SwapsThisPass++;
            }

            LeftSlot++;
            Step = GuideStep.PlaceOnScale;

            // In pass p the pairs end at slot (n - p): the tail is already final.
            int lastRight = order.Length - Pass;
            if (RightSlot <= lastRight)
            {
                return;
            }

            int finishedPass = Pass;
            int swaps = SwapsThisPass;
            LastPassSwaps = swaps;
            SortedFrom = lastRight;

            if (swaps == 0 || lastRight <= 1)
            {
                SortedFrom = 0;
                LeftSlot = 0;
                Step = GuideStep.Done;
                PassCompleted?.Invoke(finishedPass, swaps);
                Completed?.Invoke();
                return;
            }

            Pass++;
            LeftSlot = 0;
            SwapsThisPass = 0;
            PassCompleted?.Invoke(finishedPass, swaps);
        }
    }
}
