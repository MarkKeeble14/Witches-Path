using System.Collections.Generic;
using UnityEngine;

public abstract class PowerupItem
{
    protected abstract string SpritePath { get; }

    public List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    public List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();

    protected abstract void SetKeywords();

    public PowerupItem()
    {
        SetKeywords();
    }

    public virtual string GetAdditionalText()
    {
        return "";
    }

    public abstract Sprite GetSprite();

    public abstract string Name { get; }

    protected abstract string toolTipText { get; }

    public virtual string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }
}
