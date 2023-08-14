using System;
using UnityEngine;
using TMPro;

public class EquipmentShopOffer : ShopOffer
{

    [SerializeField] private TextMeshProUGUI attackBonus;
    [SerializeField] private TextMeshProUGUI defenseBonus;
    [SerializeField] private TextMeshProUGUI manaBonus;

    private Equipment setTo;
    public void Set(Equipment setTo, int cost)
    {
        this.setTo = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();

        attackBonus.text = setTo.GetStat(BaseStat.Damage).ToString();
        defenseBonus.text = setTo.GetStat(BaseStat.Defense).ToString();
        manaBonus.text = setTo.GetStat(BaseStat.Mana).ToString();

        GameObject spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTips(setTo, transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipEquipment(setTo);
    }
}
