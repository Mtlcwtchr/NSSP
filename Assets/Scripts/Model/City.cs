using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model
{
    public class City
    {
        public event Action<float> OnPriorityChanged;
        
        public CityType CityType { get; }
        public Vector3 WorldPosition { get; private set; }
        public WarSide WarSide { get; private set; }
        public List<Road> Roads { get; private set; }
        
        public Storage<float> Storage { get; private set; }

        public List<ISupplies<float>> MinSupplies = new List<ISupplies<float>>()
        {
            new Provision(5), new Ammo(5)
        };

        private float _priority;
        public float Priority
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
                    road.Inversed = inversed;
                }
            }

            return road;
        }

        public bool IsConnectedTo(City to)
        {
            return Roads.Any(road => road.To == to);
        }
        
        public bool IsAvailableForSupply()
        {
            if (CityType == CityType.Supplier)
            {
                return true;
            }
            
            var suppliesLeft = Storage.Supplies;

            foreach (var supplies in MinSupplies)
            {
                if (suppliesLeft.TryGetValue(supplies.Type, out var value))
                {
                    if (value.Value < supplies.Value * 2)
                    {
                        Debug.Log($"{this} is not available for supply, {supplies.Type}={value.Value} less than {supplies.Value * 2}");
                        return false;
                    }
                }
            }
            
            Debug.Log($"{this} is available for supply");
            return true;
        }

        public bool IsSupplyRequired()
        {
            if (CityType == CityType.Supplier)
            {
                return false;
            }
            
            var suppliesLeft = Storage.Supplies;

            foreach (var supplies in MinSupplies)
            {
                if (suppliesLeft.TryGetValue(supplies.Type, out var value))
                {
                    if (value.Value < supplies.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static (bool, List<Road>) TryFindShortestPath(City from, City to)
        {
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
    }

    public enum CityType
    {
        Consumer,
        Supplier
    }
}