using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTextStyle
{
    UpAndFade,
    Fade,
}

public enum EffectIconStyle
{
    Fade,
    FadeAndGrow
}

public class EffectTextDisplay : MonoBehaviour
{
    [SerializeField] private EffectText effectTextPrefab;
    [SerializeField] private SerializableDictionary<EffectTextStyle, string> effectTextTriggerStringDict = new SerializableDictionary<EffectTextStyle, string>();

    [SerializeField] private EffectIcon effectIconPrefab;
    [SerializeField] private SerializableDictionary<EffectIconStyle, string> effectIconTriggerStringDict = new SerializableDictionary<EffectIconStyle, string>();

    [SerializeField] private Transform spawnOn;

    public void SpawnEffectText(EffectTextStyle style, string text, Color c, Sprite withIcon)
    {
        EffectText spawned = Instantiate(effectTextPrefab, spawnOn);
        spawned.SetColor(c);
        spawned.SetText(text);

        if (withIcon != null)
        {
            spawned.SetIcon(withIcon);
        }

        spawned.SetAnimation(effectTextTriggerStringDict[style]);
    }

    public void SpawnEffectIcon(EffectIconStyle style, Sprite s)
    {
        EffectIcon spawned = Instantiate(effectIconPrefab, spawnOn);
        spawned.SetSprite(s);
        spawned.SetAnimation(effectIconTriggerStringDict[style]);
    }
}
