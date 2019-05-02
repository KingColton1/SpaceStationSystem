using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SpaceStationSystem
{

    /* Programmer: Colton Bailiff
        * Course: Programming Fundamental II: Mobile Domain - NMAD.181
        * Date: April 22, 2019
        * 
        * Assignment: Final Project
        * Purpose: The Space Station is going to use new system, so Colton have developed new system for Commander David to allow Commander to
        * manage the ship by assigning them to their dock and convert if needed. The system would take over the service part and let them go after
        * the service on a ship is finished.
        */

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
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
        }
    }
}
