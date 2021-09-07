using EtsWebClient.Http;
using EtsWebClient;
using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Enums;
using SlackBotMessages.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtsClientCore;
using EtsClientData;
using EtsWebClient.SlackModels;
using EtsWebClient.Test;
using EtsWebClient.EtsHttpModels;
using Microsoft.Extensions.Caching.Memory;

namespace EtsWebClient.Http
{
    public class EtsHttpClient
    {
        //static string lokalJsonFile = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\EtsWebClient\Test\WritingTest.json";

        //static string parsingJsonLocaly = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\EtsWebClient\Test\ParsingTest.json";
        //string jsonFile = File.ReadAllText(parsingJsonLocaly);


        private static EtsDataContext _context = new EtsDataContext();
        private static EtsClientServices _clientServices = new EtsClientServices(_context);
        private static IUserServices userServices = _clientServices;

        private EtsUser _user;
        public EtsHttpClient(EtsUser user)
        {
            this._user = user;



        }

        private static DateTime date = DateTime.Now;

        // DateFrom - First day in month by default
        internal static string dateFrom = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");

        //DateTo - Actual day in month by default
        internal static string dateTo = date.ToString("yyyy-MM-dd");


        internal static UserEtsParametars UserEtsParametars { get; set; }

        public static CollectedUserEtsData CollectedUserEtsData { get; set; }



        private static TimeSpan casheExpirationTime = System.TimeSpan.FromMinutes(60);
        private static DateTime casheExpirationArg = TimeZoneInfo.ConvertTime(DateTime.Now.ToUniversalTime(), MainTimer.StatusChecker.TimeZone).Add(casheExpirationTime);

        public static IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());



        private static volatile bool OnVacation = false;
        private static List<EtsTimeUnitData> TimeUnits { get; set; }
        private static List<EmployeeScheduledTime> ScheduledTime { get; set; }
        private static EmployeeVacations EmployeeVacations { get; set; }
        private static List<Project> Projects { get; set; }
        private static List<ProjectTasks> ProjectTasks { get; set; }
        private static Notification Notification { get; set; }













