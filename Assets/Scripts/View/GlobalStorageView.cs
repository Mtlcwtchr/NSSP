using System;
using System.Collections;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class GlobalStorageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoLabel;
        [SerializeField] private TMP_Text provisionLabel;

        [SerializeField] private Button addAmmoButton;
        [SerializeField] private Button removeAmmoButton;
        
        [SerializeField] private Button addProvisionButton;
        [SerializeField] private Button removeProvisionButton;

        private GlobalStorage GlobalStorage => GlobalStorage.Instance;
        
        private void Awake()
        {
            StartCoroutine(AttachToModel());
        }

        private void OnDestroy()
        {
            GlobalStorage.OnStorageUpdated -= StorageUpdated;
            
            addAmmoButton.onClick.RemoveListener(AddAmmo);
            addProvisionButton.onClick.RemoveListener(AddProvision);
            removeAmmoButton.onClick.RemoveListener(RemoveAmmo);
            removeProvisionButton.onClick.RemoveListener(RemoveProvision);
        }

        private IEnumerator AttachToModel()
        {
            yield return null;
            
            GlobalStorage.OnStorageUpdated += StorageUpdated;
            StorageUpdated();
            
            addAmmoButton.onClick.AddListener(AddAmmo);
            addProvisionButton.onClick.AddListener(AddProvision);
            removeAmmoButton.onClick.AddListener(RemoveAmmo);
            removeProvisionButton.onClick.AddListener(RemoveProvision);
        }

        private void StorageUpdated()
        {
            if (GlobalStorage.TryGetSupplies(SuppliesType.Ammo, out var ammo))
            {
                ammoLabel.text = $"Ammo: {ammo.Value}";
            }
            if (GlobalStorage.TryGetSupplies(SuppliesType.Provision, out var provision))
            {
                provisionLabel.text = $"Provision: {provision.Value}";
            }
        }

        private void RemoveProvision()
        {
            GlobalStorage.TryConsumeSupplies(SuppliesType.Provision, 100, out _);
        }

        private void RemoveAmmo()
        {
            GlobalStorage.TryConsumeSupplies(SuppliesType.Ammo, 100, out _);
        }

        private void AddAmmo()
        {
            GlobalStorage.AddSupplies(SuppliesType.Ammo, new Ammo(100));
        }
        
        private void AddProvision()
        {
            GlobalStorage.AddSupplies(SuppliesType.Provision, new Provision(100));
        }
    }
}