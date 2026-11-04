using System;
using System.Text;
using SortingVR.Core;

namespace SortingVR.BubbleSort
{
    /// <summary>
    /// Bubble sort variables (n, p, i, a[i], a[i + 1], swapped, a) derived from the guide state.
    /// The highlighted code line is about to run, so the values are the ones before it.
    /// Pure C#: listens to guide.Changed, remembers what changed and raises Changed only then.
    /// </summary>
    public sealed class BubbleSortVariables : IVariableWatch, IDisposable
    {
        private const int VarN = 0;
        private const int VarP = 1;
        private const int VarI = 2;
        private const int VarLeft = 3;
        private const int VarRight = 4;
        private const int VarSwapped = 5;
        private const int VarArray = 6;
        private const int ScalarCount = 6;
        private const int None = -1;

        private static readonly string[] Names = { "n", "p", "i", "a[i]", "a[i + 1]", "swapped", "a" };

        private readonly BubbleSortGuide guide;
        private readonly int[] scalars = new int[ScalarCount];
        private readonly int[] scratch = new int[ScalarCount];
        private readonly bool[] scalarChanged = new bool[ScalarCount];
        private readonly int[] array;
        private readonly int[] arrayScratch;
        private readonly bool[] elementChanged;

        public BubbleSortVariables(BubbleSortGuide guide)
        {
            this.guide = guide ?? throw new ArgumentNullException(nameof(guide));
            array = new int[guide.SlotCount];
            arrayScratch = new int[guide.SlotCount];
            elementChanged = new bool[guide.SlotCount];

            Read(scalars, array);
            guide.Changed += Update;
        }

        public int Count => Names.Length;

        public event Action Changed;

        public string GetName(int index) => Names[index];

        public void Dispose()
        {
            guide.Changed -= Update;
        }

        public void AppendValue(int index, CodeLanguage language, StringBuilder sb, string changedTag, string finalTag)
        {
            if (index == VarArray)
            {
                AppendArray(language, sb, changedTag, finalTag);
                return;
            }

            bool changed = scalarChanged[index];
            if (changed) sb.Append(changedTag);

            int value = scalars[index];
            if (index == VarSwapped)
            {
                if (language == CodeLanguage.C) sb.Append(value);
                else sb.Append(value != 0 ? "True" : "False");
            }
            else if (value == None)
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(value);
            }

            if (changed) sb.Append("</color>");
        }

        private void AppendArray(CodeLanguage language, StringBuilder sb, string changedTag, string finalTag)
        {
            sb.Append(language == CodeLanguage.C ? '{' : '[');
            for (int k = 0; k < array.Length; k++)
            {
                if (k > 0) sb.Append(", ");

                string tag = elementChanged[k] ? changedTag : k >= guide.SortedFrom ? finalTag : null;
                if (tag != null) sb.Append(tag);
                sb.Append(array[k]);
                if (tag != null) sb.Append("</color>");
            }
            sb.Append(language == CodeLanguage.C ? '}' : ']');
        }

        private void Update()
        {
            Read(scratch, arrayScratch);

            bool any = false;
            for (int k = 0; k < ScalarCount; k++)
            {
                if (scratch[k] != scalars[k]) { any = true; break; }
            }
            if (!any)
            {
                for (int k = 0; k < array.Length; k++)
                {
                    if (arrayScratch[k] != array[k]) { any = true; break; }
                }
            }

            // Nothing new (e.g. only a mistake hint changed): keep the last highlights.
            if (!any)
            {
                return;
            }

            for (int k = 0; k < ScalarCount; k++)
            {
                scalarChanged[k] = scratch[k] != scalars[k];
                scalars[k] = scratch[k];
            }

            for (int k = 0; k < array.Length; k++)
            {
                elementChanged[k] = arrayScratch[k] != array[k];
                array[k] = arrayScratch[k];
            }

            Changed?.Invoke();
        }

        private void Read(int[] s, int[] a)
        {
            int n = guide.SlotCount;
            for (int k = 0; k < n; k++)
            {
                a[k] = guide.ValueInSlot(k);
            }

            bool done = guide.IsDone;
            s[VarN] = n;
            s[VarP] = guide.Pass;
            s[VarI] = done ? None : guide.LeftSlot;
            s[VarLeft] = done ? None : a[guide.LeftSlot];
            s[VarRight] = done ? None : a[guide.RightSlot];
            s[VarSwapped] = guide.SwapsThisPass > 0 ? 1 : 0;
        }
    }
}
