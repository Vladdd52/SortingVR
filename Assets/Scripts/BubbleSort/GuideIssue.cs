namespace SortingVR.BubbleSort
{
    /// <summary>A mistake the guide noticed in the current rack / scale state.</summary>
    public enum GuideIssue
    {
        None,

        /// <summary>A tube that is not part of the current pair is on the scale.</summary>
        WrongTubeOnScale,

        /// <summary>The current pair is on the scale, but on the wrong pans.</summary>
        PansMixedUp,

        /// <summary>A slot outside the current pair does not hold its tube.</summary>
        OtherSlotTouched,

        /// <summary>The pair was swapped in the rack before it was weighed.</summary>
        CompareFirst,

        /// <summary>After weighing, a tube was returned to the wrong slot.</summary>
        WrongReturn
    }
}
