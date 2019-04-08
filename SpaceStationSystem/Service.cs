using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceStationSystem
{
    class Service
    {
        // Fields to store information
        private Ship _ship;
        private bool _refuel;
        private bool _offloadCargo;
        private bool _onloadCargo;
        private bool _rechargeWeapons;
        private bool _cleanWasteTanks;
        private bool _replenish;
        private bool _repair;
        private int _dockTime;
        private int _serviceTime;

        // All the properties are read only because they all affect the time calculations
        // and should not be changed after the times have been calculated.
        public Ship Ship
        {
            get => _ship;
        }

        public bool Refuel
        {
            get => _refuel;
        }

        public bool OffloadCargo
        {
            get => _offloadCargo;
        }

        public bool OnloadCargo
        {
            get => _onloadCargo;
        }

        public bool RechargeWeapons
        {
            get => _rechargeWeapons;
        }

        public bool CleanWasteTanks
        {
            get => _cleanWasteTanks;
        }

        public bool Replenish
        {
            get => _replenish;
        }

        public bool Repair
        {
            get => _repair;
        }

        public int DockTime
        {
            get => _dockTime;
        }

        public int ServiceTime
        {
            get => _serviceTime;
        }

        public Service(Ship ship, bool refuel, bool offloadCargo, bool onloadCargo, bool rechargeWeapons,
                       bool cleanWasteTanks, bool replenish, bool repair)
        {
            try
            {
                _ship = ship;
                _refuel = refuel;
                _offloadCargo = offloadCargo;
                _onloadCargo = onloadCargo;
                _rechargeWeapons = rechargeWeapons;
                _cleanWasteTanks = cleanWasteTanks;
                _replenish = replenish;
                _repair = repair;

                CalculateDockTime();
                CalculateServiceTime();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Service.Service() - {ex.Message}");
                Console.ResetColor();
            }
        }

        private void CalculateDockTime()
        {
            try
            {
                // Used random for testing purpose
                Random num = new Random();
                _dockTime = num.Next(5, 20);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Service.CalculateDockTime() - {ex.Message}");
                Console.ResetColor();
            }
        }

        private void CalculateServiceTime()
        {
            try
            {
                // Used random for testing purpose
                Random num = new Random();
                _serviceTime = num.Next(40, 120);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Service.CalculateServiceTime() - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
