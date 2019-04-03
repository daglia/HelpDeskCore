using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Models.Models
{
    public class ReportData
    {
        public string question { get; set; }
        public double point { get; set; }
    }
    public class WeeklyReport
    {
        public string date { get; set; }
        public int count { get; set; }
    }

    public class TechReport
    {
        public string nameSurname { get; set; }
        public double point { get; set; }
    }
}
