using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNotification
{
    public class LotResponse
    {
        public int Total { get; set; }
        public List<Lot> Lots { get; set; }
    }

    public class Lot
    {
        public string ItemId { get; set; }
        public int Amount { get; set; }
        public int StartPrice { get; set; }
        public int BuyoutPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AdditionalData Additional { get; set; }
    }

    public class AdditionalData
    {
        public int Qlt { get; set; }
        public float StatsRandom { get; set; }
        public int UpgradeBonus { get; set; }
        public long SpawnTime { get; set; }
    }
}
