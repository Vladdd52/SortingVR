using System.Text;
using SortingVR.Core;
using TMPro;
using UnityEngine;

namespace SortingVR.UI
{
    /// <summary>
    /// Shows a code listing on a world space board and highlights the line the algorithm is at.
    /// Meant for a monospace font (optionally forced with the TMP mspace tag). The highlight is an opaque bar behind the text
    /// (one bar per highlight, sized from TMP line metrics, so multi-row lines have no gaps).
    /// Redraws only when the trace or the language changes.
    /// </summary>
    public sealed class CodeBoardView : MonoBehaviour
    {
        [SerializeField] private CodeTraceSource source;
        [SerializeField] private CodeLanguage language = CodeLanguage.Python;

        [Header("Texts")]
        [SerializeField] private TMP_Text fileLabel;
        [SerializeField] private TMP_Text codeText;

        [Header("Highlight bars (thin cubes, children of this board, behind the text)")]
        [SerializeField] private Transform activeBar;
        [SerializeField] private Transform hintBar;
        [Tooltip("Extra height added above and below a highlighted block, in meters.")]
        [SerializeField] private float barPadding = 0.004f;

        [Header("Look")]
        [Tooltip("Forced character width as a fraction of the font size (TMP mspace). 0 = off, for a real monospace font.")]
        [SerializeField, Min(0f)] private float charWidthEm;
        [SerializeField] private string lineNumberColor = "#5E6B78";

        private const char Pad = ' '; // non-breaking space keeps indentation

        private readonly StringBuilder sb = new StringBuilder(2048);
        private ICodeTrace trace;
        private CodeListing listing;
        private string[] rows;
        private string header;
        private Transform textTransform;

        public CodeLanguage Language => language;

        private void Start()
        {
            textTransform = codeText.transform;
            trace = source.Trace;
            trace.Changed += Render;
            BuildRows();
            Render();
        }

        private void OnDestroy()
        {
            if (trace != null)
            {
                trace.Changed -= Render;
            }
        }

        /// <summary>Switches the shown language (user action, not per frame).</summary>
        public void SetLanguage(CodeLanguage value)
        {
            if (value == language && listing != null)
            {
                return;
            }

            language = value;
            if (trace != null)
            {
                BuildRows();
                Render();
            }
        }

        // Builds the full text with line numbers. Allocates, so it runs only on start or language change.
        private void BuildRows()
        {
            listing = source.GetListing(language);
            fileLabel.SetText(listing.FileName);

            int count = listing.RowCount;
            var all = new StringBuilder(2048);
            if (charWidthEm > 0f)
            {
                all.Append("<mspace=")
                   .Append(charWidthEm.ToString(System.Globalization.CultureInfo.InvariantCulture))
                   .Append("em>");
            }
            for (int i = 0; i < count; i++)
            {
                if (i > 0) all.Append('\n');
                all.Append("<color=").Append(lineNumberColor).Append('>');
                if (i + 1 < 10) all.Append(Pad);
                all.Append(i + 1).Append("</color>").Append(Pad).Append(Pad);
                all.Append(listing.GetRow(i).Replace(' ', Pad));
            }

            header = all.ToString();
            codeText.SetText(header);
            codeText.ForceMeshUpdate();
        }

        private void Render()
        {
            PlaceBar(activeBar, trace.ActiveLine);
            PlaceBar(hintBar, trace.HintLine);
        }

        private void PlaceBar(Transform bar, int logicalLine)
        {
            if (bar == null)
            {
                return;
            }

            int first = listing.RowOf(logicalLine);
            int span = listing.SpanOf(logicalLine);
            TMP_TextInfo info = codeText.textInfo;
            if (first < 0 || span <= 0 || first + span > info.lineCount)
            {
                bar.gameObject.SetActive(false);
                return;
            }

            float top = info.lineInfo[first].ascender + barPadding;
            float bottom = info.lineInfo[first + span - 1].descender - barPadding;
            Rect rect = codeText.rectTransform.rect;

            // Text local -> board local (the text is an unrotated, unscaled child of the board).
            Vector3 origin = textTransform.localPosition;
            Vector3 scale = bar.localScale;
            bar.localPosition = new Vector3(origin.x + rect.center.x, origin.y + (top + bottom) * 0.5f, bar.localPosition.z);
            bar.localScale = new Vector3(rect.width, top - bottom, scale.z);
            bar.gameObject.SetActive(true);
        }
    }
}
