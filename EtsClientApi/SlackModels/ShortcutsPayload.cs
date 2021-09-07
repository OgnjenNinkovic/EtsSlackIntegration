

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
    public class ShortcutsPayload
    {



        [JsonProperty("type")]
        public string payloadType { get; set; }
        public User user { get; set; }
        public string api_app_id { get; set; }


        public string token { get; set; }
        public Container container { get; set; }
        public string trigger_id { get; set; }
        public Team team { get; set; }
        public object enterprise { get; set; }
        public bool is_enterprise_install { get; set; }
        public View view { get; set; }

        public Action[] actions { get; set; }



        public string action_ts { get; set; }
        public string callback_id { get; set; }
        public Channel channel { get; set; }
        public Message message { get; set; }

        public string response_url { get; set; }

        public ShortcutsPayload()
        {
            this.view = new View();
        }

    }

    public class Action
    {
        public string type { get; set; }
        public string action_id { get; set; }
        public string block_id { get; set; }
        public string selected_date { get; set; }
        public string action_ts { get; set; }
    }


    public class View
    {
        public string id { get; set; }
        public string team_id { get; set; }
        public string type { get; set; }
        public Block[] blocks { get; set; }
        public string private_metadata { get; set; }
        public string callback_id { get; set; }
        public ViewState state { get; set; }
        public string hash { get; set; }
        public bool clear_on_close { get; set; }
        public bool notify_on_close { get; set; }
        public Close close { get; set; }


        public Submit submit { get; set; }
        public Title title { get; set; }

        public string previous_view_id { get; set; }
        public string root_view_id { get; set; }
        public string app_id { get; set; }
        public string external_id { get; set; }
        public string app_installed_team_id { get; set; }
        public string bot_id { get; set; }


    }

    public class Close
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }
    public class ViewState
    {
        public ViewStateValues values { get; set; }


    }
    public class ViewStateValues
    {
        [JsonProperty("userNameInput")]
        public Usernameinput userNameInput { get; set; }

        [JsonProperty("userPswInput")]
        public Userpswinput userPswInput { get; set; }

        [JsonProperty("reminderState")]
        public Reminderstate reminderState { get; set; }


        [JsonProperty("radioButtonOption")]
        public Radiobuttonoption radioButtonOption { get; set; }



        [JsonProperty("etsCrudFormActionSelector")]
        public Etscrudformactionselector etsCrudFormActionSelector { get; set; }


        [JsonProperty("etsCrudFormDateSelector")]
        public Etscrudformdateselector etsCrudFormDateSelector { get; set; }

        [JsonProperty("etsCrudFormTimeUnitSelect")]
        public Etscrudformtimeunitselect etsCrudFormTimeUnitSelect { get; set; }


        [JsonProperty("etsCrudFormProjectSelect")]
        public Etscrudformprojectselect etsCrudFormProjectSelect { get; set; }

        [JsonProperty("etsCrudFormTasksSelect")]
        public Etscrudformtasksselect etsCrudFormTasksSelect { get; set; }


        [JsonProperty("etsCrudFormTimeSelect")]
        public Etscrudformtimeselect etsCrudFormTimeSelect { get; set; }

        [JsonProperty("etsCrudFormDescription")]
        public Etscrudformdescription etsCrudFormDescription { get; set; }

        public ViewStateValues()
        {
            this.etsCrudFormActionSelector = new Etscrudformactionselector();
        }

    }



    public class Etscrudformdescription
    {
        public Etscruddescriptiontext etsCrudDescriptionText { get; set; }
    }

    public class Etscruddescriptiontext
    {
        public string type { get; set; }
        public string value { get; set; }
    }



    public class Etscrudformtimeselect
    {
        public Etscrudformtimeselected etsCrudFormTimeSelected { get; set; }
    }


    public class Etscrudformtimeselected
    {
        public string type { get; set; }
        public FormTimeSelected_Option selected_option { get; set; }
    }

    public class FormTimeSelected_Option
    {
        public FormTimeText text { get; set; }
        public string value { get; set; }
    }

    public class FormTimeText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



    public class Etscrudformtasksselect
    {
        public Etscrudformtaskselected etsCrudFormTaskSelected { get; set; }
    }


    public class Etscrudformtaskselected
    {
        public string type { get; set; }
        public FormTaskSelected_Option selected_option { get; set; }
    }


    public class FormTaskSelected_Option
    {
        public FormTaskText text { get; set; }
        public string value { get; set; }
    }

    public class FormTaskText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



    public class Etscrudformprojectselect
    {
        public Etscrudformprojectselected etsCrudFormProjectSelected { get; set; }
    }

    public class Etscrudformprojectselected
    {
        public string type { get; set; }
        public FormProjecSelected_Option selected_option { get; set; }
    }


    public class FormProjecSelected_Option
    {
        public FormProjecText text { get; set; }
        public string value { get; set; }
    }

    public class FormProjecText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



    public class Etscrudformtimeunitselect
    {
        public Etscrudformtimeunitselected etsCrudFormTimeUnitSelected { get; set; }
    }


    public class Etscrudformtimeunitselected
    {
        public string type { get; set; }
        public FormTimeUnitSelected_Option selected_option { get; set; }
    }


    public class FormTimeUnitSelected_Option
    {
        public FormTimeUnitText text { get; set; }
        public string value { get; set; }
    }


    public class FormTimeUnitText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



    public class Etscrudformdateselector
    {
        public Etscrudformdateselected etsCrudFormDateSelected { get; set; }
    }

    public class Etscrudformdateselected
    {
        public string type { get; set; }
        public FormDateSelected_Option selected_option { get; set; }
    }
    public class FormDateSelected_Option
    {
        public FormDateText text { get; set; }
        public string value { get; set; }
    }


    public class FormDateText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }


    public class Etscrudformactionselector
    {
        public Etscrudformactionselected etsCrudFormActionSelected { get; set; }

        public Etscrudformactionselector()
        {
            this.etsCrudFormActionSelected = new Etscrudformactionselected();
        }
    }



    public class Etscrudformactionselected
    {
        public string type { get; set; }
        public Selected_Option selected_option { get; set; }
    }



    public class Selected_Option
    {
        public Text text { get; set; }
        public string value { get; set; }
    }



   




  



  

  





    


  


    



   




   




    


    public class Radiobuttonoption
    {
        [JsonProperty("radioButtonSelection")]
        public Radiobuttonselection radioButtonSelection { get; set; }
    }


    public class Radiobuttonselection
    {
        [JsonProperty("type")]
        public string type { get; set; }


        [JsonProperty("selected_option")]
        public RadioBtnSelected_Option selected_option { get; set; }
    }

    public class RadioBtnSelected_Option
    {
        [JsonProperty("text")]
        public SelectedOptionText text { get; set; }

        [JsonProperty("value")]

        public string value { get; set; }
    }

    public class SelectedOptionText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



    public class Reminderstate
    {

        public Reminderday reminderDay { get; set; }
        public Remindertype reminderType { get; set; }
        public Selecteddate selectedDate { get; set; }
        public Selectedtime_1 selectedTime_1 { get; set; }
        public Selectedtime_2 selectedTime_2 { get; set; }
        public Selectedtime_3 selectedTime_3 { get; set; }
        public Selectedtime_4 selectedTime_4 { get; set; }
        public Selectedtime_5 selectedTime_5 { get; set; }



        public Reminderstate()
        {



            selectedTime_1 = new Selectedtime_1();
            selectedTime_2 = new Selectedtime_2();
            selectedTime_3 = new Selectedtime_3();
            selectedTime_4 = new Selectedtime_4();
            selectedTime_5 = new Selectedtime_5();



        }


    }

    public class Reminderday
    {
        public string type { get; set; }
        public ReminderDay_Option selected_option { get; set; }
    }


    public class ReminderDay_Option
    {
        public ReminderDayText text { get; set; }
        public string value { get; set; }
    }


    public class ReminderDayText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }


    public class Remindertype
    {
        public string type { get; set; }
        public ReminderTypeSelected selected_option { get; set; }


    }

    public class ReminderTypeSelected
    {
        public Text text { get; set; }
        public string value { get; set; }


    }

    public class Selecteddate
    {
        public string type { get; set; }
        public string selected_date { get; set; }


    }

    public class Selectedtime_1
    {
        public string type { get; set; }
        public string selected_time { get; set; }





    }


    public class Selectedtime_2
    {
        public string type { get; set; }
        public string selected_time { get; set; }



    }

    public class Selectedtime_3
    {
        public string type { get; set; }
        public string selected_time { get; set; }


    }

    public class Selectedtime_4
    {
        public string type { get; set; }
        public string selected_time { get; set; }



    }

    public class Selectedtime_5
    {
        public string type { get; set; }
        public string selected_time { get; set; }


    }


    public class Submit
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


    }


    public class Title
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


    }




    public class Usernameinput
    {
        [JsonProperty("enteredUserName")]
        public Enteredusername enteredUserName { get; set; }


    }


    public class Enteredusername
    {
        public string type { get; set; }
        public string value { get; set; }


    }



    public class Userpswinput
    {
        [JsonProperty("enteredEtsPassword")]
        public Enteredetspassword enteredEtsPassword { get; set; }


    }

    public class Enteredetspassword
    {
        public string type { get; set; }
        public string value { get; set; }



    }





    public class User
    {
        public string id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string team_id { get; set; }

    }

    public class Container
    {
        public string type { get; set; }
        public string view_id { get; set; }
        public string message_ts { get; set; }
        public string channel_id { get; set; }
        public bool is_ephemeral { get; set; }


    }

    public class Team
    {
        public string id { get; set; }
        public string domain { get; set; }

    }

    public class Channel
    {
        public string id { get; set; }
        public string name { get; set; }


    }

    public class Message
    {
        public string bot_id { get; set; }
        public string type { get; set; }
        public string text { get; set; }
        public string user { get; set; }
        public string ts { get; set; }
        public string team { get; set; }
        public Block[] blocks { get; set; }


    }

    public class Block
    {
        public string type { get; set; }
        public string block_id { get; set; }
        public Element[] elements { get; set; }



    }

    public class Element
    {
        public string type { get; set; }
        public string action_id { get; set; }
        public Placeholder placeholder { get; set; }
        public Option[] options { get; set; }
        public string initial_date { get; set; }
        public Text text { get; set; }
        public string value { get; set; }


    }

    public class Placeholder
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }



    }

    public class Text
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }



    }

    public class Option
    {
        public Text1 text { get; set; }
        public string value { get; set; }


    }

    public class Text1
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


    }




}
