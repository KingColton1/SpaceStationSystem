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

                // Dad - Make a folder name Reports in the folder that the program is running in.
                //       Use System.Environment.CurrentDirectory and append Reports to that path.
                //       Then make a folder inside Reports named Temp.  That is where we'll create the files in method PerformService()
                //       Use an if statement to check if the folders exist before creating them. Directory.Exists(System.Environment.CurrentDirectory + @"\Reports")
                if (Directory.Exists(System.Environment.CurrentDirectory + @"\Reports\Temp"))
                {
                    File.AppendText("Temp.txt");
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
                Console.WriteLine($"Start docking service for space station {Name}");
                Console.WriteLine("");
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
            Ship ship;
            Bay dockingBay;
            // TimeCycles timing;
            List<string> messages;
            string message;
            string lastShip = "";
            int shipIndex;

            try
            {
                messages = new List<string>();
                shipIndex = 1;  // Dad - I added this variable to use in the filename for the files created in PerformService().

                while (!_lastShipComplete)
                {

                    // Check if there are any ships in the service queue.
                    if (_serviceQueue.Count > 0)
                    {
                        // Get the next ship in line to be serviced.
                        ship = _serviceQueue.Dequeue();

                        message = $"\nShip {ship.ShipName} (Federation ID {ship.ShipFedId} | Class {ship.ShipClassId}) has been dequeued";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                        Console.WriteLine(message);

                        // Select the docking bay. Loop unit a docking bay is available.
                        while (true)
                        {
                            dockingBay = SelectDockingBay(ship);

                            if (dockingBay.InUse == false)
                            {
                                break;
                            }
                            Thread.Sleep(2000);
                        }

                        // A available docking bay is selected
                        message = $"Docking bay {dockingBay.DockId} has been selected";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                        Console.WriteLine(message);

                        // Check if the bay need to be converted.
                        if (dockingBay.ConvertEnvironment)
                        {
                            message = "Converting docking bay environment";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                            Console.WriteLine(message);

                            // The bay does need to be converted, allow them take 30 cycles (30 seconds) to fully converted.
                            Thread.Sleep(30000);
                        }
                        else
                        {
                            message = "Preparing docking bay";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                            Console.WriteLine(message);

                            // The bay doesn't need to be converted, take 10 cycles instead.
                            Thread.Sleep(10000);
                        }

                        // Inform to the user that the docking bay is ready for a ship to dock
                        message = $"Docking bay ready";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                        Console.WriteLine(message);

                        Thread.Sleep(1000); // 1 cycle (1 second)

                        // Begin dock a ship
                        message = $"Begin docking ship {ship.ShipName} (Class {ship.ShipClassId}) in docking bay {dockingBay.DockId}";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                        Console.WriteLine(message);

                        // Start timing for a ship to dock
                        DockingTimeCycle(ship);

                        dockingBay.InUse = true;

                        // Once docking is completed, inform to the user that a ship is docked
                        message = $"Ship {ship.ShipName} (Class {ship.ShipClassId}) is docked.";
                        messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                        Console.WriteLine(message);

                        // Spawn a new thread to perform the service.
                        Thread performService = new Thread(() => PerformService(ship, messages, shipIndex));
                        performService.Name = $"Service Ship {ship.ShipName}";
                        performService.IsBackground = true;
                        performService.Start();

                        lastShip = ship.ShipName;
                        shipIndex += 1;
                    }
                    else
                    {
                        // Used in method PerformService() to detect when the last ship has been completed.
                        _lastShipServiced = lastShip;
                    }

                    // Slow down the loop so the new threads don't get mixed up.
                    Thread.Sleep(2000);
                }

                Console.WriteLine("All ships have been serviced. Press enter exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.MonitorQueue() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Perform services that offer refuel, cargo load, cargo unload, clean waste tank, repair, and food
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="messages"></param>
        /// <param name="shipIndex"></param>
        private void PerformService(Ship ship, List<string> messages, int shipIndex)
        {
            try
            {
                string fileName = shipIndex.ToString("000") + "Temp.txt";
                // Dad - Add a using block here with StreamWriter and put everything in method inside it.
                //       Make StreamWriter create a file in the Temp folder you created in the constructor.
                //       Name the new file starting with shipIndex with two leading zeros (e.g., fileName = shipIndex.ToString("000") + "Temp.txt"
                //       Loop through variable messages and write each message to a line in the text file.
                //       Then for every task in this method write out to the console and the file.
                //       Go ahead and code more tasks in this method.
                using (StreamWriter writer = new StreamWriter(@"\Reports\Temp\Temp.txt"))
                {
                    //foreach (writer = File.AppendText(fileName))
                    //{

                        Console.WriteLine($"Begin service on ship {ship.ShipName} (Federation ID {ship.ShipFedId} | Class {ship.ShipClassId}), requesting Repair Code {ship.RepairCode}");
                        writer.Write(fileName);

                        // Perform repairs based on the ship's classes and Repair Code using combined switch

                        // The Load Food Code have exact same time cycles as Repair Code, but I don't want to make it incredibly long code for computer to read.
                        // Should I add '&&' in the if statement, e.g. if (ship.RepairCode == 1 && ship.FoodCode == 1)?

                        // Repair Code 1
                        if (ship.RepairCode == 1)
                        {
                            switch (ship.ShipClassId)
                            {
                                case 1:
                                case 3:
                                    {
                                        Console.WriteLine("Repairing... (4 Cycles)"); // Added these WriteLine to record them into the file.
                                        writer.Write(fileName);
                                        Thread.Sleep(4000);
                                        break;
                                    }
                                case 2:
                                case 4:
                                    {
                                        Console.WriteLine("Repairing... (5 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(5000);
                                        break;
                                    }
                                case 5:
                                case 10:
                                    {
                                        Console.WriteLine("Repairing... (7 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(7000);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("Repairing... (8 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(8000);
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.WriteLine("Repairing... (9 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(9000);
                                        break;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Console.WriteLine("Repairing... (11 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(11000);
                                        break;
                                    }
                                case 11:
                                    {
                                        Console.WriteLine("Repairing... (12 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(12000);
                                        break;
                                    }
                                case 12:
                                    {
                                        Console.WriteLine("Repairing... (15 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(15000);
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("No repair is needed for this ship.");
                                        writer.Write(fileName);
                                        break;
                                    }
                            }
                        }

                        // Repair Code 2
                        if (ship.RepairCode == 2)
                        {
                            switch (ship.ShipClassId)
                            {
                                case 1:
                                case 2:
                                case 4:
                                    {
                                        Console.WriteLine("Repairing... (5 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(5000);
                                        break;
                                    }
                                case 3:
                                    {
                                        Console.WriteLine("Repairing... (6 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(6000);
                                        break;
                                    }
                                case 5:
                                    {
                                        Console.WriteLine("Repairing... (9 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(9000);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("Repairing... (10 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(10000);
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.WriteLine("Repairing... (11 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(11000);
                                        break;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Console.WriteLine("Repairing... (13 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(13000);
                                        break;
                                    }
                                case 10:
                                    {
                                        Console.WriteLine("Repairing... (7 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(7000);
                                        break;
                                    }
                                case 11:
                                    {
                                        Console.WriteLine("Repairing... (14 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(14000);
                                        break;
                                    }
                                case 12:
                                    {
                                        Console.WriteLine("Repairing... (18 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(18000);
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("No repair is needed for this ship.");
                                        writer.Write(fileName);
                                        break;
                                    }
                            }
                        }

                        // Repair Code 3
                        if (ship.RepairCode == 3)
                        {
                            switch (ship.ShipClassId)
                            {
                                case 1:
                                case 3:
                                    {
                                        Console.WriteLine("Repairing... (6 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(6000);
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.WriteLine("Repairing... (5 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(5000);
                                        break;
                                    }
                                case 4:
                                case 10:
                                    {
                                        Console.WriteLine("Repairing... (7 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(7000);
                                        break;
                                    }
                                case 5:
                                    {
                                        Console.WriteLine("Repairing... (10 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(10000);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("Repairing... (11 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(11000);
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.WriteLine("Repairing... (13 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(13000);
                                        break;
                                    }
                                case 8:
                                    {
                                        Console.WriteLine("Repairing... (15 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(15000);
                                        break;
                                    }
                                case 9:
                                    {
                                        Console.WriteLine("Repairing... (17 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(17000);
                                        break;
                                    }
                                case 11:
                                    {
                                        Console.WriteLine("Repairing... (19 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(19000);
                                        break;
                                    }
                                case 12:
                                    {
                                        Console.WriteLine("Repairing... (21 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(21000);
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("No repair is needed for this ship.");
                                        writer.Write(fileName);
                                        break;
                                    }
                            }
                        }

                        // Repair Code 4
                        if (ship.RepairCode == 4)
                        {
                            switch (ship.ShipClassId)
                            {
                                case 1:
                                case 3:
                                case 4:
                                case 10:
                                    {
                                        Console.WriteLine("Repairing... (9 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(9000);
                                        break;
                                    }
                                case 2:
                                    {
                                        Console.WriteLine("Repairing... (8 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(8000);
                                        break;
                                    }
                                case 5:
                                    {
                                        Console.WriteLine("Repairing... (10 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(10000);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("Repairing... (12 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(12000);
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.WriteLine("Repairing... (14 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(14000);
                                        break;
                                    }
                                case 8:
                                    {
                                        Console.WriteLine("Repairing... (17 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(17000);
                                        break;
                                    }
                                case 9:
                                    {
                                        Console.WriteLine("Repairing... (18 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(18000);
                                        break;
                                    }
                                case 11:
                                    {
                                        Console.WriteLine("Repairing... (20 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(20000);
                                        break;
                                    }
                                case 12:
                                    {
                                        Console.WriteLine("Repairing... (24 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(24000);
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("No repair is needed for this ship.");
                                        writer.Write(fileName);
                                        break;
                                    }
                            }
                        }

                        // Repair Code 5
                        if (ship.RepairCode == 5)
                        {
                            switch (ship.ShipClassId)
                            {
                                case 1:
                                    {
                                        Console.WriteLine("Repairing... (10 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(10000);
                                        break;
                                    }
                                case 2:
                                case 3:
                                case 10:
                                    {
                                        Console.WriteLine("Repairing... (9 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(9000);
                                        break;
                                    }
                                case 4:
                                case 11:
                                    {
                                        Console.WriteLine("Repairing... (11 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(11000);
                                        break;
                                    }
                                case 5:
                                    {
                                        Console.WriteLine("Repairing... (12 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(12000);
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.WriteLine("Repairing... (14 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(14000);
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.WriteLine("Repairing... (16 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(16000);
                                        break;
                                    }
                                case 8:
                                case 9:
                                    {
                                        Console.WriteLine("Repairing... (19 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(19000);
                                        break;
                                    }
                                case 12:
                                    {
                                        Console.WriteLine("Repairing... (30 Cycles)");
                                        writer.Write(fileName);
                                        Thread.Sleep(30000);
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("No repair is needed for this ship.");
                                        writer.Write(fileName);
                                        break;
                                    }
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Ship {ship.ShipName} service complete.");
                        writer.Write(fileName);
                        Console.ResetColor();

                        // Check to see if this was the last ship in the queue.
                        if (_lastShipServiced == ship.ShipName)
                        {
                            // Allow the Monitor Queue thread to complete.
                            _lastShipComplete = true;
                        }
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.PerformService() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Responsible to select and verify the bay is compatible with the ship class and race
        /// </summary>
        /// <param name="ship"></param>
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
        /// Time cycle required to dock a ship
        /// </summary>
        /// <param name="ship"></param>
        public void DockingTimeCycle(Ship ship)
        {
            // Originally I was attempting to put this method in TimeCycles class for easy to find and maintainability.
            // It was successful but I couldn't implenement them in this class because it need to be assigned. So I put this method back to here.
            // Dad - This is good.  But you should combine the ships that have the same docking time (see below).  Always go for less code.

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
                case ShipClass.CargoTransport:
                    {
                        Thread.Sleep(7000);
                        break;
                    }
                case ShipClass.ScoutShip:
                    {
                        Thread.Sleep(8000);
                        break;
                    }
                case ShipClass.PersonnelTransport:
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
    }
}
