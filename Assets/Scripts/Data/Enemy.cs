using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy")]
public class Enemy : ScriptableObject
{
    [SerializeField] private Sprite combatSprite;
    [SerializeField] private new string name;

    [Header("Combat Settings")]
    [SerializeField] private int maxHP;
    [SerializeField] private int startingHP;
    [SerializeField] private int basicAttackDamage;

    public string Name => name;

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetStartingHP()
    {
        return startingHP;
    }

    public int GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public Sprite GetCombatSprite()
    {
        return combatSprite;
    }
}