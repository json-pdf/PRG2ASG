
using PRG2ASG;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

//Javier - feature 1

// Bonus Feature (Favourite Orders) storage - Javier
Dictionary<string, List<string>> favouriteDict = new Dictionary<string, List<string>>();

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
    Console.WriteLine("1.  List all restaurants and menu items");
    Console.WriteLine("2.  List all orders");
    Console.WriteLine("3.  Create a new order");
    Console.WriteLine("4.  Process an order");
    Console.WriteLine("5.  Modify an existing order");
    Console.WriteLine("6.  Delete an existing order");
    Console.WriteLine("7.  Bulk process Pending orders (Today)");
    Console.WriteLine("8.  Display total order amount");
    Console.WriteLine("9.  Save an order as Favourite");
    Console.WriteLine("10. Reorder from Favourite");
    Console.WriteLine("0.  Exit");
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
    else if (choice == "7")
    {
        BulkProcessPendingOrdersToday();
    }
    else if (choice == "8")
    {
        DisplayTotalOrderAmount();
    }
        else if (choice == "9")
    {
        AddFavouriteOrder();
    }
    else if (choice == "10")
    {
        CreateOrderFromFavourite();
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

    // Validate Customer Email
    string email = "";
    while (true)
    {
        Console.Write("Enter Customer Email: ");
        email = Console.ReadLine().Trim();

        if (email == "")
        {
            Console.WriteLine("Error: Email cannot be empty.");
            continue;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            Console.WriteLine("Error: Invalid email format.");
            continue;
        }

        break;
    }

    // Validate Restaurant ID
    string restaurantId = "";
    Restaurant restaurant = null;
    while (true)
    {
        Console.Write("Enter Restaurant ID: ");
        restaurantId = Console.ReadLine().Trim();

        if (restaurantId == "")
        {
            Console.WriteLine("Error: Restaurant ID cannot be empty.");
            continue;
        }

        restaurant = restaurantList.Find(r => r.RestaurantId == restaurantId);
        if (restaurant == null)
        {
            Console.WriteLine("Error: Restaurant not found.");
            continue;
        }
        break;
    }

    // Validate Delivery Date
    DateTime deliverydate = DateTime.MinValue; //placeholder value, so the code compiles without error.
    while (true)
    {
        Console.Write("Enter Delivery Date (dd/mm/yyyy): ");
        string inputdate = Console.ReadLine().Trim();

        if (inputdate == "")
        {
            Console.WriteLine("Error: Date cannot be empty.");
            continue;
        }

        try
        {
            deliverydate = DateTime.ParseExact(inputdate, "dd/MM/yyyy", null);
            if (deliverydate.Date < DateTime.Now.Date)
            {
                Console.WriteLine("Error: Delivery date cannot be in the past.");
                continue;
            }
            break;
        }
        catch
        {
            Console.WriteLine("Error: Invalid date format. Use dd/mm/yyyy.");
        }
    }

    // Validate Delivery Time
    TimeSpan deliverytime = TimeSpan.Zero;
    DateTime deliverydatetime = DateTime.MinValue;
    while (true)
    {
        Console.Write("Enter Delivery Time (hh:mm): ");
        string inputtime = Console.ReadLine().Trim();

        if (inputtime == "")
        {
            Console.WriteLine("Error: Time cannot be empty.");
            continue;
        }

        try
        {
            deliverytime = TimeSpan.Parse(inputtime);
            deliverydatetime = deliverydate.Date + deliverytime;

            if (deliverydatetime <= DateTime.Now)
            {
                Console.WriteLine("Error: Delivery time must be in the future.");
                continue;
            }
            break;
        }
        catch
        {
            Console.WriteLine("Error: Invalid time format. Use hh:mm.");
        }
    }


    // Validate Delivery Address
    string address = "";
    while (true)
    {
        Console.Write("Enter Delivery Address: ");
        address = Console.ReadLine().Trim();

        if (address == "")
        {
            Console.WriteLine("Error: Address cannot be empty.");
            continue;
        }
        break;
    }


    Customer customer = customerList.Find(c => c.EmailAddress == email);
    if (customer == null)
    {
        Console.WriteLine("Customer not found. Creating new customer account...");

        string customerName = "";
        while (true)
        {
            Console.Write("Enter Customer Name: ");
            customerName = Console.ReadLine().Trim();

            if (customerName == "")
            {
                Console.WriteLine("Error: Name cannot be empty.");
                continue;
            }
            break;
        }

        customer = new Customer(email, customerName);
        customerList.Add(customer);

        try
        {
            string customerLine = customerName + "," + email;
            using (StreamWriter sw = new StreamWriter("customers.csv", true))
            {
                sw.WriteLine(customerLine);
            }
            Console.WriteLine("New customer created successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving customer: " + ex.Message);
        }
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
        string itemInput = Console.ReadLine().Trim();

        int itemchoice;
        if (!int.TryParse(itemInput, out itemchoice))
        {
            Console.WriteLine("Error: Invalid number.");
            continue;
        }

        if (itemchoice == 0)
        {
            if (order.orderedItems.Count == 0)
            {
                Console.WriteLine("Error: Must order at least one item.");
                continue;
            }
            break;
        }

        if (itemchoice > 0 && itemchoice <= availableItems.Count)
        {
            FoodItem foodItem = availableItems[itemchoice - 1];

            // Validate Quantity
            int qty = 0;
            while (true)
            {
                Console.Write("Enter quantity: ");
                try
                {
                    qty = Convert.ToInt32(Console.ReadLine());
                    if (qty <= 0)
                    {
                        Console.WriteLine("Error: Quantity must be greater than 0.");
                        continue;
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("Error: Invalid quantity.");
                }
            }

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
            Console.WriteLine("Error: Invalid item number.");
        }
    }

    Console.Write("Add special request? [Y/N]: ");
    string reqans = Console.ReadLine().Trim().ToUpper();

    if (reqans == "Y")
    {
        Console.Write("Enter special request: ");
        string sreq = Console.ReadLine().Trim();

        if (sreq != "" && order.orderedItems.Count > 0)
        {
            order.orderedItems[0].Customise = sreq;
        }
    }


    // Jayson - Advance (c) special offer
    string disc = "";
    while (true)
    {
        Console.Write("Enter discount code if any (or press Enter to skip): ");
        disc = Console.ReadLine().Trim().ToUpper();

        if (disc == "")
        {
            break; // No discount code, continue with order
        }
        else if (disc == "PHOL")
        {
            order.special = new SpecialOffer("PHOL", "10% off order", 10);
            break;
        }
        else if (disc == "DELI")
        {
            order.special = new SpecialOffer("DELI", "Free delivery on orders more than $30", 0);
            break;
        }
        else if (disc == "X")
        {
            break;
        }
    }

    double orderTotal = order.CalculateOrderTotal();
    double subtotal = 0;
    foreach (var item in order.orderedItems)
    {
        subtotal += item.CalculateSubtotal();
    }

    double delivery = 5.00;
    double discount = 0;

    if (order.special != null)
    {
        if (order.special.OfferCode == "PHOL")
        {
            discount = subtotal * 0.10;
        }
        if (order.special.OfferCode == "DELI" && subtotal >= 30)
        {
            delivery = 0;
        }
    }

    Console.WriteLine($"\nOrder Total: ${subtotal:F2} + ${delivery:F2} (delivery) - ${discount:F2} (discount) = ${orderTotal:F2}");

    Console.Write("Proceed to payment? [Y/N]: ");
    string propay = Console.ReadLine().Trim().ToUpper();

    if (propay != "Y")
    {
        Console.WriteLine("Order cancelled.");
        return;
    }

    // Validation for Payment Method
    string paymentmethod = "";
    while (true)
    {
        Console.WriteLine("Payment method: ");
        Console.Write("[CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");
        paymentmethod = Console.ReadLine().Trim().ToUpper();

        if (paymentmethod == "CC")
        {
            order.OrderPaymentMethod = "Credit Card";
            break;
        }
        else if (paymentmethod == "PP")
        {
            order.OrderPaymentMethod = "PayPal";
            break;
        }
        else if (paymentmethod == "CD")
        {
            order.OrderPaymentMethod = "Cash on Delivery";
            break;
        }
        else
        {
            Console.WriteLine("Error: Invalid payment method.");
        }
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
        using (StreamWriter sw = new StreamWriter("orders.csv", true))
        {
            string itemsString = "";
            for (int i = 0; i < order.orderedItems.Count; i++)
            {
                if (i > 0) itemsString += "|";
                itemsString += order.orderedItems[i].ItemName + "," + order.orderedItems[i].QtyOrdered;
            }

            sw.WriteLine(
                $"{order.OrderID},{customer.EmailAddress},{restaurant.RestaurantId}," +
                $"{order.DeliveryDateTime:dd/MM/yyyy},{order.DeliveryDateTime:HH:mm}," +
                $"{order.DeliveryAddress},{order.OrderDateTime:dd/MM/yyyy HH:mm}," +
                $"{order.OrderTotal},{order.OrderStatus},\"{itemsString}\""
            );
        }
        Console.WriteLine("Order saved successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error saving order: " + ex.Message);
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

    // Validation for Customer Email
    string email = "";
    Customer customer = null;
    while (true)
    {
        Console.Write("Enter Customer Email: ");
        email = Console.ReadLine().Trim();

        if (email == "")
        {
            Console.WriteLine("Error: Email cannot be empty.");
            continue;
        }

        customer = customerList.Find(c => c.EmailAddress == email);
        if (customer == null)
        {
            Console.WriteLine("Error: Customer does not exist.");
            continue;
        }
        break;
    }

    //only find pending orders
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

    // Validate Order ID
    Order order = null;
    while (true)
    {
        Console.Write("Enter Order ID: ");
        try
        {
            int orderID = Convert.ToInt32(Console.ReadLine());

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
                Console.WriteLine("Error: Order does not exist or is not pending.");
                continue;
            }
            break;
        }
        catch
        {
            Console.WriteLine("Error: Invalid Order ID.");
        }
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

    // Validate Modification Option
    int modoption = 0;
    while (true)
    {
        Console.Write("\nModify: [1] Items [2] Address [3] Delivery Time: ");
        try
        {
            modoption = Convert.ToInt32(Console.ReadLine());

            if (modoption < 1 || modoption > 3)
            {
                Console.WriteLine("Error: Please enter 1, 2, or 3.");
                continue;
            }
            break;
        }
        catch
        {
            Console.WriteLine("Error: Invalid option.");
        }
    }

    if (modoption == 1)
    {
        // Validate Item Modification Choice
        string itemmod = "";
        while (true)
        {
            Console.Write("Enter changes to items [R]emove/[A]dd items: ");
            itemmod = Console.ReadLine().Trim().ToUpper();

            if (itemmod == "")
            {
                Console.WriteLine("Error: Input cannot be empty.");
                continue;
            }

            if (itemmod != "R" && itemmod != "A")
            {
                Console.WriteLine("Error: Please enter R or A.");
                continue;
            }
            break;
        }

        if (itemmod == "R")
        {
            if (order.orderedItems.Count == 0)
            {
                Console.WriteLine("Error: No items to remove.");
                return;
            }

            Console.WriteLine("Current items: ");
            for (int i = 0; i < order.orderedItems.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {order.orderedItems[i].ItemName} - {order.orderedItems[i].QtyOrdered}");
            }

            // Validate Item Removal
            while (true)
            {
                Console.Write("Enter item number to remove: ");
                try
                {
                    int remove = Convert.ToInt32(Console.ReadLine());

                    if (remove < 1 || remove > order.orderedItems.Count)
                    {
                        Console.WriteLine("Error: Invalid item number.");
                        continue;
                    }

                    OrderedFoodItem item = order.orderedItems[remove - 1];
                    string itemName = item.ItemName;
                    order.RemoveOrderedFoodItem(item);
                    Console.WriteLine($"{itemName} has been removed.");

                    // deletes empty orders
                    if (order.orderedItems.Count == 0)
                    {
                        customer.OrderList.Remove(order);
                        orderList.Remove(order);
                        Console.WriteLine($"Order {order.OrderID} has been deleted as all items were removed.");
                        return;
                    }

                    break;
                }
                catch
                {
                    Console.WriteLine("Error: Invalid input.");
                }
            }
        }

        if (itemmod == "A")
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

            if (availableItems.Count == 0)
            {
                Console.WriteLine("Error: No items available.");
                return;
            }

            // Validate Item Selection
            while (true)
            {
                Console.Write("Enter item number to add: ");
                try
                {
                    int choice = Convert.ToInt32(Console.ReadLine());

                    if (choice < 1 || choice > availableItems.Count)
                    {
                        Console.WriteLine("Error: Invalid item number.");
                        continue;
                    }

                    FoodItem foodItem = availableItems[choice - 1];

                    // Validate Quantity
                    int qty = 0;
                    while (true)
                    {
                        Console.Write("Enter quantity: ");
                        try
                        {
                            qty = Convert.ToInt32(Console.ReadLine());
                            if (qty <= 0)
                            {
                                Console.WriteLine("Error: Quantity must be greater than 0.");
                                continue;
                            }
                            break;
                        }
                        catch
                        {
                            Console.WriteLine("Error: Invalid quantity.");
                        }
                    }

                    OrderedFoodItem orderedFoodItem = new OrderedFoodItem(
                        foodItem.ItemName,
                        foodItem.ItemDesc,
                        foodItem.ItemPrice,
                        foodItem.Customise,
                        qty,
                        0);
                    order.AddOrderedFoodItem(orderedFoodItem);
                    Console.WriteLine($"Order {order.OrderID} updated. {qty} of {foodItem.ItemName} has been added.");
                    break;
                }
                catch
                {
                    Console.WriteLine("Error: Invalid input.");
                }
            }
        }
        order.CalculateOrderTotal();
        Console.WriteLine($"Updated Order Total: ${order.OrderTotal:F2}");
    }
    else if (modoption == 2)
    {
        // Validate New Address
        string newaddress = "";
        while (true)
        {
            Console.Write("Enter new Delivery Address: ");
            newaddress = Console.ReadLine().Trim();

            if (newaddress == "")
            {
                Console.WriteLine("Error: Address cannot be empty.");
                continue;
            }
            break;
        }
        order.DeliveryAddress = newaddress;
        Console.WriteLine($"Order {order.OrderID} updated.\nNew Address: {newaddress}");
    }
    else if (modoption == 3)
    {
        // Validate New Delivery Time
        while (true)
        {
            Console.Write("Enter new Delivery Time (hh:mm): ");
            string newtime = Console.ReadLine().Trim();

            if (newtime == "")
            {
                Console.WriteLine("Error: Time cannot be empty.");
                continue;
            }

            try
            {
                TimeSpan time = TimeSpan.Parse(newtime);
                DateTime newDeliveryDateTime = order.DeliveryDateTime.Date + time;

                if (newDeliveryDateTime <= DateTime.Now)
                {
                    Console.WriteLine("Error: Delivery time must be in the future.");
                    continue;
                }

                order.DeliveryDateTime = newDeliveryDateTime;
                Console.WriteLine($"Order {order.OrderID} updated. New Delivery Time: {time}");
                break;
            }
            catch
            {
                Console.WriteLine("Error: Invalid time format. Use hh:mm.");
            }
        }
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



/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//Advance Feature (a) - Javier

void BulkProcessPendingOrdersToday()
{
    Console.WriteLine("\nBulk Processing of Pending Orders (Today)");
    Console.WriteLine("=========================================");

    DateTime today = DateTime.Today;
    int totalPending = 0;

    Console.WriteLine("\nPending Orders:");

    for (int i = 0; i < restaurantList.Count; i++)
    {
        Restaurant r = restaurantList[i];
        bool printedRestaurant = false;

        foreach (Order o in r.orderQueue)
        {
            if (o.OrderStatus == "Pending" &&
                o.DeliveryDateTime.Date == today)
            {
                if (!printedRestaurant)
                {
                    Console.WriteLine("\nRestaurant: " + r.RestaurantName +
                                      " (" + r.RestaurantId + ")");
                    printedRestaurant = true;
                }

                Console.WriteLine("Order ID: " + o.OrderID +
                                  " | Delivery: " +
                                  o.DeliveryDateTime.ToString("HH:mm"));

                totalPending++;
            }
        }
    }

    Console.WriteLine("\nTotal Pending orders today: " + totalPending);

    if (totalPending == 0)
    {
        Console.WriteLine("No Pending orders to process.");
        return;
    }

    int processedCount = 0;
    int preparingCount = 0;
    int rejectedCount = 0;

    for (int i = 0; i < restaurantList.Count; i++)
    {
        Restaurant r = restaurantList[i];
        int queueSize = r.orderQueue.Count;

        for (int j = 0; j < queueSize; j++)
        {
            Order o = r.orderQueue.Dequeue();

            if (o.OrderStatus == "Pending" &&
                o.DeliveryDateTime.Date == today)
            {
                TimeSpan timeLeft = o.DeliveryDateTime - DateTime.Now;

                if (timeLeft.TotalMinutes < 60)
                {
                    o.OrderStatus = "Rejected";
                    rejectedCount++;
                }
                else
                {
                    o.OrderStatus = "Preparing";
                    preparingCount++;
                }

                processedCount++;
            }

            r.orderQueue.Enqueue(o);
        }
    }

    double percentage = (processedCount * 100.0) / totalPending;

    Console.WriteLine("\nSummary");
    Console.WriteLine("-------");
    Console.WriteLine("Orders processed: " + processedCount);
    Console.WriteLine("Preparing orders: " + preparingCount);
    Console.WriteLine("Rejected orders: " + rejectedCount);
    Console.WriteLine("Percentage auto processed: " +
                      percentage.ToString("0.00") + "%");
}


//Advance Feature (b) - Jayson



void DisplayTotalOrderAmount()
{
    Console.WriteLine("\nDisplay Total Order Amount");
    Console.WriteLine("==========================");

    double deliveryFee = 5.00;

    double grandTotalSales = 0;   // money earned from Delivered (less delivery fee per order)
    double grandTotalRefunds = 0; // money refunded from Cancelled/Rejected

    for (int i = 0; i < restaurantList.Count; i++)
    {
        Restaurant r = restaurantList[i];

        double restaurantSales = 0;
        double restaurantRefunds = 0;

        int deliveredCount = 0;

        // Go through every order in the restaurant queue
        foreach (Order o in r.orderQueue)
        {

            if (o.OrderStatus == "Delivered")
            {
                deliveredCount++;

                // minus delivery fee per order
                double net = o.OrderTotal - deliveryFee;
                if (net < 0) net = 0;

                restaurantSales += net;
            }
            // Refunded orders
            else if (o.OrderStatus == "Cancelled" || o.OrderStatus == "Rejected")
            {
                restaurantRefunds += o.OrderTotal;
            }
        }

        grandTotalSales += restaurantSales;
        grandTotalRefunds += restaurantRefunds;

        Console.WriteLine("\nRestaurant: " + r.RestaurantName + " (" + r.RestaurantId + ")");
        Console.WriteLine("Total Delivered Orders (less $5 delivery fee each): $" + restaurantSales.ToString("0.00"));
        Console.WriteLine("Total Refunds (Cancelled/Rejected): $" + restaurantRefunds.ToString("0.00"));
    }

    double finalEarnings = grandTotalSales - grandTotalRefunds;

    Console.WriteLine("\nOverall Summary");
    Console.WriteLine("--------------");
    Console.WriteLine("Total Order Amount: $" + grandTotalSales.ToString("0.00"));
    Console.WriteLine("Total Refunds: $" + grandTotalRefunds.ToString("0.00"));
    Console.WriteLine("Final Amount Gruberoo Earns: $" + finalEarnings.ToString("0.00"));
}




// Advance Featuere (Favourite Orders) - Javier

void LoadFavouritesFromFile()
{
    favouriteDict.Clear();

    if (!File.Exists("favourites.csv"))
        return;

    string[] lines;

    try
    {
        lines = File.ReadAllLines("favourites.csv");
    }
    catch
    {
        Console.WriteLine("Error reading favourites.csv");
        return;
    }

    for (int i = 0; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i])) continue;

        try
        {
            // email,restaurantId,"item1,qty|item2,qty"
            string[] data = lines[i].Split(',', 3);
            if (data.Length < 3) continue;

            string email = data[0].Trim();
            string restId = data[1].Trim();
            string items = data[2].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(email)) continue;
            if (string.IsNullOrWhiteSpace(restId)) continue;
            if (string.IsNullOrWhiteSpace(items)) continue;

            string template = restId + "|" + items;

            if (!favouriteDict.ContainsKey(email))
            {
                favouriteDict[email] = new List<string>();
            }

            // avoid duplicate templates
            bool exists = false;
            for (int j = 0; j < favouriteDict[email].Count; j++)
            {
                if (favouriteDict[email][j] == template)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                favouriteDict[email].Add(template);
            }
        }
        catch
        {
            // ignore bad lines, continue loading
        }
    }
}

