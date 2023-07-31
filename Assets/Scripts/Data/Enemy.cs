using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy")]
public class Enemy : ScriptableObject
{
    [SerializeField] private Sprite combatSprite;

    [Header("Combat Settings")]
    [SerializeField] private float maxHP;
    [SerializeField] private float startingHP;
    [SerializeField] private float basicAttackDamage;

    public float GetMaxHP()
    {
        return maxHP;
    }

    public float GetStartingHP()
    {
        return startingHP;
    }

    public float GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public Sprite GetCombatSprite()
    {
        return combatSprite;
    }
}