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

        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipEquipment(representingEquipment);
    }

    // Tool Tips
    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnComparisonToolTips(
            new ToolTippableComparisonData[]
                {
                        new ToolTippableComparisonData("Offering: ", representingEquipment),
                        new ToolTippableComparisonData("Current: ", GameManager._Instance.GetEquippedEquipmentOfSameType(representingEquipment))
                },
            transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
