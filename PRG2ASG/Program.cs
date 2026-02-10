
using PRG2ASG;
using System.Reflection.Metadata.Ecma335;
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
        ListAllOrdersBasicInfo(orderList);
    }
    else if (choice == "3")
    {
        neworder();
    }
    else if (choice == "4")
    {
       ProcessOrder();
    }
    else if (choice == "5")
    {
        Modifyorder();
    }
    else if (choice == "6")
    {
        DeleteOrder();
        
    }
    else if (choice == "0")
    {
        Console.WriteLine("Thank you for using Gruberoo!!");
        break;
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

//Javier - feature 4

static void ListAllOrdersBasicInfo(List<Order> orderList)
{
    Console.WriteLine("All Orders");
    Console.WriteLine("==========");


    Console.WriteLine(
        string.Format(
            "{0,-10}{1,-15}{2,-18}{3,-22}{4,-10}{5}",
            "Order ID",
            "Customer",
            "Restaurant",
            "Delivery Date/Time",
            "Amount",
            "Status"
        )
    );


    Console.WriteLine(
        string.Format(
            "{0,-10}{1,-15}{2,-18}{3,-22}{4,-10}{5}",
            "--------",
            "--------------",
            "-----------------",
            "--------------------",
            "--------",
            "--------"
        )
    );


    foreach (Order o in orderList)
    {
        string customerName = o.Customer != null ? o.Customer.CustomerName : "";
        string restaurantName = o.Restaurant != null ? o.Restaurant.RestaurantName : "";

        Console.WriteLine(
            string.Format(
                "{0,-10}{1,-15}{2,-18}{3,-22}{4,-10}{5}",
                o.OrderID,
                customerName,
                restaurantName,
                o.DeliveryDateTime.ToString("dd/MM/yyyy HH:mm"),
                "$" + o.OrderTotal.ToString("0.00"),
                o.OrderStatus
            )
        );
    }
}


//Jayson - feature 5

        void neworder()
        {
            Console.WriteLine("\nCreate a New Order");
            Console.WriteLine("===================");
            Console.Write("Enter Customer Email:");
            string email = Console.ReadLine();

            Console.Write("Enter Restaurant ID: ");
            string restaurantId = Console.ReadLine();

            Console.Write("Enter Delivery Date (dd/mm/yyyy): ");
            string inputdate = Console.ReadLine();

            DateTime deliverydate = DateTime.ParseExact(inputdate, "dd/MM/yyyy", null);

            Console.Write("Enter Delivery Time (hh:mm): ");
            string inputtime = Console.ReadLine();
            TimeSpan deliverytime = TimeSpan.Parse(inputtime);
            DateTime deliverydatetime = deliverydate.Date + deliverytime;

            Console.Write("Enter Delivery Address: ");
            string address = Console.ReadLine();
    
    Customer customer = customerList.Find(c => c.EmailAddress == email);
        if (customer == null)
        {
            Console.WriteLine("Customer not found. Creating new customer account...");
            Console.Write("Enter Customer Name: ");
            string customerName = Console.ReadLine();

            customer = new Customer(email, customerName);
            customerList.Add(customer);

            // Append new customer to customers.csv
            try
            {
                string customerLine = customerName + "," + email;
                File.AppendAllText("customers.csv", "\n" + customerLine);
                Console.WriteLine("New customer created successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving customer to file: " + ex.Message);
            }
        }



    Restaurant restaurant = restaurantList.Find(r => r.RestaurantId == restaurantId);
            if (restaurant == null)
            {
        Console.WriteLine("Restaurant not found.");
                    return;
            }

            Order order = new Order(customer, restaurant);
            order.DeliveryDateTime = deliverydatetime;
            order.DeliveryAddress = address;

            Console.WriteLine("\nAvailable Food Items:");
            List<FoodItem> availableItems = new List<FoodItem>();
            int itemnumber = 1;

            foreach (Menu m in restaurant.menuList)
            {
                foreach (FoodItem f in m.GetFoodList())
                {
                    Console.WriteLine($"{itemnumber}. {f.ItemName} - ${f.ItemPrice}");
                    availableItems.Add(f);
                    itemnumber++;
                }
            }

            while (true)
            {
                Console.Write("Enter item number (0 to finish): ");
                int itemchoice = Convert.ToInt32(Console.ReadLine());


                if (itemchoice == 0)
                {
                    break;
                }
                if (itemchoice > 0 && itemchoice <= availableItems.Count)
                {
                    FoodItem foodItem = availableItems[itemchoice - 1];
                    Console.Write("Enter quantity: ");
                    int qty = Convert.ToInt32(Console.ReadLine());
                    OrderedFoodItem orderedFoodItem = new OrderedFoodItem(
                        foodItem.ItemName,
                        foodItem.ItemDesc,
                        foodItem.ItemPrice,
                        foodItem.Customise,
                        qty,
                        0);
                    order.AddOrderedFoodItem(orderedFoodItem);

                }
                else
                {
                    Console.WriteLine("Invalid item number. Please try again.");
                }
            }
            Console.Write("Add special request? [Y/N]: ");
            string reqans = Console.ReadLine();

            if (reqans.ToUpper() == "Y")
            {
                Console.Write("Enter special request: ");
                string sreq = Console.ReadLine();

                if (order.orderedItems.Count > 0)
                {
                    order.orderedItems[0].Customise = sreq;
                }
            }
            double orderTotal = order.CalculateOrderTotal();
            double subtotal = orderTotal - 5.00;
            Console.WriteLine($"\nOrder Total: ${subtotal:F2} + $5.00 (delivery) = ${orderTotal:F2}");

            Console.Write("Proceed to payment? [Y/N]: ");
            string propay = Console.ReadLine();

            if(propay.ToUpper() == "N")
            {
                Console.WriteLine("Order cancelled.");
                return;

            }

            Console.WriteLine("Payment method: ");
            Console.Write("[CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");
            String paymentmethod = Console.ReadLine().ToUpper();

            if (paymentmethod == "CC")
            {
                order.OrderPaymentMethod = "Credit Card";
            }
            else if (paymentmethod == "PP")
            {
                order.OrderPaymentMethod = "PayPal";
            }
            else if (paymentmethod == "CD")
            {
                order.OrderPaymentMethod = "Cash on Delivery";
            }

            order.OrderPaid = true;
            order.OrderStatus = "Pending";

            int maxOrderId = 1000;
            foreach (Order o in orderList)
            {
                if (o.OrderID > maxOrderId)
                {
                    maxOrderId = o.OrderID;
                }
            }
            order.OrderID = maxOrderId + 1;

            orderList.Add(order);
            customer.AddOrder(order);
            restaurant.orderQueue.Enqueue(order);
            try
{
    string items = "";
    for (int i = 0; i < order.orderedItems.Count; i++)
    {
        if (i > 0) items += "|";
        items += order.orderedItems[i].ItemName + "," + order.orderedItems[i].QtyOrdered;
    }
    
    string line = order.OrderID + "," + 
                  customer.EmailAddress + "," + 
                  restaurant.RestaurantId + "," + 
                  order.DeliveryDateTime.ToString("dd/MM/yyyy") + "," + 
                  order.DeliveryDateTime.ToString("HH:mm") + "," + 
                  order.DeliveryAddress + "," + 
                  order.OrderDateTime.ToString("dd/MM/yyyy HH:mm") + "," + 
                  order.OrderTotal + "," + 
                  order.OrderStatus + "," + 
                  "\"" + items + "\"";
    
    File.AppendAllText("orders.csv", "\n" + line);
}
catch (Exception ex)
{
    Console.WriteLine("Error saving: " + ex.Message);
}

            Console.WriteLine($"\nOrder {order.OrderID} created successfully! Status: {order.OrderStatus}");
        }

// Javier - feature 6

void ProcessOrder()
{
    Console.WriteLine("\nProcess Order");
    Console.WriteLine("=============");

    Restaurant restaurant = null;

    while (restaurant == null)
    {
        Console.Write("Enter Restaurant ID: ");
        string restaurantId = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(restaurantId))
        {
            Console.WriteLine("Restaurant ID cannot be empty.\n");
            continue;
        }

        restaurantId = restaurantId.Trim();
        restaurant = restaurantList.Find(r => r.RestaurantId == restaurantId);

        if (restaurant == null)
        {
            Console.WriteLine("Restaurant not found. Please try again.\n");
        }
    }

    if (restaurant.orderQueue.Count == 0)
    {
        Console.WriteLine("\nNo orders to process.");
        return;
    }

    foreach (Order order in restaurant.orderQueue)
    {
        Console.WriteLine($"\nOrder {order.OrderID}:");

        string custName = "N/A";
        if (order.Customer != null)
        {
            custName = order.Customer.CustomerName;
        }
        Console.WriteLine("Customer: " + custName);

        Console.WriteLine("Ordered Items:");
        for (int i = 0; i < order.orderedItems.Count; i++)
        {
            OrderedFoodItem item = order.orderedItems[i];
            Console.WriteLine((i + 1) + ". " + item.ItemName + " - " + item.QtyOrdered);
        }

        Console.WriteLine("Delivery date/time: " + order.DeliveryDateTime.ToString("dd/MM/yyyy HH:mm"));
        Console.WriteLine("Total Amount: $" + order.OrderTotal.ToString("0.00"));
        Console.WriteLine("Order Status: " + order.OrderStatus);


        string choice = "";
        bool validChoice = false;

        while (!validChoice)
        {
            Console.Write("\n[C]onfirm / [R]eject / [S]kip / [D]eliver: ");
            choice = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(choice))
            {
                Console.WriteLine("Input cannot be empty.");
                continue;
            }

            choice = choice.Trim().ToUpper();

            if (choice == "C" || choice == "R" || choice == "S" || choice == "D")
            {
                validChoice = true;
            }
            else
            {
                Console.WriteLine("Invalid option. Please enter C, R, S or D.");
            }
        }

        Console.WriteLine();


        if (choice == "S")
        {
            Console.WriteLine("Order skipped.");
            continue;
        }
        else if (choice == "C")
        {
            if (order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Preparing";
                Console.WriteLine("Order " + order.OrderID + " confirmed. Status: Preparing");
            }
            else
            {
                Console.WriteLine("Order cannot be confirmed because it is not Pending.");
            }
        }
        else if (choice == "R")
        {
            if (order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Rejected";
                Console.WriteLine("Order " + order.OrderID + " rejected. Refund initiated.");
            }
            else
            {
                Console.WriteLine("Order cannot be rejected because it is not Pending.");
            }
        }
        else if (choice == "D")
        {
            if (order.OrderStatus == "Preparing")
            {
                order.OrderStatus = "Delivered";
                Console.WriteLine("Order " + order.OrderID + " delivered successfully.");
            }
            else
            {
                Console.WriteLine("Order cannot be delivered because it is not Preparing.");
            }
        }
    }
}


