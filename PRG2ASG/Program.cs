
using PRG2ASG;

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

    // Skip header line (index 0)
    for (int i = 1; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        try
        {
            // Expected: RestaurantId,RestaurantName,RestaurantEmail
            string[] data = lines[i].Split(',');

            if (data.Length < 3) continue;

            string id = data[0].Trim();
            string name = data[1].Trim();
            string email = data[2].Trim();

            Restaurant r = new Restaurant(id, name, email);

            // Create 1 default menu (since fooditems.csv has no menu info)
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

    // Skip header line (index 0)
    for (int i = 1; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        try
        {
            // Expected: RestaurantId,ItemName,ItemDesc,ItemPrice
            string[] data = lines[i].Split(',');

            if (data.Length < 4) continue;

            string restaurantId = data[0].Trim();
            string itemName = data[1].Trim();
            string itemDesc = data[2].Trim();

            double itemPrice = 0;
            double.TryParse(data[3].Trim(), out itemPrice);

            Restaurant restaurant = FindRestaurantById(restaurantList, restaurantId);

            if (restaurant != null)
            {
                // Your FoodItem uses Customise (string), keep empty for now
                FoodItem foodItem = new FoodItem(itemName, itemDesc, itemPrice, "");

                // Add to first menu that was created in LoadRestaurantsFromFile
                if (restaurant.menuList.Count > 0)
                {
                    restaurant.menuList[0].AddFoodItem(foodItem);
                }
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
        string[] lines = File.ReadAllLines("orders_-_Copy.csv");

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i] == "") continue;

            try
            {
                string[] data = lines[i].Split(',', 10);
                if (data.Length < 10) continue;

                int orderId = Convert.ToInt32(data[0]);
                string customerEmail = data[1];
                string restaurantId = data[2];
                DateTime deliveryDate = Convert.ToDateTime(data[3]);
                TimeSpan deliveryTime = TimeSpan.Parse(data[4]);
                DateTime deliveryDateTime = deliveryDate.Date + deliveryTime;
                string deliveryAddress = data[5];
                DateTime createdDateTime = Convert.ToDateTime(data[6]);
                double totalAmount = Convert.ToDouble(data[7]);
                string status = data[8];
                string items = data[9].Trim('"');

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
                        catch
                        {
                            Console.WriteLine($"Error parsing item in order {orderId}");
                        }
                    }

                    orderList.Add(order);
                    customer.AddOrder(order);
                    restaurant.orderQueue.Enqueue(order);
                }
            }
            catch
            {
                Console.WriteLine($"Error parsing order line {i + 1}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading orders_-_Copy.csv: {ex.Message}");
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

