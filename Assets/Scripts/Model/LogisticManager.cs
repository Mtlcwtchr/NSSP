using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using View.Factory;

namespace Model
{
    public class LogisticManager
    {
        private int _maxWagonsIncoming;
        private int _maxWagonsOutgoing;

        private List<City> _cities;
        private List<City> _suppliers;
        private Dictionary<City, List<SupplyWagon<float>>> _incomingWagons;
        private Dictionary<City, List<SupplyWagon<float>>> _outgoingWagons;

        public LogisticManager(List<City> cities, int maxWagonsIncoming, int maxWagonsOutgoing)
        {
            _cities = new List<City>();
            _cities.AddRange(cities);

            _suppliers = new List<City>();
            foreach (var city in _cities)
            {
                if (city.CityType == CityType.Supplier)
                {
                    _suppliers.Add(city);
                }
            }

            _maxWagonsIncoming = maxWagonsIncoming;
            _maxWagonsOutgoing = maxWagonsOutgoing;

            _incomingWagons = new Dictionary<City, List<SupplyWagon<float>>>();
            _outgoingWagons = new Dictionary<City, List<SupplyWagon<float>>>();
            foreach (var city in _cities)
            {
                _incomingWagons.Add(city, new List<SupplyWagon<float>>(_maxWagonsIncoming));
                _outgoingWagons.Add(city, new List<SupplyWagon<float>>(_maxWagonsOutgoing));
            }

            SupplyWagon<float>.OnDestinationReached += WagonDestinationReached;
            SupplyWagon<float>.OnWagonDestroyed += WagonDestroyed;
        }

        public void Tick()
        {
            var allyConsumers = _cities.Where(city => city.CityType == CityType.Consumer && city.WarSide == WarSide.Player).ToList();
            var cities = new List<City>();
            cities.AddRange(allyConsumers);
            foreach (var city in cities)
            {
                if (RequiresSupplies(city) && CanSendWagonTo(city))
                {
                    var suppliers = new List<City>();
                    suppliers.AddRange(_suppliers);
                    var supplier = GetNearestSupplier(city, suppliers);
                    while (supplier != null &&
                           !CanSendWagonFrom(supplier))
                    {
                        suppliers.Remove(supplier);
                        supplier = GetNearestSupplier(city, suppliers);
                    }

                    if (supplier == null)
                    {
                        var pseudoSuppliers = new List<City>();
                        pseudoSuppliers.AddRange(allyConsumers);

                        supplier = GetNearestSupplier(city, pseudoSuppliers.Where(candidate => CanProvideSupplies(candidate, city)).ToList());
                        while (supplier != null &&
                               !CanSendWagonFrom(supplier))
                        {
                            pseudoSuppliers.Remove(supplier);
                            supplier = GetNearestSupplier(city, pseudoSuppliers.Where(candidate => CanProvideSupplies(candidate, city)).ToList());
                        }

                        if (supplier == null)
                        {
                            Debug.Log("No available supplier");
                            return;
                        }
                    }

                    ChargeSupply(supplier, city, CreateDefaultAmmoSupplies(), CreateDefaultProvisionSupplies());
                }
            }
        }

        private void ChargeSupply(City from, City to, params ISupplies<float>[] supplies)
        {
            foreach (var supply in supplies)
            {
                from.Storage.ConsumeSupplies(supply.Type, supply.Value);
            }

            SupplyWagon<float> wagon = new SupplyWagon<float>(supplies, from, to);
            WagonsFactory.Instance.CreateWagon(wagon);

            _incomingWagons[to].Add(wagon);
            _outgoingWagons[from].Add(wagon);
        }

        private void WagonDestroyed(SupplyWagon<float> wagon)
        {
            _incomingWagons[wagon.To].Remove(wagon);
            _outgoingWagons[wagon.From].Remove(wagon);
            wagon.Dispose();
        }

        private void WagonDestinationReached(SupplyWagon<float> wagon)
        {
            _incomingWagons[wagon.To].Remove(wagon);
            _outgoingWagons[wagon.From].Remove(wagon);

            foreach (var supplies in wagon.Supplies)
            {
                wagon.To.Storage.AddSupplies(supplies.Type, supplies);
            }

            wagon.Dispose();
        }

        private bool RequiresSupplies(City city)
        {
            return city.IsSupplyRequired();
        }

        private bool CanProvideSupplies(City supplier, City consumer)
        {
            return supplier.Priority < consumer.Priority && supplier.IsAvailableForSupply();
        }

        private bool CanSendWagonTo(City city)
        {
            return _incomingWagons.TryGetValue(city, out var wagons) && wagons.Count < _maxWagonsIncoming;
        }

        private bool CanSendWagonFrom(City city)
        {
            return _outgoingWagons.TryGetValue(city, out var wagons) && wagons.Count < _maxWagonsOutgoing;
        }

        private Ammo CreateDefaultAmmoSupplies()
        {
            return new Ammo(5);
        }

        private Provision CreateDefaultProvisionSupplies()
        {
            return new Provision(5);
        }

        private City GetNearestSupplier(City to, List<City> suppliers)
        {
            if (suppliers is not { Count: > 1 })
            {
                return suppliers.FirstOrDefault();
            }

            City sup = suppliers[0];
            List<Road> shortestPath = null;
            foreach (var supplier in suppliers)
            {
                var (isRouted, routes) = City.TryFindShortestPath(supplier, to);
                if (isRouted && (shortestPath == null || routes.Count < shortestPath.Count))
                {
                    sup = supplier;
                    shortestPath = routes;
                }
            }

            return sup;
        }
    }
}