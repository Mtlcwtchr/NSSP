using Model;
using UnityEngine;

namespace View.Factory
{
    public class WagonsFactory : MonoBehaviour
    {
        [SerializeField] private SupplyWagonView prefab;

        private static WagonsFactory _instance;
        public static WagonsFactory Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        public SupplyWagonView CreateWagon(SupplyWagon wagon)
        {
            SupplyWagonView wagonView = Instantiate(prefab, wagon.From.WorldPosition, Quaternion.identity);
            wagon.AttachView(wagonView);
            wagonView.AttachModel(wagon);
            return wagonView;
        }
    }
}