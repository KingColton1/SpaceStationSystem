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
        ///  Get the list of
        /// </summary>
        public List<Bay> DockingBays;

        /// <summary>
        /// The ships that need to be serviced.
        /// </summary>
        public Dictionary<int, Ship> Ships;

        /// <summary>
        /// Loads the resources files into class properties and creates the queue for servicing the ships.
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

        // The method used for the Monitor Queue thread.
        private void MonitorQueue()
        {
            Ship ship;
            Crew people;
            Bay dock;
            string lastShip = "";

            try
            {
                while (!_lastShipComplete)
                {
                    // Check if there are any ships in the service queue.
                    if (_serviceQueue.Count > 0)
                    {
                        // Get the next ship in line to be serviced.
                        ship = _serviceQueue.Dequeue();

                        // Select the docking bay.
                        // Todo: Select a compatible open docking bay.  
                        //       If no compatible docking bays are available but a non-compatible bay is open, convert it.

                        // I tried 4 ways; use Bay for current enviroment from ship name, compare race for bay's current enviroment, etc.
                        if (people.Race == Human)
                        {

                        }
                        

                        // Prepare the docking bay.
                        Thread.Sleep(10000);

                        Console.WriteLine($"Begin docking ship {ship.ShipName} (Class {ship.ShipClassId}) in docking bay {"?"}");

                        // Pause for the time it takes to dock the ship based on the ship's class.
                        // Case 1 is using ProgressBar class, but that's the only source I can find to Append text. Do you think we can simplify that class?
                        // ProgressBar class is also used in PerformService method
                        switch (ship.ShipClassId)
                        {
                            case 1:
                                {
                                    // Progress bar system
                                    Console.Write($"Ship {ship.ShipName} docking... ");
                                    using (var progress = new ProgressBar())
                                    {
                                        for (int i = 0; i <= 100; i++)
                                        {
                                            progress.Report((double)i / 100);
                                            Thread.Sleep(3000);
                                        }
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    Thread.Sleep(3000);
                                    break;
                                }
                            case 3:
                                {
                                    Thread.Sleep(4000);
                                    break;
                                }
                            case 4:
                                {
                                    Thread.Sleep(4000);
                                    break;
                                }
                            case 5:
                                {
                                    Thread.Sleep(5000);
                                    break;
                                }
                            case 6:
                                {
                                    Thread.Sleep(7000);
                                    break;
                                }
                            case 7:
                                {
                                    Thread.Sleep(9000);
                                    break;
                                }
                            case 8:
                                {
                                    Thread.Sleep(7000);
                                    break;
                                }
                            case 9:
                                {
                                    Thread.Sleep(9000);
                                    break;
                                }
                            case 10:
                                {
                                    Thread.Sleep(8000);
                                    break;
                                }
                            case 11:
                                {
                                    Thread.Sleep(11000);
                                    break;
                                }
                            case 12:
                                {
                                    Thread.Sleep(15000);
                                    break;
                                }
                        }

                        Console.WriteLine($"Ship {ship.ShipName} (Class {ship.ShipClassId}) is docked.");
                        Thread.Sleep(1000);

                        // Spawn a new thread to perform the service.
                        Thread performService = new Thread(() => PerformService(ship));
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

                    // Slow down the loop so the new threads don't get mixed up.
                    Thread.Sleep(2000);
                }

                Console.WriteLine("All ships have been serviced.  Press enter exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.MonitorQueue() - {ex.Message}");
                Console.ResetColor();
            }
        }

        // Perform services that offer refuel, cargo load, cargo unload, clean waste tank, repair, and food
        // For now I'll be using refuel and repair only to see if it's suitable
        // I tried using time matrix (multi-dem) but I find them too advanced and complicated for this project. I want to keep it clean, simple, and easier to edit.
        private void PerformService(Ship ship)
        {
            try
            {
                Console.WriteLine($"Begin service on ship {ship.ShipName} (Class {ship.ShipClassId}), requesting Repair Code {ship.RepairCode}");

                // Perform repairs based on the ship's classes and Repair Code (For now, we'll be using Code 1)
                // I plan to use "progress bar" to provide real time update instead of waiting for a new line to appear
                if (ship.RepairCode == 1)
                {
                    switch (ship.ShipClassId)
                    {
                        case 1:
                            {
                                // Progress bar system
                                Console.Write("Performing repair... ");
                                using (var progress = new ProgressBar())
                                {
                                    for (int i = 0; i <= 100; i++)
                                    {
                                        progress.Report((double) i / 100);
                                        Thread.Sleep(4000);
                                    }
                                }
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
    }
}
