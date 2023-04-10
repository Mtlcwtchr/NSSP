using System;
using Model;
using UnityEngine;

namespace View.Factory
{
    public class CityFactory: MonoBehaviour
    {
        [SerializeField] private CityView prefab;

        private static CityFactory _instance;
        public static CityFactory Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public CityView CreateView(City city)
        {
            CityView view = Instantiate(prefab, city.WorldPosition, Quaternion.identity, null);
            view.Model = city;
            view.SetColor(city.WarSide switch
            {
                WarSide.Player => Color.blue,
                WarSide.Enemy => Color.red,
                _ => Color.black
            } );
            return view;
        }
    }
}