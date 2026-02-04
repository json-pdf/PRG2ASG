using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG2ASG
{
    internal class Restaurant
    {
        public string RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantEmail { get; set; }



        public Queue<Order> orderQueue { get; set; }
        public List<SpecialOffer> specialList = new List<SpecialOffer>();
        public List<Menu> menuList = new List<Menu>();

        public Restaurant()
        {
            orderQueue = new Queue<Order>();
            menuList = new List<Menu>();
            specialList = new List<SpecialOffer>();
        }
        public Restaurant(string id, string rname, string email)
        {
            RestaurantId = id;
            RestaurantName = rname;
            RestaurantEmail = email;
            orderQueue = new Queue<Order>();
            specialList = new List<SpecialOffer>();
            menuList = new List<Menu>();
        }

        public void DisplayOrders()
        {
            if (orderQueue.Count == 0)
            {
                Console.WriteLine("No orders.");
                return;
            }

            foreach (Order o in orderQueue)
            {
                Console.WriteLine(o);
            }
        }

        public void DisplaySpecialOffers()
        {
            if (specialList.Count == 0)
            {
                Console.WriteLine("No special offers.");
                return;
            }

            foreach (SpecialOffer s in specialList)
            {
                Console.WriteLine(s);
            }
        }

        public void DisplayMenu()
        {
            if (menuList.Count == 0)
            {
                Console.WriteLine("No menus available.");
                return;
            }

            foreach (Menu m in menuList)
            {
                Console.WriteLine(m);
            }
        }

        public void AddMenu(Menu m)
        {
            if (m != null)
            {
                menuList.Add(m);
            }
        }

        public bool RemoveMenu(Menu m)
        {
            return menuList.Remove(m);
        }

        public override string ToString()
        {
            return "ID:" + RestaurantId + " Name:" + RestaurantName + " Email:" + RestaurantEmail;
        }
    }
}
