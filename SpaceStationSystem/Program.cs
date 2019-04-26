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
    }
}
