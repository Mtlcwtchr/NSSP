using System.Linq;
using Model;
using TMPro;
using UnityEngine;

namespace View
{
    public class SupplyWagonView : MonoBehaviour
    {
        [SerializeField] private float velocity;

        [SerializeField] private TMP_Text ammoLabel;
        [SerializeField] private TMP_Text provisionLabel;

        private SupplyWagon _model;

        public void AttachModel(SupplyWagon model)
        {
            _model = model;

            ammoLabel.text = $"ammo: {_model.Supplies.FirstOrDefault(sup => sup.Type == SuppliesType.Ammo)?.Value}";
            provisionLabel.text = $"provision: {_model.Supplies.FirstOrDefault(sup => sup.Type == SuppliesType.Provision)?.Value}";
        }

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            var destination = _model.CurrentRoad.To.WorldPosition;
            var position = transform.position;
            var delta = (destination - position).normalized;
            transform.position += delta * (velocity * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (_model == null)
            {
                return;
            }

            if (col.TryGetComponent<CityView>(out var city))
            {
                _model.CityReached(city.Model);
            }
        }

        public void DestroyWagon()
        {
            _model.DestroyWagon();
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}