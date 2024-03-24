using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JusGiveaway
{
    internal class CashOutDetails
    {
        public required string Name { get; set; }
        public required string UID { get; set; }
        public required string EmailAddress { get; set; }
        public string? Sex { get; set; }
        public required string DeviceInfo { get; set; }
        public required string BankAccountNumber { get; set; }
        public required int CashoutAmount { get; set; }

        //cashout amount
        //cashout time
    }
}
