using SortingVR.Core;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SortingVR.UI
{
    /// <summary>
    /// Shows the item's value in a small label above it while a hand or ray hovers over it
    /// (or holds it). Sockets and gaze do not count, so items resting in a rack stay unlabeled.
    /// Event driven; the label object is inactive while hidden and this component is disabled,
    /// so only a visible label runs LateUpdate (keeps it upright above the item, facing the headset).
    /// </summary>
    public sealed class TubeValueLabel : MonoBehaviour
    {
        [SerializeField] private SortableItem item;
        [SerializeField] private XRBaseInteractable interactable;

        [Tooltip("Label root (child of this object). Inactive while hidden.")]
        [SerializeField] private Transform label;
        [SerializeField] private TMP_Text text;

        [Tooltip("Height of the item's top above its pivot, in local units (before scale).")]
        [SerializeField] private float itemHeight = 0.25f;

        [Tooltip("Gap between the item's top and the label center, in meters.")]
        [SerializeField] private float gap = 0.06f;

        private Transform self;
        private Transform head;
        private bool subscribed;

        private void Awake()
        {
            self = transform;

            // Keep the label's world size independent of the item's scale.
            float scale = self.lossyScale.y;
            if (scale > 0.0001f)
            {
                label.localScale = Vector3.one / scale;
            }

            label.gameObject.SetActive(false);
            enabled = false;
            Subscribe();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                head = Camera.main.transform;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // Subscriptions live from Awake to OnDestroy: enabled toggles only the LateUpdate work.
        private void Subscribe()
        {
            if (subscribed) return;
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || interactable == null) return;
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
            subscribed = false;
        }

        private void OnHoverEntered(HoverEnterEventArgs args) => Refresh();

        private void OnHoverExited(HoverExitEventArgs args) => Refresh();

        private void OnSelectEntered(SelectEnterEventArgs args) => Refresh();

        private void OnSelectExited(SelectExitEventArgs args) => Refresh();

        private void Refresh()
        {
            bool show = HasHand();
            if (show == label.gameObject.activeSelf)
            {
                return;
            }

            if (show)
            {
                text.SetText("{0}", item.Value);
                Place();
            }

            label.gameObject.SetActive(show);
            enabled = show;
        }

        private bool HasHand()
        {
            var hovering = interactable.interactorsHovering;
            for (int i = 0; i < hovering.Count; i++)
            {
                if (IsHand(hovering[i])) return true;
            }

            var selecting = interactable.interactorsSelecting;
            for (int i = 0; i < selecting.Count; i++)
            {
                if (IsHand(selecting[i])) return true;
            }

            return false;
        }

        private static bool IsHand(IXRInteractor interactor)
        {
            return !(interactor is XRSocketInteractor) && !(interactor is XRGazeInteractor);
        }

        private void LateUpdate()
        {
            Place();
        }

        private void Place()
        {
            // Above the item's top, even when it is tilted in the hand.
            label.position = self.TransformPoint(0f, itemHeight, 0f) + Vector3.up * gap;
            if (head != null)
            {
                Vector3 away = label.position - head.position;
                away.y = 0f;
                if (away.sqrMagnitude > 0.0001f)
                {
                    label.rotation = Quaternion.LookRotation(away, Vector3.up);
                }
            }
        }
    }
}
