using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class Character : ScriptableObject
{
    [SerializeField] private float startingCurrency;
    [SerializeField] private Robe startingRobe;
    [SerializeField] private Hat startingHat;
    [SerializeField] private Wand startingWand;
    [SerializeField] private BookLabel startingBook;

    [Header("UI")]
    [SerializeField] private Sprite combatSprite;

    [Header("Combat Settings")]
    [SerializeField] private float maxHP;
    [SerializeField] private float startingHP;
    [SerializeField] private float maxMana;
    [SerializeField] private float startingMana;
    [SerializeField] private float basicAttackDamage;

    public float GetMaxHP()
    {
        return maxHP;
    }

    public float GetMaxMana()
    {
        return maxMana;
    }

    public float GetStartingHP()
    {
        return startingHP;
    }

    public float GetStartingMana()
    {
        return startingMana;
    }

    public float GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public float GetStartingCurrency()
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

    public Sprite GetCombatSprite()
    {
        return combatSprite;
    }
}
