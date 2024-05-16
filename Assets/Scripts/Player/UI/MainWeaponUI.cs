using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainWeaponUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameTMP;
    [SerializeField] private Image weaponImage;
    [SerializeField] private TextMeshProUGUI ammoCount;
    [SerializeField] private Image[] ammoImages;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }
    public void ChangeWeaponUI(Weapon weapon)
    {
        if (weapon == null)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        weaponNameTMP.text = weapon.ItemData.ItemName;
        weaponImage.sprite = ((WeaponItemSO)weapon.ItemData).ItemDetailIcon;
        ChangeAmmoCount(weapon);
    }

    public void ChangeAmmoCount(Weapon weapon)
    {
        int maxAmmo = 0;
        int currentAmmo = 0;
        if (weapon is Gun)
        {
            maxAmmo = ((GunItemSO)weapon.ItemData).MaxAmmoCount;
            currentAmmo = ((Gun)weapon).currentAmmoCount;
            for (int i = 0; i < ammoImages.Length; i++)
            {

                ammoImages[i].gameObject.SetActive(i < currentAmmo);

            }
        }
        else if (weapon is MilyWeapon)
        {
            maxAmmo = 1;
            currentAmmo = 1;
            foreach (Image image in ammoImages)
            {
                if (image.gameObject.activeSelf)
                    image.gameObject.SetActive(false);
            }
        }


        ammoCount.text = $"{currentAmmo}/{maxAmmo}";

    }
}
