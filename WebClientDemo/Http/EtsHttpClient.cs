using Newtonsoft.Json;
using SlackBotMessages;
using SlackBotMessages.Enums;
using SlackBotMessages.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebClientDemo.Models;

namespace WebClientDemo.Http
{
    class EtsHttpClient
    {
        static string lokalJsonFile = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\WebClientDemo\Test\WritingTest.json";

        static string parsingJsonLocaly = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\WebClientDemo\Test\ParsingTest.json";
         string jsonFile = File.ReadAllText(parsingJsonLocaly);


        private User _user;
        public EtsHttpClient(User user)
        {
            this._user = user;
        }



        private static List<EtsTimeUnitData> timeUnits { get; set; }

        private static List<EmployeeScheduledTime> scheduledTime { get; set; }



        private static EmployeeVacations employeeVacations { get; set; }


        private static UserEtsParametars userEtsParametars { get; set; }

        private static List<Project> projects { get; set; }

        private static List<ProjectTasks> projectTasks { get; set; }

        private static Notification notification { get; set; }


        //Params
        DateTime date = DateTime.Now;

       public void collectEtsUserData()
        {
            // DateFrom - First day in month by default
            string dateFrom = new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd");

            //DateTo - Actual day in month by default
            string dateTo = date.ToString("yyyy-MM-dd");

            //The production ETS Domian
           Uri root = new Uri("https://ets.akvelon.com/api/");

             //The test ETS Domain
         // Uri root = new Uri("https://ets-dev.inyar.ru/api/");


            Uri authentication = new Uri(root, "auth/sign-in");
            Uri postNewTimeUnit = new Uri(root, "time-units/");
            Autentificate(authentication,_user).Wait();


            Uri employeeScheduledTime = new Uri(root, "employees/schedule")
           .AddQuery("start_date", dateFrom)
           .AddQuery("end_date", dateTo)
           .AddQuery("employee_ids", userEtsParametars.employee_details.id);

            Uri emplyeeProjects = new Uri(root, "projects/")
                .AddQuery("employee_id", userEtsParametars.employee_details.id);

            Uri employeeTimeUnits = new Uri(root, "time-units/")
                .AddQuery("start_date", dateFrom)
                .AddQuery("end_date", dateTo)
                .AddQuery("employee_id", userEtsParametars.employee_details.id);


            Uri employeeVacation = new Uri(root, "employees/" + userEtsParametars.employee_details.id + "/vacations/")
                .AddQuery("start_date", dateFrom)
                .AddQuery("end_date", dateTo);


            Uri getTaskTypes = new Uri(root, "task-types/");


          

            collectData(employeeScheduledTime, new List<EmployeeScheduledTime>(), false).Wait();
            collectData(employeeTimeUnits, new List<EtsTimeUnitData>(), false).Wait();
            collectData(employeeVacation, new EmployeeVacations(), false).Wait();
            collectData(emplyeeProjects, new List<Project>(), false).Wait();
            collectData(getTaskTypes, new List<ProjectTasks>(), true).Wait();
            CreateNotification(dateFrom, dateTo, User.userData.username).Wait();
            Console.WriteLine("**Data collected**");
           // ars.Set();

        }





        public async Task CreateOrUpdateTimeUnit(Uri uri, object toBeSerialized, string timeUnitId = "")
        {

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userEtsParametars.access_token);


