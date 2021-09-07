# Draft framework description

## Parts

This application is divided on two parts: working part and management part.

The target of working parts is watching on time and when the next event occurs get information from ETS and send it to subscribers.

The targets of management part are adding/removing/editing new subscribers, event subscription to subscribers

## Structures

Reminder

 * ReminderType: Daily, Weekly, Monthly
 * UserId: link to user
 * NextEventTime: DateTime when user should be reminded (indexed in db)

User

 * UserId
 * SlackId
 * EtsLogin
 * EtsPasswd
 * UserType: regular or admin — for future purposes

## Working part

### public/internal Timer MainTimer

Manage the next call for reminding users

#### Constructor()

Creating all needed variables and structures.

#### Initialization()

Find the DateTime of next event. (`Select min(NextEventTime) from Reminders`)Set Timer (MainTimer variable) for this time. If the time of next event is impossible to determinate set «dueTime» to Infinite.

If there are Reminders with NextEventTime in past set dueTime to zero to call callback immediately. 

#### RemindUsers()

It is the callback function for Timer.

Get list of Reminders with NextEventTime <= `Now: List<Reminder> GetListUnremnideredUsers(Now)`

Get set (!) of users for these Reminders <= Now: `Set<User> GetSetUsersFromReminders(List<Reminder>)`

Send reminders for each of these Users: `bool SendReminder(User)` 

Recalculate and save new DateTime for NextEventTime for each of these Reminders: `void SetNewEventTimes(List<Reminder>, Now)`

After these actions we need to call `Initialization` method to set new Time to awaking.

#### DoQuit()

Call Dispose method for MainTimer and do all needed actions for finishing work.

#### void SetNewEventTime(Reminder, DateTime now)

Based on now parameter and type of Reminder (Daily, Weekly, Monthly) set new value for NextEventTime and store Reminder in the Storage (DB).

...

### Management part

#### bool AddReminder(ReminderType, User)

Create new Reminder with specified type for User.

Calculate NextEventTime based on ReminderType and Now (`SetNewEventTime`) and save it in Storage

#### bool AddUser

...

#### bool RemoveUser

...





