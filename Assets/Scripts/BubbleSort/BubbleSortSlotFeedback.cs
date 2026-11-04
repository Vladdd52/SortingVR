using SortingVR.UI;
using UnityEngine;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Lights the rack slots from the bubble sort guide: green flash under both slots when a pair is put
    /// back correctly, red under the slot(s) of a placement mistake until it is fixed.
    /// Scale mistakes (wrong tube on the scale, pans mixed up) are not rack placements and do not light.
    /// Event driven, no allocations after Start.
    /// </summary>
    public sealed class BubbleSortSlotFeedback : MonoBehaviour
    {
        [SerializeField] private BubbleSortGuideDriver driver;
        [SerializeField] private SlotLights lights;

        private BubbleSortGuide guide;
        private bool[] wantBad;

        private void Start()
        {
            guide = driver.Guide;
            wantBad = new bool[lights.Count];
            guide.PairPlaced += OnPairPlaced;
            guide.Changed += OnChanged;
            OnChanged();
        }

        private void OnDestroy()
        {
            if (guide != null)
            {
                guide.PairPlaced -= OnPairPlaced;
                guide.Changed -= OnChanged;
            }
        }

        private void OnPairPlaced(int leftSlot, int rightSlot, bool swapped)
        {
            lights.FlashGood(leftSlot);
            lights.FlashGood(rightSlot);
        }

        private void OnChanged()
        {
            for (int i = 0; i < wantBad.Length; i++)
            {
                wantBad[i] = false;
            }

            switch (guide.Issue)
            {
                case GuideIssue.WrongReturn:
                case GuideIssue.OtherSlotTouched:
                    Mark(guide.IssueSlot);
                    break;

                case GuideIssue.CompareFirst:
                    // Swapped (or moved) without weighing: the whole pair is wrong.
                    Mark(guide.LeftSlot);
                    Mark(guide.RightSlot);
                    break;
            }

            for (int i = 0; i < wantBad.Length; i++)
            {
                lights.SetBad(i, wantBad[i]);
            }
        }

        private void Mark(int slot)
        {
            if (slot >= 0 && slot < wantBad.Length)
            {
                wantBad[slot] = true;
            }
        }
    }
}
