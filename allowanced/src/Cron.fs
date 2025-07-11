module Cron
open System

// Design decision.
// I don't need to persist cron jobs to a database. We know what timers need to run
// even if they don't have work to do.

[<Struct>]
type CronValue = Time of int | Star

[<Struct>]
type CronDayOfWeek = Day of DayOfWeek | Star

type TaskSchedule = {
  minute : CronValue
  hour : CronValue
  dayOfMonth : CronValue
  month : CronValue
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
        let minuteMatch = match schedule.minute with Time t -> time.Minute = t | CronValue.Star -> true
        let hourMatch = match schedule.hour with Time t -> time.Hour = t | CronValue.Star -> true
        let dayOfMonthMatch = match schedule.dayOfMonth with Time t -> time.Day = t | CronValue.Star -> true
        let monthMatch = match schedule.month with Time t -> time.Month = t | CronValue.Star -> true
        let dayOfWeekMatch = match schedule.dayOfWeek with Day t -> time.DayOfWeek = t | CronDayOfWeek.Star -> true
        minuteMatch && hourMatch && dayOfMonthMatch && monthMatch && dayOfWeekMatch

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

