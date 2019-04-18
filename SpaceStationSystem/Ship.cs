using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    class Ship
    {
        /// <summary>
        /// The unique identifier of the ship's class.
        /// </summary>
        public int ShipClassId { get; set; }

        /// <summary>
        /// The name of the ship's class.
        /// </summary>
        public ShipClass ShipClass
        {
            get { return (ShipClass)ShipClassId; }
        }

        /// <summary>
        /// The name of the ship.
        /// </summary>
        public string ShipName { get; set; }

        /// <summary>
        /// The unique number assinged to the ship by the Federation.
        /// </summary>
        public int ShipFedId { get; set; }

        /// <summary>
        /// The ship captain's name.
        /// </summary>
        public string CaptainName { get; set; }
        
        /// <summary>
        /// The amount of fuel the ship can hold.
        /// </summary>
        public int FuelCapacity { get; set; }
        
        /// <summary>
        /// The amount of fuel on the ship.
        /// </summary>
        public int FuelOnBoard { get; set; }

        /// <summary>
        /// The amount of cargo that needs to be unloaded from the ship.
        /// </summary>
        public int CargoToUnload { get; set; }

        /// <summary>
        /// The amount of cargo that needs to be loaded onto the ship.
        /// </summary>
        public int CargoToLoad { get; set; }

        /// <summary>
        /// The amount of waste the ship can hold.
        /// </summary>
        public int WasteCapacity { get; set; }

        /// <summary>
        /// The amount of waste currently onboard the ship.
        /// </summary>
        public int CurrentWaste { get; set; }

        /// <summary>
        /// Whether the ship is docked to the space station or not.
        /// </summary>
        public bool Docked { get; set; }


        public int RepairCode { get; set; }


        public int FoodCode { get; set; }

        // Changed the name from DefensePowerLevel to CurrentPower and updated FinalShips2019 file.
        public int CurrentPower { get; set; }


        public string Race { get; set; }
    }
}