                if (string.IsNullOrEmpty(timeUnitId))
                {
                    var postBody = new StringContent(JsonConvert.SerializeObject(toBeSerialized), Encoding.UTF8, "application/json");
                    var postResponse = await httpClient.PostAsync(uri, postBody);

                    if (postResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var body = await postResponse.Content.ReadAsStringAsync();
                        throw new Exception(body);

                    }
                }
                else
                {
                    Uri putUri = new Uri(uri, timeUnitId);
                    var putBody = new StringContent(JsonConvert.SerializeObject(toBeSerialized), Encoding.UTF8, "application/json");
                    var putResponse = await httpClient.PutAsync(putUri, putBody);

                    if (putResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var body = await putResponse.Content.ReadAsStringAsync();
                        throw new Exception(body);


                    }
                }







            }
        }

        private async Task Autentificate(Uri uri, User user)
        {

            using (var httpClient = new HttpClient())
            {

                var serializedRequest = JsonConvert.SerializeObject(user);
                var stringContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(uri, stringContent);

                if (response.IsSuccessStatusCode == false)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception(body);



                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();

                    userEtsParametars = JsonConvert.DeserializeObject<UserEtsParametars>(body);

                }

            }
        }





        private  async Task CreateNotification(string dateFrom, string dateTo, string userName)
        {
            try
            {

                List<TimeEntry> timeEntries = new List<TimeEntry>();
                var sumOfMinuteByDate = from t in timeUnits
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
                st = scheduledTime.FirstOrDefault();
                notification = new Notification(timeEntries, timeUnits, projects, projectTasks);
                notification.scheduledTime = st.scheduled_hours;
                notification.employeeUserName = userName;






            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }




        }
        public  async void SendNotification(string url)
        {
          

            try
            {
                //Shape the JSON notiofication
                var ProjectsTasks = from p in notification.projects
                                    join t in notification.projectTasks on p.id equals t.project_id into g
                                    select new { ProjectTitle = p.title, ProjectID = p.id, TaskTypes = g };
                await Task.FromResult(ProjectsTasks.ToList());

                var MissingTime = from t in notification.dateTime
                                  join n in notification.timeUnits on t.date equals n.date into j
                                  from sub in j.DefaultIfEmpty()
                                  where int.Parse(t.sumOfTimeUnits)  != 480
                                  select new { Date = t.date, SumOfTime = t.sumOfTimeUnits, TimeUnits = sub };
                await Task.FromResult(MissingTime.ToList());
                var payload = new SortedList<string, object>();

                payload.Add("ProjectsTasks", ProjectsTasks);
                payload.Add("MissingTime", MissingTime);



                //Send the notification

                SbmClient testSlackChanell = new SbmClient(url);
                string jsonNotification = JsonConvert.SerializeObject(payload, Formatting.Indented);


                var msg = new Message().SetUserWithIconUrl("Ets-Client " + DateTime.Now.ToString(), "https://codeshare.co.uk/media/1505/sbmlogo.jpg")
                    .AddAttachment(
                    new Attachment()
                    .SetColor(Color.Green)
                    .AddField("User data", jsonNotification, false)
                    );
                await testSlackChanell.Send(msg);
                Program.notificationSentCount++;
                Console.WriteLine($"*************Notification sent****************** Count:{Program.notificationSentCount}");






                // In this case serialize JSON directly to a file
                using (var file = new StreamWriter(lokalJsonFile))
                {


                    await file.WriteLineAsync(jsonNotification);


                    Console.WriteLine("*************Notification json saved******************");
                    payload.Clear();

                }


                //For the testing purposes read the local serialized file and send the notification
                //using (var file = new StreamReader(lokalJsonFile))
                //{

                //    SbmClient testSlackChanell = new SbmClient(url);


                //    var msg = new Message().SetUserWithIconUrl("Ets-Client " + DateTime.Now.ToString(), "https://codeshare.co.uk/media/1505/sbmlogo.jpg")
                //        .AddAttachment(
                //        new Attachment()
                //        .SetColor(Color.Green)
                //        .AddField("User data", file.ReadToEnd(), false)
                //        );
                //    await testSlackChanell.Send(msg);

                //    Console.WriteLine("*************Notification sent******************");

                //}



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        private  async Task collectData(Uri uri, object obj, bool httpPost)
        {

            try
            {


                HttpResponseMessage result = await GetAsync(uri, httpPost);



                if (obj.GetType() == new List<EmployeeScheduledTime>().GetType())
                {
                    scheduledTime = JsonConvert.DeserializeObject<List<EmployeeScheduledTime>>(result.Content.ReadAsStringAsync().Result);
                }
                else if (obj.GetType() == new List<EtsTimeUnitData>().GetType())
                {
                    timeUnits = JsonConvert.DeserializeObject<List<EtsTimeUnitData>>(result.Content.ReadAsStringAsync().Result);

                }
                else if (obj.GetType() == new EmployeeVacations().GetType())
                {
                    employeeVacations = JsonConvert.DeserializeObject<EmployeeVacations>(result.Content.ReadAsStringAsync().Result);

                    if (employeeVacations.vacations != null)
                    {
                        foreach (var item in employeeVacations.vacations)
                        {
                            //store the data into database
                            Console.WriteLine(item.StartDate, item.EndDate);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no vacations for this employee");
                    }

                }
                else if (obj.GetType() == new List<Project>().GetType())
                {
                    projects = JsonConvert.DeserializeObject<List<Project>>(result.Content.ReadAsStringAsync().Result);
                }
                else if (obj.GetType() == new List<ProjectTasks>().GetType())
                {
                    projectTasks = JsonConvert.DeserializeObject<List<ProjectTasks>>(result.Content.ReadAsStringAsync().Result);
                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }




        }


        private  async Task<HttpResponseMessage> GetAsync(Uri uri, bool httpPost)
        {

            using (HttpClient client = new HttpClient())
            {

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userEtsParametars.access_token);
                if (!httpPost)
                {
                    var response = await client.GetAsync(uri);

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        throw new Exception(body);



                    }
                    else
                    {
                        return response;

                    }

                  
                }
                else
                {
                    var requestPayload = from p in projects
                                         let ids = projects.Select(i => i.id)
                                         select new { project_ids = ids };

                    var postBody = new StringContent(JsonConvert.SerializeObject(requestPayload.FirstOrDefault()), Encoding.UTF8, "application/json");
                    var postResponse = await client.PostAsync(uri, postBody);

                    if (postResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var body = await postResponse.Content.ReadAsStringAsync();
                        throw new Exception(body);



                    }
                    else
                    {
                        return postResponse;

                    }
                }

            }





        }


        private  List<string> datesGenerator(string dateFrom, string dateTo)
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
