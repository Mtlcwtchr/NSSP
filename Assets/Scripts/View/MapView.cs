using System.Collections;
using Model;
using UnityEngine;
using View.Factory;

namespace View
{
    public class MapView : MonoBehaviour
    {
        private MapData _mapData;

        private const float T = 2f;
        private float _dT;

        private void Awake()
        {
            StartCoroutine(CreateMap());
        }

        private void Update()
        {
            _dT += Time.deltaTime;
            if (_dT >= T)
            {
                _dT = 0;
                _mapData.LogisticManager.Tick();
            }
        }

        private IEnumerator CreateMap()
        {
            yield return null;

            _mapData = new MapData();
            foreach (var city in _mapData.Cities)
            {
                CityView view = CityFactory.Instance.CreateView(city);
            }

            foreach (var city in _mapData.Cities)
            {
                foreach (var road in city.Roads)
                {
                    RoadView roadView = RoadFactory.Instance.CreateView(road);
                }
            }
        }
    }
}