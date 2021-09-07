using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;

using System.Globalization;
using WebClientDemo.ReminderLogic;
using EtsClientCore;
using EtsClientData;
using Microsoft.Data.SqlClient;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;
using TableDependency.SqlClient.Base.Enums;
using EtsWebClient.MainTimer;

namespace WebClientDemo
{
 





  


    class Program
    {
        //For testing purposes
        //public static string smapleData = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\EtsWebClient\Test\sampleData.txt";
        //public static string smapleDataDir = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\EtsWebClient\Test\";








     



        static void Main(string[] args)
        {
           
            RemindersMainTimer mainTimer = new RemindersMainTimer();
            mainTimer.StartMainTimerAsync();


            Console.ReadKey();



        }











    }


}
