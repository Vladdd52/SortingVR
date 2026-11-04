using System;

namespace SortingVR.Core
{
    /// <summary>
    /// Source code of an algorithm in one language. Rows may contain TextMeshPro color tags
    /// for syntax highlighting; a literal "&lt;" is written as <see cref="LessThan"/>.
    /// Logical lines (language independent ids, defined by each algorithm) are mapped to rows,
    /// so the same trace can highlight the right rows in any language. A logical line may span
    /// several rows (e.g. a swap written as three statements).
    /// </summary>
    public sealed class CodeListing
    {
        /// <summary>A "&lt;" that TextMeshPro will not read as the start of a tag.</summary>
        public const string LessThan = "<noparse><</noparse>";

        private readonly string[] rows;
        private readonly int[] rowByLine;
        private readonly int[] spanByLine;

        /// <param name="fileName">Name shown above the code, e.g. "bubble_sort.py".</param>
        /// <param name="rows">Code rows, top to bottom.</param>
        /// <param name="rowByLine">rowByLine[logicalLine] = first row, or -1 if the line has no row.</param>
        /// <param name="spanByLine">Rows covered by each logical line; null means 1 for all.</param>
        public CodeListing(CodeLanguage language, string fileName, string[] rows, int[] rowByLine, int[] spanByLine = null)
        {
            Language = language;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            this.rows = rows ?? throw new ArgumentNullException(nameof(rows));
            this.rowByLine = rowByLine ?? throw new ArgumentNullException(nameof(rowByLine));
            if (spanByLine != null && spanByLine.Length != rowByLine.Length)
            {
                throw new ArgumentException("spanByLine must match rowByLine.", nameof(spanByLine));
            }

            this.spanByLine = spanByLine;
        }

        public CodeLanguage Language { get; }

        public string FileName { get; }

        public int RowCount => rows.Length;

        public string GetRow(int row) => rows[row];

        /// <summary>First row of a logical line, or -1 if it is unknown or has no row.</summary>
        public int RowOf(int logicalLine)
        {
            return logicalLine >= 0 && logicalLine < rowByLine.Length ? rowByLine[logicalLine] : -1;
        }

        /// <summary>Number of rows a logical line covers (0 if it has none).</summary>
        public int SpanOf(int logicalLine)
        {
            if (RowOf(logicalLine) < 0) return 0;
            return spanByLine == null ? 1 : spanByLine[logicalLine];
        }

        /// <summary>True if the row belongs to the logical line.</summary>
        public bool Covers(int logicalLine, int row)
        {
            int first = RowOf(logicalLine);
            return first >= 0 && row >= first && row < first + SpanOf(logicalLine);
        }
    }
}
