using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class Character : ScriptableObject
{
    [Header("UI")]
    [SerializeField] private Sprite combatSprite;

    [Header("Starting Stats")]
    [SerializeField] private Robe startingRobe;
    [SerializeField] private Hat startingHat;
    [SerializeField] private Wand startingWand;
    [SerializeField] private BookLabel startingBook;
    [SerializeField] private SpellLabel[] startingSpells;

    [SerializeField] private int startingCurrency;

    [Header("Combat Settings")]
    [SerializeField] private int maxHP;
    [SerializeField] private int startingHP;
    [SerializeField] private int maxMana;
    [SerializeField] private int startingMana;
    [SerializeField] private int basicAttackDamage;
    [SerializeField] private int manaPerTurn;

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }

    public int GetManaPerTurn()
    {
        return manaPerTurn;
    }

    public int GetStartingHP()
    {
        return startingHP;
    }

    public int GetStartingMana()
    {
        return startingMana;
    }

    public int GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public int GetStartingCurrency()
    {
        return startingCurrency;
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

    public Sprite GetCombatSprite()
    {
        return combatSprite;
    }
}
