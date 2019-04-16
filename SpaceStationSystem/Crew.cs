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

        // I decide to move 3 classes to here because it make sense to belong here and easier to call
        // IF you agree with this, I'll delete 3 classes
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
