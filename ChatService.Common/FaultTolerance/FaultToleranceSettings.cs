namespace ChatService.FaultTolerance
{
    public class FaultToleranceSettings
    {
        public int TimeoutLength { get; set; }
        public int ExceptionsAllowedBeforeBreaking { get; set; }
        public int DurationOfBreakInMinutes { get; set; }
    }
}
