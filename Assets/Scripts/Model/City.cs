using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model
{
    public class City
    {
        public event Action<bool> OnPriorityChanged;
        public event Action<WarSide> OnWarSideChanged;

        public CityType CityType { get; }

        private WarSide _warSide;

        public WarSide WarSide
        {
            get => _warSide;
            set
            {
                if (_warSide != value)
                {
                    _warSide = value;
                    OnWarSideChanged?.Invoke(_warSide);
                }
            }
        }

        public Vector3 WorldPosition { get; private set; }
        public List<Road> Roads { get; private set; }

        public Storage<float> Storage { get; private set; }
        
        public Dictionary<SuppliesType, float> StorageCapacity { get; set; } = new()
        {
            { SuppliesType.Ammo, 20 },
            { SuppliesType.Provision, 20 }
        };
        
        public Dictionary<SuppliesType, float> MinStorageCapacity { get; set; } = new()
        {
            { SuppliesType.Ammo, 5 },
            { SuppliesType.Provision, 5 }
        };

        private bool _priority;

        public bool Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPriorityChanged?.Invoke(_priority);
            }
        }

        public City(CityType type, WarSide warSide, Vector3 worldPosition, Storage<float> storage)
        {
            CityType = type;
            WarSide = warSide;
            WorldPosition = worldPosition;
            Storage = storage;
            Roads = new List<Road>();
        }

        public void Tick()
        {
            TryConsumeSupplies(SuppliesType.Ammo, 5f, out  _);
            TryConsumeSupplies(SuppliesType.Provision, 5f, out  _);
        }

        public Road AddRoad(City to, float distance)
        {
            Road road = null;
            if (!IsConnectedTo(to))
            {
                road = new Road(this, to, distance);
                Roads.Add(road);

                if (!to.IsConnectedTo(this))
                {
                    var inversed = to.AddRoad(this, distance);
                    inversed.Inversed = road;
                    road.Inversed = inversed;
                }
            }

            return road;
        }

        public bool IsConnectedTo(City to)
        {
            return Roads.Any(road => road.To == to);
        }

        public bool IsAvailableForSupply(City consumer)
        {
            if (consumer == this)
            {
                return false;
            }
            
            if (consumer.WarSide != WarSide)
            {
                return false;
            }
            
            if (CityType == CityType.Supplier)
            {
                if (GlobalStorage.Instance.CanSendSupplies())
                {
                    return true;
                }
            }

            if (!consumer.Priority && Priority)
            {
                return false;
            }

            var suppliesLeft = Storage.Supplies;

            foreach (var supplies in MinStorageCapacity)
            {
                if (suppliesLeft.TryGetValue(supplies.Key, out var value))
                {
                    if (value.Value < supplies.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool IsSupplyRequired()
        {
            var suppliesLeft = Storage.Supplies;

            foreach (var supplies in StorageCapacity)
            {
                if (suppliesLeft.TryGetValue(supplies.Key, out var value))
                {
                    if (value.Value < supplies.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Dictionary<SuppliesType, float> GetLackSupplies()
        {
            Dictionary<SuppliesType, float> supplies = new Dictionary<SuppliesType, float>();

            var suppliesLeft = Storage.Supplies;
            foreach (var sup in StorageCapacity)
            {
                if (suppliesLeft.TryGetValue(sup.Key, out var value))
                {
                    float supplyLack = sup.Value - value.Value;
                    supplies.Add(sup.Key, supplyLack);
                }
            }

            return supplies;
        }

        public static (bool, List<Road>) TryFindShortestPath(City from, City to)
        {
            if (from == null || to == null)
            {
                return (false, null);
            }

            return from.TryFindShortestPath(to, new List<Road>());
        }

        private (bool, List<Road>) TryFindShortestPath(City to, List<Road> currentPath)
        {
            if (this == to)
            {
                return (true, currentPath);
            }

            var roads = Roads;
            var availableRoads = new List<Road>();

            var shortestPath = currentPath;
            var pathRouted = false;

            foreach (var road in roads)
            {
                if (road.AvailableForSupply && !currentPath.Contains(road))
                {
                    availableRoads.Add(road);
                }
            }

            foreach (var road in availableRoads)
            {
                var nPath = new List<Road>();
                nPath.AddRange(currentPath);
                nPath.Add(road);
                var (isRouted, route) = road.To.TryFindShortestPath(to, nPath);
                if (isRouted && (!pathRouted || CompareRoutes(route, shortestPath)))
                {
                    pathRouted = true;
                    shortestPath = route;
                }
            }

            return (pathRouted, shortestPath);
        }

        private bool CompareRoutes(List<Road> r1, List<Road> r2)
        {
            float r1Dist = r1.Sum(road => road.Distance);
            float r2Dist = r2.Sum(road => road.Distance);
            return r1Dist < r2Dist;
        }

        public override string ToString()
        {
            return $"City: {WorldPosition}";
        }

        public bool TryConsumeSupplies(SuppliesType type, float value, out float diff)
        {
            float extra = value;
            if (CityType == CityType.Supplier)
            {
                if (GlobalStorage.Instance.TryConsumeSupplies(type, value, out extra))
                {
                    diff = 0;
                    return true;
                }
            }

            diff = 0f;
            Storage.ConsumeSupplies(type, extra);
            return true;
        }

        public bool AddSupplies(SuppliesType type, ISupplies<float> value, out float diff)
        {
            diff = 0f;
            Storage.AddSupplies(type, value);
            return true;
        }
    }

    public enum CityType
    {
        Consumer,
        Supplier
    }
}