# CronTimer

Simple .net Timer that is based on cron expressions with second accuracy to fire timer events to a very specific schedule.

Regular timers are very useful for tasks that do not really require any precision like polling a service at a rough interval but sometimes there is a need for more precision based on time. There is already a great time schedule expression syntax that originated from [Cron](https://en.wikipedia.org/wiki/Cron). Normally you would likely schedule such jobs via the operating system if they are things like on every Friday at 18:00 run a report but there are schedules that make more sense to have running in process like to not have a certain overhead of launching a whole job process or just because the process is already running. This small library makes it super easy to define such timers on a specific schedule.

## Example


Fire a timer event every 10 minutes from Monday through Friday between 8:00 and 17:00

```
var timer = new CronTimer("*/10 08-17 * * 1-5", "Europe/Amsterdam", includingSeconds: false);
timer.OnOccurence += (s, ea) => Console.Out.WriteLineAsync(ea + " - " + DateTime.Now);
timer.Start();
```