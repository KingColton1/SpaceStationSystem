﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
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

        // Store information into a list and dictionary
        private List<Bay> _dockingBays;
        private Dictionary<int, Ship> _ships;
        
        // Define a delegate for the Service Complete event.
        private delegate void ServiceCompleteEventHandler(int dockId, int shipFedId);

        // Define an event object for the Service Complete event.
        private event ServiceCompleteEventHandler ServiceComplete;

        private DateTime _startTime;

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
                
                // Use the ship information to load the Ships dictionary.
                foreach (Ship ship in ships)
                {
                    _ships.Add(ship.ShipFedId, ship);
                }

                // Use file ShipQueue2019 to load the ships into the service queue.
                using (var reader = new StringReader(Properties.Resources.ShipQueue2019))
                {
                    string line;
                    int FedId;

                    while ((line = reader.ReadLine()) != null)
                    {
                        int.TryParse(line, out FedId);

                        if (FedId > 0)
                        {
                            Ship ship = _ships[FedId];
                            if (ship != null)
                            {
                                _serviceQueue.Enqueue(ship);
                            }
                        }
                    }
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
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// The method of service ships to handle with the service.
        /// </summary>
        public void ServiceShips()
        {
            string message;
            int choice = 0;
            bool mainLoop;
            bool menuLoop;

            try
            {
                _startTime = DateTime.Now;
                Console.WriteLine($"Start docking service for space station {Name}");
                Thread.Sleep(2000);

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
                Console.WriteLine("\rCommander Lawence David Identified.\n");
                Thread.Sleep(1500);
                Console.WriteLine("Press Enter to view the ship queue");
                Console.ReadLine();

                mainLoop = true;
                while (mainLoop)
                {
                    Bay selectedBay = null;
                    List<string> messages = new List<string>();

                    // Get the next ship in line to be serviced.
                    Ship ship = _serviceQueue.Dequeue();
                    ship.ServiceComplete = false;
                    message = $"Ship {ship.ShipName.Trim()} (Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Crew: {ship.Race}) has been dequeued.";
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                    menuLoop = true;

                    while (menuLoop)
                    {
                        Console.Write($"Next ship in the queue is ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{ship.ShipName.Trim()}.");
                        Console.ResetColor();
                        Console.WriteLine($"Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} ({ship.ShipClassId}) | Crew: {ship.Race}");
                        Console.WriteLine("");

                        Console.WriteLine("1. View compatible docking bays" +
                                    "\n2. View convertible docking bays" +
                                    "\n3. View occupied docking bays" +
                                    "\n4. View available docking bays" +
                                    "\n5. View docked ships" +
                                    "\n6. View next five queued ships " +
                                    "\n7. Exit");

                        Console.Write("Option: ");
                        int.TryParse(Console.ReadLine(), out choice);

                        switch (choice)
                        {
                            case 1:
                                {
                                    // View compatible docking bays.
                                    Console.WriteLine("\nCompatible docking bay(s):");

                                    Dictionary<int, string> compatibleBays = new Dictionary<int, string>();
                                    foreach (Bay bay in _dockingBays)
                                    {
                                        if (!bay.InUse)
                                        {
                                            // If bay is compatible with the ship, add menu option.
                                            if (IsBayCompatible(bay, ship))
                                            {
                                                compatibleBays.Add(bay.DockId, $"Dock ID: {bay.DockId} | Environment: {bay.CurrentEnvironment}; Dual? {bay.DualEnvironment} | Human? {bay.SupportsHuman} | Aqua? {bay.SupportsAqua} | Mega? {bay.SupportsMega} | Class Min: {bay.ClassMin} | Class Max: {bay.ClassMax}");
                                            }
                                        }
                                    }

                                    // If a compatible bay is available, allow to select.
                                    if (compatibleBays.Count > 0)
                                    {
                                        selectedBay = SelectDockingBay(compatibleBays);
                                        if (selectedBay != null)
                                        {
                                            // Tell the system the bay's environment does not need to be converted.
                                            selectedBay.ConvertEnvironment = false;

                                            // Exit menu loop.
                                            menuLoop = false;
                                        }

                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("No compatible docking bays are currently available. Please wait until one is available.");
                                        Console.WriteLine("Or a convertible docking bay may be available.");
                                        Console.ResetColor();
                                        Console.WriteLine("\nPress enter to continue");
                                        Console.ReadLine();
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    // View convertible docking bays.
                                    Console.WriteLine("\nConvertible docking bay(s):");

                                    Dictionary<int, string> convertibleBays = new Dictionary<int, string>();
                                    foreach (Bay bay in _dockingBays)
                                    {
                                        if (!bay.InUse)
                                        {
                                            // If bay is compatible with the ship, add menu option.
                                            if (IsBayConvertible(bay, ship))
                                            {
                                                convertibleBays.Add(bay.DockId, $"Dock ID: {bay.DockId} | Environment: {bay.CurrentEnvironment}; Dual? {bay.DualEnvironment} | Human? {bay.SupportsHuman} | Aqua? {bay.SupportsAqua} | Mega? {bay.SupportsMega} | Class Min: {bay.ClassMin} | Class Max: {bay.ClassMax}");
                                            }
                                        }
                                    }

                                    // If a convertible bay is available, allow to select.
                                    if (convertibleBays.Count > 0)
                                    {
                                        selectedBay = SelectDockingBay(convertibleBays);
                                        if (selectedBay != null)
                                        {
                                            // Tell the system to convert the bay's environment.
                                            selectedBay.ConvertEnvironment = true;

                                            // Exit menu loop.
                                            menuLoop = false;
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("No convertibleBays docking bays are currently available. Please wait until one is available.");
                                        Console.WriteLine("Or a compatible docking bay may be available.");
                                        Console.ResetColor();
                                        Console.WriteLine("\nPress enter to continue");
                                        Console.ReadLine();
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    // View occupied docking bays.
                                    bool occupy = false;

                                    Console.WriteLine("\nOccupied docking bays\n");

                                    foreach (Bay bay in _dockingBays)
                                    {
                                        if (bay.InUse == true)
                                        {
                                            occupy = true;
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.Write($"Docking Bay {bay.DockId} | Environment: {bay.CurrentEnvironment}; Dual? {bay.DualEnvironment} | Human? {bay.SupportsHuman} | Aqua? {bay.SupportsAqua} | Mega? {bay.SupportsMega} | Class Min: {bay.ClassMin}; Class Max: {bay.ClassMax} | ");
                                            Console.ResetColor();
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write($"Occupied by {bay.ShipName.Trim()} Ship\n");
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
                                    // View available docking bays.
                                    bool available = false;

                                    Console.WriteLine("\nAvailable docking bays\n");

                                    foreach (Bay bay in _dockingBays)
                                    {
                                        if (bay.InUse == false)
                                        {
                                            available = true;
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"Docking Bay {bay.DockId} | Environment: {bay.CurrentEnvironment}; Dual? {bay.DualEnvironment} | Human? {bay.SupportsHuman} | Aqua? {bay.SupportsAqua} | Mega? {bay.SupportsMega} | Class Min: {bay.ClassMin}; Class Max: {bay.ClassMax} | ");
                                            Console.ResetColor();
                                        }
                                    }

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
                            case 5:
                                {
                                    // View docked ships.
                                    bool shipDocked = false;

                                    Console.WriteLine("\nCurrently Docked Ships:\n");

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    foreach (KeyValuePair<int, Ship> item in _ships)
                                    {
                                        Ship checkShip = item.Value;
                                        if (checkShip.Docked) // If any ship is docked, output message.
                                        {
                                            shipDocked = true;
                                            Console.WriteLine($"Ship Name: {checkShip.ShipName.Trim()} | Class: {checkShip.ShipClass} | Crew: {checkShip.Race} | Dock ID: {checkShip.DockId}");
                                        }
                                    }
                                    Console.ResetColor();

                                    // Ship aren't docked to any bay, output message.
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
                            case 6:
                                {
                                    // View next five queued ships.
                                    Console.WriteLine("\nCurrent 5 queued ships pending for the service\n");

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    for (int i = 0; i <= 4; i++) // Display 5 ships, not 4.
                                    {
                                        if (_serviceQueue.Count > i)
                                        {
                                            // Used ElementAt to get items from the queue without removing them from the queue.
                                            Ship queuedShip = _serviceQueue.ElementAt(i);

                                            Console.WriteLine($"Ship Name: {queuedShip.ShipName.Trim()} | FedID: {queuedShip.ShipFedId} | Class: {queuedShip.ShipClass}" +
                                                                $"\nCaptain: {queuedShip.CaptainName} | Crew: {queuedShip.Race}" +
                                                                $"\nCurrent Fuel: {queuedShip.FuelOnBoard} | Maximum Fuel: {queuedShip.FuelCapacity}" +
                                                                $"\nOn-Board Cargo: {queuedShip.CargoToUnload} | Expected Cargo: {queuedShip.CargoToLoad}" +
                                                                $"\nCurrent Waste: {queuedShip.CurrentWaste} | Total Waste Capacity: {queuedShip.WasteCapacity}" +
                                                                $"\nDefense Power: {queuedShip.CurrentPower}" +
                                                                $"\nRepair Code: {queuedShip.RepairCode}" +
                                                                $"\nFood Code: {queuedShip.FoodCode}\n");
                                        }
                                    }
                                    Console.ResetColor();

                                    Console.WriteLine("\nPress enter to continue");
                                    Console.ReadLine();
                                    break;
                                }
                            case 7:
                                {
                                    Thread.Sleep(1500);
                                    Console.WriteLine("\nShutting down Space Station system, good bye Commander David!");
                                    Thread.Sleep(1500);
                                    Environment.Exit(0);
                                    break;
                                }
                            default:
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Invalid entry");
                                    Console.ResetColor();
                                    Console.WriteLine("Press Enter to continue");
                                    Console.ReadLine();
                                    break;
                                }
                        }
                    }

                    // An available docking bay is selected.
                    message = $"\nDocking bay {selectedBay.DockId} has been selected.";
                    Console.WriteLine($"{message}");
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                    Thread.Sleep(1000);

                    // Check if the bay need to be converted.
                    if (selectedBay.ConvertEnvironment)
                    {
                        message = "Converting docking bay environment";
                        Console.WriteLine(message);
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // The bay does need to be converted, allow them take 30 cycles (30 seconds) to fully converted.
                        Thread.Sleep(30000);
                    }
                    else
                    {
                        message = $"Preparing docking bay {selectedBay.DockId}";
                        Console.WriteLine(message);
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                        // The bay doesn't need to be converted, take 10 cycles (10 seconds) instead.
                        Thread.Sleep(10000);
                    }

                    // Inform to the user that the docking bay is ready for a ship to dock
                    message = $"Docking bay {selectedBay.DockId} ready.";
                    Console.WriteLine($"{message}\n");
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                    Thread.Sleep(1000); // 1 cycle (1 second)

                    // Begin dock a ship
                    message = $"Begin docking ship {ship.ShipName.Trim()} in docking bay {selectedBay.DockId}.";
                    Console.WriteLine(message);
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                    // Start timing for a ship to dock
                    DockingTimeCycle(ship);

                    // Update the bay information.
                    foreach (Bay item in _dockingBays)
                    {
                        if (item.DockId == selectedBay.DockId)
                        {
                            item.InUse = true;
                            item.ShipName = ship.ShipName.Trim();
                        }
                    }

                    // Update the ship information;
                    ship.Docked = true;
                    ship.DockId = selectedBay.DockId;

                    // Once docking is completed, inform to the user that a ship is docked.
                    message = $"Ship {ship.ShipName.Trim()} is docked.";
                    Console.WriteLine($"{message}\n");
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");

                    Thread.Sleep(1500);
                    Console.WriteLine($"Start service on ship {ship.ShipName.Trim()}.\n");
                    Thread.Sleep(1500);

                    // Spawn a new thread to perform the service.
                    Thread performService = new Thread(() => PerformService(ship, selectedBay, messages));
                    performService.Name = $"Service Ship {ship.ShipName.Trim()}";
                    performService.IsBackground = true;
                    performService.Start();

                    if (_serviceQueue.Count == 0)
                    {
                        // All ships have been docked.
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("All ships have been docked. Please wait for the remaining ships to be serviced.");
                        Console.WriteLine("");
                        Console.ResetColor();

                        // Wait for the remaining ships to be serviced.
                        while (true)
                        {
                            int shipCount = 0;
                            foreach (Bay bayCheck in _dockingBays)
                            {
                                if (bayCheck.InUse)
                                {
                                    shipCount += 1;
                                }
                            }

                            Console.Write($"\rShips remaining: {shipCount}");
                            
                            if (shipCount == 0)
                            {
                                mainLoop = false;
                                break;
                            }
                            Thread.Sleep(2000);
                        }
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
                Console.WriteLine("All ships have been serviced. Press enter to exit and view report.");
                Console.ResetColor();
                Console.ReadLine();

                // Open the service report in Notepad.
                Process.Start("notepad.exe", System.Environment.CurrentDirectory + @"\Reports\" + fileName);

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.ServiceShips() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Check if the bay is compatible with the ship.
        /// </summary>
        /// <param name="bay">The bay to check.</param>
        /// <param name="ship">The ship used to check the bay.</param>
        /// <returns>True if the bay is compatible with the ship.</returns>
        private bool IsBayCompatible(Bay bay, Ship ship)
        {
            bool compatibleBay = true;

            try
            {
                // Check if the docking bay is compatible with the ship class.
                if (ship.ShipClassId < bay.ClassMin || ship.ShipClassId > bay.ClassMax)
                {
                    compatibleBay = false;
                }

                // Check if the docking bay is compatible with the race and get the required environment.
                switch (ship.Race.ToUpper())
                {
                    case "HUMAN":
                        {
                            if (!bay.SupportsHuman)
                            {
                                compatibleBay = false;
                            }
                            else if (bay.CurrentEnvironment != "O")
                            {
                                compatibleBay = false;
                            }
                            break;
                        }
                    case "MEGA":
                        {
                            if (!bay.SupportsMega)
                            {
                                compatibleBay = false;
                            }
                            else if (bay.CurrentEnvironment != "O")
                            {
                                compatibleBay = false;
                            }

                            break;
                        }
                    case "AMPHIBIAN":
                        {
                            if (!bay.SupportsAqua)
                            {
                                compatibleBay = false;
                            }
                            else if (bay.CurrentEnvironment != "A")
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

                return compatibleBay;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.IsBayCompatible() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
                return false;
            }
        }

        /// <summary>
        /// Check if the bay is can be converted for the ship.
        /// </summary>
        /// <param name="bay">The bay to check.</param>
        /// <param name="ship">The ship used to check the bay.</param>
        /// <returns>True if the bay can be converted for the ship.</returns>
        private bool IsBayConvertible(Bay bay, Ship ship)
        {
            string shipEnvironment = "";
            bool convertibleBay = true;

            try
            {
                // Only dual environment bays are convertible.
                if (bay.DualEnvironment)
                {
                    // Check if the docking bay is compatible with the ship class.
                    if (ship.ShipClassId < bay.ClassMin || ship.ShipClassId > bay.ClassMax)
                    {
                        convertibleBay = false;
                    }
                    else
                    {
                        // Get the ship's environment
                        switch (ship.Race.ToUpper())
                        {
                            case "HUMAN":
                                {
                                    shipEnvironment = "O";
                                    break;
                                }
                            case "MEGA":
                                {
                                    shipEnvironment = "O";
                                    break;
                                }
                            case "AMPHIBIAN":
                                {
                                    shipEnvironment = "A";
                                    break;
                                }
                            default:
                                {
                                    throw new Exception($"Ship race {ship.Race} is not recognized");
                                }
                        }

                        if (bay.CurrentEnvironment == shipEnvironment)
                        {
                            // Only different environments can be converted.
                            convertibleBay = false;
                        }
                    }
                }
                else
                {
                    convertibleBay = false;
                }

                return convertibleBay;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.IsBayCompatible() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
                return false;
            }
        }

        /// <summary>
        /// Responsible for selecting a docking bay from a list of docking bays.
        /// </summary>
        /// <param name="bays">The docking bays to select from. The key is the DockId.</param>
        /// <returns>The selected docking bay.</returns>
        private Bay SelectDockingBay(Dictionary<int, string> bays)
        {
            Bay selectedBay = null;
            int choice;

            try
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach (KeyValuePair<int, string> item in bays)
                    {
                        Console.WriteLine(item.Value);
                    }
                    Console.ResetColor();

                    Console.Write("\nEnter Dock ID to select a bay: ");
                    int.TryParse(Console.ReadLine(), out choice);

                    if (bays.ContainsKey(choice))
                    {
                        // Get the selected bay from the List of docking bays.
                        foreach (Bay bay in _dockingBays)
                        {
                            if (bay.DockId == choice)
                            {
                                selectedBay = bay;
                                break;
                            }
                        }
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid entry");
                        Console.ResetColor();
                        Console.WriteLine("Press Enter to continue");
                        Console.ReadLine();
                    }
                }

                return selectedBay;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.SelectDockingBay() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
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
            decimal timeCyclesCalc;
            int timeCycles;
            int time;

            try
            {
                Service service = new Service();

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Begin service on ship {ship.ShipName.Trim()}");

                // Refuel ship
                if (ship.FuelOnBoard < ship.FuelCapacity)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s current fuel amount: {ship.FuelOnBoard} | Capacity: {ship.FuelCapacity}. Beginning refuel.");

                    // Find out how much fuel is needed to fill
                    int fuelToLoad = ship.FuelCapacity - ship.FuelOnBoard;

                    // Calculate how many times 50 goes into the amount of fuel needed. Cast int to decimal to keep the remainder.
                    timeCyclesCalc = (decimal)fuelToLoad / 50;

                    // Round up to make sure we get accurate time cycle
                    timeCycles = (int)Math.Ceiling(timeCyclesCalc);

                    // Multiply by 4 to get the number of time cycles
                    timeCycles = timeCycles * 4;

                    // Multiply by 1000 to convert time cycles to seconds
                    time = timeCycles * 1000;

                    // Use the modified time cycle to perform refuel.
                    Thread.Sleep(time);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s fuel on board is already at its capacity; no refuel necessary.");
                }

                // Cargo to unload
                if (ship.CargoToUnload > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s current cargo: {ship.CargoToUnload}. Beginning unload cargo.");

                    // Calculate how many times 20 goes into the amount of cargo need to unload. Cast int to decimal to keep the remainder.
                    timeCyclesCalc = (decimal)ship.CargoToUnload / 20;

                    // Round up to make sure we get accurate time cycle
                    timeCycles = (int)Math.Ceiling(timeCyclesCalc);

                    // Multiply by 4 to get the number of time cycles
                    timeCycles = timeCycles * 4;

                    // Multiply by 1000 to convert time cycles to seconds
                    time = timeCycles * 1000;

                    // Use the modified time cycle to perform unload cargo
                    Thread.Sleep(time);

                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} has emptied the cargo.");
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s cargo is already empty, no unload cargo service necessary.");
                }

                // Cargo to load
                if (ship.CargoToLoad > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s current cargo: {ship.CargoToLoad}. Beginning load cargo.");

                    // Calculate how many times 30 goes into the amount of cargo need to load. Cast int to decimal to keep the remainder.
                    timeCyclesCalc = (decimal)ship.CargoToLoad / 20;

                    // Round up to make sure we get accurate time cycle
                    timeCycles = (int)Math.Ceiling(timeCyclesCalc);

                    // Multiply by 4 to get the number of time cycles
                    timeCycles = timeCycles * 4;

                    // Multiply by 1000 to convert time cycles to seconds
                    time = timeCycles * 1000;

                    // Use the modified time cycle to perform load cargo
                    Thread.Sleep(time);

                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} has filled the cargo.");
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s cargo is already loaded, no load cargo service necessary.");
                }

                // Cleaning waste
                if (ship.CurrentWaste <= ship.WasteCapacity)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s current waste amount: {ship.CurrentWaste} | Waste Capacity: {ship.WasteCapacity}. Beginning cleaning waste tank.");

                    // Calculate how many times 30 goes into the amount of waste to clean. Cast int to decimal to keep the remainder.
                    timeCyclesCalc = (decimal)ship.CurrentWaste / 30;

                    // Round up to make sure we get accurate time cycle
                    timeCycles = (int)Math.Ceiling(timeCyclesCalc);

                    // Multiply by 6 to get the number of time cycles
                    timeCycles = timeCycles * 6;

                    // Multiply by 1000 to convert time cycles to seconds
                    time = timeCycles * 1000;

                    // Use the modified time cycle to clean up waste tank
                    Thread.Sleep(time);

                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} has emptied and cleaned the waste tank.");
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s waste tank is empty and clean, no cleaning waste service necessary.");
                }

                // Repair Code
                if (ship.RepairCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s repair code = {ship.RepairCode} beginning repairs.");
                    service.RepairShip(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s repair code = 0 no repairs necessary.");
                }

                // Recharge defense power
                if (ship.CurrentPower > 0)
                {
                    service.DefensePowerService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} is already at the maximum defense power, no recharge defense power necessary.");
                }

                // Food Code
                if (ship.FoodCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s food code = {ship.FoodCode} beginning food resupply.");
                    service.FoodSupply(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()}'s food code = 0 no food resupply necessary.");
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} service complete.");

                // Undock ship.
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Begin undocking ship {ship.ShipName.Trim()}.");
                DockingTimeCycle(ship);
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName.Trim()} is undocked.");

                // Create a temporty report file for this ship and write the messages to the file.
                string fileName = $"{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_{ship.ShipName.Trim()}_Temp.txt";
                using (StreamWriter writer = new StreamWriter(System.Environment.CurrentDirectory + @"\Reports\Temp\" + fileName))
                {
                    foreach (string message in messages)
                    {
                        writer.WriteLine(message);
                    }
                }

                // Fire the Service Complete event.
                ServiceComplete(dockingBay.DockId, ship.ShipFedId);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.PerformService() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
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
            string shipName = "";
            int shipDockId = 0;

            try
            {
                // Make the docking bay available.
                foreach (Bay dockingBay in _dockingBays)
                {
                    if (dockingBay.DockId == dockId)
                    {
                        shipName = dockingBay.ShipName;
                        shipDockId = dockingBay.DockId;
                        dockingBay.InUse = false;
                        dockingBay.ShipName = "";
                        break;
                    }
                }
                
                // Update ship information.
                _ships[shipFedId].Docked = false;
                _ships[shipFedId].DockId = 0;
                _ships[shipFedId].ServiceComplete = true;

                // Display the message box when the service is completed
                MessageBox.Show($"{shipName.Trim()} has completed service; left Bay {shipDockId} from the Space Station.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.ServiceComplete() - {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
        }
    }
}
