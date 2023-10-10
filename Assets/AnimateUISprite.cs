using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AnimateUISprite : MonoBehaviour
{
    private Image image;

    private int curIndex;
    private Dictionary<string, Sprite[]> spriteMachine = new Dictionary<string, Sprite[]>();

    private Coroutine animationCoroutine;
    private bool end;

    public void AddAnimation(string key, Sprite[] sprites)
    {
        spriteMachine.Add(key, sprites);
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        curIndex = 0;
    }

    public void EndSpriteMachine()
    {
        spriteMachine.Clear();
        end = true;
    }

    public IEnumerator Animate(string animName, bool shouldLoop, float secondsBetweenSpriteChanges = .125f, Action callAtLoopEnd = null)
    {
        if (image == null) image = GetComponent<Image>();

        Sprite[] animationComponents = spriteMachine[animName];
        image.sprite = animationComponents[0];
        curIndex = 1;

        while (!end)
        {
            yield return new WaitForSeconds(secondsBetweenSpriteChanges);

            // Set new Sprite
            if (curIndex + 1 >= animationComponents.Length)
            {
                curIndex = 0;
            }
            else
            {
                curIndex++;
            }
            image.sprite = animationComponents[curIndex];

            // Callback
            callAtLoopEnd?.Invoke();

            if (!shouldLoop) break;
        }
        end = false;
    }
}
