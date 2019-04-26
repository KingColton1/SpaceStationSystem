using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    /// <summary>
    /// The main function to run thread in the background and display menu
    /// </summary>
    class SpaceStation
    {
        // Create a queue for the ship to wait for their turn
        private Queue<Ship> _serviceQueue = new Queue<Ship>();

        // Establish thread to allow the program to operate the service for the ship on its own
        private Thread _monitorQueue;

        // Store information into a list and dictionary
        private List<Bay> _dockingBays;
        private Dictionary<int, Ship> _ships;
        
        // Dad - Defines the definition of the method used to handle an event.
        //       The items it defines are the method's return type (here it's void) and the method's parameters (here there are two parameters named dockId and shipFedId).
        // Define a delegate for the Service Complete event.
        private delegate void ServiceCompleteEventHandler(int dockId, int shipFedId);

        // Dad - Use the delegate definition created above to set the delegate definition of the event.
        // Define an event object for the Service Complete event.
        private event ServiceCompleteEventHandler ServiceComplete;

        private DateTime _startTime;
        private string _lastShipServiced = "";
        private bool _lastShipComplete = false;

        /// <summary>
        /// The name of the space station.
        /// </summary>
        public string Name { get; set; }        

        /// <summary>
        /// Loads the resource files into class properties and creates the queue for servicing the ships.
        /// </summary>
        /// <param name="name">The name of the space station.</param>
        public SpaceStation(string name)
        {
            try
            {
                Name = name;
                _ships = new Dictionary<int, Ship>();

                // Dad - Event handler method names should always start with "On" and then the name of the event.
                // Assign method OnServiceComplete as a delegate to handle the Service Complete event.
                ServiceComplete += new ServiceCompleteEventHandler(OnServiceComplete);

                // Needed for null values in JSON files. Ignores null values.
                var settings = new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    };
                
                // Load the docking bay information from file Docks2019.json into class property DockingBays.
                _dockingBays = JsonConvert.DeserializeObject<List<Bay>>(Properties.Resources.Docks2019_json, settings);

                // Load the ship information from file FinalShips2019.json into a List.
                List<Ship> ships = JsonConvert.DeserializeObject<List<Ship>>(Properties.Resources.FinalShips2019_json, settings);

                // Use the ship information to load the Ships dictionary and the service queue.
                foreach (Ship ship in ships)
                {
                    _ships.Add(ship.ShipFedId, ship);
                    _serviceQueue.Enqueue(ship);
                }

                // Dad - You need to create the required folders here not files.  So check if the directory does NOT exist.
                //       If the required directories don't exist, create them.
                // Create required folders for reports.
                if (!Directory.Exists(System.Environment.CurrentDirectory + @"\Reports"))
                {
                    Directory.CreateDirectory(System.Environment.CurrentDirectory + @"\Reports");
                }
                if (!Directory.Exists(System.Environment.CurrentDirectory + @"\Reports\Temp"))
                {
                    Directory.CreateDirectory(System.Environment.CurrentDirectory + @"\Reports\Temp");
                }
                else
                {
                    // Delete temporary files from past ship services.
                    foreach (string file in Directory.GetFiles(System.Environment.CurrentDirectory + @"\Reports\Temp"))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.SpaceStation() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// The method of service ships to handle with the service
        /// </summary>
        public void ServiceShips()
        {
            Ship ship;
            int choice;

            try
            {
                _startTime = DateTime.Now;
                Console.WriteLine($"Start docking service for space station {Name}");
                Thread.Sleep(2000);

                // Dad - This is the UI for the ship coordinator to start servicing ships.
                InitializeService();

                // Spawn a thread that will monitor the ship queue.
                _monitorQueue = new Thread(new ThreadStart(MonitorQueue)); // Here the start method is MonitorQueue().
                _monitorQueue.Name = "Monitor Queue";
                _monitorQueue.IsBackground = true;
                _monitorQueue.Start();

                // Dad - I removed this because it was what was stopping the program from ending.
                //       Now we're going to have a menu so we don't need to block the Monitor Queue thread that is spawned above.
                // Wait until the thread is complete.
                //_monitorQueue.Join();

                // Dad - This menu loop is now what will keep the program from ending.
                //       You're good with menus.  I started a basic menu.  You need to enhance it.

                // Release and queued ship aren't practical option, considering it would fill the program with 500 ships. The FinalShip JSON file have 500 ships by the way.
                // I renamed View open docking bays to Select docking bays, and renamed View occupied docking bays to View docking bays status.
                // On the first comment I made above, I think we should just show only 4 closest to the front of the line that are soon to be docked (View queued ships)
                while (true)
                {                     
                    Console.WriteLine("\n1. View docked ships" +
                                      "\n2. Select docking bays" +
                                      "\n3. View docking bays status" +
                                      "\n4. View queued ships" +
                                      "\n5. Exit");

                    Console.Write("Option: ");
                    int.TryParse(Console.ReadLine(), out choice);

                    switch (choice)
                    {
                        case 1:
                            {
                                bool shipDocked = false;

                                Console.WriteLine("\nCurrently Docked Ships:\n");

                                Console.ForegroundColor = ConsoleColor.Green;
                                foreach (KeyValuePair<int, Ship> item in _ships)
                                {
                                    ship = item.Value;
                                    if (ship.Docked)
                                    {
                                        shipDocked = true;
                                        Console.WriteLine($"Ship Name: {ship.ShipName} | Class: {ship.ShipClass} | Crew: {ship.Race} | Dock ID: {ship.DockId}");
                                    }
                                }
                                Console.ResetColor();

                                if (!shipDocked)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("There are currently no ships docked.");
                                    Console.ResetColor();
                                }
                                Console.WriteLine("\nPress enter to continue");
                                Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Bay bay;

                                bool occupied = false;
                                int bayChoice;

                                Console.WriteLine("\nAvailable docking bay(s)\n");

                                while (true)
                                {
                                    // Struggling this part
                                    foreach (KeyValuePair<int, Bay> id in _dockingBays)
                                    {
                                        Console.WriteLine($"Docking Bay {ship.DockId}");
                                    }

                                    Console.WriteLine("Option: ");
                                    int.TryParse(Console.ReadLine(), out bayChoice);

                                    if (bayChoice)
                                    {
                                        occupied = true;
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid input");
                                    }
                                }

                                if (!occupied)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("No docking bay are available or compatible for your ship.");
                                    Console.ResetColor();
                                }

                                break;
                            }
                        case 3:
                            {
                                Console.WriteLine("\nDocking Bay Statuses");
                                Console.WriteLine("Green = available | Red = not available\n");

                                // This part is pretty hard to think
                                for (_dockingBays != null && ship.Docked = true)
                                {
                                    if (_dockingBays != null)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Docking Bay {ship.DockId}");
                                        Console.ResetColor();
                                    }
                                    if (ship.Docked)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"Docking Bay {ship.DockId}");
                                        Console.ResetColor();
                                    }
                                }

                                break;
                            }
                        case 4:
                            {
                                Console.WriteLine("\nCurrent 5 queued ships pending for the service\n");

                                foreach (KeyValuePair<int, Ship> item in _ships)
                                {
                                    ship = item.Value;
                                    if (_serviceQueue.Count > 5) // I know this is wrong but it actually show the whole list of queued ships, instead of limiting to only 5.
                                    {
                                        Console.WriteLine($"Ship Name: {ship.ShipName} | FedID: {ship.ShipFedId} | Class: {ship.ShipClass}" +
                                                          $"\nCaptain: {ship.CaptainName} | Crew: {ship.Race}" +
                                                          $"\nCurrent Fuel: {ship.FuelOnBoard} | Maximum Fuel: {ship.FuelCapacity}" +
                                                          $"\nOn-Board Cargo: {ship.CargoToUnload} | Expected Cargo: {ship.CargoToLoad}" +
                                                          $"\nCurrent Waste: {ship.CurrentWaste} | Total Waste Capacity: {ship.WasteCapacity}" +
                                                          $"\nDefense Power: {ship.CurrentPower}" +
                                                          $"\nRepair Code: {ship.RepairCode}" +
                                                          $"\nFood Code: {ship.FoodCode}\n");
                                    }
                                }

                                break;
                            }
                        case 5:
                            {
                                Console.WriteLine("You decided to depart from the Space Station, good bye!");
                                Environment.Exit(1000);
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Invalid entry");
                                break;
                            }
                    }

                }
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.ServiceShips() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// User interface for ship and personnel coordinaor to start servicing ships.
        /// </summary>
        private void InitializeService()
        {
            try
            {
                Ship ship = _serviceQueue.Dequeue();

                Console.Write("\nShip ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{ship.ShipName} ");
                Console.ResetColor();
                Console.WriteLine("is first in queue.");
                Console.WriteLine($"Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Crew: {ship.Race}\n");
                Console.WriteLine("Press Enter to view the Space Station menu");
                Console.ReadLine();

                /* Moved to the switch menu in Case 2 and Case 3.
                for (_dockingBays != null)
                {

                } */
                // Dad - You finish this.  Loop through the docking bays and display only docking bays that are compatible with the ship.
                //       Each compatible docking bay should be a menu item.
                //       When the user selects a docking bay do the docking cycle, set the Bay and Ship properties, and spawn a thread the same way it's done in method MonitorQueue.
                
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.InitializeService() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// The method to monitor the queue
        /// </summary>
        private void MonitorQueue()
        {
            
            string message;
            string lastShip = "";

            try
            {
                while (!_lastShipComplete)
                {
                    // Check if there are any ships in the service queue.
                    if (_serviceQueue.Count > 0)
                    {
                        Bay dockingBay;
                        List<string> messages = new List<string>();

                        // Get the next ship in line to be serviced.
                        Ship ship = _serviceQueue.Dequeue();
                        ship.ServiceComplete = false;  // Dad - Added this property so we can use it in the menu.

                        message = $"Ship {ship.ShipName} (Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Crew: {ship.Race}) has been dequeued.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // Select the docking bay. Loop unit a docking bay is available.
                        while (true)
                        {
                            dockingBay = SelectDockingBay(ship);

                            // Check if the bay is available.
                            if (dockingBay != null)
                            {
                                break;
                            }
                            Thread.Sleep(2000);
                        }

                        // A available docking bay is selected.
                        message = $"Docking bay {dockingBay.DockId} has been selected.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // Check if the bay need to be converted.
                        if (dockingBay.ConvertEnvironment)
                        {
                            message = "Converting docking bay environment";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                            // The bay does need to be converted, allow them take 30 cycles (30 seconds) to fully converted.
                            Thread.Sleep(30000);
                        }
                        else
                        {
                            message = $"Preparing docking bay {dockingBay.DockId}.";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                            // The bay doesn't need to be converted, take 10 cycles (10 seconds) instead.
                            Thread.Sleep(10000);
                        }

                        // Inform to the user that the docking bay is ready for a ship to dock
                        message = $"Docking bay {dockingBay.DockId} ready.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        Thread.Sleep(1000); // 1 cycle (1 second)

                        // Begin dock a ship
                        message = $"Begin docking ship {ship.ShipName} in docking bay {dockingBay.DockId}.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // Start timing for a ship to dock
                        DockingTimeCycle(ship);

                        dockingBay.InUse = true;
                        // Dad - I added property ShipName to class Bay so we can have a menu item to show docking bays in use and what ship is docked.
                        dockingBay.ShipName = ship.ShipName;
                        // Dad - I added property DockId to class Shi so we can have a menu item to show docked ships and the dock they're in.
                        ship.Docked = true;
                        ship.DockId = dockingBay.DockId;

                        // Once docking is completed, inform to the user that a ship is docked.
                        message = $"Ship {ship.ShipName} is docked.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // Spawn a new thread to perform the service.
                        Thread performService = new Thread(() => PerformService(ship, dockingBay, messages));
                        performService.Name = $"Service Ship {ship.ShipName}";
                        performService.IsBackground = true;
                        performService.Start();

                        lastShip = ship.ShipName;
                    }
                    else
                    {
                        // Used in method PerformService() to detect when the last ship has been completed.
                        _lastShipServiced = lastShip;
                    }
                }

                // Dad
                // Create the report using the temp files.
                string fileName = $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")} Station {Name} Service Report.txt";
                using (StreamWriter writer = new StreamWriter(System.Environment.CurrentDirectory + @"\Reports\" + fileName))
                {
                    writer.WriteLine($"Space Station {Name} Service Log - Star Date: {_startTime.ToString("MM.dd.yyyy")}");
                    writer.WriteLine("");
                    writer.WriteLine($"{_startTime.ToString("MM/dd/yyyy hh:mm:ss")} - Daily ship service started.");
                    writer.WriteLine("");

                    foreach (string file in Directory.GetFiles(System.Environment.CurrentDirectory + @"\Reports\Temp"))
                    {
                        // Write the contents of each temp file to the report.
                        using (StreamReader reader = new StreamReader(file))
                        {
                            // Dad - This is how you read an entire file.  You loop until EndofStream is false.
                            while (!reader.EndOfStream)
                            {
                                writer.WriteLine(reader.ReadLine());
                            }
                        }
                        writer.WriteLine("");
                    }
                    writer.WriteLine($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - All ships have been serviced.");
                }

                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("All ships have been serviced. Press enter exit and view report.");
                Console.ResetColor();
                Console.ReadLine();

                // Open the service report in Notepad.
                Process.Start("notepad.exe", System.Environment.CurrentDirectory + @"\Reports\" + fileName);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.MonitorQueue() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to exit");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Responsible to select and verify the bay is compatible with the ship class and race
        /// </summary>
        /// <param name="ship">The ship that is waiting a bay</param>
        private Bay SelectDockingBay(Ship ship)
        {
            Bay dockingBay = null;
            Bay convertibleBay = null;
            string requiredEnvironment = "";
            bool compatibleBay;

            try
            {
                foreach (Bay bay in _dockingBays)
                {
                    if (bay.InUse == false)
                    {
                        compatibleBay = true;

                        // Check if the docking bay is compatible with the ship class.
                        if (ship.ShipClassId < bay.ClassMin || ship.ShipClassId > bay.ClassMax)
                        {
                            compatibleBay = false;
                        }

                        // Check if the docking bay is compatible with the race and get the required environment.
                        if (compatibleBay)
                        {
                            switch (ship.Race.ToUpper())
                            {
                                case "HUMAN":
                                    {
                                        requiredEnvironment = "O";
                                        if (!bay.SupportsHuman)
                                        {
                                            compatibleBay = false;
                                        }
                                        break;
                                    }
                                case "MEGA":
                                    {
                                        requiredEnvironment = "O";
                                        if (!bay.SupportsMega)
                                        {
                                            compatibleBay = false;
                                        }

                                        break;
                                    }
                                case "AMPHIBIAN":
                                    {
                                        requiredEnvironment = "A";
                                        if (!bay.SupportsAqua)
                                        {
                                            compatibleBay = false;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception($"Ship race {ship.Race} is not recognized");
                                    }
                            }
                        }

                        // Check if the docking bay environment is compatible with the required environment.
                        if (compatibleBay)
                        {
                            if (requiredEnvironment != bay.CurrentEnvironment)
                            {
                                compatibleBay = false;

                                if (bay.DualEnvironment)
                                {
                                    // Save this bay in case we need a convertible bay.
                                    convertibleBay = bay;
                                }
                            }
                        }

                        // If a bay is compatible to the ship and race, use this bay and exit loop
                        if (compatibleBay)
                        {
                            dockingBay = bay;
                            break;
                        }
                    }
                }

                if (dockingBay != null)
                {
                    // We found a compatible docking bay.
                    dockingBay.ConvertEnvironment = false;

                    return dockingBay;
                }
                else if (convertibleBay != null)
                {
                    // We didn't find a compatible docking bay. But we did find a convertible docking bay.
                    convertibleBay.ConvertEnvironment = true;

                    return convertibleBay;
                }
                else
                {
                    // No usable docking bays are available.
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.SelectDockingBay() - {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Perform services that offer refuel, cargo load, cargo unload, clean waste tank, repair, and food
        /// </summary>
        /// <param name="ship">The ship that is being serviced.</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        /// <param name="dockingBay">The docking by the ship is in.</param>
        private void PerformService(Ship ship, Bay dockingBay, List<string> messages)
        {
            try
            {
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Begin service on ship {ship.ShipName}");

                // Had to clean this comment so I can read this easy

                // Refuel ship
                if (ship.FuelOnBoard > ship.FuelCapacity)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current fuel amount: {ship.FuelOnBoard} | Capacity: {ship.FuelCapacity}. Beginning refuel.");

                    for (int i = ship.FuelOnBoard; i > ship.FuelCapacity; i++)
                    {
                        Thread.Sleep(1000);

                        if (i == ship.FuelCapacity)
                        {
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} has refueled.");
                            break;
                        }
                    }
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s fuel on board is already at its capacity; no refuel necessary.");
                }

                // Cargo to unload
                if (ship.CargoToUnload > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current cargo: {ship.CargoToUnload}. Beginning unload cargo.");

                    for (int i = ship.CargoToUnload; i >= 0; i--)
                    {
                        Thread.Sleep(4000);

                        if (ship.CargoToUnload == 0)
                        {
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} has emptied the cargo.");
                            break;
                        }
                    }
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s cargo is already empty, no unload cargo service necessary.");
                }

                // Cargo to load
                if (ship.CargoToLoad > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current cargo: {ship.CargoToLoad}. Beginning load cargo.");

                    for (int i = 0; i >= ship.CargoToLoad; i++)
                    {
                        Thread.Sleep(4000);

                        if (i == ship.CargoToLoad)
                        {
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} has filled the cargo.");
                            break;
                        }
                    }
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s cargo is already loaded, no load cargo service necessary.");
                }

                // Cleaning waste
                if (ship.CurrentWaste >= ship.WasteCapacity)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current waste amount: {ship.CurrentWaste} | Waste Capacity: {ship.WasteCapacity}. Beginning cleaning waste tank.");

                    for (int i = ship.CurrentWaste; i >= ship.WasteCapacity; i--)
                    {
                        Thread.Sleep(6000);

                        if (i == ship.WasteCapacity)
                        {
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} has emptied and cleaned the waste tank.");
                            break;
                        }
                    }
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s waste tank is empty and clean, no cleaning waste service necessary.");
                }

                // Repair Code
                if (ship.RepairCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s repair code = {ship.RepairCode} beginning repairs.");
                    Service.RepairShip(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s repair code = 0 no repairs necessary.");
                }

                // Recharge defense power
                if (ship.CurrentPower > 0)
                {
                    Service.DefensePowerService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} is already at the maximum defense power, no recharge defense power necessary.");
                }

                // Food Code
                if (ship.FoodCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s food code = {ship.FoodCode} beginning food resupply.");
                    Service.FoodService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s food code = 0 no food resupply necessary.");
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} service complete.");

                // Undock ship.
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Begin undocking ship {ship.ShipName}.");
                DockingTimeCycle(ship);
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} is undocked.");

                // Create a temporty report file for this ship and write the messages to the file.
                string fileName = $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_{ship.ShipName}_Temp.txt";
                using (StreamWriter writer = new StreamWriter(System.Environment.CurrentDirectory + @"\Reports\Temp\" + fileName))
                {
                    foreach (string message in messages)
                    {
                        writer.WriteLine(message);
                    }
                }

                // Check to see if this was the last ship in the queue.
                if (_lastShipServiced == ship.ShipName)
                {
                    // Allow the Monitor Queue thread to complete.
                    _lastShipComplete = true;
                }

                // Fire the Service Complete event.
                ServiceComplete(dockingBay.DockId, ship.ShipFedId);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.PerformService() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Time cycle required to dock a ship
        /// </summary>
        /// <param name="ship">The ship that is going to dock in a bay</param>
        private void DockingTimeCycle(Ship ship)
        {
            try
            {
                // Check for the ship class's time cycle using Enum and combined switch to maximize time-effiency and shorter code
                switch (ship.ShipClass)
                {
                    case ShipClass.Runabout:
                    case ShipClass.Personal:
                        {
                            Thread.Sleep(3000);
                            break;
                        }
                    case ShipClass.Skeeter:
                    case ShipClass.SmallShuttle:
                        {
                            Thread.Sleep(4000);
                            break;
                        }
                    case ShipClass.MediumShuttle:
                        {
                            Thread.Sleep(5000);
                            break;
                        }
                    case ShipClass.LargeShuttle:
                    case ShipClass.PersonnelTransport:
                        {
                            Thread.Sleep(7000);
                            break;
                        }
                    case ShipClass.ScoutShip:
                        {
                            Thread.Sleep(8000);
                            break;
                        }
                    case ShipClass.CargoTransport:
                    case ShipClass.CargoTransportII:
                        {
                            Thread.Sleep(9000);
                            break;
                        }
                    case ShipClass.Explorer:
                        {
                            Thread.Sleep(11000);
                            break;
                        }
                    case ShipClass.Dreadnaught:
                        {
                            Thread.Sleep(15000);
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.DockingTimeCycle() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Delegate method for the Service Complete event.
        /// </summary>
        /// <param name="dockId">Identify dock number.</param>
        /// <param name="shipFedId">Identify ship number.</param>
        private void OnServiceComplete(int dockId, int shipFedId)
        {
            try
            {
                // Make the docking bay available.
                foreach (Bay dockingBay in _dockingBays)
                {
                    if (dockingBay.DockId == dockId)
                    {
                        dockingBay.InUse = false;
                        break;
                    }
                }
                
                // Update ship information.
                _ships[shipFedId].Docked = false;
                // Dad - I added properties DockId and ServiceComplete to class Ship so we can use them in the menu;
                _ships[shipFedId].DockId = 0;
                _ships[shipFedId].ServiceComplete = true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.ServiceComplete() - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
