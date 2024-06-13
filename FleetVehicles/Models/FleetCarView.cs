using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class FleetCarView
    {
        public int FleetCarID { get; set; }
        public string CarInfo { get; set; }
        public string DriverName { get; set; }
        public string VinNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string ColorName { get; set; }
    }

}
