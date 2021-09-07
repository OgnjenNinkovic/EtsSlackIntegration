# The main project components are: 

- Slack application (front end)

- Microsoft Azure ASP.NET service

- The "ETS" API




## Slack application (front end)

**All user interaction will be done in the "Slack"**

**Slack app need to be created**
   - The application will communicate with ASP.NET service deployed on the "Microsoft Azure" cloud.
   - The app will parse JSON payload received from the "ETS". A user can evaluate the data in the user interface(table view). 
   - Users can modify the data and submit the changes. The data payload will be sent to the "ETS" API for the given user.
   - The users can adjust the app settings(reminder frequency, vacations ). Those settings will be saved in the Azure service database.







## Microsoft Azure ASP.NET service

 * The service will communicate with the slack application using webhooks.
 * The service will also communicate with the "ETS API".
 * When the reminder event is triggered for the given user,
  then the service will query the "ETS" database via API and send the payload to the slack application in JSON format.
 * When the user modifies the data and posts it back to the service, then the service makes the "PUT" request to the "ETS" for the given user.
 * The service will have its own database

* Application data will be stored in the SQL database.
  **The database will consist of Three tables**
      


##### Users
   
       -UserID         int
       -EtsUserName    nvarchar(100)
       -EtsPassword    nvarchar(100)
       -SlackChannelId nvarchar(250)
       -UserType       nvarchar(100)
     
##### Reminders 

      -ReminderId  int
      -Type        nvarchar(50)
      -UserId      int
      -Date        datetime
      
      
##### ReminderTimes
      -ReminderTimeId int
      -ReminderId     int
      -Time           datetime
      -Notified       bool
      

**The service API**


* Receive the user data as well as a reminder frequency(daily, weekly, monthly).

* Provides an option to change stored data for the users.
 
**The service internal logic**
* The received data will be stored in the Azure service data base

* The service will permanently scan the stored data for the reminders event.

* When the reminder event is triggered, then the HTTP client logs into the "ETS" and collects the user data via the "Ets API".

* When the data is collected the service creates the JSON notification and sends the payload to the "Slack" application on the corresponding channel.

* At the end of each day, the state of daily reminders will be reinitialized. This means the state will be changed from "notified=true" to "notified=false". 

* The same logic will be applied for the weekly reminders on the first day of the week, and for the monthly reminders on the first day of the month. The state will be changed from "notified=true" to "notified=false".


      
## The "ETS" API

  * The "ETS API" service provides all needed URLs to "GET" or "PUT" the data