using UnityEngine;

public abstract class Equipment : ScriptableObject
{
    [SerializeField] private int damageChange;
    [SerializeField] private int defenseChange;
    [SerializeField] private int manaChange;
}