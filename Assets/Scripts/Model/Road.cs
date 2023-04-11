using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class Road
    {
        public event Action<bool> OnBlockStatusChanged;
        public event Action<float> OnCapacityChanged;

        public City From { get; private set; }
        public City To { get; private set; }
        public float Distance { get; private set; }

        public Road Inversed { get; set; }

        public float CapacityFactor => Mathf.Max(_wagons?.Count ?? 0, 1.0f) * 0.1f;

        private List<SupplyWagon> _wagons;

        public bool AvailableForSupply => From.WarSide == To.WarSide &&
                                          !IsBlockedByAnyReason;

        private List<string> _blockers;

        public bool IsBlockedByAnyReason => _blockers?.Count > 0;

        public bool AddBlocker(string blockerId)
        {
            Inversed?.AddBlocker(blockerId);
            _blockers ??= new List<string>();
            if(!_blockers.Contains(blockerId))
            {
                _blockers.Add(blockerId);
                OnBlockStatusChanged?.Invoke(IsBlockedByAnyReason);
                return true;
            }

            return false;
        }

        public bool ClearBlocker(string blockerId)
        {
            Inversed?.ClearBlocker(blockerId);
            if (_blockers == null)
            {
                return false;
            }
            if (_blockers.Contains(blockerId))
            {
                _blockers.Remove(blockerId);
                OnBlockStatusChanged?.Invoke(IsBlockedByAnyReason);
                return true;
            }

            return false;
        }

        public void InvolveInDelivery(SupplyWagon wagon)
        {
            Inversed?.InvolveInDelivery(wagon);
            _wagons ??= new List<SupplyWagon>();
            if (!_wagons.Contains(wagon))
            {
                _wagons.Add(wagon);
                OnCapacityChanged?.Invoke(CapacityFactor);
            }
        }

        public void ExcludeFromDelivery(SupplyWagon wagon)
        {
            Inversed?.ExcludeFromDelivery(wagon);
            if (_wagons == null)
            {
                return;
            }
            if (_wagons.Contains(wagon))
            {
                _wagons.Remove(wagon);
                OnCapacityChanged?.Invoke(CapacityFactor);
            }
        }

        public Road(City from, City to, float distance)
        {
            From = from;
            To = to;
            Distance = distance;
        }

        public override string ToString()
        {
            return $"From: {From}, To:{To}, Distance:{Distance}, Wagons On: {_wagons?.Count}, Capacity: {CapacityFactor}";
        }
    }
}