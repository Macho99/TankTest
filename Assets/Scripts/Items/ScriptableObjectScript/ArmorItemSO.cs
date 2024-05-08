using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum ArmorType { Helmet, Armor }
[CreateAssetMenu(fileName = "New Armor", menuName = "SO/ItemSO/Armor")]
public class ArmorItemSO : EquipmentItemSO
{
    [SerializeField] private ArmorType armorType;
    [SerializeField] private float reductionAmount;


    public override ItemInstance CreateItemData(int count = 1)
    {
        return new ArmorItemInstance(this);
    }
}


public class ArmorItemInstance : EquipmentItemInstance
{
    public ArmorItemInstance(ItemSO itemData, int count = 1) : base(itemData,count)
    {
    }
    public override ItemInstance Clone()
    {
        ArmorItemInstance cloneItem = new ArmorItemInstance(this.ItemData);

        return cloneItem;
    }
    public override Item CreateNetworkItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);
        item.Init(this, count);
        item.name = this.itemData.ItemName;
        return item;
    }
}