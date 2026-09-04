using System;
using System.Collections.Generic;
using System.Text;
using Employee___Leave_Management.Enum;
namespace Employee___Leave_Management.Models
{
    public class Leave_Requests 
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int RequestedDays { get; set; }

        public LeaveStatus Status { get; set; }
    }
}
