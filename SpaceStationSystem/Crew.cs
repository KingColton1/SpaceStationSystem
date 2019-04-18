using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceStationSystem
{
    class Crew : Ship
    {
        /// <summary>
        /// The race of the beings that inhabit the ship.
        /// </summary>
        public string Race { get; set; }

        /// <summary>
        /// Each races have their own needs for bay enviroment
        /// </summary>
        public void Human()
        {
            bool useOxygen = true;
            bool useHeavyFloor = false;
            bool useWater = false;
        }

        public void Mega()
        {
            bool useOxygen = true;
            bool useHeavyFloor = true;
            bool useWater = false;
        }

        public void Amphibian()
        {
            bool useOxygen = false;
            bool useHeavyFloor = false;
            bool useWater = true;
        }
    }
}
