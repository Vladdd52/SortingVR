using System.Collections.Generic;
using SortingVR.Core;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// XR layer for BubbleSortGuide: listens to the rack slots and the scale holders,
    /// builds a snapshot of which tube is where and feeds it to the guide.
    /// Event driven: no per-frame work, no allocations after Awake.
    /// </summary>
    public sealed class BubbleSortGuideDriver : MonoBehaviour
    {
        [Tooltip("Rack slots, left to right from the player's view.")]
        [SerializeField] private XRSocketInteractor[] slots;

        [Tooltip("Tubes in their starting order: tubes[i] starts in slots[i].")]
        [SerializeField] private SortableItem[] tubes;

        [SerializeField] private XRSocketInteractor leftPan;
        [SerializeField] private XRSocketInteractor rightPan;

        private readonly Dictionary<Transform, int> idByTransform = new Dictionary<Transform, int>();
        private int[] snapshot;
        private BubbleSortGuide guide;

        public BubbleSortGuide Guide
        {
            get
            {
                if (guide == null)
                {
                    Build();
                }

                return guide;
            }
        }

        private void Awake()
        {
            if (guide == null)
            {
                Build();
            }

            for (int i = 0; i < slots.Length; i++)
            {
                Subscribe(slots[i]);
            }

            Subscribe(leftPan);
            Subscribe(rightPan);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Unsubscribe(slots[i]);
            }

            Unsubscribe(leftPan);
            Unsubscribe(rightPan);
        }

        private void Build()
        {
            int count = tubes.Length;
            var initialOrder = new int[count];
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                initialOrder[i] = i;
                values[i] = tubes[i].Value;
                idByTransform[tubes[i].transform] = i;
            }

            snapshot = new int[slots.Length];
            guide = new BubbleSortGuide(initialOrder, values);
        }

        private void Subscribe(XRSocketInteractor socket)
        {
            if (socket == null) return;
            socket.selectEntered.AddListener(OnSelectEntered);
            socket.selectExited.AddListener(OnSelectExited);
        }

        private void Unsubscribe(XRSocketInteractor socket)
        {
            if (socket == null) return;
            socket.selectEntered.RemoveListener(OnSelectEntered);
            socket.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args) => Evaluate();

        private void OnSelectExited(SelectExitEventArgs args) => Evaluate();

        private void Evaluate()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                snapshot[i] = GetId(slots[i]);
            }

            guide.Evaluate(snapshot, GetId(leftPan), GetId(rightPan));
        }

        private int GetId(XRSocketInteractor socket)
        {
            if (socket == null || !socket.hasSelection)
            {
                return BubbleSortGuide.Empty;
            }

            return idByTransform.TryGetValue(socket.interactablesSelected[0].transform, out int id)
                ? id
                : BubbleSortGuide.Empty;
        }
    }
}
