using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EtsClientApi.SlackModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static EtsClientData.EtsDataContext;
using EtsClientCore;
using EtsWebClient.Http;
using EtsClientData;
using System.IO;
using static EtsClientApi.SlackModels.AddUserControl;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using EtsClientApi.SlackHttpClient;
using EtsWebClient.MainTimer;
using EtsClientApi.Extensions;
using System.Globalization;
using EtsClientApi.EtsModels;
using EtsWebClient.EtsHttpModels;
using static EtsClientApi.SlackModels.CreateNewReminder;
using EtsClientApi.SlackViews;

namespace EtsClientApi.Api
{


    [Route("[controller]")]
    [ApiController]
    public class SlackController : ControllerBase
    {
        private IUserServices _userServices;





        public static BotResponse botResponse { get; set; }

        public static UserProfile UserProfile { get; set; }

        public static volatile int insertedReminderID = 0;




        public static ControlConfigurator addOrEditViewConfiguration = null;
        static CreateNewReminder CreateReminderViewRef = new CreateNewReminder();



        static TimeUnitCRUDForm TimeUnitCRUDFormRef = null;

        static EtsCrudFormConfigurator EtsCrudFormConfigurator = null;


        private static AddUserControl addUserControlRef = null;

        EtsWebClient.EtsHttpModels.CollectedUserEtsData CollectedUserEtsData = null;

        static int reminderForUpdateId = 0;

        static string EtsEmployeeID = string.Empty;


        static List<ReminderTime> remindersTimesForUpdate = null;

        private static volatile string shortcutPayloadActionType = string.Empty;


        private static volatile string shortcutPayloadActionID = string.Empty;

        private static volatile string viewActionId = string.Empty;








        public SlackController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Slack controller initialized");
        }


        private Task<string> ParseCommand(string input)
        {

            Task<string> parser = Task.Run(() =>
            {


                string trimCommand = input.Split('*').Last();
                string command = string.Empty;

                if (trimCommand.Remove(trimCommand.Length - 14, 14).Trim().Contains("tel:"))
                {
                    int index = trimCommand.IndexOf("<");
                    string trimmed = trimCommand.Remove(index, index + 16);
                    command = trimmed.Remove(trimmed.Length - 14, 14).Replace(">", string.Empty).Trim();
                    return command;
                }
                else
                {


                    command = trimCommand.Remove(trimCommand.Length - 14, 14).Trim();
                    return command;
                }

            });

            return parser;
        }





        [HttpPost]
        [Route("Shortcuts")]
        public IActionResult Post([FromForm] string payload)
        {
            ShortcutsPayload shortcutsPayload = new ShortcutsPayload();
            try
            {
                shortcutsPayload = JsonConvert.DeserializeObject<ShortcutsPayload>(payload);


                shortcutPayloadActionType = shortcutsPayload.actions != null ? shortcutsPayload.actions[0].type : string.Empty;

                viewActionId = shortcutsPayload.view != null ? shortcutsPayload.view.callback_id : string.Empty;

                shortcutPayloadActionID = shortcutsPayload.actions != null ? shortcutsPayload.actions[0].action_id : string.Empty;


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Ok();
            }



            EtsDataContext.Reminder.reminderEntityChanged += (e) =>
            {
                insertedReminderID = e.GetType().Name.Contains("InsertedEntry") ? ((EntityFrameworkCore.Triggers.IInsertedEntry<Reminder>)e).Entity.ReminderId : 0;
            };




            Task shootcutResponse = Task.Run(async () =>
            {
                EtsDataContext _context = new EtsDataContext();
                EtsClientServices _clientServices = new EtsClientServices(_context);
                IUserServices userServices = _clientServices;

                SlackHttpClient.SlackBotClient slackBotClient = new SlackHttpClient.SlackBotClient(_userServices.SlackBotToken());

                // Check is the user subscribed
                var subscription = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault();
                if (subscription == null && shortcutsPayload.callback_id != "subscription" && shortcutsPayload.view.callback_id != "addNewUser")
                {
                    InfoPopUp infoPopUp = new InfoPopUp("ETS-Client bot", "Ok", "Please check the subscription in the shortcuts section");
                    infoPopUp.channel = shortcutsPayload.user.id;
                    infoPopUp.trigger_id = shortcutsPayload.trigger_id;

                    //Open the confirmation dialog
                    await slackBotClient.WriteMessage(infoPopUp, SlackHttpClient.SlackBotClient.PostUriType.openViews);
                }
                else
                {

                    if (shortcutsPayload.callback_id == "createReminder")
                    {

                        //Get user's profile data
                        slackBotClient.SlackUserID = shortcutsPayload.user.id;
                        await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);



                        addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.createNewReminder);



                        var userControl = await ViewsInitializer.CreateOrUpdateReminderControl(addOrEditViewConfiguration, UserProfile);

                        var staticSelect = userControl.view.Blocks[0].Elements.OfType<StaticSelect>().FirstOrDefault();

                        userControl.view.Blocks[0].Elements.Clear();

                        userControl.view.Blocks[0].Elements.Add(staticSelect);



                        userControl.channel = shortcutsPayload.user.id;
                        userControl.trigger_id = shortcutsPayload.trigger_id;
                        userControl.view.callback_id = "createNewReminder";
                        await slackBotClient.WriteMessage(userControl, SlackHttpClient.SlackBotClient.PostUriType.openViews);
                        userControl.view_id = botResponse.view.id;

                        CreateReminderViewRef = userControl;





                    }
                    else if (shortcutsPayload.payloadType == "block_actions" && shortcutPayloadActionID == "EtsDataBtnClick")
                    {




                        InfoPopUp infoPopUp = new InfoPopUp("Collecting the ETS data", "Cancel", "Please wait...");
                        infoPopUp.channel = shortcutsPayload.user.id;
                        infoPopUp.trigger_id = shortcutsPayload.trigger_id;


                        //Informing a user that the "ETS" data is being collected.
                        await slackBotClient.WriteMessage(infoPopUp, SlackHttpClient.SlackBotClient.PostUriType.openViews);
                        infoPopUp.view_id = botResponse.view.id;




                        var userData = await _clientServices.UserDetails(shortcutsPayload.user.id);

                        var etsUser = new EtsWebClient.EtsHttpModels.EtsUser(userData.EtsUserName, userData.EtsPassword, userData.SlackChannelID);

                        var EtsClient = new EtsHttpClient(etsUser);


                        try
                        {
                            CollectedUserEtsData = await EtsClient.CreateViewModel();


                            if (CollectedUserEtsData != null)
                            {
                                EtsEmployeeID = CollectedUserEtsData.EmployeeDetails.Id;


                                EtsCrudFormConfigurator = new EtsCrudFormConfigurator(EtsCrudFormConfigurator.EtsCrudFormType.create);

                                var userControl = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData);
                                userControl.channel = shortcutsPayload.user.id;
                                userControl.trigger_id = shortcutsPayload.trigger_id;
                                userControl.view_id = infoPopUp.view_id;
                                await slackBotClient.WriteMessage(userControl, SlackHttpClient.SlackBotClient.PostUriType.updateViews);


                                TimeUnitCRUDFormRef = userControl;

                            }
                            else
                            {
                                InfoPopUp invalidLogIn = new InfoPopUp("Failed", "Ok", "Unable to log in to the 'ETS' service.\nPlease check the 'ETS' credentials.");

                                invalidLogIn.channel = shortcutsPayload.user.id;
                                invalidLogIn.trigger_id = shortcutsPayload.trigger_id;
                                invalidLogIn.view_id = infoPopUp.view_id;

                                await slackBotClient.WriteMessage(invalidLogIn, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                            }


                        }
                        catch (Exception ex)
                        {

                            Debug.WriteLine(ex.Message);
                        }










                    }
                    else if (viewActionId == "createNewReminder" && shortcutPayloadActionType == "static_select")
                    {
                        string reminderType = shortcutsPayload.actions[0].action_id == "reminderType" ?
                        shortcutsPayload.view.state.values.reminderState.reminderType.selected_option.value :
                        string.Empty;

                        CreateNewReminder reminderTypeUserControl = null;

                        switch (reminderType)
                        {
                            case "daily":

                                //Get user's profile data
                                slackBotClient.SlackUserID = shortcutsPayload.user.id;
                                await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);



                                addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.createNewReminder);


                                reminderTypeUserControl = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.daily);


                                reminderTypeUserControl.view_id = botResponse.view.id;
                                reminderTypeUserControl.channel = shortcutsPayload.user.id;
                                reminderTypeUserControl.trigger_id = shortcutsPayload.trigger_id;
                                reminderTypeUserControl.view.callback_id = "createNewReminder";
                                await slackBotClient.WriteMessage(reminderTypeUserControl, SlackHttpClient.SlackBotClient.PostUriType.updateViews);


                                CreateReminderViewRef = reminderTypeUserControl;

                                break;

                            case "weekly":

                                //Get user's profile data
                                slackBotClient.SlackUserID = shortcutsPayload.user.id;
                                await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);



