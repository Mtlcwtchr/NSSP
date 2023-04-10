using System;
using Model;
using TMPro;
using UnityEngine;

namespace View
{
    public class CityView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private TMP_Text priorityLabel;
        [SerializeField] private TMP_Text ammoLabel;
        [SerializeField] private TMP_Text provisionLabel;

        private Material _mat;

        private City _model;

        public City Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    switch (_model.CityType)
                    {
                        case CityType.Consumer:
                            _model.OnPriorityChanged += PriorityChanged;
                            _model.Storage.OnSuppliesUpdated += SuppliesUpdated;
                            PriorityChanged(_model.Priority);
                            foreach (var (key, supplies) in _model.Storage.Supplies)
                            {
                                SuppliesUpdated(key, supplies.Value);
                            }
                            break;
                        case CityType.Supplier:
                            ammoLabel.text = "ammo: Inf.";
                            provisionLabel.text = "provision: Inf.";
                            break;
                    }
                }
            }
        }

        private void PriorityChanged(float prior)
        {
            priorityLabel.text = $"priority: {prior}";
        }

        private void SuppliesUpdated(SuppliesType key, float value)
        {
            switch (key)
            {
                case SuppliesType.Ammo:
                    ammoLabel.text = $"ammo: {value}";
                    break;
                case SuppliesType.Provision:
                    provisionLabel.text = $"provision: {value}";
                    break;
            }
        }

        private void Awake()
        {
            _mat = Instantiate(meshRenderer.sharedMaterial);
            meshRenderer.sharedMaterial = _mat;
        }

        private void OnDestroy()
        {
            _model.OnPriorityChanged -= PriorityChanged;
            _model.Storage.OnSuppliesUpdated -= SuppliesUpdated;
        }

        public void SetColor(Color color)
        {
            _mat.color = color;
        }
    }
}