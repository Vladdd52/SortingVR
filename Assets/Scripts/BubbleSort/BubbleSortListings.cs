using SortingVR.Core;

namespace SortingVR.BubbleSort
{
    /// <summary>Bubble sort source code shown on the code board, with syntax colors.</summary>
    public static class BubbleSortListings
    {
        // Syntax colors (TextMeshPro rich text).
        private const string Kw = "<color=#A6E8D6>";   // keywords: bright mint
        private const string Fn = "<color=#C2E0F5>";   // builtins and numbers: bright soft blue
        private const string End = "</color>";

        private const string Lt = CodeListing.LessThan;

        private static CodeListing python;
        private static CodeListing c;

        public static CodeListing Get(CodeLanguage language)
        {
            switch (language)
            {
                case CodeLanguage.C:
                    return CListing;
                default:
                    return Python;
            }
        }

        private static CodeListing CListing
        {
            get
            {
                if (c != null)
                {
                    return c;
                }

                string[] rows =
                {
                    Kw + "void" + End + " bubble_sort(" + Kw + "int" + End + " a[], " + Kw + "int" + End + " n) {",
                    "    " + Kw + "for" + End + " (" + Kw + "int" + End + " p = " + Fn + "1" + End + "; p " + Lt + " n; p++) {",
                    "        " + Kw + "int" + End + " swapped = " + Fn + "0" + End + ";",
                    "        " + Kw + "for" + End + " (" + Kw + "int" + End + " i = " + Fn + "0" + End + "; i " + Lt + " n - p; i++) {",
                    "            " + Kw + "if" + End + " (a[i] > a[i + " + Fn + "1" + End + "]) {",
                    "                " + Kw + "int" + End + " tmp = a[i];",
                    "                a[i] = a[i + " + Fn + "1" + End + "];",
                    "                a[i + " + Fn + "1" + End + "] = tmp;",
                    "                swapped = " + Fn + "1" + End + ";",
                    "            }",
                    "        }",
                    "        " + Kw + "if" + End + " (!swapped) " + Kw + "break" + End + ";",
                    "    }",
                    "}",
                };

                var rowByLine = new int[(int)BubbleSortCodeLine.Count];
                var spanByLine = new int[(int)BubbleSortCodeLine.Count];
                for (int i = 0; i < spanByLine.Length; i++)
                {
                    spanByLine[i] = 1;
                }

                rowByLine[(int)BubbleSortCodeLine.Header] = 0;
                rowByLine[(int)BubbleSortCodeLine.InitN] = 0; // n is a parameter in C
                rowByLine[(int)BubbleSortCodeLine.OuterLoop] = 1;
                rowByLine[(int)BubbleSortCodeLine.ResetSwapped] = 2;
                rowByLine[(int)BubbleSortCodeLine.InnerLoop] = 3;
                rowByLine[(int)BubbleSortCodeLine.Compare] = 4;
                rowByLine[(int)BubbleSortCodeLine.Swap] = 5;
                spanByLine[(int)BubbleSortCodeLine.Swap] = 3;
                rowByLine[(int)BubbleSortCodeLine.SetSwapped] = 8;
                rowByLine[(int)BubbleSortCodeLine.CheckSwapped] = 11;
                rowByLine[(int)BubbleSortCodeLine.Break] = 11;

                c = new CodeListing(CodeLanguage.C, "bubble_sort.c", rows, rowByLine, spanByLine);
                return c;
            }
        }

        private static CodeListing Python
        {
            get
            {
                if (python != null)
                {
                    return python;
                }

                string[] rows =
                {
                    Kw + "def" + End + " bubble_sort(a):",
                    "    n = " + Fn + "len" + End + "(a)",
                    "    " + Kw + "for" + End + " p " + Kw + "in" + End + " " + Fn + "range" + End + "(" + Fn + "1" + End + ", n):",
                    "        swapped = " + Kw + "False" + End,
                    "        " + Kw + "for" + End + " i " + Kw + "in" + End + " " + Fn + "range" + End + "(n - p):",
                    "            " + Kw + "if" + End + " a[i] > a[i + " + Fn + "1" + End + "]:",
                    "                a[i], a[i + " + Fn + "1" + End + "] = a[i + " + Fn + "1" + End + "], a[i]",
                    "                swapped = " + Kw + "True" + End,
                    "        " + Kw + "if not" + End + " swapped:",
                    "            " + Kw + "break" + End,
                };

                var rowByLine = new int[(int)BubbleSortCodeLine.Count];
                for (int i = 0; i < rowByLine.Length; i++)
                {
                    rowByLine[i] = i; // Python rows are in logical line order.
                }

                python = new CodeListing(CodeLanguage.Python, "bubble_sort.py", rows, rowByLine);
                return python;
            }
        }
    }
}
