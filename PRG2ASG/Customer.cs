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
    internal class Customer
    {
        public string EmailAddress { get; set; }
        public string CustomerName { get; set; }

        public List<Order> OrderList { get; set; }
        public Customer()
        {
            OrderList = new List<Order>();
        }

        public Customer(string email, string cname)
        {
            EmailAddress = email;
            CustomerName = cname;
            OrderList = new List<Order>();
        }

        public void AddOrder(Order order)
        {

            if (order != null)
            {
                OrderList.Add(order);
            }
        }
        public void DisplayAllOrders()
        {
            foreach (Order o in OrderList)
            {
                Console.WriteLine(o);
            }

        }
        public bool RemoveOrder(Order order)
        {
            return OrderList.Remove(order);
        }

        public override string ToString()
        {
            return "Email:" + EmailAddress + " Name:" + CustomerName;
        }
    }
}
