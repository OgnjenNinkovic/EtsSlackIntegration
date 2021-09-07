using EtsClientApi.Api;
using EtsClientApi.SlackModels;
using EtsWebClient.EtsHttpModels;
using EtsWebClient.MainTimer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EtsClientApi.SlackModels.CreateNewReminder;
using static EtsClientApi.SlackModels.AddUserControl;


namespace EtsClientApi.SlackViews
{
    public class ViewsInitializer
    {

        public enum createNewReminderControlType { daily, weekly, monthly };


        public static string ConvertBackToUserTime(DateTime storedTime, int usersTimeOffset)
        {

            DateTime serverTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), RemindersMainTimer.ManTimerTimeZone);


            double serverTimeOffset = Math.Round(((serverTimeZone.TimeOfDay - DateTime.UtcNow.TimeOfDay).TotalSeconds));

            var timeDifference = Math.Round((((serverTimeOffset - usersTimeOffset) / 60) / 60)) < 0 ?
                (Math.Round((((serverTimeOffset - usersTimeOffset) / 60) / 60))) * -1 :
                Math.Round((((serverTimeOffset - usersTimeOffset) / 60) / 60));


            DateTime convertBack = usersTimeOffset < 0 ? storedTime.AddHours(-timeDifference) : storedTime.AddHours(timeDifference);

