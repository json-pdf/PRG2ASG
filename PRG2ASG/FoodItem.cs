//==========================================================
// Student Number : S10268704
// Student Name : Jayson Chai
// Partner Name : Javier Chua
//==========================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2ASG
{
    internal class FoodItem
    {
        public string ItemName { get; set; }
        public string ItemDesc { get; set; }
        public double ItemPrice { get; set; }
        public string Customise { get; set; }

        public FoodItem() { }

        public FoodItem(string name, string desc, double price, string customise)
        {
            ItemName = name;
            ItemDesc = desc;
            ItemPrice = price;
            Customise = customise;
        }

        public override string ToString()
        {
            return "Name:" + ItemName + " Description:" + ItemDesc + " Price:" + ItemPrice + " Customise:" + Customise;

        }
    }
}
