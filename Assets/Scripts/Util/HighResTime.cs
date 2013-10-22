using System;
using System.Diagnostics;

public static class HighResTime {
	public const long SECOND2TICK = 10000000L;
	public const double TICK2SECOND = 1.0 / SECOND2TICK;
	
    private static DateTime _startTime;
    private static Stopwatch _stopWatch = null;
    private static TimeSpan _maxIdle = TimeSpan.FromSeconds(10);

    public static DateTime UtcNow {
        get {
            if ((_stopWatch == null) || (_startTime.Add(_maxIdle) < DateTime.UtcNow)) {
                Reset();
            }
            return _startTime.AddTicks(_stopWatch.ElapsedTicks);
        }
    }

    private static void Reset() {
        _startTime = DateTime.UtcNow;
        _stopWatch = Stopwatch.StartNew();
    }
}
