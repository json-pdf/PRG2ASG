
using PRG2ASG;
using System.Runtime.CompilerServices;

//Javier - feature 1
static void LoadRestaurantsFromFile(List<Restaurant> restaurantList)
{
    string[] lines;

    try
    {
        lines = File.ReadAllLines("restaurants.csv");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error reading restaurants.csv: " + ex.Message);
        return;
    }

    // Skip header line
    for (int i = 1; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        try
        {
            // RestaurantId,RestaurantName,RestaurantEmail
            string[] data = lines[i].Split(',');

            if (data.Length < 3) continue;

            string id = data[0].Trim();
            string name = data[1].Trim();
            string email = data[2].Trim();

            Restaurant r = new Restaurant(id, name, email);

            // Create 1 default menu for each restaurant
            Menu m = new Menu("M_" + id, name + " Menu");
            r.AddMenu(m);

            restaurantList.Add(r);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error parsing line " + (i + 1) + " in restaurants.csv: " + ex.Message);
        }
    }
}

static void LoadFoodItemsFromFile(List<Restaurant> restaurantList)
{
    string[] lines;

    try
    {
        lines = File.ReadAllLines("fooditems.csv");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error reading fooditems.csv: " + ex.Message);
        return;
    }


    for (int i = 1; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        try
        {
            // RestaurantId,ItemName,ItemDesc,ItemPrice
            string[] data = lines[i].Split(',', 4);

            if (data.Length < 4) continue;

            string restaurantId = data[0].Trim();
            string itemName = data[1].Trim();
            string itemDesc = data[2].Trim();

            double itemPrice = 0;
            double.TryParse(data[3].Trim(), out itemPrice);

            Restaurant restaurant = FindRestaurantById(restaurantList, restaurantId);

            if (restaurant != null)
            {
                FoodItem foodItem = new FoodItem(itemName, itemDesc, itemPrice, "");

                // Safety: ensure menu exists
                if (restaurant.menuList.Count == 0)
                {
                    Menu m = new Menu("M_" + restaurant.RestaurantId,
                                      restaurant.RestaurantName + " Menu");
                    restaurant.AddMenu(m);
                }

                restaurant.menuList[0].AddFoodItem(foodItem);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error parsing line " + (i + 1) + " in fooditems.csv: " + ex.Message);
        }
    }
}

static Restaurant FindRestaurantById(List<Restaurant> restaurantList, string restaurantId)
{
    for (int i = 0; i < restaurantList.Count; i++)
    {
        if (restaurantList[i].RestaurantId == restaurantId)
        {
            return restaurantList[i];
        }
    }
    return null;
}





//Jayson - feature 2
void LoadCustomersFromFile(List<Customer> customerList)
{
    string[] stringArray;
    try
    {
        stringArray = File.ReadAllLines("customers.csv");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error Reading customer.csv: {ex.Message}");
        return;
    }
    for (int i = 1; i < stringArray.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(stringArray[i])) continue;
        try
        {
            string[] dataArray = stringArray[i].Split(',');
            string name = dataArray[0].Trim();
            string email = dataArray[1].Trim();

            Customer c = new Customer(email, name);
            customerList.Add(c);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing line {i + 1} in customers.csv: {ex.Message}");
        }
    }
}


