using UnityEngine;

namespace SortingVR.Core
{
    /// <summary>
    /// The value carried by a sortable object (for example, a test tube).
    /// Shared by all sorting rooms.
    /// </summary>
    public sealed class SortableItem : MonoBehaviour
    {
        [SerializeField, Min(0)] private int value;

        public int Value => value;
    }
}
