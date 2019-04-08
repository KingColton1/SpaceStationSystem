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
        // Stores information in this field section
        private string _name;
        private ShipType _type;
        private int _class;

        // Properties
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        // All CaptainName are in Ships2019.json, there are like 120 of them.
        public string CaptainName
        {
            get;
            set;
        }

        // All Federation ID are in Ships2019.json, again there are 120 of them. They're unique.
        public int ShipFedId
        {
            get;
            set;
        }

        // If the ship type changes, the SetClass() method is called.
        public ShipType Type
        {
            get => _type;
            set
            {
                _type = value;
                SetClass();
            }
        }

        // Read-Only property, the way the class number always matches the ship type.
        public int Class
        {
            get => _class;
        }

        // Standard constructor for class Ship.  Uses the ship type to set the ship class.
        /// <param name="name">The name of the ship.</param>
        /// <param name="type">The type of the ship.</param>
        public Ship(string name, ShipType type)
        {
            _name = name;
            _type = type;

            // Set the class by ship type to guarantee the class always matches the ship type.
            SetClass();
        }

        // Set ship classes
        private void SetClass()
        {
            try
            {
                switch (_type)
                {
                    case ShipType.Runabout:
                        {
                            _class = 1;
                            break;
                        }
                    case ShipType.Personal:
                        {
                            _class = 2;
                            break;
                        }
                    case ShipType.Skeeter:
                        {
                            _class = 3;
                            break;
                        }
                    case ShipType.SmallShuttle:
                        {
                            _class = 4;
                            break;
                        }
                    case ShipType.MediumShuttle:
                        {
                            _class = 5;
                            break;
                        }
                    case ShipType.LargeShuttle:
                        {
                            _class = 6;
                            break;
                        }
                    case ShipType.PersonnelTransport:
                        {
                            _class = 7;
                            break;
                        }
                    case ShipType.CargoTransport:
                        {
                            _class = 8;
                            break;
                        }
                    case ShipType.CargoTransportII:
                        {
                            _class = 9;
                            break;
                        }
                    case ShipType.ScoutShip:
                        {
                            _class = 10;
                            break;
                        }
                    case ShipType.Explorer:
                        {
                            _class = 11;
                            break;
                        }
                    case ShipType.Dreadnaught:
                        {
                            _class = 12;
                            break;
                        }
                    default:
                        {
                            throw new Exception($"Ship type {_type} was not recognized");
                        }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Ship.SetClass() - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
