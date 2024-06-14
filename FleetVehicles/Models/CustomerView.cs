using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class CustomerView
    {
        public int IdCustomer { get; set; }
        public string StopList { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public decimal? TotalSpent { get; set; }
        public int TotalTrips { get; set; }
    }
}
