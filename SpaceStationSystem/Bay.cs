using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    public class Bay
    {

        public int DockId { get; set; }


        public bool DualEnvironment { get; set; }


        public string CurrentEnvironment { get; set; }


        public bool SupportsHuman { get; set; }


        public bool SupportsAqua { get; set; }


        public bool SupportsMega { get; set; }


        public int ClassMin { get; set; }


        public int ClassMax { get; set; }

        public bool ConvertEnvironment { get; set; }

        public bool InUse { get; set; }
    }
}
