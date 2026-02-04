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
    internal class Menu
    {
        public string MenuID { get; set; }
        public string MenuName { get; set; }

        private List<FoodItem> foodList = new List<FoodItem>();
        public Menu()
        {
            foodList = new List<FoodItem>();
        }

        public Menu(string id, string name)
        {
            MenuID = id;
            MenuName = name;
            foodList = new List<FoodItem>();

        }


        public FoodItem FindFoodItem(string itemName)
        {
            for (int i = 0; i < foodList.Count; i++)
            {
                if (foodList[i].ItemName == itemName)
                {
                    return foodList[i];
                }
            }
            return null;
        }
        public List<FoodItem> GetFoodList()
        {
            return new List<FoodItem>(foodList);
        }

        public void AddFoodItem(FoodItem f)
        {
            if (f != null)
            {
                foodList.Add(f);
            }
        }

        public bool RemoveFoodItem(FoodItem item)
        {
            return foodList.Remove(item);
        }

        public void DisplayFoodItems()
        {
            for (int i = 0; i < foodList.Count; i++)
            {
                Console.WriteLine(foodList[i]);
            }
        }

        public override string ToString()
        {
            return " MenuID:" + MenuID + " Name:" + MenuName;
        }
    }
}
