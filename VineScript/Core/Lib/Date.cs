using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Date
    {
        [VineBinding]
        public static VineVar CurrentDateTime()
        {
            return DateTime.Now.ToString();
        }

        [VineBinding]
        public static VineVar CurrentDateTime(string format)
        {
            return DateTime.Now.ToString(format);
        }
        
        [VineBinding]
        public static VineVar CurrentDate()
        {
            return DateTime.Today.ToShortDateString();
        }
        
        [VineBinding]
        public static VineVar CurrentDateLong()
        {
            return DateTime.Today.ToLongDateString();
        }
        
        [VineBinding]
        public static VineVar CurrentTime()
        {
            return DateTime.Now.ToShortTimeString();
        }
        
        [VineBinding]
        public static VineVar CurrentTimeLong()
        {
            return DateTime.Now.ToLongTimeString();
        }
        
        [VineBinding]
        public static VineVar MonthDay()
        {
            return DateTime.Today.Day;
        }
        
        [VineBinding]
        public static VineVar Weekday()
        {
            return DateTime.Today.DayOfWeek.ToString();
        }
    }
}