void LoadOrdersFromFile(List<Order> orderList, List<Customer> customerList, List<Restaurant> restaurantList)
{
    try
    {
        string[] lines = File.ReadAllLines("orders.csv");  

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            try
            {
                string[] data = lines[i].Split(',', 10);
                if (data.Length < 10) continue;

                int orderId = Convert.ToInt32(data[0]);
                string customerEmail = data[1].Trim();
                string restaurantId = data[2].Trim();

               
                DateTime deliveryDate = DateTime.ParseExact(data[3].Trim(), "dd/MM/yyyy", null);
                TimeSpan deliveryTime = TimeSpan.Parse(data[4].Trim());
                DateTime deliveryDateTime = deliveryDate.Date + deliveryTime;

                string deliveryAddress = data[5].Trim();

             
                DateTime createdDateTime = DateTime.ParseExact(data[6].Trim(), "dd/MM/yyyy HH:mm", null);

                double totalAmount = Convert.ToDouble(data[7].Trim());
                string status = data[8].Trim();
                string items = data[9].Trim().Trim('"');

                Customer customer = customerList.Find(c => c.EmailAddress == customerEmail);
                Restaurant restaurant = restaurantList.Find(r => r.RestaurantId == restaurantId);

                if (customer != null && restaurant != null)
                {
                    Order order = new Order(orderId, createdDateTime, totalAmount, status,
                                            deliveryDateTime, deliveryAddress, "", false);
                    order.Customer = customer;
                    order.Restaurant = restaurant;

                    string[] itemPairs = items.Split('|');
                    for (int j = 0; j < itemPairs.Length; j++)
                    {
                        try
                        {
                            string[] parts = itemPairs[j].Split(',');
                            if (parts.Length < 2) continue;

                            string itemName = parts[0].Trim();
                            int qty = Convert.ToInt32(parts[1].Trim());

                            FoodItem foodItem = FindFoodItemInRestaurant(restaurant, itemName);

                            if (foodItem != null)
                            {
                                OrderedFoodItem orderedItem = new OrderedFoodItem(
                                    foodItem.ItemName,
                                    foodItem.ItemDesc,
                                    foodItem.ItemPrice,
                                    foodItem.Customise,
                                    qty,
                                    0
                                );
                                order.AddOrderedFoodItem(orderedItem);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing item in order {orderId}: {ex.Message}");
                        }
                    }

                    orderList.Add(order);
                    customer.AddOrder(order);
                    restaurant.orderQueue.Enqueue(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing order line {i + 1}: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading orders.csv: {ex.Message}");
    }
}


FoodItem FindFoodItemInRestaurant(Restaurant restaurant, string itemName)
{

    for (int i = 0; i < restaurant.menuList.Count; i++)
    {
        Menu menu = restaurant.menuList[i];


        FoodItem item = menu.FindFoodItem(itemName);
        if (item != null)
        {
            return item;
        }

    }

    return null;
}

List<Restaurant> restaurantList = new List<Restaurant>();
List<Customer> customerList = new List<Customer>();
List<Order> orderList = new List<Order>();


LoadRestaurantsFromFile(restaurantList);
LoadFoodItemsFromFile(restaurantList);
LoadCustomersFromFile(customerList);
LoadOrdersFromFile(orderList, customerList, restaurantList);


int totalFoodItems = 0;
for (int i = 0; i < restaurantList.Count; i++)
{
    for (int j = 0; j < restaurantList[i].menuList.Count; j++)
    {
        totalFoodItems = totalFoodItems + restaurantList[i].menuList[j].GetFoodList().Count;
    }
}


Console.WriteLine("Welcome to the Gruberoo Food Delivery System");
Console.WriteLine(restaurantList.Count + " restaurants loaded!");
Console.WriteLine(totalFoodItems + " food items loaded!");
Console.WriteLine(customerList.Count + " customers loaded!");
Console.WriteLine(orderList.Count + " orders loaded!");


bool exit = false;
while (exit == false)
{
    Console.WriteLine("\n===== Gruberoo Food Delivery System =====");
    Console.WriteLine("1. List all restaurants and menu items");
    Console.WriteLine("2. List all orders");
    Console.WriteLine("3. Create a new order");
    Console.WriteLine("4. Process an order");
    Console.WriteLine("5. Modify an existing order");
    Console.WriteLine("6. Delete an existing order");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    string choice = Console.ReadLine();

    if (choice == "1")
    {
       menu();

    }
    else if (choice == "2")
    {
        
    }
    else if (choice == "3")
    {
        
    }
    else if (choice == "4")
    {
       
    }
    else if (choice == "5")
    {
       
    }
    else if (choice == "6")
    {
        
    }
    else if (choice == "0")
    {
       
    }
    else
    {
       
    }
}


// Jayson - feature 3
void menu()
{
    Console.WriteLine("All Restaurants and Menu Items:");
    Console.WriteLine("==============================");
    foreach (Restaurant r in restaurantList)
    {
        Console.WriteLine($"Restaurant: {r.RestaurantName} ({r.RestaurantId})");
        foreach (Menu m in r.menuList)
        {
            m.DisplayFoodItems();
        }
        Console.WriteLine();
    }
}