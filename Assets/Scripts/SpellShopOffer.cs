﻿using System;
using UnityEngine;

public class SpellShopOffer : ShopOffer
{
    [SerializeField] private Spell spell;
    [SerializeField] private VisualSpellDisplay visualSpellDisplay;

    protected override ToolTippable toolTippable => spell;

    public void Set(Spell setTo, int cost)
    {
        spell = setTo;
        this.cost = cost;
        visualSpellDisplay.SetSpell(setTo);

        visualSpellDisplay.SetCVLocked(true);
        visualSpellDisplay.GetCanvasGroup().blocksRaycasts = false;

        costText.text = cost.ToString();
    }

    protected override void Purchase()
    {
        GameManager._Instance.AddSpellToSpellBook(spell);
        DestroyToolTip();
    }

    private void Update()
    {
        bool hide = GameManager._Instance.OverlaidUIOpen || MapManager._Instance.MapOpen;
        cv.blocksRaycasts = !hide;
        cv.alpha = hide ? 0 : 1;
    }
}
