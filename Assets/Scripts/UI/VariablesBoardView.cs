using System.Text;
using SortingVR.Core;
using TMPro;
using UnityEngine;

namespace SortingVR.UI
{
    /// <summary>
    /// Shows the algorithm's variables on a small world space board, one "name = value" per row,
    /// monospaced with the "=" signs aligned. Values that just changed are tinted, final array
    /// elements are green. Redraws only when the values or the language change.
    /// </summary>
    public sealed class VariablesBoardView : MonoBehaviour
    {
        [SerializeField] private CodeTraceSource source;
        [SerializeField] private CodeLanguage language = CodeLanguage.Python;

        [SerializeField] private TMP_Text body;

        [Header("Look")]
        [Tooltip("Forced character width as a fraction of the font size (TMP mspace). 0 = off, for a real monospace font.")]
        [SerializeField, Min(0f)] private float charWidthEm;
        [SerializeField] private string nameColor = "#C2E0F5";
        [SerializeField] private string changedColor = "#FFD93D";
        [SerializeField] private string finalColor = "#6FCF97";

        private readonly StringBuilder sb = new StringBuilder(512);
        private IVariableWatch watch;
        private string[] rowPrefixes;
        private string header;
        private string changedTag;
        private string finalTag;

        private void Start()
        {
            watch = source.Variables;

            int width = 0;
            for (int i = 0; i < watch.Count; i++)
            {
                if (watch.GetName(i).Length > width) width = watch.GetName(i).Length;
            }

            // Prefixes are built once: colored name, padding, " = ".
            rowPrefixes = new string[watch.Count];
            for (int i = 0; i < watch.Count; i++)
            {
                string name = watch.GetName(i);
                rowPrefixes[i] = "<color=" + nameColor + ">" + name.Replace(' ', ' ') + "</color>"
                                 + new string(' ', width - name.Length) + " = ";
            }

            header = charWidthEm > 0f
                ? "<mspace=" + charWidthEm.ToString(System.Globalization.CultureInfo.InvariantCulture) + "em>"
                : string.Empty;
            changedTag = "<color=" + changedColor + ">";
            finalTag = "<color=" + finalColor + ">";

            watch.Changed += Render;
            Render();
        }

        private void OnDestroy()
        {
            if (watch != null)
            {
                watch.Changed -= Render;
            }
        }

        /// <summary>Switches the value syntax (True/False vs 1/0, [..] vs {..}).</summary>
        public void SetLanguage(CodeLanguage value)
        {
            if (value == language)
            {
                return;
            }

            language = value;
            if (watch != null)
            {
                Render();
            }
        }

        private void Render()
        {
            sb.Clear();
            sb.Append(header);
            for (int i = 0; i < rowPrefixes.Length; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(rowPrefixes[i]);
                watch.AppendValue(i, language, sb, changedTag, finalTag);
            }

            body.SetText(sb);
        }
    }
}
