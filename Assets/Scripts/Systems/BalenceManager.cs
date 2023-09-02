using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveSpellSpecDictionary
{
    [SerializeField] private int cooldown = 1;
    [SerializeField] private int manaCost = 1;
    [SerializeField] private int outOfCombatCooldown = 1;
    [SerializeField] private SerializableDictionary<string, int> additionalParameters = new SerializableDictionary<string, int>();

    public int GetSpec(string identifier)
    {
        switch (identifier)
        {
            case "OutOfCombatCooldown":
                return outOfCombatCooldown;
            case "Cooldown":
                return cooldown;
            case "ManaCost":
                return manaCost;
            default:
                return additionalParameters[identifier];
        }
    }
}


[System.Serializable]
public class PassiveSpellSpecDictionary
{
    [SerializeField] private int outOfCombatCooldown;
    [SerializeField] private SerializableDictionary<string, int> additionalParameters = new SerializableDictionary<string, int>();

    public int GetSpec(string identifier)
    {
        switch (identifier)
        {
            case "OutOfCombatCooldown":
                return outOfCombatCooldown;
            default:
                return additionalParameters[identifier];
        }
    }
}

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [Header("Artifacts")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> artifactSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Books")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> bookSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Afflictions")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> afflictionSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Spells")]
    [SerializeField]
    private SerializableDictionary<string, PassiveSpellSpecDictionary> passiveSpellSpecDict
        = new SerializableDictionary<string, PassiveSpellSpecDictionary>();

    [SerializeField]
    private SerializableDictionary<string, ActiveSpellSpecDictionary> activeSpellSpecDict
    = new SerializableDictionary<string, ActiveSpellSpecDictionary>();

    [Header("Map")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> mapNodeSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Events")]
    [SerializeField]
    private SerializableDictionary<EventLabel, SerializableDictionary<string, int>> eventSpecDict
        = new SerializableDictionary<EventLabel, SerializableDictionary<string, int>>();

    [Header("Potions")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int[]>> ingredientSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int[]>>();

    [Header("Equipment")]
    [SerializeField]
    private SerializableDictionary<ReforgeModifier, List<ReforgeModifierEffect>> reforgeModifierEffects
        = new SerializableDictionary<ReforgeModifier, List<ReforgeModifierEffect>>();
    [SerializeField]
    private SerializableDictionary<ReforgeModifier, int> costToReforgeModifier
        = new SerializableDictionary<ReforgeModifier, int>();

    private void Awake()
    {
        _Instance = this;
    }

    public List<ReforgeModifierEffect> GetReforgeModifierEffect(ReforgeModifier reforgeModifier)
    {
        return reforgeModifierEffects[reforgeModifier];
    }

    public bool PotionHasSpec(PotionIngredientType type, string param)
    {
        return ingredientSpecDict[type.ToString()].ContainsKey(param);
    }

    public int GetValue(PotionIngredientType ingredientType, string identifier, int potency)
    {
        return ingredientSpecDict[ingredientType.ToString()][identifier][potency - 1];
    }

    public int GetValue(PotionIngredientType ingredientType, string identifier)
    {
        return ingredientSpecDict[ingredientType.ToString()][identifier][0];
    }

    public int GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    public int UpdateValue(BookLabel bookLabel, string identifier, int changeBy)
    {
        if (bookSpecDict[bookLabel.ToString()].ContainsKey(identifier))
        {
            int currentValue = bookSpecDict[bookLabel.ToString()][identifier];
            int newValue = currentValue + changeBy;
            bookSpecDict[bookLabel.ToString()][identifier] = newValue;
            return newValue;
        }
        return Utils.StandardSentinalValue;
    }

    // Simply returns the value found in the dict
    public int GetValue(BookLabel bookLabel, string specIdentifier)
    {
        return bookSpecDict[bookLabel.ToString()][specIdentifier];
    }

    public int GetValue(AfflictionType afflictionType, string identifier)
    {
        return afflictionSpecDict[afflictionType.ToString()][identifier];
    }

    public int GetValue(SpellLabel spellLabel, SpellType spellType, string identifier)
    {
        switch (spellType)
        {
            case SpellType.Active:
                return activeSpellSpecDict[spellLabel.ToString()].GetSpec(identifier);
            case SpellType.Passive:
                return passiveSpellSpecDict[spellLabel.ToString()].GetSpec(identifier);
            default:
                throw new Exception();
        }
    }

    public int GetValue(MapNodeType type, string identifier)
    {
        return mapNodeSpecDict[type.ToString()][identifier];
    }

    public int GetValue(EventLabel label, string identifier)
    {
        return eventSpecDict[label][identifier];
    }

    public bool EventHasValue(EventLabel label, string valueIdentifier)
    {
        if (!eventSpecDict.ContainsKey(label)) return false;
        return eventSpecDict[label].ContainsKey(valueIdentifier);
    }

    public int GetCostToReforge(ReforgeModifier reforgeModifier)
    {
        return costToReforgeModifier[reforgeModifier];
    }
}
