using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class PersonalAccountView
    {
        public int IdEmployee { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string DriverLicenseNumber { get; set; }
        public bool IsDriver { get; set; }
        public int? ProcessedOrdersCount { get; set; }
        public int? TripsCount { get; set; }
        public int? TotalMileage { get; set; }
    }
}
