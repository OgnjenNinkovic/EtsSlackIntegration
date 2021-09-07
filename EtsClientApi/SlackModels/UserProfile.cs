using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{

    public class UserProfile
    {
        public bool ok { get; set; }
        public UserProfileDetails user { get; set; }
    }

    public class UserProfileDetails
    {
        public string id { get; set; }
        public string team_id { get; set; }
        public string name { get; set; }
        public bool deleted { get; set; }
        public string color { get; set; }
        public string real_name { get; set; }
        public string tz { get; set; }
        public string tz_label { get; set; }
        public int tz_offset { get; set; }
        public Profile profile { get; set; }
        public bool is_admin { get; set; }
        public bool is_owner { get; set; }
        public bool is_primary_owner { get; set; }
        public bool is_restricted { get; set; }
        public bool is_ultra_restricted { get; set; }
        public bool is_bot { get; set; }
        public bool is_app_user { get; set; }
        public int updated { get; set; }
        public bool is_email_confirmed { get; set; }
    }

    public class Profile
    {
        public string title { get; set; }
        public string phone { get; set; }
        public string skype { get; set; }
        public string real_name { get; set; }
        public string real_name_normalized { get; set; }
        public string display_name { get; set; }
        public string display_name_normalized { get; set; }
        public object fields { get; set; }
        public string status_text { get; set; }
        public string status_emoji { get; set; }
        public int status_expiration { get; set; }
        public string avatar_hash { get; set; }
        public string status_text_canonical { get; set; }
        public string team { get; set; }
    }

}