void SaveFavouriteToFile(string email, string restaurantId, string items)
{
    try
    {
        string line = email + "," + restaurantId + "," + "\"" + items + "\"";
        File.AppendAllText("favourites.csv", line + Environment.NewLine);
    }
    catch
    {
        Console.WriteLine("Error saving to favourites.csv");
    }
}

void AddFavouriteOrder()
{
    Console.WriteLine("\nFavourite Orders - Save");
    Console.WriteLine("========================");

    //Get valid customer email (re-prompt)
    Customer customer = null;
    string email = "";

    while (customer == null)
    {
        Console.Write("Enter Customer Email: ");
        email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email cannot be empty.\n");
            continue;
        }

        email = email.Trim();
        customer = customerList.Find(c => c.EmailAddress == email);

        if (customer == null)
        {
            Console.WriteLine("Customer not found. Please try again.\n");
        }
    }

    if (customer.OrderList.Count == 0)
    {
        Console.WriteLine("No orders found for this customer.");
        return;
    }

    Console.WriteLine("\nCustomer Orders:");
    for (int i = 0; i < customer.OrderList.Count; i++)
    {
        Order o = customer.OrderList[i];

        string restName = "";
        if (o.Restaurant != null) restName = o.Restaurant.RestaurantName;

        Console.WriteLine(o.OrderID + " - " + restName + " (" + o.OrderStatus + ")");
    }

    // Get valid order id (re-prompt)
    Order order = null;

    while (order == null)
    {
        Console.Write("Enter Order ID to save as favourite: ");
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

        order = customer.OrderList.Find(o => o.OrderID == orderId);

        if (order == null)
        {
            Console.WriteLine("Order not found. Please try again.\n");
        }
        else if (order.Restaurant == null)
        {
            Console.WriteLine("This order has no restaurant linked. Please choose another order.\n");
            order = null;
        }
        else if (order.orderedItems == null || order.orderedItems.Count == 0)
        {
            Console.WriteLine("This order has no items. Please choose another order.\n");
            order = null;
        }
    }

    string items = "";
    for (int i = 0; i < order.orderedItems.Count; i++)
    {
        if (i > 0) items += "|";
        items += order.orderedItems[i].ItemName + "," + order.orderedItems[i].QtyOrdered;
    }

    string restaurantId = order.Restaurant.RestaurantId;
    string template = restaurantId + "|" + items;

    // Save in memory (avoid duplicates)
    if (!favouriteDict.ContainsKey(email))
        favouriteDict[email] = new List<string>();

    bool exists = false;
    for (int i = 0; i < favouriteDict[email].Count; i++)
    {
        if (favouriteDict[email][i] == template)
        {
            exists = true;
            break;
        }
    }

    if (exists)
    {
        Console.WriteLine("This favourite already exists.");
        return;
    }

    favouriteDict[email].Add(template);

    // Save to file
    SaveFavouriteToFile(email, restaurantId, items);

    Console.WriteLine("Favourite order saved!");
}

