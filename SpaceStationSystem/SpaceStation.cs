using System;
using System.Collections.Generic;
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
        private Queue<Service> _serviceQueue = new Queue<Service>();

        // A thread is separate process that runs outside the flow of the main code.
        // Used when you want to do multiple things at the same time.
        private Thread _monitorQueue;
        private Dictionary<int, string> _bays; // Added as a storage
        private string _name;
        private string _lastShipServiced = "";
        private bool _lastShipComplete = false;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        // I'm not sure if I did this right way, I need some more guidances from you
        public Dictionary<int, string> Bays
        {
            get
            {
                return _bays;
            }
        }

        public SpaceStation(string name)
        {
            try
            {
                _name = name;

                // Fake data for testing purpose
                for (int i = 1; i <= 10; i++)
                {
                    Ship ship = new Ship($"Explorer{ShipClass.Explorer}"); // The Ship constructor doesn't have any parameter, but this line somehow read as "don't take one parameter"
                    Service service = new Service(ship, true, true, false, false, true, true, false);

                    // Enqueue is how you add things to a Queue.
                    _serviceQueue.Enqueue(service);
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
                Console.WriteLine($"Start docking service for space station {_name}");
                Console.WriteLine("");
                Thread.Sleep(2000);

                // Spawn a thread that will monitor the queue.
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
            Service service;
            string lastShip = "";

            try
            {
                while (!_lastShipComplete)
                {
                    // Check if there are any ships in the service queue.
                    if (_serviceQueue.Count > 0)
                    {
                        // Get the next ship in line to be serviced.
                        service = _serviceQueue.Dequeue();

                        Console.WriteLine($"Begin docking ship {service.Ship.ShipName}");

                        // Pause for the time it takes to dock the ship.
                        Thread.Sleep(service.DockTime * 1000);

                        Console.WriteLine($"Ship {service.Ship.ShipName} is docked.");
                        Thread.Sleep(1000);

                        // Spawn a new thread to perform the service.
                        Thread performService = new Thread(() => PerformService(service));
                        performService.Name = $"Service Ship {service.Ship.ShipName}";
                        performService.IsBackground = true;
                        performService.Start();

                        lastShip = service.Ship.ShipName;
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

        private void PerformService(Service service)
        {
            try
            {
                Console.WriteLine($"Begin service on ship {service.Ship.ShipName}");

                // Made-up simulating the service time for testing purpose
                Thread.Sleep(service.ServiceTime * 1000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Ship {service.Ship.ShipName} service complete.");
                Console.ResetColor();

                // Check to see if this was the last ship in the queue.
                if (_lastShipServiced == service.Ship.ShipName)
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
