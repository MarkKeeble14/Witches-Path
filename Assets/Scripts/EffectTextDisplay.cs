using System;
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

    [SerializeField] private float timeBetweenSpawns = .1f;
    private Queue<Action> spawnQueue = new Queue<Action>();

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    public void CallSpawnEffectText(EffectTextStyle style, string text, Color c, Sprite withIcon)
    {
        spawnQueue.Enqueue(() => SpawnEffectText(style, text, c, withIcon));
    }

    private void SpawnEffectText(EffectTextStyle style, string text, Color c, Sprite withIcon)
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

    public void CallSpawnEffectIcon(EffectIconStyle style, Sprite s)
    {
        spawnQueue.Enqueue(() => SpawnEffectIcon(style, s));
    }

    private void SpawnEffectIcon(EffectIconStyle style, Sprite s)
    {
        EffectIcon spawned = Instantiate(effectIconPrefab, spawnOn);
        spawned.SetSprite(s);
        spawned.SetAnimation(effectIconTriggerStringDict[style]);
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            if (spawnQueue.Count > 0)
            {
                Action next = spawnQueue.Dequeue(); ;

                next();

                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            yield return null;
        }
    }
}
