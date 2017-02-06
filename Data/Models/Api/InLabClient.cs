using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Data.Models.Api
{
    public class InLabClient
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string DisplayName { get { return LastName + ", " + FirstName; } }
        public string FullName { get { return FirstName + " " + LastName; } }
        public DateTime AccessEventTime { get; set; }
        
        public double MinutesInLab
        {
            get
            {
                if (AccessEventTime == DateTime.MinValue)
                    return 0;
                
                double seconds = (DateTime.Now - AccessEventTime).TotalSeconds;
                
                if (seconds < 0 )
                    return 0;


                return seconds / 60.0;
            }
        }
    }
}