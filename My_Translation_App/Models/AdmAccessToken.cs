using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace My_Translation_App.Models
{
    public class AdmAccessToken
    {
        // required for deserialization
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }

        // properties and methods for determining expired tokens
        public DateTime tokenEndTime { get; set; }

        public bool IsExpired()
        {
            //return false;
            DateTime now = DateTime.Now;
            double secondsLeft = tokenEndTime.Subtract(now).TotalSeconds;
            if (secondsLeft < 30)
                return true;
            else
                return false;
        }

        public void Initalize()
        {
            tokenEndTime = DateTime.Now.Add(new TimeSpan(0, 0, 600));
        }
    }
}