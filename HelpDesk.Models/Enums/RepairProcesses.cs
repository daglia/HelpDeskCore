using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HelpDesk.Models.Enums
{
    public enum RepairProcesses
    {
        [Description("Başarısız")]
        Failed = 0,
        [Description("Başarılı")]
        Successful = 1,
    }
}
