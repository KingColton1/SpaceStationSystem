using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    public enum ShipClass
    {
        Runabout = 1,
        Personal = 2,
        Skeeter = 3,
        SmallShuttle = 4,
        MediumShuttle = 5,
        LargeShuttle = 6,
        PersonnelTransport = 7,
        CargoTransport = 8,
        CargoTransportII = 9,
        ScoutShip = 10,
        Explorer = 11,
        Dreadnaught = 12
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Andromeda Space Station System";
                
                // All main functions are in SpaceStation class because this Main method is static and it makes everything too complicated.
                SpaceStation station = new SpaceStation("Andromeda");

                station.ServiceShips();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Program.Main() - {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Display menu output for Captain of a ship to choose a bay and assign to it
        /// </summary>
        /// <param name="ship"></param>
        public void MenuOutput(Ship ship)
        {
            int choice;

            Console.WriteLine($"Greeting Captain of {ship.ShipName} Ship, how can we help you?" +
                              $"\nYour ship Federation ID: {ship.ShipFedId}" +
                              $"\nYour ship class: {ship.ShipClassId}, {ship.ShipClass}");

            Console.WriteLine($"1. Select Docking Bay" +
                              $"\n2. Statuses of Docking Bay" +
                              $"\n3. Exit");
            int.TryParse(Console.ReadLine(), out choice);

            switch (choice)
            {
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 3:
                    {
                        Console.WriteLine("You decided to depart from the Space Station, good bye!");
                        Environment.Exit(1000);
                        break;
                    }
            }
        }
    }
}
