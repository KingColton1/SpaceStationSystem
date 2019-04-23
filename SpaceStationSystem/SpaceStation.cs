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
        //       The items it defines are the method's return type (here it's void) and the method's parameters (here there is one parameter named dockId).
        // Define a delegate for the Service Complete event.
        private delegate void ServiceCompleteEventHandler(int dockId);

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
            try
            {
                _startTime = DateTime.Now;
                Console.WriteLine($"Start docking service for space station {Name}");
                Thread.Sleep(2000);

                // Spawn a thread that will monitor the ship queue.
                _monitorQueue = new Thread(new ThreadStart(MonitorQueue)); // Here the start method is MonitorQueue().
                _monitorQueue.Name = "Monitor Queue";
                _monitorQueue.IsBackground = true;
                _monitorQueue.Start();

                // Wait until the thread is complete.
                _monitorQueue.Join();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.ServiceShips() - {ex.Message}");
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
                        // Dad - I moved the declarations for dockingBay, messages, and ship to here to solve a bug with multi-threading.
                        //       These need to be declared inside the loop so new objects are created for every thread.
                        //       Before the threads were using the same objects and changing things in the other threads.
                        //       This is a good lesson in multi-threading.  Do not share objects between threads.
                        Bay dockingBay;
                        List<string> messages = new List<string>();

                        // Get the next ship in line to be serviced.
                        Ship ship = _serviceQueue.Dequeue();

                        message = $"Ship {ship.ShipName} (Federation ID: {ship.ShipFedId} | Class: {ship.ShipClass} | Race: {ship.Race}) has been dequeued.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                        Console.WriteLine("");
                        Console.WriteLine(message);

                        // Select the docking bay. Loop unit a docking bay is available.
                        while (true)
                        {
                            dockingBay = SelectDockingBay(ship);

                            // Dad - Did you change this to use InUse? This is causing an error when dockingBay is null.
                            //       This needs to check if dockingBay is null or not. If not null, it found a compatible docking bay.

                            // Colton -- This is for to check if a bay is available or currently being used by another ship, we did change this to InUse to do that.
                            
                            //if (dockingBay.InUse == false)
                            if (dockingBay != null)
                            {
                                break;
                            }
                            Thread.Sleep(2000);
                        }

                        // A available docking bay is selected
                        message = $"Docking bay {dockingBay.DockId} has been selected.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                        Console.WriteLine(message);

                        // Check if the bay need to be converted.
                        if (dockingBay.ConvertEnvironment)
                        {
                            message = "Converting docking bay environment";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                            Console.WriteLine(message);

                            // The bay does need to be converted, allow them take 30 cycles (30 seconds) to fully converted.
                            Thread.Sleep(30000);
                        }
                        else
                        {
                            message = $"Preparing docking bay {dockingBay.DockId}.";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                            Console.WriteLine(message);

                            // The bay doesn't need to be converted, take 10 cycles (10 seconds) instead.
                            Thread.Sleep(10000);
                        }

                        // Inform to the user that the docking bay is ready for a ship to dock
                        message = $"Docking bay {dockingBay.DockId} ready.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                        Console.WriteLine(message);

                        Thread.Sleep(1000); // 1 cycle (1 second)

                        // Begin dock a ship
                        message = $"Begin docking ship {ship.ShipName} in docking bay {dockingBay.DockId}.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                        Console.WriteLine(message);

                        // Start timing for a ship to dock
                        DockingTimeCycle(ship);

                        dockingBay.InUse = true;

                        // Once docking is completed, inform to the user that a ship is docked
                        message = $"Ship {ship.ShipName} is docked.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - {message}");
                        Console.WriteLine(message);

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
                Console.WriteLine($"Begin service on ship {ship.ShipName}");

                // Had to clean this comment so I can read this easy

                // Refuel ship
                if (ship.FuelOnBoard > ship.FuelCapacity)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current fuel amount: {ship.FuelOnBoard} | Capacity: {ship.FuelCapacity}. Begnning refuel.");

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
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current cargo: {ship.CargoToUnload}. Begnning unload cargo.");

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
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current cargo: {ship.CargoToLoad}. Begnning load cargo.");

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
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current waste amount: {ship.CurrentWaste} | Waste Capacity: {ship.WasteCapacity}. Begnning cleaning waste tank.");

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
                    RepairShip(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s repair code = 0 no repairs necessary.");
                }

                // Recharge defense power
                if (ship.CurrentPower > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current defense power: {ship.CurrentPower} | Max Defense Power: . Begnning recharge defense power.");
                    DefensePowerService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} is already at the maximum defense power, no recharge defense power necessary.");
                }

                // Food Code
                if (ship.FoodCode > 0)
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s food code = {ship.FoodCode} beginning food resupply.");
                    FoodService(ship, ref messages);
                }
                else
                {
                    messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s food code = 0 no food resupply necessary.");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Ship {ship.ShipName} service complete.");
                Console.ResetColor();
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName} service complete.");

                // Dad
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

                // Dad
                // Fire the Service Complete event.
                ServiceComplete(dockingBay.DockId);
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
        /// Repair service for a ship based on their Repair Code
        /// </summary>
        /// <param name="ship">The ship that is being serviced</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        private void RepairShip(Ship ship, ref List<string> messages)
        {
            string message;

            try
            {
                // Repair Code 1
                if (ship.RepairCode == 1)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (4 Cycles)");
                                Thread.Sleep(4000);
                                break;
                            }
                        case ShipClass.Personal:
                        case ShipClass.SmallShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (8 Cycles)");
                                Thread.Sleep(8000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (15 Cycles)");
                                Thread.Sleep(15000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized.  Repair aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Repair Code 2
                if (ship.RepairCode == 2)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Personal:
                        case ShipClass.SmallShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (6 Cycles)");
                                Thread.Sleep(6000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (13 Cycles)");
                                Thread.Sleep(13000);
                                break;
                            }
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (18 Cycles)");
                                Thread.Sleep(18000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized.  Repair aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Repair Code 3
                if (ship.RepairCode == 3)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (6 Cycles)");
                                Thread.Sleep(6000);
                                break;
                            }
                        case ShipClass.Personal:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.SmallShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (13 Cycles)");
                                Thread.Sleep(13000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (15 Cycles)");
                                Thread.Sleep(15000);
                                break;
                            }
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (17 Cycles)");
                                Thread.Sleep(17000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (19 Cycles)");
                                Thread.Sleep(19000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (21 Cycles)");
                                Thread.Sleep(21000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized.  Repair aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Repair Code 4
                if (ship.RepairCode == 4)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                        case ShipClass.SmallShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.Personal:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (8 Cycles)");
                                Thread.Sleep(8000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (17 Cycles)");
                                Thread.Sleep(17000);
                                break;
                            }
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (18 Cycles)");
                                Thread.Sleep(18000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (20 Cycles)");
                                Thread.Sleep(20000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (24 Cycles)");
                                Thread.Sleep(24000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized.  Repair aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Repair Code 5
                if (ship.RepairCode == 5)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.Personal:
                        case ShipClass.Skeeter:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.SmallShuttle:
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (16 Cycles)");
                                Thread.Sleep(16000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (19 Cycles)");
                                Thread.Sleep(19000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... (30 Cycles)");
                                Thread.Sleep(30000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized.  Repair aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairs complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.RepairShip() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Food service to resupply food into the ship based on the ship's Food Code
        /// </summary>
        /// <param name="ship">The ship that is being serviced</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public void FoodService(Ship ship, ref List<string> messages)
        {
            string message;

            try
            {
                // Food Code 1
                if (ship.FoodCode == 1)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (4 Cycles)");
                                Thread.Sleep(4000);
                                break;
                            }
                        case ShipClass.Personal:
                        case ShipClass.SmallShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (8 Cycles)");
                                Thread.Sleep(8000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (15 Cycles)");
                                Thread.Sleep(15000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Food Service aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Food Code 2
                if (ship.FoodCode == 2)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Personal:
                        case ShipClass.SmallShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (6 Cycles)");
                                Thread.Sleep(6000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (13 Cycles)");
                                Thread.Sleep(13000);
                                break;
                            }
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (18 Cycles)");
                                Thread.Sleep(18000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Food Service aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Food Code 3
                if (ship.FoodCode == 3)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (6 Cycles)");
                                Thread.Sleep(6000);
                                break;
                            }
                        case ShipClass.Personal:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (5 Cycles)");
                                Thread.Sleep(5000);
                                break;
                            }
                        case ShipClass.SmallShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (7 Cycles)");
                                Thread.Sleep(7000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (13 Cycles)");
                                Thread.Sleep(13000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (15 Cycles)");
                                Thread.Sleep(15000);
                                break;
                            }
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (17 Cycles)");
                                Thread.Sleep(17000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (19 Cycles)");
                                Thread.Sleep(19000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (21 Cycles)");
                                Thread.Sleep(21000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Food Service aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Food Code 4
                if (ship.FoodCode == 4)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                        case ShipClass.Skeeter:
                        case ShipClass.SmallShuttle:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.Personal:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (8 Cycles)");
                                Thread.Sleep(8000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (17 Cycles)");
                                Thread.Sleep(17000);
                                break;
                            }
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (18 Cycles)");
                                Thread.Sleep(18000);
                                break;
                            }
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (20 Cycles)");
                                Thread.Sleep(20000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (24 Cycles)");
                                Thread.Sleep(24000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Food Service aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                // Food Code 5
                if (ship.FoodCode == 5)
                {
                    switch (ship.ShipClass)
                    {
                        case ShipClass.Runabout:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (10 Cycles)");
                                Thread.Sleep(10000);
                                break;
                            }
                        case ShipClass.Personal:
                        case ShipClass.Skeeter:
                        case ShipClass.ScoutShip:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (9 Cycles)");
                                Thread.Sleep(9000);
                                break;
                            }
                        case ShipClass.SmallShuttle:
                        case ShipClass.Explorer:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (11 Cycles)");
                                Thread.Sleep(11000);
                                break;
                            }
                        case ShipClass.MediumShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (12 Cycles)");
                                Thread.Sleep(12000);
                                break;
                            }
                        case ShipClass.LargeShuttle:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (14 Cycles)");
                                Thread.Sleep(14000);
                                break;
                            }
                        case ShipClass.PersonnelTransport:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (16 Cycles)");
                                Thread.Sleep(16000);
                                break;
                            }
                        case ShipClass.CargoTransport:
                        case ShipClass.CargoTransportII:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (19 Cycles)");
                                Thread.Sleep(19000);
                                break;
                            }
                        case ShipClass.Dreadnaught:
                            {
                                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... (30 Cycles)");
                                Thread.Sleep(30000);
                                break;
                            }
                        default:
                            {
                                message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Food Service aborted!";
                                Console.WriteLine(message);
                                messages.Add(message);
                                break;
                            }
                    }
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Food Service complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.FoodService() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Recharge the ship's defense powers
        /// </summary>
        /// <param name="ship">The ship that is being serviced</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public void DefensePowerService(Ship ship, List<string> messages)
        {
            string message;
            int defensePower;

            try
            {
                switch (ship.ShipClass)
                {
                    case ShipClass.Runabout:
                        {
                            // It need some sort of math to figure out 5 cycles for each 100 units
                            defensePower = 100;

                            Thread.Sleep(5000);
                            break;
                        }
                    case ShipClass.Personal:
                        {
                            defensePower = 150;
                            break;
                        }
                    case ShipClass.Skeeter:
                        {
                            defensePower = 200;
                            break;
                        }
                    case ShipClass.SmallShuttle:
                        {
                            defensePower = 250;
                            break;
                        }
                    case ShipClass.MediumShuttle:
                        {
                            defensePower = 500;
                            break;
                        }
                    case ShipClass.LargeShuttle:
                        {
                            defensePower = 750;
                            break;
                        }
                    case ShipClass.PersonnelTransport:
                    case ShipClass.CargoTransport:
                        {
                            defensePower = 1200;
                            break;
                        }
                    case ShipClass.CargoTransportII:
                        {
                            defensePower = 1400;
                            break;
                        }
                    case ShipClass.ScoutShip:
                        {
                            defensePower = 2000;
                            break;
                        }
                    case ShipClass.Explorer:
                        {
                            defensePower = 5000;
                            break;
                        }
                    case ShipClass.Dreadnaught:
                        {
                            defensePower = 12000;
                            break;
                        }
                    default:
                        {
                            message = $"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - ERROR: Ship class {ship.ShipClass} was not recognized. Defense Power Service aborted!";
                            Console.WriteLine(message);
                            messages.Add(message);
                            break;
                        }
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Defense Power Service complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.DefensePowerService() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Delegate method for the Service Complete event
        /// </summary>
        /// <param name="dockId">Identify dock number</param>
        private void OnServiceComplete(int dockId)
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
