using System;
using System.Collections.Generic;
using View;

namespace Model
{
    public class SupplyWagon
    {
        public static event Action<SupplyWagon> OnDestinationReached;
        public static event Action<SupplyWagon> OnWagonDestroyed;

        private ISupplies<float>[] _supplies;

        private City _from;
        private City _to;

        private Road _current;
        private List<Road> _cachedPath;

        private SupplyWagonView _view;

        public Road CurrentRoad => _current;

        public City From => _from;
        public City To => _to;

        public ISupplies<float>[] Supplies => _supplies;

        public SupplyWagon(ISupplies<float>[] supplies, City from, City to)
        {
            _supplies = supplies;

            _from = from;
            _to = to;

            _cachedPath = CalculatePath(from, to);
            _current = _cachedPath[0];
            foreach (var road in _cachedPath)
            {
                road.InvolveInDelivery(this);
            }
        }

        public void DestroyWagon()
        {
            OnWagonDestroyed?.Invoke(this);
        }

        public void CityReached(City city)
        {
            if (city == _to)
            {
                OnDestinationReached?.Invoke(this);
                return;
            }

            if (city == _current.To)
            {
                _current = _cachedPath[_cachedPath.IndexOf(_current) + 1];
            }
        }

        public void AttachView(SupplyWagonView view)
        {
            _view = view;
        }

        public void Dispose()
        {
            foreach (var road in _cachedPath)
            {
                road.ExcludeFromDelivery(this);
            }
            _view.Dispose();
        }

        private List<Road> CalculatePath(City from, City to)
        {
            var (_, path) = City.TryFindShortestPath(from, to);
            return path;
        }
    }
}