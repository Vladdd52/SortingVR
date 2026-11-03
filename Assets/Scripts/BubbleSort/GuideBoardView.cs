using System.Text;
using TMPro;
using UnityEngine;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Shows the BubbleSortGuide state on the floating guide board.
    /// Redraws only when the guide reports a change.
    /// </summary>
    public sealed class GuideBoardView : MonoBehaviour
    {
        [SerializeField] private BubbleSortGuideDriver driver;

        [Header("Texts")]
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text progress;
        [SerializeField] private TMP_Text slotStrip;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text instruction;

        [Header("Colors")]
        [SerializeField] private Color stepColor = new Color(1f, 0.851f, 0.239f);   // #FFD93D
        [SerializeField] private Color issueColor = new Color(0.949f, 0.6f, 0.29f); // #F2994A
        [SerializeField] private Color doneColor = new Color(0.435f, 0.812f, 0.592f); // #6FCF97

        private const string PairColorTag = "<color=#FFD93D><b>";
        private const string SortedColorTag = "<color=#6FCF97>";
        private const string IdleColorTag = "<color=#7F8C99>";

        private readonly StringBuilder sb = new StringBuilder(256);
        private BubbleSortGuide guide;

        private void Start()
        {
            guide = driver.Guide;
            guide.Changed += Render;
            Render();
        }

        private void OnDestroy()
        {
            if (guide != null)
            {
                guide.Changed -= Render;
            }
        }

        private void Render()
        {
            int l = guide.LeftSlot;
            int r = guide.RightSlot;
            bool done = guide.IsDone;
            bool hasIssue = guide.Issue != GuideIssue.None;

            // Title
            sb.Clear();
            if (done) sb.Append("SORTED!");
            else sb.Append("PASS ").Append(guide.Pass);
            title.SetText(sb);

            // Progress counters
            sb.Clear();
            sb.Append("Comparisons: ").Append(guide.ComparisonCount)
              .Append("     Swaps: ").Append(guide.SwapCount);
            progress.SetText(sb);

            // Slot strip: current pair in yellow, final slots in green.
            sb.Clear();
            for (int i = 0; i < guide.SlotCount; i++)
            {
                if (i > 0) sb.Append("   ");
                if (i >= guide.SortedFrom) sb.Append(SortedColorTag).Append(i).Append("</color>");
                else if (!done && (i == l || i == r)) sb.Append(PairColorTag).Append(i).Append("</b></color>");
                else sb.Append(IdleColorTag).Append(i).Append("</color>");
            }
            slotStrip.SetText(sb);

            // Label
            if (done)
            {
                label.SetText("DONE");
                label.color = doneColor;
            }
            else if (hasIssue)
            {
                label.SetText("OOPS");
                label.color = issueColor;
            }
            else
            {
                label.SetText("NEXT STEP");
                label.color = stepColor;
            }

            // Instruction
            sb.Clear();
            if (done)
            {
                sb.Append("All tubes are in order. Pass ").Append(guide.Pass)
                  .Append(guide.LastPassSwaps == 0
                      ? " made no swaps, so the sort is finished."
                      : " placed the last tube, so the sort is finished.");
            }
            else if (hasIssue)
            {
                AppendIssue(l, r);
            }
            else
            {
                AppendStep(l, r);
            }
            instruction.SetText(sb);
        }

        private void AppendStep(int l, int r)
        {
            switch (guide.Step)
            {
                case GuideStep.PlaceOnScale:
                    if (l == 0 && guide.Pass > 1)
                    {
                        sb.Append("Pass ").Append(guide.Pass - 1)
                          .Append(" done: the heaviest tube reached Slot ").Append(guide.SortedFrom).Append(". ");
                    }
                    sb.Append("Put the tube from <b>Slot ").Append(l).Append("</b> on the LEFT pan and the tube from <b>Slot ")
                      .Append(r).Append("</b> on the RIGHT pan.");
                    break;
                case GuideStep.ReturnSwapped:
                    sb.Append("The left tube is heavier: <b>swap</b>. Put it into <b>Slot ").Append(r)
                      .Append("</b> and the other tube into <b>Slot ").Append(l).Append("</b>.");
                    break;
                case GuideStep.ReturnAsIs:
                    sb.Append("The left tube is not heavier: <b>no swap</b>. Return each tube to its own slot (")
                      .Append(l).Append(" and ").Append(r).Append(").");
                    break;
            }
        }

        private void AppendIssue(int l, int r)
        {
            switch (guide.Issue)
            {
                case GuideIssue.WrongTubeOnScale:
                    sb.Append("Only the tubes from Slot ").Append(l).Append(" and Slot ").Append(r)
                      .Append(" go on the scale. Take the other tube off.");
                    break;
                case GuideIssue.PansMixedUp:
                    sb.Append("Pans mixed up: the tube from Slot ").Append(l).Append(" goes on the LEFT pan, from Slot ")
                      .Append(r).Append(" on the RIGHT.");
                    break;
                case GuideIssue.OtherSlotTouched:
                    sb.Append("Slot ").Append(guide.IssueSlot).Append(" needs its tube back. Right now only Slot ")
                      .Append(l).Append(" and Slot ").Append(r).Append(" are in play.");
                    break;
                case GuideIssue.CompareFirst:
                    sb.Append("Weigh first! Put the tubes from Slot ").Append(l).Append(" and Slot ").Append(r)
                      .Append(" on the scale before you swap them.");
                    break;
                case GuideIssue.WrongReturn:
                    if (guide.Step == GuideStep.ReturnSwapped)
                    {
                        sb.Append("Not quite: the heavier tube goes into Slot ").Append(r)
                          .Append(", the lighter one into Slot ").Append(l).Append(".");
                    }
                    else
                    {
                        sb.Append("Not quite: no swap here. Put each tube back into its own slot (")
                          .Append(l).Append(" and ").Append(r).Append(").");
                    }
                    break;
            }
        }
    }
}
