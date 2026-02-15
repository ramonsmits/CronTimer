using System;
using NUnit.Framework;

[TestFixture]
public class CronTimerTests
{
    [Test]
    public void Constructor_SetsExpression()
    {
        var timer = new CronTimer("* * * * *");
        Assert.That(timer.Expression, Is.EqualTo("* * * * *"));
    }

    [Test]
    public void Constructor_SetsTimezone()
    {
        var timer = new CronTimer("* * * * *", "Europe/Amsterdam");
        Assert.That(timer.tz, Is.EqualTo("Europe/Amsterdam"));
    }

    [Test]
    public void Constructor_DefaultTimezone_IsUTC()
    {
        var timer = new CronTimer("* * * * *");
        Assert.That(timer.tz, Is.EqualTo(CronTimer.UTC));
    }

    [Test]
    public void Constructor_InitializesNext()
    {
        var before = DateTime.UtcNow;
        var timer = new CronTimer("* * * * *");
        var after = DateTime.UtcNow;

        Assert.That(timer.Next, Is.InRange(before.AddSeconds(-1), after.AddSeconds(1)));
    }

    [Test]
    public void Start_SetsNextToFutureOccurrence()
    {
        var timer = new CronTimer("* * * * *");
        timer.Start();

        Assert.That(timer.Next, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(timer.Next, Is.LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(2)));
    }

    [Test]
    public void Start_WithSeconds_SetsNextWithinSeconds()
    {
        var timer = new CronTimer("* * * * * *", includingSeconds: true);
        timer.Start();

        Assert.That(timer.Next, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(timer.Next, Is.LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(2)));
    }

    [Test]
    public void Stop_DoesNotThrow()
    {
        var timer = new CronTimer("* * * * *");
        timer.Start();
        Assert.DoesNotThrow(() => timer.Stop());
    }

    [Test]
    public void Start_AfterStop_RecalculatesNextFromCurrentTime()
    {
        var timer = new CronTimer("* * * * * *", includingSeconds: true);
        timer.Start();
        var firstNext = timer.Next;

        timer.Stop();
        System.Threading.Thread.Sleep(50);

        timer.Start();
        var secondNext = timer.Next;

        Assert.That(secondNext, Is.GreaterThanOrEqualTo(firstNext));
    }

    [Test]
    public void Constructor_WithNonUtcTimezone_Initializes()
    {
        var timer = new CronTimer("* * * * *", "Europe/Amsterdam");
        Assert.That(timer.Next, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void Constructor_WithIncludingSeconds_ParsesSixFieldExpression()
    {
        var timer = new CronTimer("*/5 * * * * *", includingSeconds: true);
        Assert.That(timer.Expression, Is.EqualTo("*/5 * * * * *"));
    }
}
