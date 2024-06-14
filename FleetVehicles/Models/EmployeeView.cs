using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class EmployeeView
    {
        public int IdEmployee { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string Patronymic { get; set; }
        public string PositionName { get; set; }
        public int TotalOrders { get; set; }
    }
}