// Jayson - feature 7

void Modifyorder()
{
    Console.WriteLine("Modify Order");
    Console.WriteLine("============");
    Console.Write("Enter Customer Email: ");
    string email = Console.ReadLine();

    Customer customer = customerList.Find(c => c.EmailAddress == email);
    if (customer == null)
    {
        Console.WriteLine("Customer does not exist.");
        return;
    }

    //only pending orders using a loop
    List<Order> pendingOrders = new List<Order>();
    for (int i = 0; i < customer.OrderList.Count; i++)
    {
        if (customer.OrderList[i].OrderStatus == "Pending")
        {
            pendingOrders.Add(customer.OrderList[i]);
        }
    }

    if (pendingOrders.Count == 0)
    {
        Console.WriteLine("No pending orders.");
        return;
    }

    Console.WriteLine("Pending Orders:");
    for (int i = 0; i < pendingOrders.Count; i++)
    {
        Console.WriteLine(pendingOrders[i].OrderID);
    }

    Console.Write("Enter Order ID: ");
    int orderID = Convert.ToInt32(Console.ReadLine());

    Order order = null;
    for (int i = 0; i < pendingOrders.Count; i++)
    {
        if (pendingOrders[i].OrderID == orderID)
        {
            order = pendingOrders[i];
            break;
        }
    }

    if (order == null)
    {
        Console.WriteLine("Order does not exist or is not pending.");
        return;
    }

   
    Console.WriteLine("Order Items:");
    foreach (var item in order.orderedItems)
    {
        Console.WriteLine($"{item.QtyOrdered}. {item.ItemName}");
    }
    Console.WriteLine("Address:");
    Console.WriteLine(order.DeliveryAddress);
    Console.WriteLine("Delivery Date/Time:");
    Console.WriteLine(order.DeliveryDateTime);
    Console.Write("\nModify: [1] Items [2] Address [3] Delivery Time: ");
    int modoption = Convert.ToInt32(Console.ReadLine());

    if (modoption == 1)
    {
        Console.WriteLine("Enter changes to items [R]emove/[A]dd items]: ");
        string itemmod = Console.ReadLine();

        if (itemmod.ToUpper() == "R")
        {
            Console.WriteLine("Current items: ");
            for (int i = 0; i < order.orderedItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}.{order.orderedItems[i].ItemName} - {order.orderedItems[i].QtyOrdered}");
            }

            Console.Write("Enter item number to remove: ");
            int remove = Convert.ToInt32(Console.ReadLine());
            if (remove > 0 && remove <= order.orderedItems.Count)
            {
                OrderedFoodItem item = order.orderedItems[remove - 1];
                string itemName = item.ItemName;
                order.RemoveOrderedFoodItem(item);
                Console.WriteLine($"Order {orderID} updated. {itemName} has been removed.");
            }
        }

        if (itemmod.ToUpper() == "A")
        {
            Console.WriteLine("Available Food items:");
            List<FoodItem> availableItems = new List<FoodItem>();
            int itemnumber = 1;
            foreach (Menu m in order.Restaurant.menuList)
            {
                foreach (FoodItem f in m.GetFoodList())
                {
                    Console.WriteLine($"{itemnumber}. {f.ItemName} - ${f.ItemPrice}");
                    availableItems.Add(f);
                    itemnumber++;
                }
            }
            Console.Write("Enter item number to add: ");
            int choice = Convert.ToInt32(Console.ReadLine());
            if (choice > 0 && choice <= availableItems.Count)
            {
                FoodItem foodItem = availableItems[choice - 1];
                Console.Write("Enter quantity: ");
                int qty = Convert.ToInt32(Console.ReadLine());
                OrderedFoodItem orderedFoodItem = new OrderedFoodItem(
                    foodItem.ItemName,
                    foodItem.ItemDesc,
                    foodItem.ItemPrice,
                    foodItem.Customise,
                    qty,
                    0);
                order.AddOrderedFoodItem(orderedFoodItem);
                Console.WriteLine($"Order {orderID} updated. {qty} of {foodItem.ItemName} has been added.");
            }
        }
        order.CalculateOrderTotal();
        Console.WriteLine($"Updated Order Total: ${order.OrderTotal:F2}");
    }
    else if (modoption == 2)
    {
        Console.Write("Enter new Delivery Address: ");
        string newaddress = Console.ReadLine();
        order.DeliveryAddress = newaddress;
        Console.WriteLine($"Order {orderID} updated.\nNew Address: {newaddress}");
    }
    else if (modoption == 3)
    {
        Console.Write("Enter new Delivery Time (hh:mm): ");
        string newtime = Console.ReadLine();
        TimeSpan time = TimeSpan.Parse(newtime);
        order.DeliveryDateTime = order.DeliveryDateTime.Date + time;
        Console.WriteLine($"Order {orderID} updated. New Delivery Time: {time}");
    }
}

