using System;
using System.Text;

namespace SortingVR.Core
{
    /// <summary>
    /// The algorithm's variables as they would be in the code at the current line.
    /// Pure C#, event driven. Values are written into a StringBuilder to avoid allocations.
    /// </summary>
    public interface IVariableWatch
    {
        int Count { get; }

        /// <summary>Variable name as written in the code, e.g. "a[i + 1]".</summary>
        string GetName(int index);

        /// <summary>
        /// Appends the value in the syntax of the language. Parts that changed in the last update
        /// are wrapped in changedTag, array elements that are already final in finalTag
        /// (both are opening TMP color tags closed with "&lt;/color&gt;").
        /// </summary>
        void AppendValue(int index, CodeLanguage language, StringBuilder sb, string changedTag, string finalTag);

        /// <summary>Raised when at least one value changed.</summary>
        event Action Changed;
    }
}
