using System;
using System.Collections.Generic;
using System.Text;
using System.Web;


namespace EtsWebClient.Http
{
    internal static class HttpExtensions
    {
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }





    }

    public class EtsAPI
    {



        //The production ETS Domian
       // private static Uri root = new Uri("https://ets.akvelon.com/api/");

        //The test ETS Domain
       private static Uri root = new Uri("https://ets-dev.inyar.ru/api/");


        public static Uri authentication = new Uri(root, "auth/sign-in");

        public static Uri postNewTimeUnit = new Uri(root, "time-units/");

        public static Uri employeeScheduledTime
        {
            get
            {
                return new Uri(root, "employees/schedule")
              .AddQuery("start_date", EtsHttpClient.dateFrom)
              .AddQuery("end_date", EtsHttpClient.dateTo)
              .AddQuery("employee_ids", EtsHttpClient.UserEtsParametars.employee_details.Id);

            }
        }

        public static Uri emplyeeProjects
        {
            get
            {
                return new Uri(root, "projects/")
               .AddQuery("employee_id", EtsHttpClient.UserEtsParametars.employee_details.Id);
            }
        }



        public static Uri employeeTimeUnits
        {
            get
            {
                return new Uri(root, "time-units/")
              .AddQuery("start_date", EtsHttpClient.dateFrom)
              .AddQuery("end_date", EtsHttpClient.dateTo)
              .AddQuery("employee_id", EtsHttpClient.UserEtsParametars.employee_details.Id);
            }
        }



        public static Uri employeeVacation
        {
            get
            {
                return new Uri(root, "employees/" + EtsHttpClient.UserEtsParametars.employee_details.Id + "/vacations/")
               .AddQuery("start_date", EtsHttpClient.dateFrom)
               .AddQuery("end_date", EtsHttpClient.dateTo);
            }
        }


        public static Uri getTaskTypes
        {
            get
            {
                return new Uri(root, "task-types/");
            }
        }




    }
}
