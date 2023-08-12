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

        ToolTip spawnedToolTip = null;
        string finalizedToolTipText = GameManager._Instance.FillToolTipText(ContentType.ActiveSpell, setTo.ToString(), GameManager._Instance.GetSpellOfType(setTo).ToolTipText);
        onPointerEnter += delegate
        {
            spawnedToolTip = UIManager._Instance.SpawnToolTip(finalizedToolTipText, transform, new Vector3(toolTipOffset, 0, 0));
        };
        onPointerExit += delegate
        {
            Destroy(spawnedToolTip.gameObject);
        };
    }

    protected override void Purchase()
    {
        GameManager._Instance.EquipSpell(label);
    }
}
