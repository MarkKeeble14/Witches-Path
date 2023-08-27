using System;
using UnityEngine;

public class SpellShopOffer : ShopOffer
{
    [SerializeField] private Spell spell;

    public void Set(SpellLabel setTo, int cost)
    {
        spell = Spell.GetSpellOfType(setTo);
        this.cost = cost;

        itemText.text = Utils.SplitOnCapitalLetters(setTo.ToString());
        costText.text = cost.ToString();

        // Tool Tips
        onPointerEnter += SpawnToolTip;
        onPointerExit += DestroyToolTip;
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipSpell(spell.Label);
    }

    // Tool Tips
    private GameObject spawnedToolTip;

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(spell, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