                                addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.createNewReminder);
                                addOrEditViewConfiguration.showWeekdaysStaticSelect = true;




                                reminderTypeUserControl = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.weekly);


                                reminderTypeUserControl.view_id = botResponse.view.id;
                                reminderTypeUserControl.channel = shortcutsPayload.user.id;
                                reminderTypeUserControl.trigger_id = shortcutsPayload.trigger_id;
                                reminderTypeUserControl.view.callback_id = "createNewReminder";
                                await slackBotClient.WriteMessage(reminderTypeUserControl, SlackHttpClient.SlackBotClient.PostUriType.updateViews);


                                CreateReminderViewRef = reminderTypeUserControl;
                                break;

                            case "monthly":

                                //Get user's profile data
                                slackBotClient.SlackUserID = shortcutsPayload.user.id;
                                await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);



                                addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.createNewReminder);



                                reminderTypeUserControl = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.monthly);




                                reminderTypeUserControl.view_id = botResponse.view.id;
                                reminderTypeUserControl.channel = shortcutsPayload.user.id;
                                reminderTypeUserControl.trigger_id = shortcutsPayload.trigger_id;
                                reminderTypeUserControl.view.callback_id = "createNewReminder";
                                await slackBotClient.WriteMessage(reminderTypeUserControl, SlackHttpClient.SlackBotClient.PostUriType.updateViews);


                                CreateReminderViewRef = reminderTypeUserControl;

                                break;



                        }

                    }
                    else if (shortcutsPayload.callback_id == "changeReminder")
                    {


                        var user = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault();

                        List<RemindersSelection> remindersForModifications = (from r in userServices.Reminders(user.EtsUserName).Result
                                                                              select new RemindersSelection { ReminderType = r.Type, Date = r.Date.ToString("yyyy-MM-dd"), ReminderID = r.ReminderId.ToString() }).ToList();

                        var selectReminder = await ViewsInitializer.CreateReminderSelectionControl(remindersForModifications, "Modify the reminder", "Select the reminder to modify", "updateReminder");

                        selectReminder.channel = shortcutsPayload.user.id;
                        selectReminder.trigger_id = shortcutsPayload.trigger_id;
                        await slackBotClient.WriteMessage(selectReminder, SlackHttpClient.SlackBotClient.PostUriType.openViews);

                    }
                    else if (shortcutsPayload.callback_id == "deleteReminder")
                    {



                        var user = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault();


                        List<RemindersSelection> remindersForDeliting = (from r in userServices.Reminders(user.EtsUserName).Result
                                                                         select new RemindersSelection { ReminderType = r.Type, Date = r.Date.ToString("yyyy-MM-dd"), ReminderID = r.ReminderId.ToString() }).ToList();



                        var deleteReminderControl = await ViewsInitializer.CreateReminderSelectionControl(remindersForDeliting, "Delete a reminder", "Select a reminder to delete", "deleteReminderRadioButton");

                        deleteReminderControl.channel = shortcutsPayload.user.id;
                        deleteReminderControl.trigger_id = shortcutsPayload.trigger_id;

                        await slackBotClient.WriteMessage(deleteReminderControl, SlackHttpClient.SlackBotClient.PostUriType.openViews);
                    }
                    else if (shortcutsPayload.callback_id == "subscription")
                    {
                        var user = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault();
                        if (user == null)
                        {
                            var addUserControl = await ViewsInitializer.CreateAddUserControl(formTitle: "Subscribe");
                            addUserControl.trigger_id = shortcutsPayload.trigger_id;
                            addUserControl.channel = shortcutsPayload.user.id;
                            await slackBotClient.WriteMessage(addUserControl, SlackHttpClient.SlackBotClient.PostUriType.openViews);


                        }
                        else
                        {
                            var addUserControl = await ViewsInitializer.CreateAddUserControl("Update 'ETS' login", user.EtsUserName, user.EtsPassword);
                            addUserControl.trigger_id = shortcutsPayload.trigger_id;
                            addUserControl.channel = shortcutsPayload.user.id;
                            await slackBotClient.WriteMessage(addUserControl, SlackHttpClient.SlackBotClient.PostUriType.openViews);
                            addUserControl.view_id = botResponse.view.id;

                            addUserControlRef = addUserControl;
                        }




                    }
                    else if (shortcutsPayload.view.callback_id == "updateReminder_submit" && shortcutPayloadActionID == "reminderType")
                    {



                        string reminderType = shortcutsPayload.actions[0].action_id == "reminderType" ?
                       shortcutsPayload.view.state.values.reminderState.reminderType.selected_option.value :
                       string.Empty;




                        // retrive all reminder timers for the given reminder
                        remindersTimesForUpdate = await userServices.RemindersTimers(reminderForUpdateId);

                        addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.updateReminder);




                        addOrEditViewConfiguration.ReminderTimes = remindersTimesForUpdate;


                        //Send user control to slack
                        CreateNewReminder userControll = null;


                        // Create initial view based on type of the reminder

                        switch (reminderType)
                        {
                            case "daily":

                                userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.daily);

                                break;

                            case "weekly":


                                userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.weekly);


                                break;

                            case "monthly":

                                userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.monthly);

                                break;
                        }



                        userControll.view_id = botResponse.view.id;
                        userControll.channel = shortcutsPayload.user.id;
                        userControll.trigger_id = shortcutsPayload.trigger_id;
                        userControll.view.callback_id = "updateReminder_submit";



                        userControll.channel = shortcutsPayload.user.id;
                        userControll.trigger_id = shortcutsPayload.trigger_id;

                        await slackBotClient.WriteMessage(userControll, SlackHttpClient.SlackBotClient.PostUriType.updateViews);

                        //Update the reference view
                        CreateReminderViewRef = userControll;
                        CreateReminderViewRef.view_id = botResponse.view.id;


                    }
                    else if (shortcutsPayload.payloadType == "view_submission")
                    {
                        string callback = shortcutsPayload.view.callback_id;

                        switch (callback)
                        {


                            case "unsubscribe_submit":

                                var userToDelete = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault();

                                if (await userServices.DeleteUser(userToDelete.EtsUserName))
                                {
                                    var timeUnitSuccess = SlackMessageBlock.createMessage("Success", $"The user \"{userToDelete.EtsUserName}\" has been unsubscribed", true, shortcutsPayload.user.id);
                                    await slackBotClient.WriteMessage(timeUnitSuccess);
                                }
                                else
                                {
                                    var timeUnitFailed = SlackMessageBlock.createMessage("Failed", $"Unable to delete the user {userToDelete.EtsUserName}.", false, shortcutsPayload.user.id);
                                    await slackBotClient.WriteMessage(timeUnitFailed);
                                }

                                break;


                            case "createNewReminder":
                                #region handle for creating new reminder

                                string reminderType = shortcutsPayload.view.state.values.reminderState.reminderType.selected_option == null ? string.Empty :
                                shortcutsPayload.view.state.values.reminderState.reminderType.selected_option.text.text;


                                var reminderDate = shortcutsPayload.view.state.values.reminderState.selectedDate;



                                var reminderTime = shortcutsPayload.view.state.values.reminderState.selectedTime_1;

                                var reminderDay = shortcutsPayload.view.state.values.reminderState.reminderDay;

                                int timeZoneOffset = UserProfile.user.tz_offset;

                                if (!string.IsNullOrEmpty(reminderType))
                                {

                                    string etsUserName = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault().EtsUserName;


                                    Reminder reminder = null;

                                    DateTime reminderTimer = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone);


                                    SlackMessageBlock validationMessage = null;


                                    List<string> selectedTimes = new List<string>();

                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_1.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_2.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_3.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_4.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_5.selected_time);


                                    string selectedType = reminderType;

                                    switch (selectedType)
                                    {
                                        case "daily":

                                            if (selectedTimes.Where(t => !string.IsNullOrEmpty(t)).Count() != 0)
                                            {
                                                reminder = new Reminder { Type = reminderType };
                                                await userServices.CreateOrUpdateReminder(reminder, etsUserName);

                                                //Stop the database scanning while entity update completes
                                                Startup.mainTimer.DoQuit();



                                                foreach (string time in selectedTimes)
                                                {
                                                    if (!String.IsNullOrEmpty(time))
                                                    {
                                                        string[] timeParams = time.Split(":");
                                                        var _reminderTime = new DateTime(reminderTimer.Year, reminderTimer.Month, reminderTimer.Day, int.Parse(timeParams[0]), int.Parse(timeParams[1]), 00).AddHours(-(((timeZoneOffset) / 60) / 60));

                                                        await userServices.CreateOrUpdateReminderTimer(new ReminderTime { Time = TimeZoneInfo.ConvertTimeFromUtc(_reminderTime, EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone) }, insertedReminderID);

                                                    }
                                                }


                                                //Resume the database monitoring
                                                Startup.mainTimer.StartMainTimerAsync();

                                                validationMessage = SlackMessageBlock.createMessage("Success", "The reminder has been created", true, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);


                                            }
                                            else
                                            {
                                                validationMessage = SlackMessageBlock.createMessage("Failed", "Please select a reminder time.", false, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);
                                            }


                                            break;





                                        case "weekly":


                                            if (selectedTimes.Where(t => !string.IsNullOrEmpty(t)).Count() != 0 && reminderDay.selected_option != null)
                                            {
                                                var weeklyReminderDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), RemindersMainTimer.ManTimerTimeZone)
                                              .GetDatesOfWeek(CultureInfo.CurrentCulture)
                                               .FirstOrDefault(d => d.ToString("dddd") == reminderDay.selected_option.value);


                                                reminder = new Reminder { Type = reminderType, Date = weeklyReminderDate };
                                                await userServices.CreateOrUpdateReminder(reminder, etsUserName);


                                                //Stop the database scanning while entity update completes
                                                Startup.mainTimer.DoQuit();


                                                foreach (string time in selectedTimes)
                                                {
                                                    if (!String.IsNullOrEmpty(time))
                                                    {
                                                        string[] timeParams = time.Split(":");
                                                        var weeklyDateSelected = new DateTime(reminderTimer.Year, reminderTimer.Month, reminderTimer.Day, int.Parse(timeParams[0]), int.Parse(timeParams[1]), 00).AddHours(-(((timeZoneOffset) / 60) / 60));

                                                        await userServices.CreateOrUpdateReminderTimer(new ReminderTime { Time = TimeZoneInfo.ConvertTimeFromUtc(weeklyDateSelected, EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone) }, insertedReminderID);

                                                    }
                                                }


                                                //Resume the database monitoring
                                                Startup.mainTimer.StartMainTimerAsync();

                                                validationMessage = SlackMessageBlock.createMessage("Success", "The reminder has been created", true, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);

                                            }
                                            else
                                            {
                                                validationMessage = SlackMessageBlock.createMessage("Failed", "Please select the reminder day and time.", false, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);
                                            }





                                            break;

                                        case "monthly":

                                            string[] dateParams = reminderDate.selected_date.Split("-");



                                            DateTime selectedDate = new DateTime(int.Parse(dateParams[0]), int.Parse(dateParams[1]), int.Parse(dateParams[2]));
                                            reminder = new Reminder { Type = reminderType, Date = TimeZoneInfo.ConvertTimeFromUtc(selectedDate, RemindersMainTimer.ManTimerTimeZone) };
                                            await userServices.CreateOrUpdateReminder(reminder, etsUserName);


                                            //Stop the database scanning while entity update completes
                                            Startup.mainTimer.DoQuit();


                                            foreach (string time in selectedTimes)
                                            {
                                                if (!String.IsNullOrEmpty(time))
                                                {
                                                    string[] timeParams = time.Split(":");
                                                    var monthlyDateSelected = new DateTime(reminderTimer.Year, reminderTimer.Month, reminderTimer.Day, int.Parse(timeParams[0]), int.Parse(timeParams[1]), 00).AddHours(-(((timeZoneOffset) / 60) / 60));

                                                    await userServices.CreateOrUpdateReminderTimer(new ReminderTime { Time = TimeZoneInfo.ConvertTimeFromUtc(monthlyDateSelected, RemindersMainTimer.ManTimerTimeZone) }, insertedReminderID);

                                                }
                                            }


                                            //Resume the database monitoring
                                            Startup.mainTimer.StartMainTimerAsync();

                                            validationMessage = SlackMessageBlock.createMessage("Success", "The reminder has been created", true, shortcutsPayload.user.id);

                                            await slackBotClient.WriteMessage(validationMessage);



                                            break;
                                    }
                                }
                                else
                                {
                                    var validationMessage = SlackMessageBlock.createMessage("Faild", "Please select a reminder type.", false, shortcutsPayload.user.id);

                                    await slackBotClient.WriteMessage(validationMessage);
                                }

                                #endregion


                                break;


                            case "theEtsCrudForm":

                                string formType = shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option == null ? "create" :
                                shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option.value;


                                var userData = await _clientServices.UserDetails(shortcutsPayload.user.id);

                                var etsUser = new EtsWebClient.EtsHttpModels.EtsUser(userData.EtsUserName, userData.EtsPassword, userData.SlackChannelID);


                                EtsTimeUnit timeUnit = new EtsTimeUnit();
                                timeUnit.employee_id = EtsEmployeeID;

                                switch (formType)
                                {
                                    case "create":


                                        timeUnit.project_id = shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option.value;
                                        timeUnit.task_type_id = shortcutsPayload.view.state.values.etsCrudFormTasksSelect.etsCrudFormTaskSelected.selected_option.value;
                                        timeUnit.minutes = (int)(double.Parse(shortcutsPayload.view.state.values.etsCrudFormTimeSelect.etsCrudFormTimeSelected.selected_option.value) * 60);
                                        timeUnit.description = shortcutsPayload.view.state.values.etsCrudFormDescription.etsCrudDescriptionText.value;
                                        timeUnit.date = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.text.text.Split(" ")[0];
                                        timeUnit.overtime = false;
                                        timeUnit.next_time_unit_id = null;


                                        if (EtsHttpClient.CreateOrUpdateTimeUnit(etsUser, timeUnit).Result)
                                        {
                                            var timeUnitSuccess = SlackMessageBlock.createMessage("Success", $"The time entry for the {timeUnit.date} has been created", true, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitSuccess);
                                        }
                                        else
                                        {
                                            var timeUnitFailed = SlackMessageBlock.createMessage("Failed", $"Unable to create time entry", false, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitFailed);
                                        }


                                        break;


                                    case "update":

                                        string timeUnitID = shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option.value;



                                        timeUnit.project_id = shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option.value;
                                        timeUnit.task_type_id = shortcutsPayload.view.state.values.etsCrudFormTasksSelect.etsCrudFormTaskSelected.selected_option.value;
                                        timeUnit.minutes = (int)(double.Parse(shortcutsPayload.view.state.values.etsCrudFormTimeSelect.etsCrudFormTimeSelected.selected_option.value) * 60);
                                        timeUnit.description = shortcutsPayload.view.state.values.etsCrudFormDescription.etsCrudDescriptionText.value;
                                        timeUnit.date = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.text.text.Split(" ")[0];
                                        timeUnit.overtime = false;
                                        timeUnit.next_time_unit_id = null;

                                        try
                                        {
                                            await EtsHttpClient.CreateOrUpdateTimeUnit(etsUser, timeUnit, timeUnitID);
                                            var timeUnitSuccess = SlackMessageBlock.createMessage("Success", $"The time entry for the {timeUnit.date} has been modified", true, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitSuccess);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex.Message);
                                            var timeUnitFailed = SlackMessageBlock.createMessage("Failed", $"Unable to modify the time emtry", false, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitFailed);

                                        }


                                        break;


                                    case "delete":

                                        string timeUnitIDForDeleting = shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option.value;
                                        string timeUnitDate = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.text.text.Split(" ")[0];


                                        try
                                        {
                                            await EtsHttpClient.DeleteTimeUnit(etsUser, timeUnitIDForDeleting);
                                            var timeUnitSuccess = SlackMessageBlock.createMessage("Success", $"The time entry for the {timeUnitDate} has been deleted", true, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitSuccess);
                                        }
                                        catch (Exception ex)
                                        {

                                            Debug.WriteLine(ex.Message);
                                            var timeUnitFailedt = SlackMessageBlock.createMessage("Failed", $"Unable to delete the time entry", false, shortcutsPayload.user.id);
                                            await slackBotClient.WriteMessage(timeUnitFailedt);
                                        }



                                        break;
                                }








                                break;




                            case "addNewUser":

                                #region handle for creating a new user


                                string userName = shortcutsPayload.view.state.values.userNameInput.enteredUserName.value;

                                string userPsw = shortcutsPayload.view.state.values.userPswInput.enteredEtsPassword.value;

                                var user = new EtsClientData.EtsDataContext.User { EtsUserName = userName, EtsPassword = userPsw, SlackChannelID = shortcutsPayload.user.id };


                                if (EtsWebClient.Http.EtsHttpClient.CheckUserCredentials(user).Result)
                                {


                                    if (shortcutsPayload.view.title.text == "Update 'ETS' login")
                                    {


                                        if (userServices.CreateOrUpdateUser(user, slackChannelId: shortcutsPayload.user.id).Result)
                                        {
                                            var slackMessage = SlackMessageBlock.createMessage("Success", "The 'ETS' login credentials have been changed.", true, shortcutsPayload.user.id);

                                            await slackBotClient.WriteMessage(slackMessage);
                                        }
                                        else
                                        {
                                            var slackMessage = SlackMessageBlock.createMessage("Failed", "Unable to change 'ETS' login credentials.", false, shortcutsPayload.user.id);

                                            await slackBotClient.WriteMessage(slackMessage);
                                        }
                                    }
                                    else
                                    {
                                        await userServices.CreateOrUpdateUser(user);

                                        var slackMessage = SlackMessageBlock.createMessage("Success", "The user has been created", true, shortcutsPayload.user.id);

                                        await slackBotClient.WriteMessage(slackMessage);
                                    }





                                }
                                else
                                {

                                    var slackMessage = SlackMessageBlock.createMessage("Failed", "Please check the 'ETS' credentials", false, shortcutsPayload.user.id);
                                    await slackBotClient.WriteMessage(slackMessage);

                                }



                                break;
                            #endregion


                            case "deleteReminderRadioButton":

                                int reminderId = int.Parse(shortcutsPayload.view.state.values.radioButtonOption.radioButtonSelection.selected_option.value);
                                await userServices.DeleteReminder(reminderId);

                                var slackMsg = SlackMessageBlock.createMessage("Success", $"The reminder with ID:{reminderId} has been deleted", true, shortcutsPayload.user.id);
                                await slackBotClient.WriteMessage(slackMsg);
                                break;


                            case "updateReminder":
                                #region

                                //Get user's profile data
                                slackBotClient.SlackUserID = shortcutsPayload.user.id;
                                await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);


                                string[] reminderDetails = shortcutsPayload.view.state.values.radioButtonOption.radioButtonSelection.selected_option.text.text.Trim().Split(" ");

                                reminderForUpdateId = int.Parse(shortcutsPayload.view.state.values.radioButtonOption.radioButtonSelection.selected_option.value);


                                // retrive all reminder timers for the given reminder
                                remindersTimesForUpdate = await userServices.RemindersTimers(reminderForUpdateId);


                                addOrEditViewConfiguration = new ControlConfigurator(ControlConfigurator.controlInterface.updateReminder);

                                addOrEditViewConfiguration.ReminderTimes = remindersTimesForUpdate;

                                addOrEditViewConfiguration.ReminderTypePlaceholderText = $"Modify the type: {reminderDetails[0]}";


                                //Send user control to slack
                                CreateNewReminder userControll = null;


                                // Create initial view based on type of the reminder

                                switch (reminderDetails[0])
                                {
                                    case "daily":

                                        userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.daily);

                                        break;

                                    case "weekly":

                                        addOrEditViewConfiguration.WeekdayPlaceholder = $"Modify the day: {reminderDetails[2]}";
                                        userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.weekly);

                                        break;

                                    case "monthly":

                                        addOrEditViewConfiguration.ReminderDatePlaceholder = $"Modify the date: {reminderDetails[2]}";
                                        userControll = await ViewsInitializer.ReminderUserControlSwitcher(addOrEditViewConfiguration, UserProfile, ViewsInitializer.createNewReminderControlType.monthly);

                                        break;
                                }



                                userControll.channel = shortcutsPayload.user.id;
                                userControll.trigger_id = shortcutsPayload.trigger_id;

                                await slackBotClient.WriteMessage(userControll, SlackHttpClient.SlackBotClient.PostUriType.openViews);

                                //Update the reference view
                                CreateReminderViewRef = userControll;
                                CreateReminderViewRef.view_id = botResponse.view.id;


                                break;
                            #endregion

                            case "updateReminder_submit":
                                #region



                                //Params
                                string reminderTypeSubmit = shortcutsPayload.view.state.values.reminderState.reminderType.selected_option == null ? string.Empty :
                               shortcutsPayload.view.state.values.reminderState.reminderType.selected_option.text.text;


                                var reminderDateSubmit = shortcutsPayload.view.state.values.reminderState.selectedDate;

                                int updateTimeZoneOffset = UserProfile.user.tz_offset;

                                var reminderTimeSubmit = shortcutsPayload.view.state.values.reminderState.selectedTime_1;

                                var reminderDaySubmit = shortcutsPayload.view.state.values.reminderState.reminderDay;

                                int timeZoneOffsetSubmit = UserProfile.user.tz_offset;


                                if (!string.IsNullOrEmpty(reminderTypeSubmit))
                                {

                                    string etsUserName = _context.Users.Where(u => u.SlackChannelID == shortcutsPayload.user.id).FirstOrDefault().EtsUserName;


                                    Reminder reminder = null;

                                    DateTime reminderTimer = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone);


                                    SlackMessageBlock validationMessage = null;


                                    List<string> selectedTimes = new List<string>();

                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_1.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_2.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_3.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_4.selected_time);
                                    selectedTimes.Add(shortcutsPayload.view.state.values.reminderState.selectedTime_5.selected_time);


                                    var submitedNullExcluded = selectedTimes.Where(s => !String.IsNullOrEmpty(s)).ToList();




                                    string selectedType = reminderTypeSubmit;

                                    switch (selectedType)
                                    {

                                        case "daily":

                                            if (selectedTimes.Where(t => !string.IsNullOrEmpty(t)).Count() != 0)
                                            {
                                                reminder = new Reminder { Type = selectedType };
                                                await userServices.CreateOrUpdateReminder(reminder, etsUserName, reminderForUpdateId);


                                            }
                                            else
                                            {
                                                validationMessage = SlackMessageBlock.createMessage("Failed", "Unable to update the reminder. Please select a reminder time.", false, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);
                                            }


                                            break;





                                        case "weekly":


                                            if (selectedTimes.Where(t => !string.IsNullOrEmpty(t)).Count() != 0 && reminderDaySubmit.selected_option != null)
                                            {
                                                var weeklyReminderDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), RemindersMainTimer.ManTimerTimeZone)
                                              .GetDatesOfWeek(CultureInfo.CurrentCulture)
                                               .FirstOrDefault(d => d.ToString("dddd") == reminderDaySubmit.selected_option.value);


                                                reminder = new Reminder { Type = selectedType, Date = weeklyReminderDate };
                                                await userServices.CreateOrUpdateReminder(reminder, etsUserName, reminderForUpdateId);




                                            }
                                            else
                                            {
                                                validationMessage = SlackMessageBlock.createMessage("Failed", "Unable to update the reminder", false, shortcutsPayload.user.id);

                                                await slackBotClient.WriteMessage(validationMessage);
                                            }





                                            break;

                                        case "monthly":

                                            string[] dateParams = reminderDateSubmit.selected_date.Split("-");



                                            DateTime selectedDate = new DateTime(int.Parse(dateParams[0]), int.Parse(dateParams[1]), int.Parse(dateParams[2]));
                                            reminder = new Reminder { Type = selectedType, Date = TimeZoneInfo.ConvertTimeFromUtc(selectedDate, RemindersMainTimer.ManTimerTimeZone) };
                                            await userServices.CreateOrUpdateReminder(reminder, etsUserName, reminderForUpdateId);

                                            break;
                                    }



                                    Startup.mainTimer.DoQuit();


                                    int excessReminderTime = submitedNullExcluded.Count();





                                    //Delete excess reminder times.
                                    if (excessReminderTime < remindersTimesForUpdate.Count)
                                    {
                                        foreach (int timerId in remindersTimesForUpdate.Skip(excessReminderTime).Select(r => r.ReminderTimeId).ToList())
                                        {
                                            await userServices.DeleteReminderTimer(timerId);
                                        }
                                    }


                                    //Update reminder times changes
                                    var serverTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), RemindersMainTimer.ManTimerTimeZone);

                                    int reminderTimerId = 0;

                                    var reminderTimerIdQueue = new Queue<ReminderTime>(remindersTimesForUpdate);
                                    foreach (string time in submitedNullExcluded)
                                    {

                                        reminderTimerId = reminderTimerIdQueue.Count != 0 ? reminderTimerIdQueue.Dequeue().ReminderTimeId : 0;

                                        string[] timeParams = time.Split(":");
                                        var reminderDateChangeConverted = new DateTime(serverTime.Year, serverTime.Month, serverTime.Day, int.Parse(timeParams[0]), int.Parse(timeParams[1]), 00).AddHours(-(((updateTimeZoneOffset) / 60) / 60));


                                        await userServices.CreateOrUpdateReminderTimer(new ReminderTime { Time = TimeZoneInfo.ConvertTimeFromUtc(reminderDateChangeConverted, EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone) }, reminderForUpdateId, reminderTimerId);


                                    }




                                    //Resume the database monitoring
                                    Startup.mainTimer.StartMainTimerAsync();



                                    var msgSuccess = SlackMessageBlock.createMessage("Success", "The reminder has been modified", true, shortcutsPayload.user.id);

                                    await slackBotClient.WriteMessage(msgSuccess);




                                }


                                break;
                                #endregion




                        }









                    }
                    else if (shortcutsPayload.payloadType == "block_actions")
                    {

                        string btnClick = shortcutsPayload.actions[0].action_id;

                        string etsDataCrudFormAction = shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option != null ?
                       shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option.value : string.Empty;

                        var userData = await _clientServices.UserDetails(shortcutsPayload.user.id);

                        var etsUser = new EtsWebClient.EtsHttpModels.EtsUser(userData.EtsUserName, userData.EtsPassword, userData.SlackChannelID);

                        var EtsClient = new EtsHttpClient(etsUser);

                        string projectId = string.Empty;
                        string timeUnitID = string.Empty;
                        string timeUnitsDate = string.Empty;

                        switch (btnClick)
                        {

                            #region reminders handling

                            case "addReminderTimer":


                                int timePickersCount = CreateReminderViewRef.view.Blocks[0].Elements.OfType<Timepicker>().Count();
                                addOrEditViewConfiguration.InitilNumOfAddTimeControl = timePickersCount + 1;


                                //Get user's profile data
                                slackBotClient.SlackUserID = shortcutsPayload.user.id;
                                await slackBotClient.WriteMessage(postUriType: SlackHttpClient.SlackBotClient.PostUriType.userInfo);






                                var userControll = await ViewsInitializer.CreateOrUpdateReminderControl(addOrEditViewConfiguration, UserProfile);



                                userControll.view_id = botResponse.view.id;
                                userControll.channel = shortcutsPayload.user.id;
                                userControll.trigger_id = shortcutsPayload.trigger_id;
                                await slackBotClient.WriteMessage(userControll, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                                CreateReminderViewRef = userControll;
                                break;

                            case "deleteTimer":




                                var timePickers = CreateReminderViewRef.view.Blocks[0].Elements.OfType<Timepicker>().ToList();

                                if (timePickers.Count > 1)
                                {
                                    timePickers.RemoveAt(timePickers.Count - 1);
                                };




                                StaticSelect staticSelect = CreateReminderViewRef.view.Blocks[0].Elements.OfType<StaticSelect>().ElementAt(0);
                                DatePicker datePicker = CreateReminderViewRef.view.Blocks[0].Elements.OfType<DatePicker>().FirstOrDefault();
                                StaticSelect reminderDayPicker = CreateReminderViewRef.view.Blocks[0].Elements.OfType<StaticSelect>().Count() > 1 ? CreateReminderViewRef.view.Blocks[0].Elements.OfType<StaticSelect>().ElementAt(1) : null;
                                List<Button> buttons = CreateReminderViewRef.view.Blocks[0].Elements.OfType<Button>().ToList();

                                var viewElements = new List<BlockElement>();
                                viewElements.Add(staticSelect);
                                viewElements.Add(datePicker);
                                viewElements.Add(reminderDayPicker);


                                if (timePickers.Count == 1)
                                {
                                    buttons.RemoveAt(1);
                                }


                                CreateReminderViewRef.view.Blocks[0].Elements.Clear();

                                CreateReminderViewRef.view.Blocks[0].Elements.AddRange(viewElements.Where(e => e != null));

                                CreateReminderViewRef.view.Blocks[0].Elements.AddRange(timePickers);

                                CreateReminderViewRef.view.Blocks[0].Elements.AddRange(buttons);

                                var userControlUpdated = CreateReminderViewRef;

                                await slackBotClient.WriteMessage(userControlUpdated, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                                CreateReminderViewRef.view_id = botResponse.view.id;
                                CreateReminderViewRef = userControlUpdated;
                                break;



                            case "unsub_btn_click":



                                InfoPopUp infoPopUp = new InfoPopUp("Submit", "unsubscribe_submit", "Unsubscribe", "Cancel", "Are you sure? All reminders will be deleted as well.");
                                infoPopUp.channel = shortcutsPayload.user.id;
                                infoPopUp.trigger_id = shortcutsPayload.trigger_id;
                                infoPopUp.view_id = addUserControlRef.view_id;

                                //Open the confirmation dialog
                                await slackBotClient.WriteMessage(infoPopUp, SlackHttpClient.SlackBotClient.PostUriType.updateViews);



                                break;

                            #endregion
                            case "etsCrudFormProjectSelected":



                                projectId = shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option != null ?
                                shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option.value : string.Empty;

                                if (!string.IsNullOrEmpty(projectId))
                                {
                                    string timeUnitsDateSelected = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option != null ?
                                        shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.value : string.Empty;

                                    timeUnitID = shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect != null ?
                                    shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option != null ?
                                    shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option.value : string.Empty : string.Empty;

                                    CollectedUserEtsData = await EtsClient.CreateViewModel();



                                    var updateView = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, projectId, timeUnitsDateSelected, timeUnitId: timeUnitID);
                                    updateView.channel = shortcutsPayload.user.id;
                                    updateView.trigger_id = shortcutsPayload.trigger_id;
                                    updateView.view_id = TimeUnitCRUDFormRef.view_id;
                                    await slackBotClient.WriteMessage(updateView, SlackHttpClient.SlackBotClient.PostUriType.updateViews);

                                }
                                break;



                            case "etsCrudFormDateSelected":


                                string selectedFormAction = etsDataCrudFormAction == "" ? "create" : etsDataCrudFormAction;

                                timeUnitsDate = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option != null ?
                                shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.value :
                                string.Empty;

                                if (!string.IsNullOrEmpty(timeUnitsDate))
                                {
                                    CollectedUserEtsData = await EtsClient.CreateViewModel();


                                    if (selectedFormAction.Equals("create"))
                                    {




                                        var timeUnitsView = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, timeUntDate: timeUnitsDate);
                                        timeUnitsView.channel = shortcutsPayload.user.id;
                                        timeUnitsView.trigger_id = shortcutsPayload.trigger_id;
                                        timeUnitsView.view_id = TimeUnitCRUDFormRef.view_id;
                                        await slackBotClient.WriteMessage(timeUnitsView, SlackHttpClient.SlackBotClient.PostUriType.updateViews);

                                    }
                                    else if (selectedFormAction.Equals("update"))
                                    {




                                        var timeUnitsView = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, timeUntDate: timeUnitsDate);
                                        timeUnitsView.channel = shortcutsPayload.user.id;
                                        timeUnitsView.trigger_id = shortcutsPayload.trigger_id;
                                        timeUnitsView.view_id = TimeUnitCRUDFormRef.view_id;
                                        await slackBotClient.WriteMessage(timeUnitsView, SlackHttpClient.SlackBotClient.PostUriType.updateViews);


                                    }
                                    else if (selectedFormAction.Equals("delete"))
                                    {




                                        var timeUnitsView = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, timeUntDate: timeUnitsDate);
                                        timeUnitsView.channel = shortcutsPayload.user.id;
                                        timeUnitsView.trigger_id = shortcutsPayload.trigger_id;
                                        timeUnitsView.view_id = TimeUnitCRUDFormRef.view_id;
                                        await slackBotClient.WriteMessage(timeUnitsView, SlackHttpClient.SlackBotClient.PostUriType.updateViews);

                                    }

                                }
                                break;




                            case "etsCrudFormActionSelected":


                                string selectedOption = shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option.value;

                                projectId = shortcutsPayload.view.state.values.etsCrudFormProjectSelect != null ?
                                             shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option != null ?
                                             shortcutsPayload.view.state.values.etsCrudFormProjectSelect.etsCrudFormProjectSelected.selected_option.value :
                                             string.Empty :
                                             string.Empty;


                                timeUnitsDate = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option != null ?
                                                shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.value :
                                                string.Empty;


                                CollectedUserEtsData = await EtsClient.CreateViewModel();


                                EtsEmployeeID = CollectedUserEtsData.EmployeeDetails.Id;

                                switch (selectedOption)
                                {
                                    case "create":



                                        EtsCrudFormConfigurator = new EtsCrudFormConfigurator(EtsCrudFormConfigurator.EtsCrudFormType.create);

                                        var etsCrudFormCreateOption = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, projectId: projectId, timeUntDate: timeUnitsDate);

                                        etsCrudFormCreateOption.channel = shortcutsPayload.user.id;
                                        etsCrudFormCreateOption.trigger_id = shortcutsPayload.trigger_id;
                                        etsCrudFormCreateOption.view_id = TimeUnitCRUDFormRef.view_id;
                                        await slackBotClient.WriteMessage(etsCrudFormCreateOption, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                                        TimeUnitCRUDFormRef = etsCrudFormCreateOption;

                                        break;

                                    case "update":



                                        timeUnitID = shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect != null ?
                                         shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option != null ?
                                         shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option.value : string.Empty : string.Empty;


                                        EtsCrudFormConfigurator = new EtsCrudFormConfigurator(EtsCrudFormConfigurator.EtsCrudFormType.update);


                                        var etsCrudFormUpdateOption = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, projectId: projectId, timeUntDate: timeUnitsDate, timeUnitId: timeUnitID);

                                        etsCrudFormUpdateOption.channel = shortcutsPayload.user.id;
                                        etsCrudFormUpdateOption.trigger_id = shortcutsPayload.trigger_id;
                                        etsCrudFormUpdateOption.view_id = TimeUnitCRUDFormRef.view_id;

                                        await slackBotClient.WriteMessage(etsCrudFormUpdateOption, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                                        TimeUnitCRUDFormRef = etsCrudFormUpdateOption;

                                        break;


                                    case "delete":



                                        EtsCrudFormConfigurator = new EtsCrudFormConfigurator(EtsCrudFormConfigurator.EtsCrudFormType.delete);

                                        var etsCrudFormDeleteOption = await ViewsInitializer.CreateETS_CRUD_Form(EtsCrudFormConfigurator, CollectedUserEtsData, timeUntDate: timeUnitsDate);

                                        etsCrudFormDeleteOption.channel = shortcutsPayload.user.id;
                                        etsCrudFormDeleteOption.trigger_id = shortcutsPayload.trigger_id;
                                        etsCrudFormDeleteOption.view_id = TimeUnitCRUDFormRef.view_id;
                                        await slackBotClient.WriteMessage(etsCrudFormDeleteOption, SlackHttpClient.SlackBotClient.PostUriType.updateViews);
                                        TimeUnitCRUDFormRef = etsCrudFormDeleteOption;

                                        break;
                                }

                                break;


                            case "etsCrudFormTimeUnitSelected":


                                if (shortcutsPayload.view.state.values.etsCrudFormActionSelector.etsCrudFormActionSelected.selected_option.value != "delete")
                                {

                                    timeUnitID = shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option != null ?
                                    shortcutsPayload.view.state.values.etsCrudFormTimeUnitSelect.etsCrudFormTimeUnitSelected.selected_option.value :
                                    string.Empty;

                                    if (!string.IsNullOrEmpty(timeUnitID))
                                    {
                                        timeUnitsDate = shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option != null ?
                                                  shortcutsPayload.view.state.values.etsCrudFormDateSelector.etsCrudFormDateSelected.selected_option.value :
                                                  string.Empty;

                                        CollectedUserEtsData = await EtsClient.CreateViewModel();

                                        var etsCrudFormTimeUitUpdate = await ViewsInitializer.CreateETS_CRUD_Form(new EtsCrudFormConfigurator(EtsCrudFormConfigurator.EtsCrudFormType.update), CollectedUserEtsData, timeUntDate: timeUnitsDate, timeUnitId: timeUnitID);

                                        etsCrudFormTimeUitUpdate.channel = shortcutsPayload.user.id;
                                        etsCrudFormTimeUitUpdate.trigger_id = shortcutsPayload.trigger_id;
                                        etsCrudFormTimeUitUpdate.view_id = TimeUnitCRUDFormRef.view_id;

                                        await slackBotClient.WriteMessage(etsCrudFormTimeUitUpdate, SlackHttpClient.SlackBotClient.PostUriType.updateViews);

                                    }

                                }
                                break;


                        }







                    }


                }





            });



            return Ok();
        }



        [HttpPost]
        public IActionResult SlackHandler(ContentRoot content)
        {


            if (content.Type.Contains("event_callback"))
            {
                if (content.Event.Type == "app_mention" || (content.Event.Type == "message" && content.Event.client_msg_id != string.Empty))
                {

                    string responseChannel = content.Event.Type == "app_mention" ? content.Event.Channel : content.Event.User;
                    Task processBotRequest = Task.Run(async () => { await ProcessBotAppMentionEvent(content, responseChannel); });

                    return Ok();
                }
                else if (content.Event.Type.Equals("app_home_opened"))
                {
                    string responseChannel = content.Event.Type == "app_mention" ? content.Event.Channel : content.Event.User;
                    Task sendHomeTab = Task.Run(async () => { await SendBotHomeTab(responseChannel); });
                    return Ok();

                }


            }

            else
            {
                return Ok(new JsonResult(content.Challenge));

            }

            return Ok();


        }

        private async Task SendBotHomeTab(string userId)
        {
            EtsDataContext _context = new EtsDataContext();
            EtsClientServices _clientServices = new EtsClientServices(_context);
            IUserServices userServices = _clientServices;

            SlackHttpClient.SlackBotClient slackBotClient = new SlackHttpClient.SlackBotClient(userServices.SlackBotToken());

            string mainMessage = "*I can help you to manage the \"ETS\" entries from within \"Slack\".* \nJust click on the button.";
            var homeMessageBlock = new HomeMessageBlock(userId, "Hello! :smiley:", "ETS", mainMessage, "EtsDataBtnClick");

            await slackBotClient.WriteMessage(homeMessageBlock, SlackHttpClient.SlackBotClient.PostUriType.botHomeTab);
        }


        private async Task ProcessBotAppMentionEvent(ContentRoot content, string responseChannel)
        {
            EtsDataContext _context = new EtsDataContext();
            EtsClientServices _clientServices = new EtsClientServices(_context);
            IUserServices userServices = _clientServices;


            SlackHttpClient.SlackBotClient slackBotClient = new SlackHttpClient.SlackBotClient(userServices.SlackBotToken());


            DateTime serverTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), EtsWebClient.MainTimer.RemindersMainTimer.ManTimerTimeZone);



            string eventCommand = content.Event.Text.Split('*').First().Trim();


            Debug.WriteLine(content.Event.Text);
            if (eventCommand.StartsWith("<@"))
            {



                var postObject = new { channel = responseChannel, channel_type = "im", text = $"Hello! I'm waiting. {serverTime}" };
                await slackBotClient.WriteMessage(postObject);
            }
            else if (eventCommand.Equals("Add new user"))
            {
                string[] controlerParams = ParseCommand(content.Event.Text).Result.Split(' ');

                var user = new EtsClientData.EtsDataContext.User { EtsUserName = controlerParams[0], EtsPassword = controlerParams[1], SlackChannelID = content.Event.User };


                if (EtsWebClient.Http.EtsHttpClient.CheckUserCredentials(user).Result)
                {

                    await userServices.CreateOrUpdateUser(user);

                    var postObject = new { channel = responseChannel, text = "The user has been created" };

                    await slackBotClient.WriteMessage(postObject);
                }
                else
                {
                    var postObject = new { channel = responseChannel, text = "Please check the 'ETS' credentials" };
                    await slackBotClient.WriteMessage(postObject);

                }
            }
            else if (eventCommand.Equals("Update user"))
            {

                string[] controlerParams = ParseCommand(content.Event.Text).Result.Split(' ');

                var user = new EtsClientData.EtsDataContext.User { EtsUserName = controlerParams[0], EtsPassword = controlerParams[1], SlackChannelID = content.Event.User };
                bool check = await EtsWebClient.Http.EtsHttpClient.CheckUserCredentials(user);
                if (check)
                {

                    bool update = await userServices.CreateOrUpdateUser(user, slackChannelId: content.Event.User);
                    if (update)
                    {
                        var postObject = new { channel = responseChannel, text = "The user has been updated" };
                        await slackBotClient.WriteMessage(postObject);

                    }
                    else
                    {
                        var postObject = new { channel = responseChannel, text = $"Unable to update the user '{controlerParams[0]}'. The database does not contain the user. " };
                        await slackBotClient.WriteMessage(postObject);

                    }
                }
                else
                {
                    var postObject = new { channel = responseChannel, text = "Please check the 'ETS' credentials" };
                    await slackBotClient.WriteMessage(postObject);

                }
            }
            else if (eventCommand.Equals("Delete user"))
            {

                var result = await userServices.DeleteUser(ParseCommand(content.Event.Text).Result);


                if (result)
                {
                    var postObject = new { channel = responseChannel, text = $"The user {ParseCommand(content.Event.Text).Result} has been removed" };
                    await slackBotClient.WriteMessage(postObject);

                }
                else
                {
                    var postObject = new { channel = responseChannel, text = $"There is no user with '{ParseCommand(content.Event.Text).Result}' user name" };
                    await slackBotClient.WriteMessage(postObject);

                }

            }
            else if (eventCommand.Equals("Add reminder"))
            {

                string[] controlerParams = ParseCommand(content.Event.Text).Result.Split(' ');
                var reminder = new Reminder { Type = controlerParams[1], Date = DateTime.Parse(controlerParams[2]) };


                await userServices.CreateOrUpdateReminder(reminder, controlerParams[0]);
                var postObject = new { channel = responseChannel, text = $"The new reminder has been created" };
                await slackBotClient.WriteMessage(postObject);




            }
            else if (eventCommand.Equals("All reminders"))
            {
                var result = await userServices.Reminders(ParseCommand(content.Event.Text).Result);
                if (result != null)
                {
                    var postObject = new { channel = responseChannel, text = JsonConvert.SerializeObject(result) };
                    await slackBotClient.WriteMessage(postObject);

                }
                else
                {
                    var postObject = new { channel = responseChannel, text = "There are no reminders for the user" };
                    await slackBotClient.WriteMessage(postObject);

                }
            }
            else if (eventCommand.Equals("Delete reminder"))
            {

                int reminderId;

                if (int.TryParse(ParseCommand(content.Event.Text).Result, out reminderId))
                {
                    await userServices.DeleteReminder(reminderId);
                    var postObject = new { channel = responseChannel, text = "Reminder deleted" };
                    await slackBotClient.WriteMessage(postObject);

                }


            }
            else if (eventCommand.Equals("Add timer"))
            {
                int reminderId;
                DateTime reminderTimerTime;
                string[] controlerParams = ParseCommand(content.Event.Text).Result.Split(' ');

                if (int.TryParse(controlerParams[0], out reminderId) && DateTime.TryParse(controlerParams[1].Replace('/', ' '), out reminderTimerTime))
                {
                    try
                    {
                        EtsClientData.EtsDataContext.ReminderTime reminderTime = new ReminderTime { Time = reminderTimerTime };
                        await userServices.CreateOrUpdateReminderTimer(reminderTime, reminderId);
                        var postObject = new { channel = responseChannel, text = "Reminder timer created" };
                        await slackBotClient.WriteMessage(postObject);

                    }
                    catch (Exception)
                    {

                        var postObject = new { channel = responseChannel, text = "Unable to add reminder timer" };
                        await slackBotClient.WriteMessage(postObject);

                    }



                }
                else
                {
                    var postObject = new { channel = responseChannel, text = "Bad command" };
                    await slackBotClient.WriteMessage(postObject);

                }


            }
            else if (eventCommand.Equals("Change remider time"))
            {
                int reminderId;
                int reminderTimeId;
                DateTime reminderTimerTime;
                bool notified;
                string[] controlerParams = ParseCommand(content.Event.Text).Result.Split(' ');

                if (int.TryParse(controlerParams[0], out reminderId) && int.TryParse(controlerParams[1], out reminderTimeId) && DateTime.TryParse(controlerParams[2].Replace('/', ' '), out reminderTimerTime) && bool.TryParse(controlerParams[3], out notified))
                {
                    try
                    {
                        EtsClientData.EtsDataContext.ReminderTime reminderTime = new ReminderTime { Time = reminderTimerTime, Notified = notified.ToString() };
                        await userServices.CreateOrUpdateReminderTimer(reminderTime, reminderId, reminderTimeId);
                        var postObject = new { channel = responseChannel, text = "Reminder time changed" };
                        await slackBotClient.WriteMessage(postObject);

                    }
                    catch (Exception)
                    {

                        var postObject = new { channel = responseChannel, text = "Unable to change reminder time" };
                        await slackBotClient.WriteMessage(postObject);

                    }



                }
                else
                {
                    var postObject = new { channel = responseChannel, text = "Bad command" };
                    await slackBotClient.WriteMessage(postObject);

                }


            }
            else if (eventCommand.Equals("All reminder timers"))
            {

                var result = await userServices.RemindersTimers(int.Parse(ParseCommand(content.Event.Text).Result));
                var postObject = new { channel = responseChannel, text = JsonConvert.SerializeObject(result) };
                await slackBotClient.WriteMessage(postObject);


            }
            else if (eventCommand.Equals("Remove reminder timer"))
            {
                try
                {


                    var result = userServices.DeleteReminderTimer(int.Parse(ParseCommand(content.Event.Text).Result));
                    var postObject = new { channel = responseChannel, text = "Reminder timer deleted" };
                    await slackBotClient.WriteMessage(postObject);

                }
                catch (Exception)
                {

                    var postObject = new { channel = responseChannel, text = "Unable to delete reminder timer" };
                    await slackBotClient.WriteMessage(postObject);
                }


            }


        }

    }
}













