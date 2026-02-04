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
    internal class OrderedFoodItem : FoodItem
    {
        public int QtyOrdered { get; set; }
        public double SubTotal { get; set; }
        public OrderedFoodItem() { }
        public OrderedFoodItem(string name, string desc, double price, string customise, int ordered, double subTotal) : base(name, desc, price, customise)
        {
            QtyOrdered = ordered;
            SubTotal = ItemPrice * QtyOrdered;

        }

        public double CalculateSubtotal()
        {
            SubTotal = ItemPrice * QtyOrdered;
            return SubTotal;
        }
    }
}
