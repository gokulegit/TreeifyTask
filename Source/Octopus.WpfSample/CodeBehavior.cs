using System;

namespace Octopus.Sample
{
    public record CodeBehavior
    {
        public bool ShouldHangInAnInDeterminateState { get; set; }
        public bool ShouldPerformAnInDeterminateAction { get; set; }
        public bool ShouldHangDuringProgress { get; set; }
        public bool ShouldThrowException { get; set; }
        public bool ShouldThrowExceptionDuringProgress { get; set; }
        public int IntervalDelay { get; set; } = 10;
        public int InDeterminateActionDelay { get; set; } = 1000;
    }
}

