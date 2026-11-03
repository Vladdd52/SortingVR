namespace SortingVR.BubbleSort
{
    /// <summary>What the player has to do next in the bubble sort guide.</summary>
    public enum GuideStep
    {
        /// <summary>Put the current pair on the scale (left slot's tube on the left pan).</summary>
        PlaceOnScale,

        /// <summary>The left tube was not heavier: return both tubes to their own slots.</summary>
        ReturnAsIs,

        /// <summary>The left tube was heavier: put each tube into the other's slot.</summary>
        ReturnSwapped,

        /// <summary>The row is sorted.</summary>
        Done
    }
}