//Javier - feature 8

void DeleteOrder()
{
    Console.WriteLine("Delete Order");
    Console.WriteLine("============");

    Customer customer = null;
    while (customer == null)
    {
        Console.Write("Enter Customer Email: ");
        string email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email cannot be empty.\n");
            continue;
        }

        email = email.Trim();
        customer = customerList.Find(c => c.EmailAddress == email);

        if (customer == null)
        {
            Console.WriteLine("Customer does not exist. Please try again.\n");
        }
    }

    Console.WriteLine("Pending Orders:");
    List<Order> pendingOrders = new List<Order>();

    for (int i = 0; i < customer.OrderList.Count; i++)
    {
        Order o = customer.OrderList[i];
        if (o.OrderStatus == "Pending")
        {
            Console.WriteLine(o.OrderID);
            pendingOrders.Add(o);
        }
    }

    if (pendingOrders.Count == 0)
    {
        Console.WriteLine("No pending orders found.");
        return;
    }

    Order order = null;

    while (order == null)
    {
        Console.Write("Enter Order ID: ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Order ID cannot be empty.\n");
            continue;
        }

        int orderId;
        bool ok = int.TryParse(input.Trim(), out orderId);

        if (!ok)
        {
            Console.WriteLine("Invalid Order ID. Please enter a number.\n");
            continue;
        }

        for (int i = 0; i < pendingOrders.Count; i++)
        {
            if (pendingOrders[i].OrderID == orderId)
            {
                order = pendingOrders[i];
                break;
            }
        }

        if (order == null)
        {
            Console.WriteLine("Order not found in Pending Orders. Please try again.\n");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Customer: " + customer.CustomerName);
    Console.WriteLine("Ordered Items:");
    for (int i = 0; i < order.orderedItems.Count; i++)
    {
        OrderedFoodItem item = order.orderedItems[i];
        Console.WriteLine((i + 1) + ". " + item.ItemName + " - " + item.QtyOrdered);
    }

    Console.WriteLine("Delivery date/time: " + order.DeliveryDateTime.ToString("dd/MM/yyyy HH:mm"));
    Console.WriteLine("Total Amount: $" + order.OrderTotal.ToString("0.00"));
    Console.WriteLine("Order Status: " + order.OrderStatus);

    string confirm = "";
    bool validConfirm = false;

    while (!validConfirm)
    {
        Console.Write("Confirm deletion? [Y/N]: ");
        confirm = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(confirm))
        {
            Console.WriteLine("Input cannot be empty.");
            continue;
        }

        confirm = confirm.Trim().ToUpper();

        if (confirm == "Y" || confirm == "N")
        {
            validConfirm = true;
        }
        else
        {
            Console.WriteLine("Invalid option. Please enter Y or N.");
        }
    }

    Console.WriteLine();

    if (confirm == "N")
    {
        Console.WriteLine("Order deletion cancelled.");
        return;
    }

    order.OrderStatus = "Cancelled";

    Console.WriteLine("Order " + order.OrderID + " cancelled. Refund of $" +
                      order.OrderTotal.ToString("0.00") + " processed.");
}

