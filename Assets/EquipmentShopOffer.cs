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

        ToolTip spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTip(setTo.ToolTipText, transform, new Vector3(toolTipOffset, 0, 0));
        };
        onPointerExit += delegate
        {
            Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipEquipment(setTo);
    }
}
