using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCharacterSpriteController : MonoBehaviour
{
    [SerializeField] private Character character;

    private void Awake()
    {
        AnimateUISprite anim = GetComponent<AnimateUISprite>();
        anim.AddAnimation("Default", character.GetCombatSprites());
        StartCoroutine(anim.Animate("Default", true));
    }
}
