using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SortingVR.Core
{
    /// <summary>
    /// Fake liquid for a cylindrical container (used with the SortingVR/Liquid shader).
    /// The liquid renderer is a full-height inner cylinder; the shader clips it at a world-space plane.
    /// The plane goes through the container's axis at the fill height (value x unit height), which keeps
    /// the volume of a cylinder exact when tilted, and stays level with the world. Moving the container
    /// makes the surface slosh (a damped spring driven by acceleration, tilt capped so the value stays readable).
    /// Runs only while the item is held or still settling, then sleeps; grab/socket events wake it.
    /// In the editor (not playing) it just shows the level surface.
    /// </summary>
    [ExecuteAlways]
    public sealed class LiquidWobble : MonoBehaviour
    {
        [SerializeField] private SortableItem item;
        [SerializeField] private Renderer liquid;
        [SerializeField] private XRBaseInteractable interactable;

        [Header("Fill (local units of this object, before scale)")]
        [Tooltip("Height of the liquid's bottom above the pivot.")]
        [SerializeField] private float bottom = 0.003f;
        [Tooltip("Liquid height per value unit.")]
        [SerializeField] private float unitHeight = 0.025f;

        [Header("Slosh")]
        [Tooltip("Largest surface tilt, in degrees. Small keeps the level readable.")]
        [SerializeField, Range(0f, 30f)] private float maxTilt = 12f;
        [Tooltip("Spring stiffness (higher = faster sloshing).")]
        [SerializeField, Min(1f)] private float stiffness = 60f;
        [Tooltip("Damping (higher = settles sooner).")]
        [SerializeField, Min(0f)] private float damping = 5f;
        [Tooltip("How strongly accelerating the container moves the liquid (1 = physical).")]
        [SerializeField, Min(0f)] private float accelerationGain = 1f;
        [Tooltip("How strongly turning the container moves the liquid.")]
        [SerializeField, Min(0f)] private float turnGain = 0.6f;
        [Tooltip("Seconds without movement before the effect goes to sleep.")]
        [SerializeField, Min(0.1f)] private float sleepAfter = 1f;

        private const float Gravity = 9.81f;
        private const float MaxAcceleration = 40f; // ignores tracking spikes

        private static readonly int PlanePointId = Shader.PropertyToID("_PlanePoint");
        private static readonly int PlaneNormalId = Shader.PropertyToID("_PlaneNormal");

        private Transform self;
        private MaterialPropertyBlock block;
        private Vector2 tilt;
        private Vector2 tiltVelocity;
        private Vector3 lastPosition;
        private Vector3 lastVelocity;
        private Vector3 lastUp;
        private float stillTime;
        private bool hasHistory;
        private bool subscribed;

        private void OnEnable()
        {
            self = transform;
            if (block == null) block = new MaterialPropertyBlock();
            hasHistory = false;
            stillTime = 0f;
            ApplyPlane(Vector3.up);

            if (Application.isPlaying && !subscribed && interactable != null)
            {
                interactable.selectEntered.AddListener(OnSelectEntered);
                interactable.selectExited.AddListener(OnSelectExited);
                subscribed = true;
            }
        }

        private void OnDestroy()
        {
            if (subscribed && interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnSelectEntered);
                interactable.selectExited.RemoveListener(OnSelectExited);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && isActiveAndEnabled)
            {
                self = transform;
                if (block == null) block = new MaterialPropertyBlock();
                ApplyPlane(Vector3.up);
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args) => Wake();

        private void OnSelectExited(SelectExitEventArgs args) => Wake();

        private void Wake()
        {
            if (!enabled)
            {
                enabled = true; // OnEnable resets the motion history
            }

            stillTime = 0f;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                ApplyPlane(Vector3.up);
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Vector3 position = self.position;
            Vector3 up = self.up;

            if (!hasHistory)
            {
                lastPosition = position;
                lastVelocity = Vector3.zero;
                lastUp = up;
                hasHistory = true;
            }

            Vector3 velocity = (position - lastPosition) / dt;
            Vector3 acceleration = Vector3.ClampMagnitude((velocity - lastVelocity) / dt, MaxAcceleration);
            Vector3 turn = (up - lastUp) / dt;
            lastPosition = position;
            lastVelocity = velocity;
            lastUp = up;

            // In the container's frame the liquid feels gravity minus acceleration: the surface tilts
            // toward the acceleration (tan = a / g). The spring pulls the tilt toward that target.
            Vector2 target = new Vector2(acceleration.x, acceleration.z) * (accelerationGain / Gravity)
                             + new Vector2(turn.x, turn.z) * (turnGain * 0.1f);
            tiltVelocity += (stiffness * (target - tilt) - damping * tiltVelocity) * dt;
            tilt += tiltVelocity * dt;

            float maxTan = Mathf.Tan(maxTilt * Mathf.Deg2Rad);
            if (tilt.sqrMagnitude > maxTan * maxTan)
            {
                tilt = tilt.normalized * maxTan;
            }

            ApplyPlane(new Vector3(tilt.x, 1f, tilt.y).normalized);

            // Sleep once nothing moves and the surface has settled (never while held in a hand).
            bool moving = velocity.sqrMagnitude > 0.0004f || turn.sqrMagnitude > 0.0004f;
            bool settled = tilt.sqrMagnitude < 1e-6f && tiltVelocity.sqrMagnitude < 1e-5f;
            if (!moving && settled && !IsHeld())
            {
                stillTime += dt;
                if (stillTime >= sleepAfter)
                {
                    tilt = Vector2.zero;
                    tiltVelocity = Vector2.zero;
                    ApplyPlane(Vector3.up);
                    enabled = false;
                }
            }
            else
            {
                stillTime = 0f;
            }
        }

        private bool IsHeld()
        {
            if (interactable == null)
            {
                return false;
            }

            var selecting = interactable.interactorsSelecting;
            for (int i = 0; i < selecting.Count; i++)
            {
                if (!(selecting[i] is XRSocketInteractor))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyPlane(Vector3 normal)
        {
            if (liquid == null || item == null || self == null)
            {
                return;
            }

            Vector3 point = self.TransformPoint(0f, bottom + item.Value * unitHeight, 0f);
            liquid.GetPropertyBlock(block);
            block.SetVector(PlanePointId, point);
            block.SetVector(PlaneNormalId, normal);
            liquid.SetPropertyBlock(block);
        }
    }
}
