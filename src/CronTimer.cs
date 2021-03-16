using System;
using System.Threading;
using NCrontab;

public class CronTimer
{
    public const string UTC = "Etc/UTC";

    static readonly TimeSpan InfiniteTimeSpan = TimeSpan.FromMilliseconds(Timeout.Infinite); // net 3.5
    static readonly EventArgs ea = new EventArgs();

    readonly CrontabSchedule schedule;
    readonly TimeZoneInfo tzi;
    readonly string id;
    readonly Timer t;

    public string tz { get; }
    public string Expression { get; }
    public event EventHandler<EventArgs> OnOccurence;

    public CronTimer(string expression, string tz = UTC, bool includingSeconds = false)
    {
        Expression = expression;
        this.tz = tz;
        id = TimeZoneConverter.TZConvert.IanaToWindows(tz);
        tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
        schedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = includingSeconds });
        OnOccurence += OnOccurenceScheduleNext;
        t = new Timer(s => OnOccurence(this, ea), null, InfiniteTimeSpan, InfiniteTimeSpan);
    }

    void OnOccurenceScheduleNext(object sender, EventArgs e)
    {
        var delay = CalculateDelay();
        //Console.WriteLine($"Next for [{tz} {expression}] in {delay}.");
        t.Change(delay, InfiniteTimeSpan);
    }

    public void Start()
    {
        var delay = CalculateDelay();
        //Console.WriteLine($"Next for [{tz} {expression}] in {delay}.");
        t.Change(delay, InfiniteTimeSpan);
    }

    TimeSpan CalculateDelay()
    {
        TimeSpan delay;
        if (tz != UTC)
        {
            var nowUtc = DateTime.UtcNow;
            var now = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzi);
            var next = schedule.GetNextOccurrence(now);
            var nextUtc = next.ToUniversalTime();
            delay = nextUtc - nowUtc;
        }
        else
        {
            var nowUtc = DateTime.UtcNow;
            var nextUtc = schedule.GetNextOccurrence(nowUtc);
            delay = nextUtc - nowUtc;
        }
        //Console.WriteLine($"Now: {nowUtc} [utc] {now} [{tz}], Next: {next} [{tz}] {nextUtc} [utc], Delay: {delay}");
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        return delay;
    }

    public void Stop()
    {
        t.Change(InfiniteTimeSpan, InfiniteTimeSpan);
    }
}
