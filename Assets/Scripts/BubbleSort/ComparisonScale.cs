using SortingVR.Core;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Analytical balance with two test tube holders (sockets). When both holders have a tube,
    /// the lamp above the heavier (larger value) tube lights up and the display shows
    /// "&gt;" (left heavier), "&lt;" (right heavier) or "=". Only the direction is shown,
    /// never the size of the difference. Purely event driven: no per-frame work.
    /// </summary>
    public sealed class ComparisonScale : MonoBehaviour
    {
        private const string LeftHeavierText = ">";
        private const string RightHeavierText = "<";
        private const string EqualText = "=";
        private const string IdleText = "--";

        [Header("Holders (sockets)")]
        [SerializeField] private XRSocketInteractor leftPan;
        [SerializeField] private XRSocketInteractor rightPan;

        [Header("Lamps above the holders")]
        [SerializeField] private Renderer leftLamp;
        [SerializeField] private Renderer rightLamp;
        [SerializeField] private Material lampOn;
        [SerializeField] private Material lampOff;

        [Header("Display")]
        [SerializeField] private TMP_Text display;

        private readonly ValueComparer comparer = new ValueComparer();
        private string shownText;

        public ValueComparer Comparer => comparer;

        private void Awake()
        {
            leftPan.selectEntered.AddListener(OnPanEntered);
            leftPan.selectExited.AddListener(OnPanExited);
            rightPan.selectEntered.AddListener(OnPanEntered);
            rightPan.selectExited.AddListener(OnPanExited);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (leftPan != null)
            {
                leftPan.selectEntered.RemoveListener(OnPanEntered);
                leftPan.selectExited.RemoveListener(OnPanExited);
            }

            if (rightPan != null)
            {
                rightPan.selectEntered.RemoveListener(OnPanEntered);
                rightPan.selectExited.RemoveListener(OnPanExited);
            }
        }

        private void OnPanEntered(SelectEnterEventArgs args) => Refresh();

        private void OnPanExited(SelectExitEventArgs args) => Refresh();

        private void Refresh()
        {
            SortableItem left = GetItem(leftPan);
            SortableItem right = GetItem(rightPan);

            if (left != null && right != null)
            {
                int result = comparer.Compare(left.Value, right.Value);
                SetLamps(result >= 0, result <= 0);
                SetDisplay(result > 0 ? LeftHeavierText : (result < 0 ? RightHeavierText : EqualText));
            }
            else
            {
                SetLamps(false, false);
                SetDisplay(IdleText);
            }
        }

        private void SetLamps(bool leftOn, bool rightOn)
        {
            leftLamp.sharedMaterial = leftOn ? lampOn : lampOff;
            rightLamp.sharedMaterial = rightOn ? lampOn : lampOff;
        }

        private void SetDisplay(string text)
        {
            // Update the text only when it changes.
            if (display == null || ReferenceEquals(text, shownText))
            {
                return;
            }

            shownText = text;
            display.text = text;
        }

        private static SortableItem GetItem(XRSocketInteractor pan)
        {
            if (!pan.hasSelection)
            {
                return null;
            }

            pan.interactablesSelected[0].transform.TryGetComponent(out SortableItem item);
            return item;
        }
    }
}
