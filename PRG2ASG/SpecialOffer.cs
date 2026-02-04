using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2ASG
{
    internal class SpecialOffer
    {
        public string OfferCode { get; set; }
        public string OfferDesc { get; set; }
        public double Discount { get; set; }

        public SpecialOffer() { }

        public SpecialOffer(string code, string odesc, double discount)
        {
            OfferCode = code;
            OfferDesc = odesc;
            Discount = discount;
        }

        public override string ToString()
        {
            return "Code:" + OfferCode + " Description:" + OfferDesc + " Discount:" + Discount;
        }

    }
}
