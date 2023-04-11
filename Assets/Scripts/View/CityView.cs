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
                    _model.OnWarSideChanged += WarSideChanged;
                    _model.OnPriorityChanged += PriorityChanged;
                    _model.Storage.OnSuppliesUpdated += SuppliesUpdated;
                    WarSideChanged(_model.WarSide);
                    PriorityChanged(_model.Priority);
                    foreach (var (key, supplies) in _model.Storage.Supplies)
                    {
                        SuppliesUpdated(key, supplies.Value);
                    }
                }
            }
        }

        private void WarSideChanged(WarSide side)
        {
            SetColor(side switch {
                WarSide.Player => Color.blue,
                WarSide.Enemy => Color.red,
                _ => Color.black
            });

            UpdateColor(_model.WarSide, _model.CityType, _model.Priority);
        }

        private void PriorityChanged(bool prior)
        {
            UpdateColor(_model.WarSide, _model.CityType, _model.Priority);
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

        public void UpdateColor(WarSide warSide, CityType cityType, bool isPrior)
        {
            Color mainColor = warSide switch
            {
                WarSide.Player => Color.blue,
                WarSide.Enemy => Color.red,
                _ => Color.black
            };

            Color typeColor = cityType switch
            {
                CityType.Consumer => Color.clear,
                CityType.Supplier => Color.green,
                _ => Color.black
            };

            Color priorColor = isPrior ? Color.yellow : Color.clear;

            SetColor(Color.Lerp(Color.Lerp(mainColor,typeColor, 0.5f), priorColor, 0.25f));
        }

        public void SetColor(Color color)
        {
            _mat.color = color;
        }
    }
}