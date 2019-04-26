using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpaceStationSystem
{
    /// <summary>
    /// Contains static methods to calculate time cycles for the ship services.
    /// </summary>
    static class Service
    {
        /// <summary>
        /// Identify the Repair Code using array instead of if and switch statements, shorten and easiest to maintain
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="messages"></param>
        public static void ArrayRepairShip(Ship ship, ref List<string> messages)
        {
            try
            {
                int[,] repairCode = new int[12, 5]
                {
                { 4, 5, 6, 9, 10 }, // Class 1 - Runabout
                { 5, 5, 5, 8, 9 }, // Class 2 - Personal
                { 4, 6, 6, 9, 9 }, // Class 3 - Skeeter
                { 5, 5, 7, 9, 11 }, // Class 4 - Small Shuttle
                { 7, 9, 10, 10, 12 }, // Class 5 - Medium Shuttle
                { 8, 10, 11, 12, 14 }, // Class 6 - Large Shuttle
                { 9, 11, 13, 14, 16 }, // Class 7 - Personnel Transport
                { 11, 13, 15, 17, 19 }, // Class 8 - Cargo Transport
                { 11, 13, 17, 18, 19 }, // Class 9 - Cargo Transport II
                { 7, 7, 7, 9, 9 }, // Class 10 - Scout Ship
                { 12, 14, 19, 20, 11 }, // Class 11 - Explorer
                { 15, 18, 21, 24, 30 }, // Class 12 - Dreadnaught
                };

                // Calculate time
                // I'm not very well knowledgable of using math with array, it confuse me a lot.
                int timeCycle = repairCode[0] + repairCode [1];

                // Thread.Sleep will depend on array's data. The message will have different number of cycle each time.
                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... ({repairCode} Cycles)");
                Thread.Sleep(timeCycle);

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairs complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.RepairShip() - {ex.Message}");
                Console.ResetColor();
            }
        }

        // Once we're done with array, I'll remove these methods with if and switch.

        /// <summary>
        /// Repair service for a ship based on their Repair Code.
        /// </summary>
        /// <param name="ship">The ship that is being serviced.</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public static void RepairShip(Ship ship, ref List<string> messages)
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
        /// Food service to resupply food into the ship based on the ship's Food Code.
        /// </summary>
        /// <param name="ship">The ship that is being serviced.</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public static void FoodService(Ship ship, ref List<string> messages)
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
                                messages.Add(message);
                                throw new Exception(message);
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
        /// Recharge the ship's defense powers.
        /// </summary>
        /// <param name="ship">The ship that is being serviced.</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public static void DefensePowerService(Ship ship, ref List<string> messages)
        {
            string message;
            int defensePower;
            int powerNeeded;
            int timeCycles;
            decimal timeCyclesRaw;

            try
            {
                switch (ship.ShipClass)
                {
                    case ShipClass.Runabout:
                        {
                            defensePower = 100;
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
                            messages.Add(message);
                            throw new Exception(message);
                        }
                }

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Ship {ship.ShipName}'s current defense power: {ship.CurrentPower} | Max Defense Power: {defensePower}.");

                // Calculate power needed for full charge.
                powerNeeded = defensePower - ship.CurrentPower;

                // Dad - Remember that dividing integers results in an integer with the remainder stipped off.
                //       Here we want to keep the remainder.  So we have to cast at least one of the integers to a decimal.
                //       For the time cycle calculation
                // Calculate how many 100 units of charge are needed. Cast int to decimal to keep the remainder.
                timeCyclesRaw = (decimal)powerNeeded / 100;

                // Round the remainder up to make sure we calculate enough time for full charge.
                timeCycles = (int)Math.Ceiling(timeCyclesRaw);

                // There are 5 time cycles required for every 100 units of charge needed.
                timeCycles = timeCycles * 5;

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Charging defenses... ({timeCycles} Cycles)");
                Thread.Sleep(timeCycles * 1000);

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Defense Power Service complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.DefensePowerService() - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
