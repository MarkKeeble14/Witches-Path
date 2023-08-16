using System;
using UnityEngine;
using TMPro;

public class EquipmentShopOffer : ShopOffer
{
    [SerializeField] private TextMeshProUGUI attackBonus;
    [SerializeField] private TextMeshProUGUI defenseBonus;
    [SerializeField] private TextMeshProUGUI manaBonus;
    private Equipment representingEquipment;

    public void Set(Equipment e, int cost)
    {
        this.representingEquipment = e;
        this.cost = cost;
        itemText.text = e.GetName();

        attackBonus.text = e.GetStat(BaseStat.Damage).ToString();
        defenseBonus.text = e.GetStat(BaseStat.Defense).ToString();
        manaBonus.text = e.GetStat(BaseStat.Mana).ToString();

        GameObject spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnComparisonToolTips(
                new ToolTippableComparisonData[]
                    {
                        new ToolTippableComparisonData("Offering: ", e),
                        new ToolTippableComparisonData("Current: ", GameManager._Instance.GetEquippedEquipmentOfSameType(e))
                    },
                transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipEquipment(representingEquipment);
    }
}
