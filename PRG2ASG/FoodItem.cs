//==========================================================
// Student Number : S10274903
// Student Name : Javier Chua
// Partner Name : Jayson Chai
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
            return "  - " + ItemName +": " + ItemDesc + " - $" + ItemPrice + " Customise:" + Customise;

        }
    }
}
