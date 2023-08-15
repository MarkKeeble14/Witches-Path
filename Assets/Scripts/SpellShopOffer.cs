using System;
using UnityEngine;

public class SpellShopOffer : ShopOffer
{
    [SerializeField] private SpellLabel label;

    public void Set(SpellLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;
        itemText.text = setTo.ToString();
        costText.text = cost.ToString();

        GameObject spawnedToolTip = null;
        Spell spell = GameManager._Instance.GetSpellOfType(setTo);
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTips(spell, transform);
        };
        onPointerExit += delegate
        {
            if (spawnedToolTip != null)
                Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipSpell(label);
    }
}
