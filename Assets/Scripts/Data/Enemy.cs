using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAction
{
    private List<EnemyIntent> enemyIntents = new List<EnemyIntent>();

    // Constructor
    public EnemyAction(List<EnemyIntent> intents)
    {
        AddEnemyIntents(intents);
    }

    // Getter
    public List<EnemyIntent> GetEnemyIntents()
    {
        return enemyIntents;
    }

    // Function to Add an IEnumerable of Intents
    public void AddEnemyIntents(IEnumerable<EnemyIntent> intents)
    {
        foreach (EnemyIntent intent in intents)
        {
            AddEnemyIntent(intent);
        }
    }

    // Function to Add a single Intent
    public void AddEnemyIntent(EnemyIntent intent)
    {
        enemyIntents.Add(intent);
    }

    public bool HasIntentType(IntentType intentType)
    {
        foreach (EnemyIntent intent in enemyIntents)
        {
            if (intent.Type == intentType) return true;
        }
        return false;
    }

    public EnemyIntent GetIntentOfType(IntentType intentType)
    {
        foreach (EnemyIntent intent in enemyIntents)
        {
            if (intent.Type == intentType) return intent;
        }
        return null;
    }
}

public abstract class EnemyIntent : ToolTippable
{
    public abstract IntentType Type { get; }
    protected abstract string name { get; }

    protected List<AfflictionType> afflictionKeywords = new List<AfflictionType>();
    protected List<ToolTipKeyword> generalKeywords = new List<ToolTipKeyword>();

    protected abstract string toolTipText { get; }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return afflictionKeywords;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return generalKeywords;
    }

    protected virtual void AddKeywords()
    {
        //
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return name;
    }

    public string GetToolTipText()
    {
        return "This enemy Intends to " + UIManager._Instance.HighlightKeywords(toolTipText);
    }
}

public abstract class EnemyAttackIntent : EnemyIntent
{
    public int DamageAmount { get; protected set; }
    public DamageType DamageType { get; protected set; }
}

public class EnemySingleAttackIntent : EnemyAttackIntent
{
    public override IntentType Type => IntentType.SingleAttack;
    protected override string name => "Attacking";

    protected override string toolTipText => "Attack for "
        + CombatManager._Instance.CalculateDamage(DamageAmount, Target.Enemy, Target.Character, DamageType, DamageSource.EnemyAttack, false) + " Damage";

    public EnemySingleAttackIntent(int damageAmount, DamageType damageType)
    {
        DamageAmount = damageAmount;
        DamageType = damageType;
    }
}

public class EnemyMultiAttackIntent : EnemyAttackIntent
{
    public override IntentType Type => IntentType.MultiAttack;
    protected override string name => "Multi-Attacking";
    public int NumAttacks { get; private set; }

    protected override string toolTipText => "Attack for "
        + CombatManager._Instance.CalculateDamage(DamageAmount, Target.Enemy, Target.Character, DamageType, DamageSource.EnemyAttack, false) + " Damage " + NumAttacks + " Times";

    public EnemyMultiAttackIntent(int damageAmount, int numAttacks, DamageType damageType)
    {
        DamageAmount = damageAmount;
        NumAttacks = numAttacks;
        DamageType = damageType;
    }
}

public class EnemyWardIntent : EnemyIntent
{
    public override IntentType Type => IntentType.Ward;
    protected override string name => "Warding";
    public int WardAmount { get; private set; }
    protected override string toolTipText => "Gain " + CombatManager._Instance.CalculateWard(WardAmount, Target.Enemy) + " Ward";

    public EnemyWardIntent(int wardAmount)
    {
        this.WardAmount = wardAmount;
    }

    protected override void AddKeywords()
    {
        base.AddKeywords();
        generalKeywords.Add(ToolTipKeyword.Ward);
    }
}

public abstract class EnemyAfflictionIntent : EnemyIntent
{
    public AfflictionType AfflictionType { get; protected set; }
    public int NumStacks { get; protected set; }

    public EnemyAfflictionIntent(AfflictionType affType, int numStacks)
    {
        AfflictionType = affType;
        NumStacks = numStacks;
        afflictionKeywords.Add(affType);
    }
}

public class EnemyApplyAfflictionIntent : EnemyAfflictionIntent
{
    public override IntentType Type => IntentType.ApplyAffliction;
    protected override string name => "Applying Affliction";
    protected override string toolTipText => "Apply " + NumStacks + " " + AfflictionType.ToString() + " to You";

    public EnemyApplyAfflictionIntent(AfflictionType affType, int numStacks) : base(affType, numStacks)
    {
    }
}

public class EnemyGainAfflictionIntent : EnemyAfflictionIntent
{
    public override IntentType Type => IntentType.GainAffliction;
    protected override string name => "Gaining Affliction";
    protected override string toolTipText => "Apply " + NumStacks + " " + AfflictionType.ToString() + " to Itself";

    public EnemyGainAfflictionIntent(AfflictionType affType, int numStacks) : base(affType, numStacks)
    {
    }
}

public enum EnemyType
{
    PanickedWizard,
    FacelessWitch,
    PossessedTome,
    MimicChest,
    HauntedClock
}

public abstract class Enemy
{
    public abstract string Name { get; }
    public abstract EnemyType EnemyType { get; }
    public Sprite CombatSprite => Resources.Load<Sprite>("EnemySprites/" + EnemyType.ToString());
    private int maxHP;
    private int basicAttackDamage;
    private Dictionary<Func<bool>, PercentageMap<EnemyAction>> enemyBehaviourDict = new Dictionary<Func<bool>, PercentageMap<EnemyAction>>();

