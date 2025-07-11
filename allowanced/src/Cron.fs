module Cron
open System

// Design decision.
// I don't need to persist cron jobs to a database. We know what timers need to run
// even if they don't have work to do.

[<Struct>]
type CronMinute = Minute of int | Any

[<Struct>]
type CronHour = Hour of int | Any

[<Struct>]
type CronDayOfMonth = DayOfMonth of int | Any

[<Struct>]
type CronMonth = Month of int | Any

[<Struct>]
type CronDayOfWeek = Day of DayOfWeek | Any

type TaskSchedule = {
  minute : CronMinute
  hour : CronHour
  dayOfMonth : CronDayOfMonth
  month : CronMonth
  dayOfWeek : CronDayOfWeek
}

type ScheduledTask = {
  lastRun : DateTime
  schedule: TaskSchedule
  action : unit -> unit
}

type Cron = {
  lastChecked : DateTime
  scheduledItems : ScheduledTask list
}

let addTask (schedule:TaskSchedule) action (cron:Cron) =
  { cron with scheduledItems = {schedule = schedule; lastRun = DateTime.MinValue; action = action} :: cron.scheduledItems }

let checkTimers (time: DateTime) (cron: Cron) =
    let isTaskDue (schedule: TaskSchedule) (lastRun: DateTime) =
        let minuteMatch = match schedule.minute with Minute t -> time.Minute = t | CronMinute.Any -> true
        let hourMatch = match schedule.hour with Hour t -> time.Hour = t | CronHour.Any -> true
        let dayOfMonthMatch = match schedule.dayOfMonth with DayOfMonth t -> time.Day = t | CronDayOfMonth.Any -> true
        let monthMatch = match schedule.month with Month t -> time.Month = t | CronMonth.Any -> true
        let dayOfWeekMatch = match schedule.dayOfWeek with Day t -> time.DayOfWeek = t | CronDayOfWeek.Any -> true
        lastRun < time && minuteMatch && hourMatch && dayOfMonthMatch && monthMatch && dayOfWeekMatch

    let runDueTasks (tasks: ScheduledTask list) =
        tasks
        |> List.map (fun task ->
            if isTaskDue task.schedule task.lastRun then
                task.action()
                { task with lastRun = time }
            else
                task)

    { cron with 
        lastChecked = time
        scheduledItems = runDueTasks cron.scheduledItems }

