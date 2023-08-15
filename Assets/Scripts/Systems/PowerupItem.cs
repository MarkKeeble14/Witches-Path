﻿using UnityEngine;

public abstract class PowerupItem
{
    protected abstract string SpritePath { get; }

    public virtual ToolTipKeyword[] Keywords => new ToolTipKeyword[] { };

    public virtual AfflictionType[] AfflictionKeywords => new AfflictionType[] { };

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
