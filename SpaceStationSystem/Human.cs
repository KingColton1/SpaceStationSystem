using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceStationSystem
{
    class Human : Crew
    {
        private bool _oxygen;
        private bool _heavyFloor;
        private bool _water;

        public bool Oxygen
        {
            get => _oxygen;
            set => _oxygen = true;
        }
        public bool HeavyFloor
        {
            get => _heavyFloor;
            set => _heavyFloor = false;
        }
        public bool Water
        {
            get => _water;
            set => _water = false;
        }
    }
}
