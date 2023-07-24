using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character")]
public class Character : ScriptableObject
{
    [SerializeField] private float maxHP;
    [SerializeField] private float startingHP;
    [SerializeField] private float startingCurrency;
    [SerializeField] private Robe startingRobe;
    [SerializeField] private Hat startingHat;
    [SerializeField] private Wand startingWand;

    public float GetMaxHP()
    {
        return maxHP;
    }

    public float GetStartingHP()
    {
        return startingHP;
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
}
