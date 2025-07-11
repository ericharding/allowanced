module CronTests

open Xunit
open System
open Cron

[<Fact>]
let ``addTask should add a new task to the cron`` () =
    let emptyCron = { lastChecked = DateTime.MinValue; scheduledItems = [] }
    let schedule = { minute = Minute 30; hour = Hour 14; dayOfMonth = CronDayOfMonth.Any ; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    let mutable actionCalled = false
    let action = fun () -> actionCalled <- true
    
    let updatedCron = addTask schedule action emptyCron
    
    Assert.Single updatedCron.scheduledItems |> ignore
    Assert.Equal(schedule, updatedCron.scheduledItems.[0].schedule)
    Assert.Equal(DateTime.MinValue, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should run task when time matches schedule`` () =
    let mutable actionCalled = false
    let action = fun () -> actionCalled <- true
    let schedule = { minute = Minute 30; hour = Hour 14; dayOfMonth = DayOfMonth 15; month = Month 6; dayOfWeek = CronDayOfWeek.Any }
    let task = { lastRun = DateTime.MinValue; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    let testTime = DateTime(2024, 6, 15, 14, 30, 0)
    let updatedCron = checkTimers testTime cron
    
    Assert.True(actionCalled)
    Assert.Equal(testTime, updatedCron.lastChecked)
    Assert.Equal(testTime, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should not run task when time doesn't match schedule`` () =
    let mutable actionCalled = false
    let action = fun () -> actionCalled <- true
    let schedule = { minute = Minute 30; hour = Hour 14; dayOfMonth = DayOfMonth 15; month = Month 6; dayOfWeek = CronDayOfWeek.Any }
    let task = { lastRun = DateTime.MinValue; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    let testTime = DateTime(2024, 6, 15, 14, 31, 0) // Wrong minute
    let updatedCron = checkTimers testTime cron
    
    Assert.False(actionCalled)
    Assert.Equal(DateTime.MinValue, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should respect wildcard values (-1)`` () =
    let mutable actionCalled = false
    let action = fun () -> actionCalled <- true
    let schedule = { minute = CronMinute.Any; hour = CronHour.Any; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    let task = { lastRun = DateTime.MinValue; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    let testTime = DateTime(2024, 12, 25, 23, 59, 0)
    let updatedCron = checkTimers testTime cron
    
    Assert.True(actionCalled)
    Assert.Equal(testTime, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should not run task twice in the same minute`` () =
    let mutable actionCallCount = 0
    let action = fun () -> actionCallCount <- actionCallCount + 1
    let schedule = { minute = Minute 30; hour = Hour 14; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    let testTime = DateTime(2024, 6, 15, 14, 30, 0)
    let task = { lastRun = testTime; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    let updatedCron = checkTimers testTime cron
    
    Assert.Equal(0, actionCallCount)
    Assert.Equal(testTime, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should run task again in next matching time period`` () =
    let mutable actionCallCount = 0
    let action = fun () -> actionCallCount <- actionCallCount + 1
    let schedule = { minute = Minute 30; hour = Hour 14; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    let firstRunTime = DateTime(2024, 6, 15, 14, 30, 0)
    let task = { lastRun = firstRunTime; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    let secondRunTime = DateTime(2024, 6, 16, 14, 30, 0) // Next day
    let updatedCron = checkTimers secondRunTime cron
    
    Assert.Equal(1, actionCallCount)
    Assert.Equal(secondRunTime, updatedCron.scheduledItems.[0].lastRun)

[<Fact>]
let ``checkTimers should handle multiple tasks`` () =
    let mutable action1Called = false
    let mutable action2Called = false
    let action1 = fun () -> action1Called <- true
    let action2 = fun () -> action2Called <- true
    
    let schedule1 = { minute = Minute 30; hour = Hour 14; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    let schedule2 = { minute = Minute 45; hour = Hour 14; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Any }
    
    let task1 = { lastRun = DateTime.MinValue; schedule = schedule1; action = action1 }
    let task2 = { lastRun = DateTime.MinValue; schedule = schedule2; action = action2 }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task1; task2] }
    
    let testTime = DateTime(2024, 6, 15, 14, 30, 0)
    let updatedCron = checkTimers testTime cron
    
    Assert.True(action1Called)
    Assert.False(action2Called)
    Assert.Equal(2, updatedCron.scheduledItems.Length)

[<Fact>]
let ``checkTimers should match day of week correctly`` () =
    let mutable actionCalled = false
    let action = fun () -> actionCalled <- true
    // Schedule for Monday (DayOfWeek = 1)
    let schedule = { minute = CronMinute.Any; hour = CronHour.Any; dayOfMonth = CronDayOfMonth.Any; month = CronMonth.Any; dayOfWeek = CronDayOfWeek.Day DayOfWeek.Monday }
    let task = { lastRun = DateTime.MinValue; schedule = schedule; action = action }
    let cron = { lastChecked = DateTime.MinValue; scheduledItems = [task] }
    
    // June 17, 2024 is a Monday
    let testTime = DateTime(2024, 6, 17, 10, 0, 0)
    let updatedCron = checkTimers testTime cron
    
    Assert.True(actionCalled)
    Assert.Equal(testTime, updatedCron.scheduledItems.[0].lastRun)
