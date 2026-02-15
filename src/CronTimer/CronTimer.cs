using System;
using System.Threading;
using NCrontab;

public class CronTimer
{
    public const string UTC = "Etc/UTC";

    static readonly TimeSpan InfiniteTimeSpan = TimeSpan.FromMilliseconds(Timeout.Infinite); // net 3.5

    readonly CrontabSchedule schedule;
    readonly TimeZoneInfo tzi;
    readonly string id;
#if NET8_0_OR_GREATER
    readonly TimeProvider timeProvider;
    readonly ITimer t;
#else
    readonly Timer t;
#endif

    public string tz { get; }
    public string Expression { get; }
    public event EventHandler<CronTimerEventArgs> OnOccurence;

    public DateTime Next { get; private set; }

#if NET8_0_OR_GREATER
#nullable enable
    public CronTimer(string expression, string tz = UTC, bool includingSeconds = false, TimeProvider? timeProvider = null)
#nullable restore
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
#else
    public CronTimer(string expression, string tz = UTC, bool includingSeconds = false)
    {
#endif
        Expression = expression;
        this.tz = tz;
        id = TimeZoneConverter.TZConvert.IanaToWindows(tz);
        tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
        schedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = includingSeconds });
        Next = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, tzi);
        OnOccurence += OnOccurenceScheduleNext;
        TimerCallback callback = s =>
        {
            var ea = new CronTimerEventArgs
            {
                At = Next
            };
            OnOccurence(this, ea);
        };
#if NET8_0_OR_GREATER
        t = this.timeProvider.CreateTimer(callback, null, InfiniteTimeSpan, InfiniteTimeSpan);
#else
        t = new Timer(callback, null, InfiniteTimeSpan, InfiniteTimeSpan);
#endif
    }

    DateTime UtcNow =>
#if NET8_0_OR_GREATER
        timeProvider.GetUtcNow().UtcDateTime;
#else
        DateTime.UtcNow;
#endif

    void OnOccurenceScheduleNext(object sender, EventArgs e)
    {
        var delay = CalculateDelay();
        t.Change(delay, InfiniteTimeSpan);
    }

    public void Start()
    {
        Next = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, tzi);
        var delay = CalculateDelay();
        t.Change(delay, InfiniteTimeSpan);
    }

    TimeSpan CalculateDelay()
    {
        var nowUtc = UtcNow;
        Next = schedule.GetNextOccurrence(Next);
        TimeSpan delay;
        if (tz != UTC)
        {
            var nextUtc = TimeZoneInfo.ConvertTimeToUtc(Next, tzi);
            delay = nextUtc - nowUtc;
        }
        else
        {
            delay = Next - nowUtc;
        }
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        return delay;
    }

    public void Stop()
    {
        t.Change(InfiniteTimeSpan, InfiniteTimeSpan);
    }
}
