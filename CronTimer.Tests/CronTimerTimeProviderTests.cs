#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

[TestFixture]
public class CronTimerTimeProviderTests
{
    [Test]
    public void Start_FiresEventAtNextOccurrence()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", timeProvider: fakeTime);

        var fired = false;
        DateTime firedAt = default;
        timer.OnOccurence += (s, ea) => { fired = true; firedAt = ea.At; };

        timer.Start();

        Assert.That(timer.Next, Is.EqualTo(new DateTime(2024, 1, 1, 12, 1, 0)));

        fakeTime.Advance(TimeSpan.FromMinutes(1));

        Assert.That(fired, Is.True);
        Assert.That(firedAt, Is.EqualTo(new DateTime(2024, 1, 1, 12, 1, 0)));
    }

    [Test]
    public void Stop_PreventsEventFromFiring()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", timeProvider: fakeTime);

        var fired = false;
        timer.OnOccurence += (s, ea) => fired = true;

        timer.Start();
        timer.Stop();

        fakeTime.Advance(TimeSpan.FromMinutes(2));

        Assert.That(fired, Is.False);
    }

    [Test]
    public void StartStopStart_DoesNotSkipInterval()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", timeProvider: fakeTime);

        var firedCount = 0;
        DateTime lastFiredAt = default;
        timer.OnOccurence += (s, ea) => { firedCount++; lastFiredAt = ea.At; };

        timer.Start();

        // Advance 30 seconds - timer hasn't fired yet
        fakeTime.Advance(TimeSpan.FromSeconds(30));
        Assert.That(firedCount, Is.EqualTo(0));

        timer.Stop();

        // Advance another 60 seconds while stopped (now at 12:01:30)
        fakeTime.Advance(TimeSpan.FromSeconds(60));
        Assert.That(firedCount, Is.EqualTo(0));

        // Restart - should calculate next from 12:01:30 -> next at 12:02:00
        timer.Start();
        Assert.That(timer.Next, Is.EqualTo(new DateTime(2024, 1, 1, 12, 2, 0)));

        // Advance 30 seconds to 12:02:00
        fakeTime.Advance(TimeSpan.FromSeconds(30));

        Assert.That(firedCount, Is.EqualTo(1));
        Assert.That(lastFiredAt, Is.EqualTo(new DateTime(2024, 1, 1, 12, 2, 0)));
    }

    [Test]
    public void MultipleOccurrences_FireInSequence()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", timeProvider: fakeTime);

        var firedTimes = new List<DateTime>();
        timer.OnOccurence += (s, ea) => firedTimes.Add(ea.At);

        timer.Start();

        fakeTime.Advance(TimeSpan.FromMinutes(1));
        fakeTime.Advance(TimeSpan.FromMinutes(1));
        fakeTime.Advance(TimeSpan.FromMinutes(1));

        Assert.That(firedTimes, Has.Count.EqualTo(3));
        Assert.That(firedTimes[0], Is.EqualTo(new DateTime(2024, 1, 1, 12, 1, 0)));
        Assert.That(firedTimes[1], Is.EqualTo(new DateTime(2024, 1, 1, 12, 2, 0)));
        Assert.That(firedTimes[2], Is.EqualTo(new DateTime(2024, 1, 1, 12, 3, 0)));
    }

    [Test]
    public void Start_WithTimezone_CalculatesCorrectDelay()
    {
        // July 1st for CEST (UTC+2)
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 7, 1, 10, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", "Europe/Amsterdam", timeProvider: fakeTime);

        var fired = false;
        timer.OnOccurence += (s, ea) => fired = true;

        timer.Start();

        // Amsterdam (CEST, UTC+2) local time is 12:00:00
        // Next occurrence: 12:01:00 local = 10:01:00 UTC
        // Advancing exactly 60 seconds should trigger
        fakeTime.Advance(TimeSpan.FromSeconds(60));

        Assert.That(fired, Is.True);
    }

    [Test]
    public void Start_WithSeconds_FiresEverySecond()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * * *", includingSeconds: true, timeProvider: fakeTime);

        var firedCount = 0;
        timer.OnOccurence += (s, ea) => firedCount++;

        timer.Start();

        fakeTime.Advance(TimeSpan.FromSeconds(1));
        fakeTime.Advance(TimeSpan.FromSeconds(1));
        fakeTime.Advance(TimeSpan.FromSeconds(1));

        Assert.That(firedCount, Is.EqualTo(3));
    }

    [Test]
    public void Start_WithTimezone_NextIsInConfiguredTimezone()
    {
        // Jan 1 at 10:00 UTC = 11:00 CET (Europe/Amsterdam in winter)
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", "Europe/Amsterdam", timeProvider: fakeTime);

        timer.Start();

        // Next should be 11:01:00 CET (not 10:01:00 UTC)
        Assert.That(timer.Next, Is.EqualTo(new DateTime(2024, 1, 1, 11, 1, 0)));
    }

    [Test]
    public void Constructor_WithTimeProvider_UsesProvidedTime()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 6, 15, 8, 30, 0, TimeSpan.Zero));
        var timer = new CronTimer("* * * * *", timeProvider: fakeTime);

        // Next should be initialized from the fake time, not real time
        Assert.That(timer.Next.Date, Is.EqualTo(new DateTime(2024, 6, 15)));
        Assert.That(timer.Next.Hour, Is.EqualTo(8));
        Assert.That(timer.Next.Minute, Is.EqualTo(30));
    }
}
#endif