void CreateOrderFromFavourite()
{
    Console.WriteLine("\nFavourite Orders - Reorder");
    Console.WriteLine("===========================");

    Customer customer = null;
    string email = "";

    while (customer == null)
    {
        Console.Write("Enter Customer Email: ");
        email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email cannot be empty.\n");
            continue;
        }

        email = email.Trim();
        customer = customerList.Find(c => c.EmailAddress == email);

        if (customer == null)
        {
            Console.WriteLine("Customer not found. Please try again.\n");
        }
    }

    if (!favouriteDict.ContainsKey(email) || favouriteDict[email].Count == 0)
    {
        Console.WriteLine("No favourite orders found.");
        return;
    }

    Console.WriteLine("\nFavourite Orders:");
    for (int i = 0; i < favouriteDict[email].Count; i++)
    {
        string template = favouriteDict[email][i];
        string[] parts = template.Split('|', 2);
        string restId = parts[0];

        Restaurant r = restaurantList.Find(x => x.RestaurantId == restId);
        string restName = (r != null) ? r.RestaurantName : restId;

        Console.WriteLine((i + 1) + ". " + restName + " (" + restId + ")");
    }

    int choice = 0;
    while (true)
    {
        Console.Write("Choose favourite number: ");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Choice cannot be empty.\n");
            continue;
        }

        bool okChoice = int.TryParse(input.Trim(), out choice);

        if (!okChoice || choice < 1 || choice > favouriteDict[email].Count)
        {
            Console.WriteLine("Invalid choice. Please try again.\n");
            continue;
        }

        break;
    }

    string selected = favouriteDict[email][choice - 1];
    string[] selectedParts = selected.Split('|', 2);
    string restaurantId = selectedParts[0];
    string items = selectedParts.Length > 1 ? selectedParts[1] : "";

    Restaurant restaurant = restaurantList.Find(r => r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        Console.WriteLine("Restaurant not found (favourite may be outdated).");
        return;
    }

    // Delivery Date+Time validation (must be future)
    DateTime deliveryDateTime;

    while (true)
    {
        Console.Write("Enter Delivery Date (dd/mm/yyyy): ");
        string inputDate = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputDate))
        {
            Console.WriteLine("Date cannot be empty.\n");
            continue;
        }

        DateTime deliveryDate;
        bool okDate = DateTime.TryParseExact(inputDate.Trim(), "dd/MM/yyyy", null,
            System.Globalization.DateTimeStyles.None, out deliveryDate);

        if (!okDate)
        {
            Console.WriteLine("Invalid date format. Please use dd/mm/yyyy.\n");
            continue;
        }

        Console.Write("Enter Delivery Time (hh:mm): ");
        string inputTime = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputTime))
        {
            Console.WriteLine("Time cannot be empty.\n");
            continue;
        }

        TimeSpan deliveryTime;
        bool okTime = TimeSpan.TryParse(inputTime.Trim(), out deliveryTime);

        if (!okTime)
        {
            Console.WriteLine("Invalid time format. Please use hh:mm.\n");
            continue;
        }

        deliveryDateTime = deliveryDate.Date + deliveryTime;

        if (deliveryDateTime <= DateTime.Now)
        {
            Console.WriteLine("Delivery date/time must be in the future. Please try again.\n");
            continue;
        }

        break;
    }

    string address = "";
    while (true)
    {
        Console.Write("Enter Delivery Address: ");
        address = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(address))
        {
            Console.WriteLine("Address cannot be empty.\n");
            continue;
        }

        address = address.Trim();
        break;
    }

    // Create order 
    Order order = new Order(customer, restaurant);
    order.DeliveryDateTime = deliveryDateTime;
    order.DeliveryAddress = address;

    // Add items back into order
    string[] itemPairs = items.Split('|');
    for (int i = 0; i < itemPairs.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(itemPairs[i])) continue;

        string[] p = itemPairs[i].Split(',');
        if (p.Length < 2) continue;

        string itemName = p[0].Trim();

        int qty;
        bool okQty = int.TryParse(p[1].Trim(), out qty);
        if (!okQty || qty <= 0) continue;

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

    if (order.orderedItems.Count == 0)
    {
        Console.WriteLine("No valid items found from this favourite order.");
        return;
    }

    order.CalculateOrderTotal();

    int maxOrderId = 1000;
    for (int i = 0; i < orderList.Count; i++)
    {
        if (orderList[i].OrderID > maxOrderId)
            maxOrderId = orderList[i].OrderID;
    }
    order.OrderID = maxOrderId + 1;

    order.OrderStatus = "Pending";
    order.OrderPaid = true;
    order.OrderPaymentMethod = "Favourite Reorder";

    orderList.Add(order);
    customer.AddOrder(order);
    restaurant.orderQueue.Enqueue(order);

    Console.WriteLine("\nOrder " + order.OrderID + " created from favourite! Status: Pending");
}
