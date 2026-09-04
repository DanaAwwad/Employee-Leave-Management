using System;
using System.Collections.Generic;
using System.Text;

namespace Employee___Leave_Management.Models
{
    public class Employees
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RemainingLeaveDays { get; set; }

        public bool IsDeleted { get; set; }
    }
}
