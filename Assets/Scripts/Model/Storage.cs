using System;
using System.Collections.Generic;

namespace Model
{
    public class Storage<T>
    {
        public event Action<SuppliesType, T> OnSuppliesUpdated;

        private readonly Dictionary<SuppliesType, ISupplies<T>> _supplies;

        public Dictionary<SuppliesType, ISupplies<T>> Supplies => _supplies;

        public Storage(Dictionary<SuppliesType, ISupplies<T>> supplies)
        {
            _supplies = supplies;
        }

        public void AddSupplies(SuppliesType key, ISupplies<T> value)
        {
            if (_supplies.TryGetValue(key, out var oldValue))
            {
                oldValue.Add(value);
                _supplies[key] = oldValue;
                OnSuppliesUpdated?.Invoke(key, oldValue.Value);
            }
        }

        public void ConsumeSupplies(SuppliesType key, T value)
        {
            if (_supplies.TryGetValue(key, out var oldValue))
            {
                oldValue.Consume(value);
                _supplies[key] = oldValue;
                OnSuppliesUpdated?.Invoke(key, oldValue.Value);
            }
        }
    }
}