using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SpaceStationSystem
{
    class TimeCycles
    {
        /// <summary>
        /// Time cycle required to dock a ship
        /// </summary>
        /// <param name="ship"></param>
        public void DockingTimeCycle(Ship ship)
        {
            switch (ship.ShipClass)
            {
                case ShipClass.Runabout:
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                case ShipClass.Personal:
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                case ShipClass.Skeeter:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
                case ShipClass.SmallShuttle:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
                case ShipClass.MediumShuttle:
                    {
                        Thread.Sleep(5000);
                        break;
                    }
                case ShipClass.LargeShuttle:
                    {
                        Thread.Sleep(7000);
                        break;
                    }
                case ShipClass.PersonnelTransport:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.CargoTransport:
                    {
                        Thread.Sleep(7000);
                        break;
                    }
                case ShipClass.CargoTransportII:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.ScoutShip:
                    {
                        Thread.Sleep(8000);
                        break;
                    }
                case ShipClass.Explorer:
                    {
                        Thread.Sleep(11000);
                        break;
                    }
                case ShipClass.Dreadnaught:
                    {
                        Thread.Sleep(15000);
                        break;
                    }
            }
        }


        /// <summary>
        /// Time cycle required to undock a ship
        /// </summary>
        /// <param name="ship"></param>
        public void UnDockingTimeCycle(Ship ship)
        {
            switch (ship.ShipClass)
            {
                case ShipClass.Runabout:
                    {
                        Thread.Sleep(1000);
                        break;
                    }
                case ShipClass.Personal:
                    {
                        Thread.Sleep(2000);
                        break;
                    }
                case ShipClass.Skeeter:
                    {
                        Thread.Sleep(3000);
                        break;
                    }
                case ShipClass.SmallShuttle:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
                case ShipClass.MediumShuttle:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
                case ShipClass.LargeShuttle:
                    {
                        Thread.Sleep(4000);
                        break;
                    }
                case ShipClass.PersonnelTransport:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.CargoTransport:
                    {
                        Thread.Sleep(9000);
                        break;
                    }
                case ShipClass.CargoTransportII:
                    {
                        Thread.Sleep(11000);
                        break;
                    }
                case ShipClass.ScoutShip:
                    {
                        Thread.Sleep(6000);
                        break;
                    }
                case ShipClass.Explorer:
                    {
                        Thread.Sleep(12000);
                        break;
                    }
                case ShipClass.Dreadnaught:
                    {
                        Thread.Sleep(17000);
                        break;
                    }
            }
        }
    }
}