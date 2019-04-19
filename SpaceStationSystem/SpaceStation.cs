using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace SpaceStationSystem
{

    class SpaceStation
    {
        // A queue is used when you want things to be removed in the order they were added.
        private Queue<Ship> _serviceQueue = new Queue<Ship>();

        // A thread is separate process that runs outside the flow of the main code.
        // Used when you want to do multiple things at the same time.
        private Thread _monitorQueue;
        private string _lastShipServiced = "";
        private bool _lastShipComplete = false;

        /// <summary>
        /// The name of the space station.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of the bays in the space station.
        /// </summary>
        public List<Bay> DockingBays;

        /// <summary>
        /// The ships that need to be serviced.
        /// </summary>
        public Dictionary<int, Ship> Ships;

        /// <summary>
        /// Loads the resource files into class properties and creates the queue for servicing the ships.
        /// </summary>
        /// <param name="name">The name of the space station.</param>
        public SpaceStation(string name)
        {
            try
            {
                Ships = new Dictionary<int, Ship>();
                Name = name;

                // Needed for null values in JSON files. Ignores null values.
                var settings = new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    };

                // Load the docking bay information from file Docks2019.json into class property DockingBays.
                DockingBays = JsonConvert.DeserializeObject<List<Bay>>(Properties.Resources.Docks2019_json, settings);

                // Load the ship information from file FinalShips2019.json into a List.
                List<Ship> ships = JsonConvert.DeserializeObject<List<Ship>>(Properties.Resources.FinalShips2019_json, settings);

                // Use the ship information to load the Ships dictionary and the service queue.
                foreach (Ship ship in ships)
                {
                    Ships.Add(ship.ShipFedId, ship);
                    _serviceQueue.Enqueue(ship);
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

            try
            {
                messages = new List<string>();

                while (!_lastShipComplete)
                {

                    // Check if there are any ships in the service queue.
                    if (_serviceQueue.Count > 0)
                    {
                        // Get the next ship in line to be serviced.
                        ship = _serviceQueue.Dequeue();

                        // Dad - I added the messages List and the message string variables to use for the report.
                        //       Here's my idea:
                        //       We'll pass these messages to the thread that performs the service.
                        //       The thread will create a text file and write all these events to it plus the service events.
                        //       When all ships have been serviced, a method will read all the files and put that information into a single text file.
                        //       That single text file will be the report.

                        // Colton - That's actually a good idea, let's do that.

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

                        // A dock is occupied by a ship
                        if (dockingBay.InUse == true)
                        {
                            // Once docking is completed, inform to the user that a ship is docked
                            message = $"Ship {ship.ShipName} (Class {ship.ShipClassId}) is docked.";
                            messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} {message}");
                            Console.WriteLine(message);

                            // Spawn a new thread to perform the service.
                            Thread performService = new Thread(() => PerformService(ship));
                            performService.Name = $"Service Ship {ship.ShipName}";
                            performService.IsBackground = true;
                            performService.Start();
                        }

                        lastShip = ship.ShipName;
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
        /// // Perform services that offer refuel, cargo load, cargo unload, clean waste tank, repair, and food
        /// </summary>
        /// <param name="ship"></param>
        private void PerformService(Ship ship)
        {
            try
            {
                Console.WriteLine($"Begin service on ship {ship.ShipName} (Federation ID {ship.ShipFedId} | Class {ship.ShipClassId}), requesting Repair Code {ship.RepairCode}");

                // Perform repairs based on the ship's classes and Repair Code (For now, use only Code 1)
                if (ship.RepairCode == 1)
                {
                    switch (ship.ShipClassId)
                    {
                        case 1:
                            {
                                Thread.Sleep(4000);
                                break;
                            }
                        case 2:
                            {
                                Thread.Sleep(5000);
                                break;
                            }
                        case 3:
                            {
                                Thread.Sleep(4000);
                                break;
                            }
                        case 4:
                            {
                                Thread.Sleep(5000);
                                break;
                            }
                        case 5:
                            {
                                Thread.Sleep(7000);
                                break;
                            }
                        case 6:
                            {
                                Thread.Sleep(8000);
                                break;
                            }
                        case 7:
                            {
                                Thread.Sleep(9000);
                                break;
                            }
                        case 8:
                            {
                                Thread.Sleep(11000);
                                break;
                            }
                        case 9:
                            {
                                Thread.Sleep(11000);
                                break;
                            }
                        case 10:
                            {
                                Thread.Sleep(7000);
                                break;
                            }
                        case 11:
                            {
                                Thread.Sleep(12000);
                                break;
                            }
                        case 12:
                            {
                                Thread.Sleep(15000);
                                break;
                            }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Ship {ship.ShipName} service complete.");
                Console.ResetColor();

                // Check to see if this was the last ship in the queue.
                if (_lastShipServiced == ship.ShipName)
                {
                    // Allow the Monitor Queue thread to complete.
                    _lastShipComplete = true;
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
                foreach (Bay bay in DockingBays)
                {
                    if (dockingBay.InUse == false)
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
            switch (ship.ShipClass)
            {
                case ShipClass.Runabout:
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                case ShipClass.Personal:
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                case ShipClass.Skeeter:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
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
                    {
                        Thread.Sleep(7000);
                        break;
                    }
                case ShipClass.PersonnelTransport:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.CargoTransport:
                    {
                        Thread.Sleep(7000);
                        break;
                    }
                case ShipClass.CargoTransportII:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.ScoutShip:
                    {
                        Thread.Sleep(8000);
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
