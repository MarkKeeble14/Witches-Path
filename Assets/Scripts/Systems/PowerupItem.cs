﻿using UnityEngine;

public abstract class PowerupItem
{
    protected abstract string SpritePath { get; }

    public virtual string GetAdditionalText()
    {
        return "";
    }

    public abstract Sprite GetSprite();

    public abstract string Name { get; }

    public abstract string ToolTipText { get; }
}
