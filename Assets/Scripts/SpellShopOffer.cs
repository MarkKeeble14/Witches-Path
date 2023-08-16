using System;
using UnityEngine;

public class SpellShopOffer : ShopOffer
{
    [SerializeField] private SpellLabel label;

    public void Set(SpellLabel setTo, int cost, Action onClick)
    {
        label = setTo;
        this.cost = cost;

        itemText.text = Utils.SplitOnCapitalLetters(setTo.ToString());
        costText.text = cost.ToString();

        Spell spell = GameManager._Instance.GetSpellOfType(setTo);

        GameObject spawnedToolTip = null;
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(spell, transform);
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
