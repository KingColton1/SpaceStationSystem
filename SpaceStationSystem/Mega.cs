using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceStationSystem
{
    class Mega : Crew
    {
        private bool _oxygen;
        private bool _heavyFloor;
        private bool _water;

        // Do they need oxygen to breathe?
        public bool Oxygen
        {
            get => _oxygen;
            set => _oxygen = true;
        }

        // Do they need Heavy Floor to walk on?
        public bool HeavyFloor
        {
            get => _heavyFloor;
            set => _heavyFloor = true;
        }

        // Do they need water to breathe?
        public bool Water
        {
            get => _water;
            set => _water = false;
        }
    }
}