    protected void AddEnemyBehaviour(Func<bool> condition, PercentageMap<EnemyAction> behaviour)
    {
        enemyBehaviourDict.Add(condition, behaviour);
    }

    public Enemy()
    {
        SetParameters();
        SetUpBehaviour();
    }

    protected abstract void SetUpBehaviour();

    protected virtual void SetParameters()
    {
        maxHP = RandomHelper.RandomIntExclusive(GetEnemySpec("MinMaxHP"), GetEnemySpec("MaxMaxHP"));
        basicAttackDamage = GetEnemySpec("BasicAttackDamage");
    }

    protected int GetEnemySpec(string identifier)
    {
        return BalenceManager._Instance.GetValue(EnemyType, identifier);
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public Sprite GetCombatSprite()
    {
        return CombatSprite;
    }

    public EnemyAction GetEnemyIntent()
    {
        // Find all Viable maps
        List<PercentageMap<EnemyAction>> viableMaps = new List<PercentageMap<EnemyAction>>();
        foreach (KeyValuePair<Func<bool>, PercentageMap<EnemyAction>> kvp in enemyBehaviourDict)
        {
            if (kvp.Key())
            {
                viableMaps.Add(kvp.Value);
            }
        }

        // if there are no viable maps, then something has gone wrong
        if (viableMaps.Count == 0)
        {
            throw new Exception();
        }
        else
        {
            return RandomHelper.GetRandomFromList(viableMaps).GetOption();
        }
    }

    protected SerializableKeyValuePair<EnemyAction, int> MakeOption(int optionOdds, params EnemyIntent[] intents)
    {
        return new SerializableKeyValuePair<EnemyAction, int>(new EnemyAction(intents.ToList()), optionOdds);
    }

    public virtual void OnDeath()
    {
        //
    }


    // Static Method
    public static Enemy GetEnemyOfType(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.FacelessWitch:
                return new FacelessWitch();
            case EnemyType.PanickedWizard:
                return new PanickedWizard();
            case EnemyType.PossessedTome:
                return new PossessedTome();
            case EnemyType.MimicChest:
                return new MimicChest();
            case EnemyType.HauntedClock:
                return new HauntedClock();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public class PossessedTome : Enemy
{
    public override string Name => "Possessed Tome";

    public override EnemyType EnemyType => EnemyType.PossessedTome;

    protected override void SetUpBehaviour()
    {
        PercentageMap<EnemyAction> simpleMap = new PercentageMap<EnemyAction>();
        simpleMap.AddOption(MakeOption(100, new EnemyMultiAttackIntent(6, 2, DamageType.Default)));
        AddEnemyBehaviour(() => true, simpleMap);
    }
}

public class HauntedClock : Enemy
{
    public override string Name => "Haunted Clock";

    public override EnemyType EnemyType => EnemyType.HauntedClock;

    private int numPowerStacksPerTurn;
    private int numWardAmountPerTurn;
    private Vector2 minMaxDamageAmount;

    private int turnTracker;

    protected override void SetParameters()
    {
        base.SetParameters();

        numPowerStacksPerTurn = GetEnemySpec("PowerStacksPerTurn");
        numWardAmountPerTurn = GetEnemySpec("WardAmountPerTurn");
        minMaxDamageAmount = new Vector2(GetEnemySpec("MinDamageAmount"), GetEnemySpec("MaxDamageAmount"));
    }

    private void IncrementTurnTracker()
    {
        turnTracker++;
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnEnemyTurnStart -= IncrementTurnTracker;
    }

    protected override void SetUpBehaviour()
    {
        turnTracker = 0;
        CombatManager._Instance.OnEnemyTurnStart += IncrementTurnTracker;

        // Turn 1
        PercentageMap<EnemyAction> turn1Map = new PercentageMap<EnemyAction>();
        turn1Map.AddOption(MakeOption(100, new EnemyGainAfflictionIntent(AfflictionType.Power, numPowerStacksPerTurn), new EnemyWardIntent(numWardAmountPerTurn)));

        // Turn 2
        PercentageMap<EnemyAction> turn2Map = new PercentageMap<EnemyAction>();
        turn2Map.AddOption(MakeOption(100, new EnemyGainAfflictionIntent(AfflictionType.Power, numPowerStacksPerTurn), new EnemyWardIntent(numWardAmountPerTurn)));

        // Turn 3
        PercentageMap<EnemyAction> turn3Map = new PercentageMap<EnemyAction>();
        turn3Map.AddOption(MakeOption(100, new EnemySingleAttackIntent(RandomHelper.RandomIntInclusive(minMaxDamageAmount), DamageType.Evil)));

        // then back to turn 1
        AddEnemyBehaviour(() => turnTracker % 3 == 0, turn1Map);
        AddEnemyBehaviour(() => turnTracker % 3 == 1, turn2Map);
        AddEnemyBehaviour(() => turnTracker % 3 == 2, turn3Map);
    }
}


public class MimicChest : Enemy
{
    public override string Name => "Mimic Chest";

    public override EnemyType EnemyType => EnemyType.MimicChest;

    protected override void SetUpBehaviour()
    {
    }
}

public class FacelessWitch : Enemy
{
    public override string Name => "Faceless Witch";

    public override EnemyType EnemyType => EnemyType.FacelessWitch;

    protected override void SetUpBehaviour()
    {
    }
}


public class PanickedWizard : Enemy
{
    public override string Name => "Panicked Wizard";

    public override EnemyType EnemyType => EnemyType.PanickedWizard;

    protected override void SetUpBehaviour()
    {
    }
}