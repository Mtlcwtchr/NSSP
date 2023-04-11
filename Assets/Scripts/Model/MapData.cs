using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class MapData
    {
        private List<City> _cities;
        private LogisticManager _logisticManager;

        public List<City> Cities => _cities;
        public LogisticManager LogisticManager => _logisticManager;

        public MapData()
        {
            var c1 = new City(CityType.Consumer, WarSide.Player, new Vector3(0f, 8f, 0f), CreateDefaultStorage());
            var c2 = new City(CityType.Consumer, WarSide.Player, new Vector3(2f, 7f, 0f), CreateDefaultStorage());
            var c3 = new City(CityType.Consumer, WarSide.Player, new Vector3(3f, 5f, 0f), CreateDefaultStorage());
            var c4 = new City(CityType.Supplier, WarSide.Player, new Vector3(1f, 4f, 0f), CreateDefaultStorage());
            var c5 = new City(CityType.Consumer, WarSide.Player, new Vector3(-1.25f, 5f, 0f), CreateDefaultStorage());
            var c6 = new City(CityType.Consumer, WarSide.Player, new Vector3(-2.5f, 7f, 0f), CreateDefaultStorage());

            var c7 = new City(CityType.Consumer, WarSide.Player, new Vector3(-10f, 0f, 0f), CreateDefaultStorage());
            var c8 = new City(CityType.Consumer, WarSide.Player, new Vector3(-15f, 2f, 0f), CreateDefaultStorage());
            var c9 = new City(CityType.Consumer, WarSide.Player, new Vector3(-20f, 0f, 0f), CreateDefaultStorage());
            
            var c10 = new City(CityType.Supplier, WarSide.Player, new Vector3(-17.5f, -1.5f, 0f), CreateDefaultStorage());
            var c11 = new City(CityType.Consumer, WarSide.Enemy, new Vector3(-25f, 0f, 0f), CreateDefaultStorage());
            var c12 = new City(CityType.Consumer, WarSide.Enemy, new Vector3(-25f, 5f, 0f), CreateDefaultStorage());
            var c13 = new City(CityType.Consumer, WarSide.Enemy, new Vector3(-25f, 10f, 0f), CreateDefaultStorage());
            var c14 = new City(CityType.Consumer, WarSide.Enemy, new Vector3(-15f, 10f, 0f), CreateDefaultStorage());
            var c15 = new City(CityType.Consumer, WarSide.Enemy, new Vector3(-20f, 7.5f, 0f), CreateDefaultStorage());

            c1.AddRoad(c2, GetDist(c1, c2));
            c2.AddRoad(c3, GetDist(c2, c3));
            c3.AddRoad(c4, GetDist(c3, c4));
            c4.AddRoad(c5, GetDist(c4, c5));
            c5.AddRoad(c6, GetDist(c5, c6));
            c6.AddRoad(c1, GetDist(c6, c1));

            c4.AddRoad(c7, GetDist(c4, c7));
            c7.AddRoad(c8, GetDist(c7, c8));
            c8.AddRoad(c9, GetDist(c8, c9));

            c9.AddRoad(c10, GetDist(c9, c10));
            c9.AddRoad(c11, GetDist(c9, c11));
            c15.AddRoad(c14, GetDist(c15, c14));
            c15.AddRoad(c13, GetDist(c15, c13));
            c15.AddRoad(c12, GetDist(c15, c12));
            c15.AddRoad(c8, GetDist(c15, c8));

            _cities = new List<City>()
            {
                c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14, c15
            };

            _logisticManager = new LogisticManager(_cities, 3, 3);
        }

        private float GetDist(City from, City to)
        {
            return (to.WorldPosition - from.WorldPosition).magnitude;
        }

        private Storage<float> CreateDefaultStorage()
        {
            var ammo = new Ammo(10);
            var provision = new Provision(10);
            return new Storage<float>(new Dictionary<SuppliesType, ISupplies<float>>()
            {
                { ammo.Type, ammo },
                { provision.Type, provision }
            });
        }
    }
}