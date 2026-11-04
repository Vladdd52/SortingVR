using System;

namespace SortingVR.Core
{
    /// <summary>
    /// Tells which logical code line the algorithm is at right now. Pure C#, event driven.
    /// Line ids are defined by each algorithm; -1 means no line.
    /// </summary>
    public interface ICodeTrace
    {
        /// <summary>The line being executed now (strong highlight).</summary>
        int ActiveLine { get; }

        /// <summary>A line that was just passed, shown as a faint hint, or -1.</summary>
        int HintLine { get; }

        /// <summary>Raised when ActiveLine or HintLine changes.</summary>
        event Action Changed;
    }
}