            return convertBack.ToString("HH:mm tt");

        }




        public static Task<CreateNewReminder> CreateOrUpdateReminderControl(ControlConfigurator UpdateControlConfigurator, UserProfile userProfile)
        {

           Task<CreateNewReminder> createUserControll = Task.Run(() =>
            {



                var placeholder = new BlockPlaceholder();
                placeholder.Type = "plain_text";
                placeholder.Text = "Select a reminder type";


                var dailyOptionText = new BlockText();
                dailyOptionText.Type = "plain_text";
                dailyOptionText.Text = "daily";
                var dailyOption = new BlockOption(dailyOptionText);

                dailyOption.Value = "daily";



                var weeklyOptionText = new BlockText();
                weeklyOptionText.Type = "plain_text";
                weeklyOptionText.Text = "weekly";

                var weeklyOption = new BlockOption(weeklyOptionText);
                weeklyOption.Value = "weekly";


                var monthlyOptionText = new BlockText();
                monthlyOptionText.Type = "plain_text";
                monthlyOptionText.Text = "monthly";


                var monthlyOption = new BlockOption(monthlyOptionText);
                monthlyOption.Value = "monthly";


                List<ControllBlock> blocks = new List<ControllBlock>();
                List<BlockElement> elements = new List<BlockElement>();
                List<BlockOption> options = new List<BlockOption>();



                options.Add(dailyOption);
                options.Add(weeklyOption);
                options.Add(monthlyOption);


                StaticSelect reminderTypeSelect = new StaticSelect(placeholder, options);

                reminderTypeSelect.Type = "static_select";
                reminderTypeSelect.ActionId = "reminderType";
                reminderTypeSelect.Placeholder.Type = "plain_text";
                reminderTypeSelect.Placeholder.Text = UpdateControlConfigurator.ReminderTypePlaceholderText;

                elements.Add(reminderTypeSelect);







                if (UpdateControlConfigurator.showWeekdaysStaticSelect)
                {
                    var weeklyReminderPlaceholder = new BlockPlaceholder();
                    weeklyReminderPlaceholder.Text = "Pick a day in week";

                    List<BlockOption> staticSelectOptions = new List<BlockOption>();


                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Monday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Tuesday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Wednesday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Thursday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Friday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Saturday")));
                    staticSelectOptions.Add(new BlockOption(new BlockText(staticOptionText: "Sunday")));

                    StaticSelect weekdays = new StaticSelect(weeklyReminderPlaceholder, staticSelectOptions);
                    weekdays.Type = "static_select";
                    weekdays.ActionId = "reminderDay";
                    weekdays.Placeholder.Type = "plain_text";
                    weekdays.Placeholder.Text = UpdateControlConfigurator.WeekdayPlaceholder;


                    elements.Add(weekdays);

                }










                BlockPlaceholder datePlacehoder = new BlockPlaceholder();
                datePlacehoder.Type = "plain_text";
                datePlacehoder.Text = UpdateControlConfigurator.ReminderDatePlaceholder;




                DatePicker datePcker = new DatePicker(datePlacehoder);

                datePcker.ElementType = "datepicker";
                datePcker.ActionId = "selectedDate";


                if (UpdateControlConfigurator.showDatepicker)
                {
                    elements.Add(datePcker);
                }





                BlockPlaceholder timePckerPlaceHolder = new BlockPlaceholder();

                timePckerPlaceHolder.Type = "plain_text";
                timePckerPlaceHolder.Text = "Select time";


                //Create timePicker user controls
                var timepickers = new List<Timepicker>();
                timepickers.Add(new Timepicker(timePckerPlaceHolder, action_id: "selectedTime_1"));
                timepickers.Add(new Timepicker(timePckerPlaceHolder, action_id: "selectedTime_2"));
                timepickers.Add(new Timepicker(timePckerPlaceHolder, action_id: "selectedTime_3"));
                timepickers.Add(new Timepicker(timePckerPlaceHolder, action_id: "selectedTime_4"));
                timepickers.Add(new Timepicker(timePckerPlaceHolder, action_id: "selectedTime_5"));





                if (UpdateControlConfigurator.isUpdate)
                {


                    int actionId = 0;
                    foreach (var time in UpdateControlConfigurator.ReminderTimes)
                    {
                        actionId++;

                        elements.Add(new Timepicker(new BlockPlaceholder($"Modify the time {ConvertBackToUserTime(time.Time, userProfile.user.tz_offset)}"), action_id: $"selectedTime_{actionId}"));
                    }
                    if (UpdateControlConfigurator.ReminderTimes.Count < UpdateControlConfigurator.InitilNumOfAddTimeControl)
                    {

                        foreach (Timepicker timePicker in timepickers.Skip(UpdateControlConfigurator.ReminderTimes.Count).Take(UpdateControlConfigurator.InitilNumOfAddTimeControl - 1))
                        {

                            elements.Add(timePicker);
                        }

                    }



                }
                else
                {
                    //Limit the number of the rendered  timePicker user contrlos 
                    foreach (Timepicker timePicker in timepickers.Take(UpdateControlConfigurator.InitilNumOfAddTimeControl))
                    {
                        elements.Add(timePicker);
                    }

                }


                BlockText btnText = new BlockText();

                btnText.Type = "plain_text";
                btnText.Text = UpdateControlConfigurator.BtnPrimaryLabel;

                Button button = new Button(btnText);
                button.ElementType = "button";
                button.Style = "primary";
                button.BtnValue = "addTimer";
                button.ActionId = "addReminderTimer";


                elements.Add(button);


                BlockText deleteBtnText = new BlockText();

                deleteBtnText.Type = "plain_text";
                deleteBtnText.Text = UpdateControlConfigurator.BtnDeleteLabel;

                Button deleteBtn = new Button(deleteBtnText);
                deleteBtn.ElementType = "button";
                deleteBtn.Style = "danger";
                deleteBtn.BtnValue = "deleteTimer";
                deleteBtn.ActionId = "deleteTimer";

                if (UpdateControlConfigurator.isUpdate)
                {
                    if (SlackController.addOrEditViewConfiguration.ReminderTimes.Count > 1 || elements.Count > 4)
                    {
                        elements.Add(deleteBtn);
                    }
                }



                ControllBlock block = new ControllBlock(elements);

                block.Type = "actions";
                block.BlockId = "reminderState";

                blocks.Add(block);

                AddReminderControll view = new AddReminderControll(blocks, new AddReminderTitle(text: UpdateControlConfigurator.DialogTitle), new AddReminderSubmitBtn(), new AddReminderCloseBtn());
                view.type = "modal";
                view.callback_id = UpdateControlConfigurator.CallBackId;



                CreateNewReminder createNewReminder = new CreateNewReminder(view);


                return createNewReminder;

            });



            return createUserControll;



        }


        public static Task<CreateNewReminder> ReminderUserControlSwitcher(ControlConfigurator controlConfigurator, UserProfile userProfile, createNewReminderControlType reminderControlType)
        {
            Task<CreateNewReminder> renderControl = Task.Run(async () =>
            {


                //Send user control to slack
                CreateNewReminder userControll = null;


                // User control elements
                StaticSelect reminderTypeControl = null;
                StaticSelect reminderDayControl = null;
                DatePicker datePickerControl = null;
                List<Timepicker> timepickerControl = null;
                List<Button> controlButtons = null;

                switch (reminderControlType)
                {
                    case createNewReminderControlType.daily:

                        //Turn on user conntrol for type of reminder

                        //Initialize renderring of user control
                        userControll = await CreateOrUpdateReminderControl(controlConfigurator, userProfile);

                        // extract the nessesery user control from rendered  view
                        reminderTypeControl = userControll.view.Blocks[0].Elements.OfType<StaticSelect>().FirstOrDefault();
                        timepickerControl = userControll.view.Blocks[0].Elements.OfType<Timepicker>().ToList();
                        controlButtons = userControll.view.Blocks[0].Elements.OfType<Button>().ToList();

                        // clear renderd control elements

                        userControll.view.Blocks[0].Elements.Clear();

                        // Add nedded elemnets to the control
                        userControll.view.Blocks[0].Elements.Add(reminderTypeControl);
                        userControll.view.Blocks[0].Elements.AddRange(timepickerControl);
                        userControll.view.Blocks[0].Elements.AddRange(controlButtons);

                        break;
                    case createNewReminderControlType.weekly:

                        SlackController.addOrEditViewConfiguration.showWeekdaysStaticSelect = true;

                        userControll = await CreateOrUpdateReminderControl(controlConfigurator, userProfile);


                        reminderTypeControl = userControll.view.Blocks[0].Elements.OfType<StaticSelect>().ElementAt(0);
                        reminderDayControl = userControll.view.Blocks[0].Elements.OfType<StaticSelect>().ElementAt(1);
                        timepickerControl = userControll.view.Blocks[0].Elements.OfType<Timepicker>().ToList();
                        controlButtons = userControll.view.Blocks[0].Elements.OfType<Button>().ToList();

                        userControll.view.Blocks[0].Elements.Clear();


                        userControll.view.Blocks[0].Elements.Add(reminderTypeControl);
                        userControll.view.Blocks[0].Elements.Add(reminderDayControl);
                        userControll.view.Blocks[0].Elements.AddRange(timepickerControl);
                        userControll.view.Blocks[0].Elements.AddRange(controlButtons);


                        break;
                    case createNewReminderControlType.monthly:

                        SlackController.addOrEditViewConfiguration.showDatepicker = true;

                        userControll = await CreateOrUpdateReminderControl(controlConfigurator, userProfile);

                        reminderTypeControl = userControll.view.Blocks[0].Elements.OfType<StaticSelect>().FirstOrDefault();
                        datePickerControl = userControll.view.Blocks[0].Elements.OfType<DatePicker>().FirstOrDefault();
                        timepickerControl = userControll.view.Blocks[0].Elements.OfType<Timepicker>().ToList();
                        controlButtons = userControll.view.Blocks[0].Elements.OfType<Button>().ToList();


                        userControll.view.Blocks[0].Elements.Clear();


                        userControll.view.Blocks[0].Elements.Add(reminderTypeControl);
                        userControll.view.Blocks[0].Elements.Add(datePickerControl);
                        userControll.view.Blocks[0].Elements.AddRange(timepickerControl);
                        userControll.view.Blocks[0].Elements.AddRange(controlButtons);

                        break;
                    default:
                        break;
                }


                return userControll;

            });

            return renderControl;
        }



        public static Task<TimeUnitCRUDForm> CreateETS_CRUD_Form(EtsCrudFormConfigurator controlType, CollectedUserEtsData collectedUserEtsData, string projectId = "", string timeUntDate = "", string timeUnitId = "")
        {
            Task<TimeUnitCRUDForm> createUserControl = Task.Run(() =>
            {


                //Initialize the form
                var CRUD_Form = new TimeUnitCRUDForm("theEtsCrudForm", "Manage ETS data");




                //Control type selector user control
                var actionBlock = new TimeUnitCRUD_ActionSelector();

                var divider = new TimeUnitCRUD_Divider();



                //Create date selector user control
                var DateSelection = new TimeUnitCRUD_StaticSelect(true, "etsCrudFormDateSelector", "Select a date", "Select time entry date", "etsCrudFormDateSelected");


                //Add options for the date static select control

                var missingTime = from t in collectedUserEtsData.MissingTime
                                  select new { Date = t.Date, Time = (double.Parse(t.SumOfTime) / 60.00).ToString() };


                if (missingTime.Any())
                {


                    foreach (var t in missingTime.Distinct())
                    {
                        string value = t.Date + " " + t.Time + "h Entered out of 8.00h";
                        DateSelection.element.options.Add(new FormElementOption(value, value));
                    }

                }
                else
                {
                    DateSelection.element.options.Add(new FormElementOption("There are no missig dates", "Selected option value"));


                }







                //Add initialized controls to the form
                CRUD_Form.view.blocks.Add(actionBlock);
                CRUD_Form.view.blocks.Add(divider);
                CRUD_Form.view.blocks.Add(DateSelection);


                //Static select for the project time
                var TimeStaticSelect = new TimeUnitCRUD_StaticSelect(false, "etsCrudFormTimeSelect", "Select a time", "Time", "etsCrudFormTimeSelected");
                List<string> timeUnitTimeValues = new List<string>();
                timeUnitTimeValues.Add("0.25");
                timeUnitTimeValues.Add("0.50");
                timeUnitTimeValues.Add("0.75");
                timeUnitTimeValues.Add("1.00");
                timeUnitTimeValues.Add("1.25");
                timeUnitTimeValues.Add("1.50");
                timeUnitTimeValues.Add("1.75");
                timeUnitTimeValues.Add("2.00");
                timeUnitTimeValues.Add("2.25");
                timeUnitTimeValues.Add("2.50");
                timeUnitTimeValues.Add("2.75");
                timeUnitTimeValues.Add("3.00");
                timeUnitTimeValues.Add("3.25");
                timeUnitTimeValues.Add("3.50");
                timeUnitTimeValues.Add("3.75");
                timeUnitTimeValues.Add("4.00");















                //Static select for the projects selection
                var ProjectsStaticSelect = new TimeUnitCRUD_StaticSelect(true, "etsCrudFormProjectSelect", "Select a project", "Project", "etsCrudFormProjectSelected");



                //Static selct for the selected project tasks
                var TasksStaticSelect = new TimeUnitCRUD_StaticSelect(false, "etsCrudFormTasksSelect", "Select a task", "Task", "etsCrudFormTaskSelected");



                //Crate multiline text input control
                var multilineInput = new TimeUnitCRUD_MultilineText(false, "etsCrudFormDescription", "Description", "Time entry description...", "etsCrudDescriptionText");



                //Static select for time units
                var TimeUnitStaticSelect = new TimeUnitCRUD_StaticSelect(true, "etsCrudFormTimeUnitSelect", "Select a time entry (time entry description)", "Select time entry to modify", "etsCrudFormTimeUnitSelected");



                //Select project tasks
                var tasks = collectedUserEtsData.ProjectsTasks.SelectMany(t => t.TaskTypes).ToList();


                switch (controlType.ControlType)
                {
                    case EtsCrudFormConfigurator.EtsCrudFormType.create:


                        #region create control type initialization
                        actionBlock.accessory.placeholder.text = "Create";

                        //Options for the projects static select
                        foreach (var p in collectedUserEtsData.ProjectsTasks)
                        {
                            ProjectsStaticSelect.element.options.Add(new FormElementOption(p.ProjectTitle, p.ProjectID));

                        }

                        CRUD_Form.view.blocks.Add(ProjectsStaticSelect);

                        //Options for the project tasks static select
                        if (string.IsNullOrEmpty(projectId))
                        {

                            TasksStaticSelect.element.options.Add(new FormElementOption("Please select a project", "Project task ID"));


                        }
                        else
                        {

                            if (collectedUserEtsData.ProjectsTasks.FirstOrDefault(p => p.ProjectID == projectId).TaskTypes.Any())
                            {
                                foreach (var task in tasks.Where(t => t.project_id == projectId))
                                {
                                    foreach (var t in task.task_types)
                                    {
                                        TasksStaticSelect.element.options.Add(new FormElementOption(t.title, t.id));

                                    }
                                }
                            }
                            else
                            {
                                TasksStaticSelect.element.options.Add(new FormElementOption("There are no tasks for the selected project", "Project task ID"));

                            }



                        }

                        CRUD_Form.view.blocks.Add(TasksStaticSelect);

                        //Add project time selector control to the form
                        if (string.IsNullOrEmpty(timeUntDate) || double.Parse(timeUntDate.Split(" ")[1].Replace("h", "")) <= 4)
                        {
                            foreach (var t in timeUnitTimeValues)
                            {
                                TimeStaticSelect.element.options.Add(new FormElementOption(t, t));
                            }
                        }
                        else
                        {
                            double enteredTime = double.Parse(timeUntDate.Split(" ")[1].Replace("h", ""));
                            double totalHours = double.Parse(timeUntDate.Split(" ")[5].Replace("h", ""));
                            List<double> timeConstraint = timeUnitTimeValues.Select(t => double.Parse(t)).Where(t => t <= (totalHours * 60 - enteredTime * 60) / 60).ToList();
                            foreach (var c in timeConstraint)
                            {
                                TimeStaticSelect.element.options.Add(new FormElementOption(c.ToString("0.00"), c.ToString("0.00")));
                            }

                        }

                        CRUD_Form.view.blocks.Add(TimeStaticSelect);
                        CRUD_Form.view.blocks.Add(multilineInput);
                        #endregion
                        break;


                    case EtsCrudFormConfigurator.EtsCrudFormType.update:
                        #region update control type initialization

                        var timeUnits = collectedUserEtsData.MissingTime.Where(t => t.TimeUnits != null).Select(t => t.TimeUnits).ToList();

                        TimeUnitStaticSelect.element.options.Clear();
                        //Add options for the time entry static selection
                        if (string.IsNullOrEmpty(timeUntDate))
                        {
                            TimeUnitStaticSelect.element.options.Add(new FormElementOption("Please select a date", "time entry ID"));

                        }
                        else
                        {


                            if (timeUnits.Where(t => t.date == timeUntDate.Split(" ")[0]).Any())
                            {
                                foreach (var u in timeUnits.Where(t => t.date == timeUntDate.Split(" ")[0]))
                                {
                                    TimeUnitStaticSelect.element.options.Add(new FormElementOption(u.description, u.id));
                                }
                            }
                            else
                            {

                                TimeUnitStaticSelect.element.options.Add(new FormElementOption("There are no time entries for the selected date", "time entry ID"));

                            }

                        }



                        CRUD_Form.view.blocks.Add(TimeUnitStaticSelect);


                        //Options for the projects static select
                        ProjectsStaticSelect.element.options.Clear();
                        foreach (var p in collectedUserEtsData.ProjectsTasks)
                        {
                            ProjectsStaticSelect.element.options.Add(new FormElementOption(p.ProjectTitle, p.ProjectID));

                        }


                        CRUD_Form.view.blocks.Add(ProjectsStaticSelect);



                        //Options for the project tasks static select
                        TasksStaticSelect.element.options.Clear();

                        if (string.IsNullOrEmpty(projectId))
                        {

                            TasksStaticSelect.element.options.Add(new FormElementOption("Please select a project", "Project task ID"));


                        }
                        else
                        {

                            if (collectedUserEtsData.ProjectsTasks.FirstOrDefault(p => p.ProjectID == projectId).TaskTypes.Any())
                            {
                                foreach (var task in tasks.Where(t => t.project_id == projectId))
                                {
                                    foreach (var t in task.task_types)
                                    {
                                        TasksStaticSelect.element.options.Add(new FormElementOption(t.title, t.id));

                                    }
                                }
                            }
                            else
                            {
                                TasksStaticSelect.element.options.Add(new FormElementOption("There are no tasks for the selected project", "Project task ID"));

                            }



                        }


                        CRUD_Form.view.blocks.Add(TasksStaticSelect);



                        //Add project time selector control to the form
                        if (!string.IsNullOrEmpty(timeUnitId))
                        {
                            int timeUnitsTotalTime = timeUnits.Where(t => t.date == timeUntDate.Split(" ")[0]).Sum(t => t.minutes);

                            int timeWithoutModifiedTimeUnit = timeUnitsTotalTime - timeUnits.First(t => t.id == timeUnitId).minutes;

                            double totalHours = double.Parse(timeUntDate.Split(" ")[5].Replace("h", ""));

                            if ((timeWithoutModifiedTimeUnit / 60) <= totalHours / 2)
                            {
                                foreach (var t in timeUnitTimeValues)
                                {
                                    TimeStaticSelect.element.options.Add(new FormElementOption(t, t));
                                }
                            }
                            else
                            {

                                List<double> timeConstraint = timeUnitTimeValues.Select(t => double.Parse(t)).Where(t => t <= (totalHours * 60 - timeWithoutModifiedTimeUnit) / 60).ToList();
                                foreach (var c in timeConstraint)
                                {
                                    TimeStaticSelect.element.options.Add(new FormElementOption(c.ToString("0.00"), c.ToString("0.00")));
                                }

                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(timeUntDate) || double.Parse(timeUntDate.Split(" ")[1].Replace("h", "")) <= 4)
                            {
                                foreach (var t in timeUnitTimeValues)
                                {
                                    TimeStaticSelect.element.options.Add(new FormElementOption(t, t));
                                }
                            }
                            else
                            {
                                double enteredTime = double.Parse(timeUntDate.Split(" ")[1].Replace("h", ""));
                                double totalHours = double.Parse(timeUntDate.Split(" ")[5].Replace("h", ""));
                                List<double> timeConstraint = timeUnitTimeValues.Select(t => double.Parse(t)).Where(t => t <= (totalHours * 60 - enteredTime * 60) / 60).ToList();
                                foreach (var c in timeConstraint)
                                {
                                    TimeStaticSelect.element.options.Add(new FormElementOption(c.ToString("0.00"), c.ToString("0.00")));
                                }

                            }

                        }



                        CRUD_Form.view.blocks.Add(TimeStaticSelect);






                        //Add multiline text input control
                        multilineInput.element.placeholder.text = "Modify the time entry description";


                        CRUD_Form.view.blocks.Add(multilineInput);
                        #endregion
                        break;










                    case EtsCrudFormConfigurator.EtsCrudFormType.delete:


                        //Add options for the time entry static selection
                        TimeUnitStaticSelect.element.options.Clear();


                        var timeUnitsForDeleteing = collectedUserEtsData.MissingTime.Where(t => t.TimeUnits != null).Select(t => t.TimeUnits).ToList();

                        TimeUnitStaticSelect.element.options.Clear();
                        //Add options for the time entry static selection
                        TimeUnitStaticSelect.element.placeholder.text = "Select time entry to delete";

                        if (string.IsNullOrEmpty(timeUntDate))
                        {
                            TimeUnitStaticSelect.element.options.Add(new FormElementOption("Please select a date", "time entry ID"));

                        }
                        else
                        {
                            if (timeUnitsForDeleteing.Where(t => t.date == timeUntDate.Split(" ")[0]).Any())
                            {
                                foreach (var u in timeUnitsForDeleteing.Where(t => t.date == timeUntDate.Split(" ")[0]))
                                {
                                    TimeUnitStaticSelect.element.options.Add(new FormElementOption(u.description, u.id));
                                }
                            }
                            else
                            {

                                TimeUnitStaticSelect.element.options.Add(new FormElementOption("There are no time entries for the selected date", "time entry ID"));

                            }

                        }



                        CRUD_Form.view.blocks.Add(TimeUnitStaticSelect);



                        break;
                    default:
                        break;
                }
                return CRUD_Form;

            });


            return createUserControl;
        }



        public static Task<AddUserControl> CreateAddUserControl(string formTitle = "Add new user", string userPlaceHolderText = "Enter ets user name", string pswPlaceholder = "Enter ets password")
        {
            Task<AddUserControl> createAddUserControll = Task.Run(() =>
            {

                var userNamePlaceHolder = new AddUserPlaceholder("plain_text", userPlaceHolderText);

                AddUserElement addUserElement = new AddUserElement(userNamePlaceHolder);
                addUserElement.type = "plain_text_input";
                addUserElement.action_id = "enteredUserName";



                AddUserLabel addUserLabel = new AddUserLabel();
                addUserLabel.type = "plain_text";
                addUserLabel.text = "Ets user name";
                addUserLabel.emoji = true;

                //block one
                AddUserInputBlock inputOne = new AddUserInputBlock(addUserElement, addUserLabel);
                inputOne.type = "input";
                inputOne.block_id = "userNameInput";





                //block two

                var userPswPlaceHolder = new AddUserPlaceholder("plain_text", pswPlaceholder);

                AddUserElement addUserElementSecond = new AddUserElement(userPswPlaceHolder);
                addUserElementSecond.type = "plain_text_input";
                addUserElementSecond.action_id = "enteredEtsPassword";



                AddUserLabel addUserLabelSecond = new AddUserLabel();
                addUserLabelSecond.type = "plain_text";
                addUserLabelSecond.text = "Ets password";
                addUserLabelSecond.emoji = true;


                AddUserInputBlock inputTwo = new AddUserInputBlock(addUserElementSecond, addUserLabelSecond);
                inputTwo.type = "input";
                inputTwo.block_id = "userPswInput";




                List<AddUserBlock> addUserBlocks = new List<AddUserBlock>();
                addUserBlocks.Add(inputOne);
                addUserBlocks.Add(inputTwo);


                if (formTitle != "Subscribe")
                {
                    AddUserActionBlock unsubscribeButton = new AddUserActionBlock();
                    addUserBlocks.Add(unsubscribeButton);
                }
             



                AddUserTitle userTitle = new AddUserTitle();
                userTitle.type = "plain_text";
                userTitle.text = formTitle;

                AddUserSubmit userSubmit = new AddUserSubmit();
                userSubmit.type = "plain_text";
                userSubmit.text = "Submit";


                var controllClose = new AddUserClose("plain_text", "Cancel");


                AddUser addUser = new AddUser(userTitle, userSubmit, controllClose, addUserBlocks);
                addUser.type = "modal";
                addUser.callback_id = "addNewUser";

                AddUserControl rootAddUserModal = new AddUserControl(addUser);


                return rootAddUserModal;

            });



            return createAddUserControll;


        }

        public static Task<RemindersSelectionUserControl> CreateReminderSelectionControl(List<RemindersSelection> reminders, string formTitle = "Title placeholder", string labelText = "Label text placeholder", string callbackId = "Callback placeholder")
        {
            Task<RemindersSelectionUserControl> createAddUserControll = Task.Run(() =>
            {


                var mngReminderTitle = new RemindersSelectionViewTitle();
                mngReminderTitle.Type = "plain_text";
                mngReminderTitle.Text = formTitle;

                var mngReminderSubmit = new RemindersSelectionViewSubmit();
                mngReminderSubmit.Type = "plain_text";
                mngReminderSubmit.Text = "Submit";


                var mngReminderClose = new RemindersSelectionViewClose();
                mngReminderClose.Type = "plain_text";
                mngReminderClose.Text = "Cancel";






                var radioBtnOptions = new List<RadioBtnElem>();


                var blockList = new List<RemindersSelectionBlockBase>();


                RemindersSelectionView rootBlock = null;
                // Add reminder ID as value
                if (reminders.Count != 0)
                {
                    foreach (var r in reminders)
                    {
                        string weeklyReminder = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(r.Date).ToUniversalTime(), RemindersMainTimer.ManTimerTimeZone)
                        .ToString("dddd");

                        string label = r.ReminderType == "daily" ? r.ReminderType : r.ReminderType == "weekly" ? r.ReminderType + $" - {weeklyReminder}" : r.ReminderType == "monthly" ? r.ReminderType + $" - {r.Date}" : r.ReminderType;
                        radioBtnOptions.Add(new RadioBtnElem(new RadioBtnText(text: $"{label}"), $"{r.ReminderID}"));
                    }
                    var radioBtnElement = new RadioButtonElement(radioBtnOptions);
                    radioBtnElement.Type = "radio_buttons";

                    var radioBtnBlock = new RemindersSelectionBlock(radioBtnElement, new DeleteReminderViewLabel(text: labelText));
                    radioBtnBlock.Block_id = "radioButtonOption";
                    blockList.Add(radioBtnBlock);
                    rootBlock = new RemindersSelectionView_Submit(blockList, mngReminderTitle, mngReminderClose, mngReminderSubmit);
                }
                else
                {

                    var mngReminderOk = new RemindersSelectionViewClose();
                    mngReminderOk.Type = "plain_text";
                    mngReminderOk.Text = "Ok";
                    var headerBlock = new RemindersSelectionHeaderBlock(new RadioBtnText(text: "There are no reminders"));
                    blockList.Add(headerBlock);
                    rootBlock = new RemindersSelectionView_Ok(blockList, mngReminderTitle, mngReminderOk);
                }






                rootBlock.Callback_id = callbackId;



                var mainView = new RemindersSelectionUserControl(rootBlock);





                return mainView;

            });



            return createAddUserControll;


        }









    }
}
