using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    class Bay
    {
        private int _bayNumber;

        // Temporary
        public int BayNumber
        {
            get => _bayNumber;
            set => _bayNumber = value;
        }
    }
}
