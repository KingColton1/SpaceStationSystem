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
        public string DockOperations { get; set; }

        public bool IsOccupied { get; set; }

        public int CurrentShipID { get; set; }

        public int DockId { get; set; }

        public bool ConvertibleEnvironment { get; set; }

        public string CurrentEnvironment { get; set; }

        public bool MetalicFloor { get; set; }

        public int ClassMin { get; set; }

        public int ClassMax { get; set; }

    }
}
