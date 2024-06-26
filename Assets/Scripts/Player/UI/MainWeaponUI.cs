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
    public void ChangeWeaponUI(Weapon weapon,int totalAmmoCount)
    {
        if (weapon == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        weaponNameTMP.text = weapon.ItemData.ItemName;
        weaponImage.sprite = ((WeaponItemSO)weapon.ItemData).ItemDetailIcon;
        ChangeAmmoCount(weapon, totalAmmoCount);
    }

    public void ChangeAmmoCount(Weapon weapon,int totalAmmoCount)
    {
        int maxAmmo = 0;
        int currentAmmo = 0;
        if (weapon is Gun)
        {
            maxAmmo = totalAmmoCount;
            currentAmmo = ((Gun)weapon).currentAmmoCount;
            Debug.Log(currentAmmo);
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
