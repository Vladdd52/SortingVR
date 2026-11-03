using UnityEngine;

namespace SortingVR.UI
{
    /// <summary>
    /// Turns this object so its front (-Z side, where world space text is read) faces the headset.
    /// Keeps the object upright (no roll). Smoothly follows the head.
    /// </summary>
    public sealed class FaceHeadset : MonoBehaviour
    {
        [Tooltip("Headset camera. If empty, Camera.main is used.")]
        [SerializeField] private Transform target;

        [Tooltip("Only turn around the vertical axis (no tilting up or down).")]
        [SerializeField] private bool yawOnly;

        [Tooltip("How fast the board turns. 0 = snap instantly.")]
        [SerializeField, Min(0f)] private float turnSpeed = 6f;

        private Transform self;

        private void Start()
        {
            self = transform;
            if (target == null && Camera.main != null)
            {
                target = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 away = self.position - target.position;
            if (yawOnly)
            {
                away.y = 0f;
            }

            if (away.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion goal = Quaternion.LookRotation(away, Vector3.up);
            self.rotation = turnSpeed <= 0f
                ? goal
                : Quaternion.Slerp(self.rotation, goal, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }
    }
}
