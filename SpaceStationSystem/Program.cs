using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    public enum ShipType
    {
        Runabout,
        Personal,
        Skeeter,
        SmallShuttle,
        MediumShuttle,
        LargeShuttle,
        PersonnelTransport,
        CargoTransport,
        CargoTransportII,
        ScoutShip,
        Explorer,
        Dreadnaught
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Andromeda Space Station System";

            try
            {
                // All main functions are in SpaceStation class because this Main method is static and it make everything too complicated
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

        // JSON Method Test- focusing on only Ships2019 file (It's in JSON folder in Solution Explorer)
        // Deserialize JSON from a file
        public void JSONFile()
        {
            // Read file into string and deserialize JSON into a type (NOTE: Need to fix this part)
            Ship shipInfo = JsonConvert.DeserializeObject<Ship>(File.ReadAllText(@"C:\Users\colto\Documents\QueueThreadDemo\QueueThreadDemo\JSON\Ships2019.json"));

            // Deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(@"C:\Users\colto\Documents\QueueThreadDemo\QueueThreadDemo\JSON\Ships2019.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                Ship shipInfo2 = (Ship)serializer.Deserialize(file, typeof(Ship));
            }
        }

    }
}
