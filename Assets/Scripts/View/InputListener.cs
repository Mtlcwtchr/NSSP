using Model;
using UnityEngine;

namespace View
{
    public class InputListener : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.TryGetComponent<CityView>(out var cityView))
                    {
                        cityView.Model.Priority = ++cityView.Model.Priority % 10;
                    }

                    if (hit.collider.TryGetComponent<SupplyWagonView>(out var wagonView))
                    {
                        wagonView.DestroyWagon();
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.TryGetComponent<CityView>(out var cityView))
                    {
                        cityView.Model.Storage.ConsumeSupplies(SuppliesType.Ammo, 5);
                        cityView.Model.Storage.ConsumeSupplies(SuppliesType.Provision, 5);
                    }
                }
            }
        }
    }
}