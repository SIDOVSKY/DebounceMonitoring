namespace DebounceMonitoring.Tests
{
    public class DebouncingSample
    {
        public static bool DebounceDuring100msStatic()
        {
            return DebounceMonitor.DebounceHereStatic<DebouncingSample>(100);
        }

        public bool DebounceDuring100ms()
        {
            return this.DebounceHere(100);
        }
    }
}
