using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class CarView
    {
        public int IdCar { get; set; }
        public string ModelName { get; set; }
        public string BodyName { get; set; }
        public int? NumberOfSeats { get; set; }
        public int? LoadCapacity { get; set; }
        public string CarInfo { get; set; }
    }
}
