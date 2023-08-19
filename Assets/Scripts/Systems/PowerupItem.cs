using System.Collections.Generic;
using UnityEngine;

public abstract class PowerupItem : ToolTippable
{
    public abstract string Name { get; }
    protected abstract string SpritePath { get; }
    protected abstract string toolTipText { get; }

    protected List<ToolTipKeyword> GeneralKeywords = new List<ToolTipKeyword>();
    protected List<AfflictionType> AfflictionKeywords = new List<AfflictionType>();

    public abstract Sprite GetSprite();

    public PowerupItem()
    {
        SetKeywords();
    }

    protected abstract void SetKeywords();

    public virtual string GetAdditionalText()
    {
        return "";
    }

    // ToolTippable
    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return AfflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return GeneralKeywords;
    }

    public virtual string GetToolTipLabel()
    {
        return Name;
    }

    public virtual string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(toolTipText);
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }
}
