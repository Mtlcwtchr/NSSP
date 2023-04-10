using Model;
using UnityEngine;

namespace View
{
    public class SupplyWagonView : MonoBehaviour
    {
        [SerializeField] private float velocity;

        private SupplyWagon<float> _model;

        public void AttachModel(SupplyWagon<float> model)
        {
            _model = model;
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