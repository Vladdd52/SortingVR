using UnityEngine;

namespace SortingVR.Core
{
    /// <summary>
    /// Scene component that gives code boards a trace and the code listings of one algorithm.
    /// Each room provides its own subclass.
    /// </summary>
    public abstract class CodeTraceSource : MonoBehaviour
    {
        public abstract ICodeTrace Trace { get; }

        public abstract IVariableWatch Variables { get; }

        public abstract CodeListing GetListing(CodeLanguage language);
    }
}
