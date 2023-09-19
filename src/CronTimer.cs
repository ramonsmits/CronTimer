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
    readonly Timer t;

    public string tz { get; }
    public string Expression { get; }
    public event EventHandler<CronTimerEventArgs> OnOccurence;

    public DateTime Next { get; private set; }

    public CronTimer(string expression, string tz = UTC, bool includingSeconds = false)
    {
        Expression = expression;
        this.tz = tz;
        id = TimeZoneConverter.TZConvert.IanaToWindows(tz);
        tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
        schedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = includingSeconds });
        Next = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        OnOccurence += OnOccurenceScheduleNext;
        t = new Timer(s =>
        {
            var ea = new CronTimerEventArgs
            {
                At = Next
            };
            OnOccurence(this, ea);
        }, null, InfiniteTimeSpan, InfiniteTimeSpan);
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
        var nowUtc = DateTime.UtcNow;
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
        //Console.WriteLine($"Now: {nowUtc} [utc] {now} [{tz}], Next: {next} [{tz}] {nextUtc} [utc], Delay: {delay}");
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        return delay;
    }

    public void Stop()
    {
        t.Change(InfiniteTimeSpan, InfiniteTimeSpan);
    }
}
