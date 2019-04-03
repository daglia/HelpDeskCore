using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HelpDesk.Models.Enums
{
    public enum TechnicianStatuses
    {
        [Description("Meşgul")]
        Busy = 0,
        [Description("Uygun")]
        Available = 1,
        [Description("Yolda")]
        OnWay = 2,
        [Description("İş Başında")]
        OnWork = 3
    }
}
