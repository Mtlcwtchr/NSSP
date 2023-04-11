using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class GlobalStorage
    {
        public event Action OnStorageUpdated;

        private readonly Storage<float> _storage;

        private static readonly GlobalStorage _instance;
        public static readonly GlobalStorage Instance = _instance ??= new GlobalStorage(new Dictionary<SuppliesType, ISupplies<float>>()
        {
            { SuppliesType.Ammo, new Ammo(1000)},
            { SuppliesType.Provision, new Provision(1000) }
        });

        private GlobalStorage(Dictionary<SuppliesType, ISupplies<float>> supplies)
        {
            _storage = new Storage<float>(supplies);
        }

        public bool CanSendSupplies()
        {
            return _storage.Supplies.All(sup => sup.Value.Value > 0f);
        }
        public void AddSupplies(SuppliesType type, ISupplies<float> supplies)
        {
            _storage.AddSupplies(type, supplies);
            OnStorageUpdated?.Invoke();
        }

        public bool TryGetSupplies(SuppliesType type, out ISupplies<float> value)
        {
            return _storage.Supplies.TryGetValue(type, out value);
        }

        public bool TryConsumeSupplies(SuppliesType type, float value, out float diff)
        {
            if (!_storage.Supplies.ContainsKey(type))
            {
                diff = value;
                return false;
            }
            
            if (_storage.Supplies[type].Value < value)
            {
                float consumption = value - _storage.Supplies[type].Value;
                _storage.Supplies[type].Value = 0;
                diff = consumption;
                OnStorageUpdated?.Invoke();
                return false;
            }
            
            _storage.Supplies[type].Value -= value;
            diff = 0;
            OnStorageUpdated?.Invoke();
            return true;
        }
    }
}