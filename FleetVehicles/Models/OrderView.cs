using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetVehicles.Models
{
    public class OrderView
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; }
        public string TripDate { get; set; }
        public string DepartureAddress { get; set; }
        public string ArrivalAddress { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string Car { get; set; }
        public string Driver { get; set; }
        public int TotalCost { get; set; }
        public string Dispatcher { get; set; }
        public string Tariff { get; set; }
        public int NumberOfPassengers { get; set; }
        public string Notes { get; set; }
       public string Status { get; set; }
        public bool IsCompleted { get; set; }
        public bool CanManageOrder { get; set; }
    }

}
