using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class Character : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private Sprite combatSprite;
    [SerializeField] private SpellColor color;

    [Header("Starting Stats")]
    [SerializeField] private Robe startingRobe;
    [SerializeField] private Hat startingHat;
    [SerializeField] private Wand startingWand;
    [SerializeField] private BookLabel startingBook;
    [SerializeField] private SpellLabel[] startingSpells;
    [SerializeField] private ArtifactLabel[] startingArtifacts;

    [SerializeField] private int startingCurrency;
    [SerializeField] private int startingClothierCurrency;

    [SerializeField] private SpellLabel defaultActiveSpell;

    [Header("Combat Settings")]
    [Header("Health")]
    [SerializeField] private int maxHP;
    [SerializeField] private int startingHP;

    [Header("Combat")]
    [SerializeField] private int basicAttackDamage;
    [SerializeField] private int startingCombatPileSize;
    [SerializeField] private int startingHandSize;

    [Header("Mana")]
    [SerializeField] private int maxMana;
    [SerializeField] private int manaPerTurnSubFromMax;

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }

    public int GetManaPerTurnSubFromMax()
    {
        return manaPerTurnSubFromMax;
    }

    public int GetStartingHP()
    {
        return startingHP;
    }

    public int GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public int GetStartingCombatPileSize()
    {
        return startingCombatPileSize;
    }

    public int GetStartingHandSize()
    {
        return startingHandSize;
    }

    public int GetStartingCurrency()
    {
        return startingCurrency;
    }

    public int GetStartingClothierCurrency()
    {
        return startingClothierCurrency;
    }

    public Robe GetStartingRobe()
    {
        return startingRobe;
    }

    public Hat GetStartingHat()
    {
        return startingHat;
    }

    public Wand GetStartingWand()
    {
        return startingWand;
    }

    public BookLabel GetStartingBook()
    {
        return startingBook;
    }

    public SpellLabel[] GetStartingSpells()
    {
        return startingSpells;
    }

    public ArtifactLabel[] GetStartingArtifacts()
    {
        return startingArtifacts;
    }

    public Sprite GetCombatSprite()
    {
        return combatSprite;
    }

    public SpellLabel GetDefaultSpell()
    {
        return defaultActiveSpell;
    }

    public SpellColor GetColor()
    {
        return color;
    }
}
