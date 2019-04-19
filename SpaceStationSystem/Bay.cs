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
        /// <summary>
        /// Identify the number of a dock
        /// </summary>
        public int DockId { get; set; }

        /// <summary>
        /// Does a selected dock have dual environment?
        /// </summary>
        public bool DualEnvironment { get; set; }

        /// <summary>
        /// Identify the dock's current environment
        /// </summary>
        public string CurrentEnvironment { get; set; }

        /// <summary>
        /// Does it support Human (only oxygen)?
        /// </summary>
        public bool SupportsHuman { get; set; }

        /// <summary>
        /// Does it support Aqua (only water)?
        /// </summary>
        public bool SupportsAqua { get; set; }

        /// <summary>
        /// Does it support Mega (heavy floor and oxygen)?
        /// </summary>
        public bool SupportsMega { get; set; }

        /// <summary>
        /// Minimum class of a dock
        /// </summary>
        public int ClassMin { get; set; }

        /// <summary>
        /// Maximum of a dock
        /// </summary>
        public int ClassMax { get; set; }

        /// <summary>
        /// Does a selected dock need to convert environment?
        /// </summary>
        public bool ConvertEnvironment { get; set; }

        /// <summary>
        /// Are the dock(s) in use by another ship?
        /// </summary>
        public bool InUse { get; set; }
    }
}
