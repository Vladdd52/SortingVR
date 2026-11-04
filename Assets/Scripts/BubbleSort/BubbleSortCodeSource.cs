using SortingVR.Core;
using UnityEngine;

namespace SortingVR.BubbleSort
{
    /// <summary>Gives code boards the bubble sort trace (from the guide driver) and listings.</summary>
    public sealed class BubbleSortCodeSource : CodeTraceSource
    {
        [SerializeField] private BubbleSortGuideDriver driver;

        private BubbleSortCodeTrace trace;
        private BubbleSortVariables variables;

        public override ICodeTrace Trace
        {
            get
            {
                if (trace == null)
                {
                    trace = new BubbleSortCodeTrace(driver.Guide);
                }

                return trace;
            }
        }

        public override IVariableWatch Variables
        {
            get
            {
                if (variables == null)
                {
                    variables = new BubbleSortVariables(driver.Guide);
                }

                return variables;
            }
        }

        public override CodeListing GetListing(CodeLanguage language) => BubbleSortListings.Get(language);

        private void OnDestroy()
        {
            trace?.Dispose();
            variables?.Dispose();
        }
    }
}
