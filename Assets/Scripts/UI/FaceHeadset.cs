using UnityEngine;

namespace SortingVR.UI
{
    /// <summary>
    /// Turns this object so its front (-Z side, where world space text is read) faces the headset.
    /// Keeps the object upright (no roll). Uses a dead zone: the board stays perfectly still for small
    /// head movements (a still board keeps text crisp, a constantly re-aimed one shimmers) and only
    /// turns, smoothly, once the headset is more than deadZoneAngle off-axis, until it faces it again.
    /// </summary>
    public sealed class FaceHeadset : MonoBehaviour
    {
        [Tooltip("Headset camera. If empty, Camera.main is used.")]
        [SerializeField] private Transform target;

        [Tooltip("Only turn around the vertical axis (no tilting up or down).")]
        [SerializeField] private bool yawOnly;

        [Tooltip("How fast the board turns. 0 = snap instantly.")]
        [SerializeField, Min(0f)] private float turnSpeed = 6f;

        [Tooltip("The board starts turning only when the headset is more than this many degrees off-axis.")]
        [SerializeField, Range(0f, 45f)] private float deadZoneAngle = 12f;

        [Tooltip("While turning, the board stops once it is within this many degrees of facing the headset.")]
        [SerializeField, Range(0.1f, 5f)] private float settleAngle = 0.5f;

        private Transform self;
        private bool turning = true; // align once at start

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
            float angle = Quaternion.Angle(self.rotation, goal);

            if (!turning)
            {
                if (angle <= deadZoneAngle)
                {
                    return;
                }

                turning = true;
            }

            if (turnSpeed <= 0f || angle <= settleAngle)
            {
                self.rotation = goal;
                turning = false;
                return;
            }

            self.rotation = Quaternion.Slerp(self.rotation, goal, 1f - Mathf.Exp(-turnSpeed * Time.deltaTime));
        }
    }
}
