using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        private Dictionary<City, List<SupplyWagon>> _incomingWagons;
        private Dictionary<City, List<SupplyWagon>> _outgoingWagons;

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

            _incomingWagons = new Dictionary<City, List<SupplyWagon>>();
            _outgoingWagons = new Dictionary<City, List<SupplyWagon>>();
            foreach (var city in _cities)
            {
                _incomingWagons.Add(city, new List<SupplyWagon>(_maxWagonsIncoming));
                _outgoingWagons.Add(city, new List<SupplyWagon>(_maxWagonsOutgoing));
            }

            SupplyWagon.OnDestinationReached += WagonDestinationReached;
            SupplyWagon.OnWagonDestroyed += WagonDestroyed;
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

                    if (TryCreateSuppliesForCity(city, out var supplies))
                    {
                        ChargeSupply(supplier, city, supplies.ToArray());
                    }
                }
            }
        }

        private void ChargeSupply(City from, City to, params ISupplies<float>[] supplies)
        {
            var wagonCapacity = WagonCapacity;
            foreach (var supply in supplies)
            {
                supply.Value = Mathf.Min(supply.Value, wagonCapacity[supply.Type].Value);
            }

            foreach (var supply in supplies)
            {
                from.Storage.ConsumeSupplies(supply.Type, supply.Value);
            }

            SupplyWagon wagon = new SupplyWagon(supplies, from, to);
            WagonsFactory.Instance.CreateWagon(wagon);

            _incomingWagons[to].Add(wagon);
            _outgoingWagons[from].Add(wagon);
        }

        private void WagonDestroyed(SupplyWagon wagon)
        {
            _incomingWagons[wagon.To].Remove(wagon);
            _outgoingWagons[wagon.From].Remove(wagon);
            wagon.Dispose();
        }

        private void WagonDestinationReached(SupplyWagon wagon)
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
            return !supplier.Priority &&
                   consumer.Priority &&
                   supplier.IsAvailableForSupply();
        }

        private bool CanSendWagonTo(City city)
        {
            return _incomingWagons.TryGetValue(city, out var wagons) && wagons.Count < _maxWagonsIncoming;
        }

        private bool CanSendWagonFrom(City city)
        {
            return _outgoingWagons.TryGetValue(city, out var wagons) && wagons.Count < _maxWagonsOutgoing;
        }

        private bool TryCreateSuppliesForCity(City city, out List<ISupplies<float>> supplies)
        {
            supplies = new List<ISupplies<float>>();
            Dictionary<SuppliesType,float> suppliesLack = city.GetLackSupplies();
            Dictionary<SuppliesType, float> suppliesToSend = new Dictionary<SuppliesType, float>();
            suppliesToSend.AddRange(suppliesLack);

            List<SupplyWagon> incomingWagons = new List<SupplyWagon>();
            incomingWagons.AddRange(_incomingWagons[city]);

            foreach (var wagon in incomingWagons)
            {
                var sentSupplies = wagon.Supplies;
                foreach (var supply in sentSupplies)
                {
                    if (suppliesToSend.ContainsKey(supply.Type))
                    {
                        suppliesToSend[supply.Type] -= supply.Value;
                    }
                }
            }
            
            foreach (var (type, value) in suppliesToSend)
            {
                if (value > 0)
                {
                    supplies.Add(CreateSupplyOfType(type, value));
                }
            }

            return supplies.Count > 0;
        }

        private ISupplies<float> CreateSupplyOfType(SuppliesType suppliesType, float value)
        {
            return suppliesType switch
            {
                SuppliesType.Ammo => new Ammo(value),
                SuppliesType.Provision => new Provision(value),
                _ => null
            };
        }

        private City GetNearestSupplier(City to, List<City> suppliers)
        {
            if (suppliers is not { Count: > 1 })
            {
                City supplier = suppliers.FirstOrDefault();
                if (supplier == null || !supplier.IsAvailableForSupply())
                {
                    return null;
                }
                var (canBeRouted, _) = City.TryFindShortestPath(supplier, to);
                return canBeRouted ? supplier : null;
            }

            City sup = null;
            List<Road> shortestPath = null;
            foreach (var supplier in suppliers)
            {
                if (supplier.IsAvailableForSupply())
                {
                    var (isRouted, routes) = City.TryFindShortestPath(supplier, to);
                    if (isRouted &&
                        (shortestPath == null || routes.Count < shortestPath.Count))
                    {
                        sup = supplier;
                        shortestPath = routes;
                    }
                }
            }

            return sup;
        }

        private Dictionary<SuppliesType, ISupplies<float>> _wagonCapacity;
        private Dictionary<SuppliesType, ISupplies<float>> WagonCapacity => _wagonCapacity ??= new Dictionary<SuppliesType, ISupplies<float>>
        {
            { SuppliesType.Ammo, new Ammo(10)}, { SuppliesType.Provision, new Provision(10)}
        };
    }
}