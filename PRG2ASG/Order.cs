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
    internal class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDateTime { get; set; }
        public double OrderTotal { get; set; }
        public string OrderStatus { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string OrderPaymentMethod { get; set; }
        public bool OrderPaid { get; set; }

        public List<OrderedFoodItem> orderedItems { get; set; }
        public Customer Customer { get; set; }

        public Restaurant Restaurant { get; set; }

        public SpecialOffer special { get; set; }

        public Order(Customer customer, Restaurant restaurant)
        {
            Customer = customer;
            Restaurant = restaurant;
            orderedItems = new List<OrderedFoodItem>();
            OrderDateTime = DateTime.Now;
            OrderStatus = "Pending";
            OrderTotal = 0;
            OrderPaymentMethod = "";
            OrderPaid = false;
        }
        public Order()
        {
            orderedItems = new List<OrderedFoodItem>();
            OrderDateTime = DateTime.Now;
            OrderStatus = "Pending";
            OrderTotal = 0;
            OrderPaymentMethod = "";
            OrderPaid = false;
        }

        public Order(int id, DateTime orderdate, double total, string status, DateTime deliverytime, string address, string payment, bool paid)
        {
            OrderID = id;
            OrderDateTime = orderdate;
            OrderTotal = total;
            OrderStatus = status;
            DeliveryDateTime = deliverytime;
            DeliveryAddress = address;
            OrderPaymentMethod = payment;
            OrderPaid = paid;

            orderedItems = new List<OrderedFoodItem>();
        }

        public double CalculateOrderTotal()
        {
            double subtotal = 0;
            for (int i = 0; i < orderedItems.Count; i++)
            {
                subtotal = subtotal + orderedItems[i].CalculateSubtotal();
            }

            double delivery = 5.00;
            double discount = 0;

            if (special != null)
            {
                if (special.OfferCode == "PHOL" || special.Discount == 10)
                {
                    discount = subtotal * (special.Discount / 100);
                }
                if (special.OfferCode == "DELI" && subtotal >= 30)
                {
                    delivery = 0;
                }

            }
            double total = subtotal + delivery - discount;
            OrderTotal = total;
            return total;
        }

        public void AddOrderedFoodItem(OrderedFoodItem item)
        {
            if (item != null)
            {
                orderedItems.Add(item);
                CalculateOrderTotal();
            }
        }

        public bool RemoveOrderedFoodItem(OrderedFoodItem item)
        {
            bool removed = orderedItems.Remove(item);
            CalculateOrderTotal();
            return removed;

        }
        public void DisplayOrderedFoodItems()
        {
            for (int i = 0; i < orderedItems.Count; i++)
            {
                Console.WriteLine(orderedItems[i]);
            }
        }

        public override string ToString()
        {
            return "OrderID:" + OrderID + " OrderDate:" + OrderDateTime + " Total:" + OrderTotal + " Status:" + OrderStatus + " Delivery Time:" + DeliveryDateTime +
                " Address:" + DeliveryAddress + " Payment Method:" + OrderPaymentMethod + " Paid:" + OrderPaid;
        }
    }
}
