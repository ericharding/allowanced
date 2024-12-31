module Cron
open System

type TaskSchedule = {
  minute : int
  hour : int
  dayOfMonth : int
  month : int
  dayOfWeek : int
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
        let minuteMatch = time.Minute = schedule.minute || schedule.minute = -1
        let hourMatch = time.Hour = schedule.hour || schedule.hour = -1
        let dayOfMonthMatch = time.Day = schedule.dayOfMonth || schedule.dayOfMonth = -1
        let monthMatch = time.Month = schedule.month || schedule.month = -1
        let dayOfWeekMatch = int time.DayOfWeek = schedule.dayOfWeek || schedule.dayOfWeek = -1

        minuteMatch && hourMatch && dayOfMonthMatch && monthMatch && dayOfWeekMatch
        && (lastRun.Date < time.Date || (lastRun.Date = time.Date && lastRun.Hour < time.Hour) || (lastRun.Date = time.Date && lastRun.Hour = time.Hour && lastRun.Minute < time.Minute))

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
