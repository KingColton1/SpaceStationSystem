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
    public class SpaceStation
    {
        // Create a queue for the ship to wait for their turn
        private Queue<Ship> _serviceQueue = new Queue<Ship>();

        // Establish thread to allow the program to operate the service for the ship on its own
        private Thread _monitorQueue;

        // Store information into a list and dictionary
        private List<Bay> _dockingBays;
        private Dictionary<int, Ship> _ships;
        
        // Define a delegate for the Service Complete event.
        private delegate void ServiceCompleteEventHandler(int dockId, int shipFedId);

        // Define an event object for the Service Complete event.
        private event ServiceCompleteEventHandler ServiceComplete;

        private DateTime _startTime;
        private string _lastShipServiced = "";
        private int _initialDockingBayID;
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

                // Display the UI for the ship coordinator to start service for ship
                InitializeService();

                // Spawn a thread that will monitor the ship queue.
                _monitorQueue = new Thread(new ThreadStart(MonitorQueue)); // Here the start method is MonitorQueue().
                _monitorQueue.Name = "Monitor Queue";
                _monitorQueue.IsBackground = true;
                _monitorQueue.Start();

                Thread.Sleep(1500);
                Console.WriteLine("Ship service procedures have started.  Press enter to view menu.");
                Console.ReadLine();

                while (true)
                {                     
                    Console.WriteLine("\n1. View docked ships" +
                                      "\n2. View available docking bays" +
                                      "\n3. View occupied docking bays" +
                                      "\n4. View next four queued ships" +
                                      "\n6. Exit");

                    Console.Write("Option: ");
                    int.TryParse(Console.ReadLine(), out choice);

                    switch (choice)
                    {
                        case 1:
                            {
                                // View docked ships

                                bool shipDocked = false;

                                Console.WriteLine("\nCurrently Docked Ships:\n");

                                Console.ForegroundColor = ConsoleColor.Green;
                                foreach (KeyValuePair<int, Ship> item in _ships)
                                {
                                    ship = item.Value;
                                    if (ship.Docked) // If any ship is docked, output message
                                    {
                                        shipDocked = true;
                                        Console.WriteLine($"Ship Name: {ship.ShipName} | Class: {ship.ShipClass} | Crew: {ship.Race} | Dock ID: {ship.DockId}");
                                    }
                                }
                                Console.ResetColor();

                                // Ship aren't docked to any bay, output message
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
                                // View avaliable docking bays

                                bool available = true;

                                Console.WriteLine("\nAvailable docking bay(s)\n");

                                Console.ForegroundColor = ConsoleColor.Green;
                                foreach (Bay bay in _dockingBays)
                                {
                                    // If bay is not occupied by a ship, output message
                                    if (!bay.InUse)
                                    {
                                        available = true;
                                        Console.WriteLine($"Docking Bay {bay.DockId} | Environment: {bay.CurrentEnvironment}; Dual? {bay.DualEnvironment} | Human? {bay.SupportsHuman} | Aqua? {bay.SupportsAqua} | Mega? {bay.SupportsMega} | Class Min: {bay.ClassMin} | Class Max: {bay.ClassMax}");
                                    }
                                }
                                Console.ResetColor();
                                
                                // If a bay is occupied, output message
                                if (!available)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("No docking bays are currently available.");
                                    Console.ResetColor();
                                }

                                Console.WriteLine("\nPress enter to continue");
                                Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                // View occupied docking bays.

                                bool occupy = true;

                                Console.WriteLine("\nOccupied docking bays\n");

                                foreach (Bay bay in _dockingBays)
                                {
                                    if (bay.InUse == true)
                                    {
                                        occupy = true;
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write($"Docking Bay {bay.DockId} | Environment: {bay.CurrentEnvironment} | Class Min: {bay.ClassMin}; Class Max: {bay.ClassMax} | ");
                                        Console.ResetColor();
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write($"Occupied by {bay.ShipName} Ship\n");
                                        Console.ResetColor();
                                    }
                                }

                                if (!occupy)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("No docking bays are occupied currently.");
                                    Console.ResetColor();
                                }

                                Console.WriteLine("\nPress enter to continue");
                                Console.ReadLine();
                                break;
                            }
                        case 4:
                            {
                                // View next four queued ships

                                Console.WriteLine("\nCurrent 5 queued ships pending for the service\n");

                                // Dad - This output should probably be green to just to be consistant.
                                for (int i = 0; i < 5; i++)
                                {
                                    // Dad - Use .ElementAt() to get items from the queue without removing them from the queue.
                                    ship = _serviceQueue.ElementAt(i);

                                    Console.WriteLine($"Ship Name: {ship.ShipName} | FedID: {ship.ShipFedId} | Class: {ship.ShipClass}" +
                                                        $"\nCaptain: {ship.CaptainName} | Crew: {ship.Race}" +
                                                        $"\nCurrent Fuel: {ship.FuelOnBoard} | Maximum Fuel: {ship.FuelCapacity}" +
                                                        $"\nOn-Board Cargo: {ship.CargoToUnload} | Expected Cargo: {ship.CargoToLoad}" +
                                                        $"\nCurrent Waste: {ship.CurrentWaste} | Total Waste Capacity: {ship.WasteCapacity}" +
                                                        $"\nDefense Power: {ship.CurrentPower}" +
                                                        $"\nRepair Code: {ship.RepairCode}" +
                                                        $"\nFood Code: {ship.FoodCode}\n");
                                }

                                Console.WriteLine("\nPress enter to continue");
                                Console.ReadLine();
                                break;
                            }
                        case 5:
                            {
                                Thread.Sleep(1500);
                                Console.WriteLine("\nShutting down Space Station system, good bye Commander David!");
                                Thread.Sleep(1500);
                                Environment.Exit(0);
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
            Dictionary<int, int> compatibleBays = new Dictionary<int, int>();
            int option;
            int choice;

            try
            {
                Console.WriteLine("\nVerifying command and control credentials.");
                Thread.Sleep(1000);
                Console.Write("Please wait");
                Thread.Sleep(1500);
                Console.Write(".");
                Thread.Sleep(1500);
                Console.Write(".");
                Thread.Sleep(1500);
                Console.Write(".");
                Thread.Sleep(2000);
                Console.WriteLine("\rCommander Lawence David Identified.\n"); // Replace "Please wait" with new message
                Thread.Sleep(1500);
                Console.WriteLine("Press Enter to view the first ship in the queue");
                Console.ReadLine();

                // It's a ship's turn! Yay!
                Ship ship = _serviceQueue.Dequeue();

                Console.Write("Ship ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{ship.ShipName} ");
                Console.ResetColor();
                Console.WriteLine("is first in queue.");
                Console.WriteLine($"Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Crew: {ship.Race}\n");
                Thread.Sleep(1500);
                Console.WriteLine("Press Enter to view compatible docking bays");
                Console.ReadLine();

                option = 0;

                foreach (Bay bay in _dockingBays)
                {
                    bool compatible = true;

                    // Check if the docking bay is compatible with the ship class.
                    if (ship.ShipClassId < bay.ClassMin || ship.ShipClassId > bay.ClassMax)
                    {
                        compatible = false;
                    }

                    switch (ship.Race.ToUpper())
                    {
                        case "HUMAN":
                            {
                                if (!bay.SupportsHuman)
                                {
                                    compatible = false;
                                }
                                break;
                            }
                        case "MEGA":
                            {
                                if (!bay.SupportsMega)
                                {
                                    compatible = false;
                                }
                                break;
                            }
                        case "AMPHIBIAN":
                            {
                                if (!bay.SupportsAqua)
                                {
                                    compatible = false;
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception($"Ship race {ship.Race} is not recognized");
                            }
                    }

                    if (compatible)
                    {
                        option += 1;
                        Console.WriteLine($"Bay {option}. Dock ID: {bay.DockId} | Environment: {bay.CurrentEnvironment} | Class Min: {bay.ClassMin} | Class Max: {bay.ClassMax}");
                        compatibleBays.Add(option, bay.DockId);
                    }
                }

                while (true)
                {
                    Console.WriteLine("Enter Bay number.");
                    int.TryParse(Console.ReadLine(), out choice);

                    if (choice > 0 && choice <= option)
                    {
                        // Dad - This is how method MonitorQueue knows which docking bay to use for the first ship.
                        //       Variable _initialDockingBay holds the DockId of the dock that the user selected.
                        //       Notice how Dictionary compatibleBays is populated in the loop above.
                        _initialDockingBayID = compatibleBays[choice];

                        Thread.Sleep(1000);
                        Console.WriteLine($"\nDocking bay {compatibleBays[choice]} ready.  Press enter to begin ship service procedures.");
                        Console.ReadLine();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Entry");
                    }
                }


                
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
                        Ship ship = _serviceQueue.Dequeue(); // I suspect this caused to skip first ship and let second ship take over. Because this part is already used in InitializeService.
                        ship.ServiceComplete = false;

                        message = $"Ship {ship.ShipName} (Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Crew: {ship.Race}) has been dequeued.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        if (_initialDockingBayID > 0)
                        {
                            dockingBay = null;

                            // This is the first ship in the queue. Use Commander David's bay selection;
                            foreach (Bay bay in _dockingBays)
                            {
                                if (bay.DockId == _initialDockingBayID)
                                {
                                    dockingBay = bay;
                                }
                            }

                            _initialDockingBayID = 0;
                        }
                        else
                        {
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
                        dockingBay.ShipName = ship.ShipName;
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
                Service service = new Service();

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Begin service on ship {ship.ShipName}");

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
                    service.RepairShip(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s repair code = 0 no repairs necessary.");
                }

                // Recharge defense power
                if (ship.CurrentPower > 0)
                {
                    service.DefensePowerService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} is already at the maximum defense power, no recharge defense power necessary.");
                }

                // Food Code
                if (ship.FoodCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s food code = {ship.FoodCode} beginning food resupply.");
                    service.FoodSupply(ship, ref messages);
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
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n{dockingBay.ShipName} has completed service, left Dock Bay {dockingBay.DockId}");
                        Console.ResetColor();
                        dockingBay.InUse = false;
                        break;
                    }
                }
                
                // Update ship information.
                _ships[shipFedId].Docked = false;
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
