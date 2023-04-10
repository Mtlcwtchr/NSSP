using System;
using Model;
using UnityEngine;

namespace View.Factory
{
    public class RoadFactory: MonoBehaviour
    {
        [SerializeField] private RoadView prefab;

        private static RoadFactory _instance;
        public static RoadFactory Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public RoadView CreateView(Road road)
        {
            RoadView view = Instantiate(prefab, (road.To.WorldPosition + road.From.WorldPosition) / 2, Quaternion.identity, null);
            view.Model = road;
            view.Draw();
            return view;
        }
    }
}