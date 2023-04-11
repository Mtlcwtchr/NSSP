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
                        if (cityView.Model.WarSide == WarSide.Player)
                        {
                            cityView.Model.Priority = !cityView.Model.Priority;
                        }
                    }

                    if (hit.collider.TryGetComponent<SupplyWagonView>(out var wagonView))
                    {
                        wagonView.DestroyWagon();
                    }

                    if (hit.collider.TryGetComponent<RoadView>(out var roadView))
                    {
                        if (!roadView.Model.AddBlocker("enemy"))
                        {
                            roadView.Model.ClearBlocker("enemy");
                        }
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
                        cityView.Model.WarSide = (cityView.Model.WarSide == WarSide.Player ? WarSide.Enemy : WarSide.Player);
                    }
                }
            }
        }
    }
}