        public async Task<bool> collectEtsUserData(bool checkVacation = true)
        {






            if (await Autentificate(EtsAPI.authentication, _user))
            {

                if (checkVacation)
                {
                    await collectData(EtsAPI.employeeVacation, new EmployeeVacations(), false);

                    if (!OnVacation)
                    {
                        await collectData(EtsAPI.employeeScheduledTime, new List<EmployeeScheduledTime>(), false);
                        await collectData(EtsAPI.employeeTimeUnits, new List<EtsTimeUnitData>(), false);
                        await collectData(EtsAPI.emplyeeProjects, new List<Project>(), false);
                        await collectData(EtsAPI.getTaskTypes, new List<ProjectTasks>(), true);
                        await CreateNotification(dateFrom, dateTo, EtsUser.userData.username);


                        Debug.WriteLine($"**Data collected** for the user: {EtsUser.userData.username.TrimStart('U', 'A', '\\')}");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"The user '{EtsUser.userData.username.TrimStart('U', 'A', '\\')}' is on vacation. Reminder sending is canceled.");
                        return false;
                    }

                }
                else
                {

                    await collectData(EtsAPI.employeeScheduledTime, new List<EmployeeScheduledTime>(), false);
                    await collectData(EtsAPI.employeeTimeUnits, new List<EtsTimeUnitData>(), false);
                    await collectData(EtsAPI.emplyeeProjects, new List<Project>(), false);
                    await collectData(EtsAPI.getTaskTypes, new List<ProjectTasks>(), true);
                    await CreateNotification(dateFrom, dateTo, EtsUser.userData.username);

                    Debug.WriteLine($"**Data collected** for the user: {EtsUser.userData.username.TrimStart('U', 'A', '\\')}");

                    return true;

                }


            }
            else
            {

                Debug.WriteLine($"Unable to authenticate the user {EtsUser.userData.username.TrimStart('U', 'A', '\\')}. The 'Ets' credentials changed.");


                var validationMessage = SlackMessageBlock.createMessage("Failed to log in", "The 'Ets' credentials were changed, unable to login", false, EtsUser.userData.slcakChannelID);


                var slckBotClient = new SlackBotClient(userServices.SlackBotToken());
                await slckBotClient.WriteMessage(validationMessage);
                return false;
            }




        }





        public static async Task<bool> CreateOrUpdateTimeUnit(EtsUser user, object timeUnit, string timeUnitId = "")
        {
            bool result = true;
            if (Autentificate(EtsAPI.authentication, user).Result)
            {


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserEtsParametars.access_token);


                    if (string.IsNullOrEmpty(timeUnitId))
                    {
                        var postBody = new StringContent(JsonConvert.SerializeObject(timeUnit), Encoding.UTF8, "application/json");
                        var postResponse = await httpClient.PostAsync(EtsAPI.postNewTimeUnit, postBody);

                        if (postResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var body = await postResponse.Content.ReadAsStringAsync();
                            result = false;
                            throw new Exception(body);

                        }
                        else if (postResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var body = await postResponse.Content.ReadAsStringAsync();
                            result = false;
                            throw new Exception(body);


                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        Uri putUri = new Uri(EtsAPI.postNewTimeUnit, timeUnitId);
                        var putBody = new StringContent(JsonConvert.SerializeObject(timeUnit), Encoding.UTF8, "application/json");
                        var putResponse = await httpClient.PutAsync(putUri, putBody);

                        if (putResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var body = await putResponse.Content.ReadAsStringAsync();
                            result = false;
                            throw new Exception(body);


                        }
                        if (putResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            var body = await putResponse.Content.ReadAsStringAsync();
                            result = false;
                            throw new Exception(body);


                        }
                        else
                        {
                            result = true;
                        }

                    }

                    //Update cashed data
                    await updateUserEtsDataCash(user);
                }


            }
            else
            {
                Debug.WriteLine($"Unable to manage the 'ETS' time unit {EtsUser.userData.username.TrimStart('U', 'A', '\\')}. The user autentification failed");
                result = false;

            }

            return result;
        }




        public static async Task DeleteTimeUnit(EtsUser user, string timeUnitId)
        {

            if (Autentificate(EtsAPI.authentication, user).Result)
            {


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserEtsParametars.access_token);




                    var postResponse = await httpClient.DeleteAsync(EtsAPI.postNewTimeUnit + timeUnitId);

                    if (postResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var body = await postResponse.Content.ReadAsStringAsync();

                        throw new Exception(body);

                    }
                    else if (postResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        var body = await postResponse.Content.ReadAsStringAsync();

                        throw new Exception(body);


                    }
                    else if (postResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var body = await postResponse.Content.ReadAsStringAsync();

                        throw new Exception(body);


                    }
                    else
                    {
                        //Update cashed data
                        await updateUserEtsDataCash(user);

                    }

                }


            }
            else
            {
                Debug.WriteLine($"Unable to manage the 'ETS' time unit {EtsUser.userData.username.TrimStart('U', 'A', '\\')}. The user autentification failed");


            }

        }





        private static async Task<bool> Autentificate(Uri uri, EtsUser user)
        {

            using (var httpClient = new HttpClient())
            {

                var serializedRequest = JsonConvert.SerializeObject(user);
                var stringContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(uri, stringContent);
                var body = await response.Content.ReadAsStringAsync();
                try
                {
                    if (response.IsSuccessStatusCode == false)
                    {

                        throw new Exception(body);

                    }
                    else
                    {
                        UserEtsParametars = JsonConvert.DeserializeObject<UserEtsParametars>(body);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;

                }



            }
        }

        public static async Task<bool> CheckUserCredentials(EtsClientData.EtsDataContext.User user)
        {
            using (var httpClient = new HttpClient())
            {
                Uri uri = EtsAPI.authentication;
                var checkCredentials = new EtsUser(user.EtsUserName, user.EtsPassword, user.SlackChannelID);
                var serializedRequest = JsonConvert.SerializeObject(checkCredentials);
                var stringContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(uri, stringContent);
                var body = await response.Content.ReadAsStringAsync();
                try
                {
                    if (response.IsSuccessStatusCode == false)
                    {


                        return false;

                        throw new Exception(body);
                    }
                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex);
                }
                return true;


            }

        }



        private async Task CreateNotification(string dateFrom, string dateTo, string userName)
        {
            if (!OnVacation)
            {


                try
                {

                    List<TimeEntry> timeEntries = new List<TimeEntry>();
                    var sumOfMinuteByDate = from t in TimeUnits
                                            group t by new { t.date }
                                              into g
                                            select new
                                            {
                                                Date = g.Key.date,
                                                Time = g.Sum(m => m.minutes)
                                            };
                    await Task.FromResult(sumOfMinuteByDate);


                    var join = from date in datesGenerator(dateFrom, dateTo)
                               join time in sumOfMinuteByDate on date equals time.Date into g
                               from sub in g.DefaultIfEmpty()
                               select new { date, sumOfTimeUnits = sub?.Time };
                    await Task.FromResult(join);

                    foreach (var item in join)
                    {
                        string time = item.sumOfTimeUnits.ToString();

                        if (string.IsNullOrEmpty(time))
                        {
                            time = "0";
                        }
                        timeEntries.Add(new TimeEntry { date = item.date, sumOfTimeUnits = time });


                    }

                    var st = new EmployeeScheduledTime();
                    st = ScheduledTime.FirstOrDefault();
                    Notification = new Notification(timeEntries, TimeUnits, Projects, ProjectTasks);
                    Notification.scheduledTime = st.scheduled_hours;
                    Notification.employeeUserName = userName;






                }
                catch (Exception ex)
                {

                    Debug.WriteLine(ex.Message);
                }
            }



        }
        public async void SendNotification(string channelID)
        {
            if (!OnVacation)
            {


                try
                {
                    //Shape the JSON notiofication
                    List<Project_Tasks> projectTasks = (from p in Notification.projects
                                                        join t in Notification.projectTasks on p.id equals t.project_id into g
                                                        select new Project_Tasks { ProjectTitle = p.title, ProjectID = p.id, TaskTypes = g.ToList() }).ToList();


                    List<Missingtime> MissingTime = (from t in Notification.dateTime
                                                     join n in Notification.timeUnits on t.date equals n.date into j
                                                     from sub in j.DefaultIfEmpty()
                                                     where int.Parse(t.sumOfTimeUnits) != 480
                                                     select new Missingtime { Date = t.date, SumOfTime = t.sumOfTimeUnits, TimeUnits = sub }).ToList();



                    CollectedUserEtsData = new CollectedUserEtsData(MissingTime, projectTasks);
                    CollectedUserEtsData.EmployeeDetails = UserEtsParametars.employee_details;



                    //Store the data in cash
                    cache.Set(channelID, CollectedUserEtsData, casheExpirationArg);









                    // Create slack message

                    string msg = $"*There is missing \"ETS\" data for {CollectedUserEtsData.MissingTime.Select(m => m.Date).Distinct().Count()} days.*";

                    var notBlockList = new List<NotificationBlock>();
                    HeaderBlock notificationText = new HeaderBlock(type: "header", blockTextType: "plain_text", blockText: "The \"Ets\" reminder");
                    AccessoryBlock accessoryBlock = new AccessoryBlock(new Accessory(value: "btn click value", actionId: "EtsDataBtnClick", btnLabel: "ETS"), text:$"{msg}\n Click on the button to enter missing data.");


                    notBlockList.Add(notificationText);
                    notBlockList.Add(accessoryBlock);

                    var slckBotNot = new BotNotificationAttachment(notBlockList);

                    var slackBotNotification = new SlackBotNotification(slckBotNot,msg, "#290187");
                    slackBotNotification.channel = channelID;



                    var slckBotClient = new SlackBotClient(userServices.SlackBotToken());
                    await slckBotClient.WriteMessage(slackBotNotification);

                    MainTimer.RemindersMainTimer.notificationSentCount++;
                    Debug.WriteLine($"*************Notification sent****************** Count:{MainTimer.RemindersMainTimer.notificationSentCount}");









                    //In this case serialize JSON directly to a file
                    //using (var file = new StreamWriter(lokalJsonFile))
                    //{
                    //    string msgFormatCheck = JsonConvert.SerializeObject(CollectedUserEtsData, Formatting.Indented);

                    //    await file.WriteLineAsync(msgFormatCheck);


                    //    Debug.WriteLine("*************Notification json saved******************");
                    //    payload.Clear();

                    //}




                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
        }





        public Task<CollectedUserEtsData> CreateViewModel()
        {




            try
            {




                Task<CollectedUserEtsData> collectedUserEtsData = Task.Run(async () =>
                {



                    CollectedUserEtsData cashedData = null;

                    if (cache.TryGetValue(_user.slcakChannelID, out cashedData))
                    {
                        Debug.Write("***Data loaded from the cash\n");
                        return cashedData;

                    }
                    else
                    {


                        var userData = await userServices.UserDetails(_user.slcakChannelID);

                        var etsUser = new EtsWebClient.EtsHttpModels.EtsUser(userData.EtsUserName, userData.EtsPassword, userData.SlackChannelID);

                        var EtsClient = new EtsHttpClient(etsUser);


                       // Collect the ets data for the user

                        if (await EtsClient.collectEtsUserData(checkVacation: false))
                            {
                                //Shape the time unit form object
                                List<Project_Tasks> projectTasks = (from p in Notification.projects
                                                                    join t in Notification.projectTasks on p.id equals t.project_id into g
                                                                    select new Project_Tasks { ProjectTitle = p.title, ProjectID = p.id, TaskTypes = g.ToList() }).ToList();


                                List<Missingtime> MissingTime = (from t in Notification.dateTime
                                                                 join n in Notification.timeUnits on t.date equals n.date into j
                                                                 from sub in j.DefaultIfEmpty()
                                                                 where int.Parse(t.sumOfTimeUnits) != 480
                                                                 select new Missingtime { Date = t.date, SumOfTime = t.sumOfTimeUnits, TimeUnits = sub }).ToList();



                                cashedData = new CollectedUserEtsData(MissingTime, projectTasks);
                                cashedData.EmployeeDetails = UserEtsParametars.employee_details;

                                cache.Set(_user.slcakChannelID, cashedData, casheExpirationArg);
                            Debug.Write("***Data loaded from the \"ETS\"\n");

                            return cashedData;
                            }

                        return null;



                    }



                });
                return collectedUserEtsData;

            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
                return null;

            }


        }


        private static async Task<bool> updateUserEtsDataCash(EtsUser User)
        {


            try
            {



                CollectedUserEtsData cashedData = null;

                Task<bool> collectUserEtsData = Task.Run(async () =>
                {


                    var EtsClient = new EtsHttpClient(User);


                    //Collect the ets data for the user

                    await EtsClient.collectEtsUserData();








                    //Shape the time unit form object
                    List<Project_Tasks> projectTasks = (from p in Notification.projects
                                                        join t in Notification.projectTasks on p.id equals t.project_id into g
                                                        select new Project_Tasks { ProjectTitle = p.title, ProjectID = p.id, TaskTypes = g.ToList() }).ToList();


                    List<Missingtime> MissingTime = (from t in Notification.dateTime
                                                     join n in Notification.timeUnits on t.date equals n.date into j
                                                     from sub in j.DefaultIfEmpty()
                                                     where int.Parse(t.sumOfTimeUnits) != 480
                                                     select new Missingtime { Date = t.date, SumOfTime = t.sumOfTimeUnits, TimeUnits = sub }).ToList();



                    cashedData = new CollectedUserEtsData(MissingTime, projectTasks);
                    cashedData.EmployeeDetails = UserEtsParametars.employee_details;

                    cache.Set(User.slcakChannelID, cashedData, casheExpirationArg);

                    return true;

                });

                return await collectUserEtsData;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return false;
            }




        }




        private async Task collectData(Uri uri, object obj, bool httpPost)
        {

            try
            {


                HttpResponseMessage result = await GetAsync(uri, httpPost);



                if (obj.GetType() == new List<EmployeeScheduledTime>().GetType())
                {
                    ScheduledTime = JsonConvert.DeserializeObject<List<EmployeeScheduledTime>>(result.Content.ReadAsStringAsync().Result);
                }
                else if (obj.GetType() == new List<EtsTimeUnitData>().GetType())
                {
                    TimeUnits = JsonConvert.DeserializeObject<List<EtsTimeUnitData>>(result.Content.ReadAsStringAsync().Result);

                }
                else if (obj.GetType() == new EmployeeVacations().GetType())
                {
                     EmployeeVacations = JsonConvert.DeserializeObject<EmployeeVacations>(result.Content.ReadAsStringAsync().Result);


                   // EmployeeVacations = JsonConvert.DeserializeObject<EmployeeVacations>(jsonFile);


                    if (EmployeeVacations.vacations != null)
                    {
                        var start = EmployeeVacations.vacations.Select(d => DateTime.Parse(d.StartDate)).Max();
                        var end = EmployeeVacations.vacations.Select(d => DateTime.Parse(d.EndDate)).Max();
                        if (DateTime.Now.Date >= start && DateTime.Now.Date <= end)
                        {
                            OnVacation = true;
                        }
                    }




                }
                else if (obj.GetType() == new List<Project>().GetType())
                {
                    Projects = JsonConvert.DeserializeObject<List<Project>>(result.Content.ReadAsStringAsync().Result);
                }
                else if (obj.GetType() == new List<ProjectTasks>().GetType())
                {
                    ProjectTasks = JsonConvert.DeserializeObject<List<ProjectTasks>>(result.Content.ReadAsStringAsync().Result);
                }

            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }




        }


        private async Task<HttpResponseMessage> GetAsync(Uri uri, bool httpPost)
        {

            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserEtsParametars.access_token);
                if (!httpPost)
                {
                    var response = await client.GetAsync(uri);

                    try
                    {

                        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var body = await response.Content.ReadAsStringAsync();
                            throw new Exception(body);



                        }

                    }
                    catch (Exception ex)
                    {

                        Debug.WriteLine(ex);
                    }

                    return response;

                }
                else
                {
                    var requestPayload = from p in Projects
                                         let ids = Projects.Select(i => i.id)
                                         select new { project_ids = ids };

                    var postBody = new StringContent(JsonConvert.SerializeObject(requestPayload.FirstOrDefault()), Encoding.UTF8, "application/json");
                    var postResponse = await client.PostAsync(uri, postBody);


                    try
                    {
                        if (postResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            var body = await postResponse.Content.ReadAsStringAsync();
                            throw new Exception(body);



                        }


                    }
                    catch (Exception ex)
                    {

                        Debug.WriteLine(ex);
                    }

                    return postResponse;


                }



            }


        }


        private List<string> datesGenerator(string dateFrom, string dateTo)
        {
            DateTime fromDate = Convert.ToDateTime(dateFrom);
            DateTime toDate = Convert.ToDateTime(dateTo);
            List<string> result = new List<string>();
            result.Clear();
            for (DateTime index = fromDate; index <= toDate; index = index.AddDays(1))
            {
                if (index.DayOfWeek == DayOfWeek.Saturday || index.DayOfWeek == DayOfWeek.Sunday) continue;

                result.Add(index.Date.ToString("yyyy-MM-dd"));

            }


            return result;
        }

    }
}
