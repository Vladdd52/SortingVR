using UnityEngine;

namespace SortingVR.UI
{
    /// <summary>
    /// A row of glowing indicator discs, one per slot (e.g. under rack slots).
    /// A slot can be marked "bad" (stays lit until cleared) or flashed "good" (lit for a moment).
    /// Bad wins over good. Event driven: Update runs only while a good flash is counting down.
    /// Off lights have their renderer disabled.
    /// </summary>
    public sealed class SlotLights : MonoBehaviour
    {
        [Tooltip("One renderer per slot, in slot order.")]
        [SerializeField] private Renderer[] lights;

        [SerializeField] private Material goodMaterial;
        [SerializeField] private Material badMaterial;

        [Header("Fake glow (optional, same order as lights)")]
        [Tooltip("Additive light pool around each light, tinted with the light's color.")]
        [SerializeField] private Renderer[] pools;
        [Tooltip("Additive white-hot core on top of each light (one material for every color).")]
        [SerializeField] private Renderer[] cores;
        [SerializeField] private Material goodGlowMaterial;
        [SerializeField] private Material badGlowMaterial;
        [Tooltip("Haze sprite at the bottom of each item (faces the headset); its object is active only while lit.")]
        [SerializeField] private Renderer[] hazes;
        [SerializeField] private Material goodHazeMaterial;
        [SerializeField] private Material badHazeMaterial;

        [Tooltip("How long a good flash stays lit, in seconds.")]
        [SerializeField, Min(0.1f)] private float goodSeconds = 1.5f;

        private bool[] bad;
        private float[] goodUntil;

        public int Count => lights.Length;

        private void Awake()
        {
            bad = new bool[lights.Length];
            goodUntil = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                Refresh(i);
            }

            enabled = false;
        }

        /// <summary>Marks a slot as wrong (lit until set back to false).</summary>
        public void SetBad(int slot, bool value)
        {
            if (bad[slot] == value)
            {
                return;
            }

            bad[slot] = value;
            Refresh(slot);
        }

        /// <summary>Lights a slot as correct for a moment.</summary>
        public void FlashGood(int slot)
        {
            goodUntil[slot] = Time.time + goodSeconds;
            Refresh(slot);
            enabled = true;
        }

        /// <summary>Turns every light off.</summary>
        public void ClearAll()
        {
            for (int i = 0; i < lights.Length; i++)
            {
                bad[i] = false;
                goodUntil[i] = 0f;
                Refresh(i);
            }

            enabled = false;
        }

        private void Update()
        {
            float now = Time.time;
            bool pending = false;
            for (int i = 0; i < goodUntil.Length; i++)
            {
                if (goodUntil[i] <= 0f)
                {
                    continue;
                }

                if (goodUntil[i] <= now)
                {
                    goodUntil[i] = 0f;
                    Refresh(i);
                }
                else
                {
                    pending = true;
                }
            }

            if (!pending)
            {
                enabled = false;
            }
        }

        private void Refresh(int slot)
        {
            Renderer light = lights[slot];
            bool on;
            bool isBad = bad[slot];
            if (isBad)
            {
                light.sharedMaterial = badMaterial;
                on = true;
            }
            else if (goodUntil[slot] > 0f)
            {
                light.sharedMaterial = goodMaterial;
                on = true;
            }
            else
            {
                on = false;
            }

            light.enabled = on;

            if (pools != null && slot < pools.Length && pools[slot] != null)
            {
                pools[slot].sharedMaterial = isBad ? badGlowMaterial : goodGlowMaterial;
                pools[slot].enabled = on;
            }

            if (cores != null && slot < cores.Length && cores[slot] != null)
            {
                cores[slot].enabled = on;
            }

            if (hazes != null && slot < hazes.Length && hazes[slot] != null)
            {
                hazes[slot].sharedMaterial = isBad ? badHazeMaterial : goodHazeMaterial;
                hazes[slot].gameObject.SetActive(on); // its billboard script runs only while lit
            }
        }
    }
}
