using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S4PIDemoFE.Zjy
{
    public class LogTagUtil
    {
        public static string getYYmmStr() {
            string time = DateTime.Now.ToString("yyyy_MM");
            return time;
        }
        public static string getYYmmDDStr()
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd");
            return time;
        }
    }
}
