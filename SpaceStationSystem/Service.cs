using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SpaceStationSystem
{
    /// <summary>
    /// Contains dictionaries for Food and Repair codes from Lookup tables, Defense Power recharge calculation, and
    /// Food/Repair Code's time cycles calculation.
    /// </summary>
    class Service
    {
        // Define List of Dictionaries for RepairTime and FoodSupply time cycles array
        private List<Dictionary<ShipClass, int>> _repairTimeCycles;
        private List<Dictionary<ShipClass, int>> _foodSupplyTimeCycles;

        public Service()
        {
            // List of Dictionaries; it used ship class as the key of the dictionary. The value is the time cycle for the ship class.
            _repairTimeCycles = new List<Dictionary<ShipClass, int>>();
            _foodSupplyTimeCycles = new List<Dictionary<ShipClass, int>>();
            _repairTimeCycles.Add(null); // There is no repair code zero. So we can't use this item in the array.
            _foodSupplyTimeCycles.Add(null);

            // Time cycles for repair code 1
            Dictionary<ShipClass, int> repairCode1 = new Dictionary<ShipClass, int>();
            repairCode1.Add(ShipClass.Runabout, 4);
            repairCode1.Add(ShipClass.Personal, 5);
            repairCode1.Add(ShipClass.Skeeter, 4);
            repairCode1.Add(ShipClass.SmallShuttle, 5);
            repairCode1.Add(ShipClass.MediumShuttle, 7);
            repairCode1.Add(ShipClass.LargeShuttle, 8);
            repairCode1.Add(ShipClass.PersonnelTransport, 9);
            repairCode1.Add(ShipClass.CargoTransport, 11);
            repairCode1.Add(ShipClass.CargoTransportII, 11);
            repairCode1.Add(ShipClass.ScoutShip, 7);
            repairCode1.Add(ShipClass.Explorer, 12);
            repairCode1.Add(ShipClass.Dreadnaught, 15);
            _repairTimeCycles.Add(repairCode1); // Add repair code 1 time cycles to index 1 of the List.

            // Time cycles for repair code 2
            Dictionary<ShipClass, int> repairCode2 = new Dictionary<ShipClass, int>();
            repairCode2.Add(ShipClass.Runabout, 5);
            repairCode2.Add(ShipClass.Personal, 5);
            repairCode2.Add(ShipClass.Skeeter, 6);
            repairCode2.Add(ShipClass.SmallShuttle, 5);
            repairCode2.Add(ShipClass.MediumShuttle, 9);
            repairCode2.Add(ShipClass.LargeShuttle, 10);
            repairCode2.Add(ShipClass.PersonnelTransport, 11);
            repairCode2.Add(ShipClass.CargoTransport, 13);
            repairCode2.Add(ShipClass.CargoTransportII, 13);
            repairCode2.Add(ShipClass.ScoutShip, 7);
            repairCode2.Add(ShipClass.Explorer, 14);
            repairCode2.Add(ShipClass.Dreadnaught, 18);
            _repairTimeCycles.Add(repairCode2); // Add repair code 2 time cycles to index 2 of the List.

            // Time cycles for repair code 3
            Dictionary<ShipClass, int> repairCode3 = new Dictionary<ShipClass, int>();
            repairCode3.Add(ShipClass.Runabout, 6);
            repairCode3.Add(ShipClass.Personal, 5);
            repairCode3.Add(ShipClass.Skeeter, 6);
            repairCode3.Add(ShipClass.SmallShuttle, 7);
            repairCode3.Add(ShipClass.MediumShuttle, 10);
            repairCode3.Add(ShipClass.LargeShuttle, 11);
            repairCode3.Add(ShipClass.PersonnelTransport, 13);
            repairCode3.Add(ShipClass.CargoTransport, 15);
            repairCode3.Add(ShipClass.CargoTransportII, 17);
            repairCode3.Add(ShipClass.ScoutShip, 7);
            repairCode3.Add(ShipClass.Explorer, 19);
            repairCode3.Add(ShipClass.Dreadnaught, 21);
            _repairTimeCycles.Add(repairCode3); // Add repair code 3 time cycles to index 3 of the List.

            // Time cycles for repair code 4
            Dictionary<ShipClass, int> repairCode4 = new Dictionary<ShipClass, int>();
            repairCode4.Add(ShipClass.Runabout, 9);
            repairCode4.Add(ShipClass.Personal, 8);
            repairCode4.Add(ShipClass.Skeeter, 9);
            repairCode4.Add(ShipClass.SmallShuttle, 9);
            repairCode4.Add(ShipClass.MediumShuttle, 10);
            repairCode4.Add(ShipClass.LargeShuttle, 12);
            repairCode4.Add(ShipClass.PersonnelTransport, 14);
            repairCode4.Add(ShipClass.CargoTransport, 17);
            repairCode4.Add(ShipClass.CargoTransportII, 18);
            repairCode4.Add(ShipClass.ScoutShip, 9);
            repairCode4.Add(ShipClass.Explorer, 20);
            repairCode4.Add(ShipClass.Dreadnaught, 24);
            _repairTimeCycles.Add(repairCode4);

            // Time cycles for repair code 5
            Dictionary<ShipClass, int> repairCode5 = new Dictionary<ShipClass, int>();
            repairCode5.Add(ShipClass.Runabout, 10);
            repairCode5.Add(ShipClass.Personal, 9);
            repairCode5.Add(ShipClass.Skeeter, 9);
            repairCode5.Add(ShipClass.SmallShuttle, 11);
            repairCode5.Add(ShipClass.MediumShuttle, 12);
            repairCode5.Add(ShipClass.LargeShuttle, 14);
            repairCode5.Add(ShipClass.PersonnelTransport, 16);
            repairCode5.Add(ShipClass.CargoTransport, 19);
            repairCode5.Add(ShipClass.CargoTransportII, 19);
            repairCode5.Add(ShipClass.ScoutShip, 9);
            repairCode5.Add(ShipClass.Explorer, 11);
            repairCode5.Add(ShipClass.Dreadnaught, 30);
            _repairTimeCycles.Add(repairCode5);

            // Repair Code above
            // ------------------------------------------------------
            // Food Code below

            // Time cycles for food code 1
            Dictionary<ShipClass, int> foodCode1 = new Dictionary<ShipClass, int>();
            foodCode1.Add(ShipClass.Runabout, 4);
            foodCode1.Add(ShipClass.Personal, 5);
            foodCode1.Add(ShipClass.Skeeter, 4);
            foodCode1.Add(ShipClass.SmallShuttle, 5);
            foodCode1.Add(ShipClass.MediumShuttle, 7);
            foodCode1.Add(ShipClass.LargeShuttle, 8);
            foodCode1.Add(ShipClass.PersonnelTransport, 9);
            foodCode1.Add(ShipClass.CargoTransport, 11);
            foodCode1.Add(ShipClass.CargoTransportII, 11);
            foodCode1.Add(ShipClass.ScoutShip, 7);
            foodCode1.Add(ShipClass.Explorer, 12);
            foodCode1.Add(ShipClass.Dreadnaught, 15);
            _foodSupplyTimeCycles.Add(foodCode1);

            // Time cycles for food code 2
            Dictionary<ShipClass, int> foodCode2 = new Dictionary<ShipClass, int>();
            foodCode2.Add(ShipClass.Runabout, 5);
            foodCode2.Add(ShipClass.Personal, 5);
            foodCode2.Add(ShipClass.Skeeter, 6);
            foodCode2.Add(ShipClass.SmallShuttle, 5);
            foodCode2.Add(ShipClass.MediumShuttle, 9);
            foodCode2.Add(ShipClass.LargeShuttle, 10);
            foodCode2.Add(ShipClass.PersonnelTransport, 11);
            foodCode2.Add(ShipClass.CargoTransport, 13);
            foodCode2.Add(ShipClass.CargoTransportII, 13);
            foodCode2.Add(ShipClass.ScoutShip, 7);
            foodCode2.Add(ShipClass.Explorer, 14);
            foodCode2.Add(ShipClass.Dreadnaught, 18);
            _foodSupplyTimeCycles.Add(foodCode2);

            // Time cycles for food code 3
            Dictionary<ShipClass, int> foodCode3 = new Dictionary<ShipClass, int>();
            foodCode3.Add(ShipClass.Runabout, 6);
            foodCode3.Add(ShipClass.Personal, 5);
            foodCode3.Add(ShipClass.Skeeter, 6);
            foodCode3.Add(ShipClass.SmallShuttle, 7);
            foodCode3.Add(ShipClass.MediumShuttle, 10);
            foodCode3.Add(ShipClass.LargeShuttle, 11);
            foodCode3.Add(ShipClass.PersonnelTransport, 13);
            foodCode3.Add(ShipClass.CargoTransport, 15);
            foodCode3.Add(ShipClass.CargoTransportII, 17);
            foodCode3.Add(ShipClass.ScoutShip, 7);
            foodCode3.Add(ShipClass.Explorer, 19);
            foodCode3.Add(ShipClass.Dreadnaught, 21);
            _foodSupplyTimeCycles.Add(foodCode3);

            // Time cycles for food code 4
            Dictionary<ShipClass, int> foodCode4 = new Dictionary<ShipClass, int>();
            foodCode4.Add(ShipClass.Runabout, 9);
            foodCode4.Add(ShipClass.Personal, 8);
            foodCode4.Add(ShipClass.Skeeter, 9);
            foodCode4.Add(ShipClass.SmallShuttle, 9);
            foodCode4.Add(ShipClass.MediumShuttle, 10);
            foodCode4.Add(ShipClass.LargeShuttle, 12);
            foodCode4.Add(ShipClass.PersonnelTransport, 14);
            foodCode4.Add(ShipClass.CargoTransport, 17);
            foodCode4.Add(ShipClass.CargoTransportII, 18);
            foodCode4.Add(ShipClass.ScoutShip, 9);
            foodCode4.Add(ShipClass.Explorer, 20);
            foodCode4.Add(ShipClass.Dreadnaught, 24);
            _foodSupplyTimeCycles.Add(foodCode4);

            // Time cycles for food code 5
            Dictionary<ShipClass, int> foodCode5 = new Dictionary<ShipClass, int>();
            foodCode5.Add(ShipClass.Runabout, 10);
            foodCode5.Add(ShipClass.Personal, 9);
            foodCode5.Add(ShipClass.Skeeter, 9);
            foodCode5.Add(ShipClass.SmallShuttle, 11);
            foodCode5.Add(ShipClass.MediumShuttle, 12);
            foodCode5.Add(ShipClass.LargeShuttle, 14);
            foodCode5.Add(ShipClass.PersonnelTransport, 16);
            foodCode5.Add(ShipClass.CargoTransport, 19);
            foodCode5.Add(ShipClass.CargoTransportII, 19);
            foodCode5.Add(ShipClass.ScoutShip, 9);
            foodCode5.Add(ShipClass.Explorer, 11);
            foodCode5.Add(ShipClass.Dreadnaught, 30);
            _foodSupplyTimeCycles.Add(foodCode5);
        }
        /// <summary>
        /// Extract from the RepairCode dictionary and calculate the time cycles to find out how long it take to repair a ship
        /// </summary>
        /// <param name="ship">The ship that is being repaired</param>
        /// <param name="messages">Display messages in Temp.txt</param>
        public void RepairShip(Ship ship, ref List<string> messages)
        {
            try
            {
                int timeCycle;

                // Extract information from repairTimeCycles dictionary and ship's RepairCode
                Dictionary<ShipClass, int> timeCycles = _repairTimeCycles[ship.RepairCode];

                timeCycle = timeCycles[ship.ShipClass];

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Repairing... ({timeCycle} Cycles)");
                Thread.Sleep(timeCycle * 1000); // Ship's own time cycle multiply by 1000

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
        /// Calculate the time cycles to find out how long it take to resupply food into ship
        /// </summary>
        /// <param name="ship">The ship that is being resupplied with foods</param>
        /// <param name="messages">Display messages in Temp.txt</param>
        public void FoodSupply(Ship ship, ref List<string> messages)
        {
            try
            {
                int timeCycle;

                // Extract information from repairTimeCycles dictionary and ship's RepairCode
                Dictionary<ShipClass, int> timeCycles = _foodSupplyTimeCycles[ship.FoodCode];

                timeCycle = timeCycles[ship.ShipClass];

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupplying foods... ({timeCycle} Cycles)");
                Thread.Sleep(timeCycle * 1000); // Ship's own time cycle multiply by 1000

                messages.Add($"{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")} - Resupply foods complete.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in SpaceStation.RepairShip() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Recharge the ship's defense powers.
        /// </summary>
        /// <param name="ship">The ship that is being serviced.</param>
        /// <param name="messages">The log messages for the events that have already happened to the ship.</param>
        public void DefensePowerService(Ship ship, ref List<string> messages)
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
