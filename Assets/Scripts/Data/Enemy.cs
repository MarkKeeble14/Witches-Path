using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyType
{
    PanickedWizard,
    FacelessWitch,
    PossessedTome,
    MimicChest,
    HauntedClock,
    LivingCandle,
    HolyGrail,
    EnchantedMace,
    EnthralledServant,
    AncientDaggerSet,
    InfestedRatPack,
    TheScienceExperiment,
    SpiritOfContempt,
    SpiritsTombGolem,
    SpiritOfPride,
    SpiritOfWar,
    SpritOfDebilitation
}

public abstract class Enemy
{
    public abstract string Name { get; }
    public abstract EnemyType EnemyType { get; }
    public Sprite CombatSprite => Resources.Load<Sprite>("EnemySprites/" + EnemyType.ToString());
    private int maxHP;
    protected abstract Vector2Int minMaxHPAmount { get; }
    protected abstract int basicAttackDamage { get; }
    private Dictionary<Func<bool>, PercentageMap<EnemyAction>> enemyBehaviourDict = new Dictionary<Func<bool>, PercentageMap<EnemyAction>>();

    protected void AddEnemyBehaviour(Func<bool> condition, PercentageMap<EnemyAction> behaviour)
    {
        enemyBehaviourDict.Add(condition, behaviour);
    }

    public Enemy()
    {
        maxHP = RandomHelper.RandomIntInclusive(minMaxHPAmount);
        SetUpBehaviour();
    }

    protected abstract void SetUpBehaviour();

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

    protected SerializableKeyValuePair<EnemyAction, int> MakeOption(int optionOdds, Action onActivate, params EnemyIntent[] intents)
    {
        return new SerializableKeyValuePair<EnemyAction, int>(new EnemyAction(intents.ToList(), onActivate), optionOdds);
    }

    protected SerializableKeyValuePair<EnemyAction, int> MakeOption(int optionOdds, EnemyAction enemyAction)
    {
        return new SerializableKeyValuePair<EnemyAction, int>(enemyAction, optionOdds);
    }

    protected EnemyAction MakeEnemyAction(Action onActivate, params EnemyIntent[] intents)
    {
        return new EnemyAction(intents.ToList(), onActivate);
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
            case EnemyType.LivingCandle:
                return new LivingCandle();
            case EnemyType.HolyGrail:
                return new HolyGrail();
            case EnemyType.EnchantedMace:
                return new EnchantedMace();
            case EnemyType.EnthralledServant:
                return new EnthralledServant();
            case EnemyType.AncientDaggerSet:
                return new AncientDaggerSet();
            case EnemyType.InfestedRatPack:
                return new InfestedRatPack();
            case EnemyType.TheScienceExperiment:
                return new TheScienceExperiment();
            case EnemyType.SpiritOfContempt:
                return new SpiritOfContempt();
            case EnemyType.SpiritsTombGolem:
                return new SpiritsTombGolem();
            case EnemyType.SpiritOfPride:
                return new SpiritOfPride();
            case EnemyType.SpiritOfWar:
                return new SpiritOfWar();
            case EnemyType.SpritOfDebilitation:
                return new SpiritOfDebilitation();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

public class PossessedTome : Enemy
{
    public override string Name => "Possessed Tome";
    public override EnemyType EnemyType => EnemyType.PossessedTome;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(100, 110);
    protected override int basicAttackDamage => 4;

    private bool levitating => CombatManager._Instance.TargetHasAffliction(AfflictionType.Levitating, Target.Enemy);

    private void ApplyLevitating()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Levitating, 1, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyLevitating;
    }

    protected override void SetUpBehaviour()
    {
        // Set Callbacks
        CombatManager._Instance.OnCombatStart += ApplyLevitating;

        // Add Actions
        PercentageMap<EnemyAction> notLevitatingMap = new PercentageMap<EnemyAction>();
        notLevitatingMap.AddOption(MakeOption(100, null, new EnemyGainAfflictionIntent(AfflictionType.Levitating, 1)));
        AddEnemyBehaviour(() => !levitating, notLevitatingMap);

        PercentageMap<EnemyAction> levitatingMap = new PercentageMap<EnemyAction>();
        levitatingMap.AddOption(MakeOption(30, null, new EnemyGainAfflictionIntent(AfflictionType.Power, 2)));
        levitatingMap.AddOption(MakeOption(55, null, new EnemySingleAttackIntent(12, DamageType.Evil)));
        levitatingMap.AddOption(MakeOption(15, null, new EnemyMultiAttackIntent(6, () => 2, DamageType.Evil)));

        AddEnemyBehaviour(() => !levitating, notLevitatingMap);
        AddEnemyBehaviour(() => levitating, levitatingMap);
    }
}

public class HauntedClock : Enemy
{
    public override string Name => "Haunted Clock";
    public override EnemyType EnemyType => EnemyType.HauntedClock;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(90, 100);
    protected override int basicAttackDamage => 3;

    private int numAttacks = 2;

    protected override void SetUpBehaviour()
    {
        // Turn 1s
        PercentageMap<EnemyAction> turn1Map = new PercentageMap<EnemyAction>();
        turn1Map.AddOption(MakeOption(100, null, new EnemyGainAfflictionIntent(AfflictionType.Power, 1), new EnemyWardIntent(5)));

        // Turn 2s
        PercentageMap<EnemyAction> turn2Map = new PercentageMap<EnemyAction>();
        turn2Map.AddOption(MakeOption(100, null, new EnemyGainAfflictionIntent(AfflictionType.Power, 1), new EnemyWardIntent(5)));

        // Turn 3s
        PercentageMap<EnemyAction> turn3sMap = new PercentageMap<EnemyAction>();
        turn3sMap.AddOption(MakeOption(100, () => numAttacks++, new EnemyMultiAttackIntent(2, () => numAttacks, DamageType.Evil)));

        // then back to turn 1

        // Add to Behaviour tree
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 3 == 0, turn1Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 3 == 1, turn2Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 3 == 2, turn3sMap);
    }
}

public class LivingCandle : Enemy
{
    public override string Name => "Living Candle";
    public override EnemyType EnemyType => EnemyType.LivingCandle;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(70, 75);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        PercentageMap<EnemyAction> actionMap = new PercentageMap<EnemyAction>();
        actionMap.AddOption(MakeOption(40, null, new EnemySingleAttackIntent(6, DamageType.Fire), new EnemyApplyAfflictionIntent(AfflictionType.Burn, 3)));
        actionMap.AddOption(MakeOption(40, null, new EnemyMultiAttackIntent(4, () => 3, DamageType.Fire), new EnemyApplyAfflictionIntent(AfflictionType.Burn, 2)));
        actionMap.AddOption(MakeOption(10, null, new EnemyGainAfflictionIntent(AfflictionType.Embolden, 5), new EnemyGainAfflictionIntent(AfflictionType.Power, 2)));
        actionMap.AddOption(MakeOption(10, null, new EnemyGainAfflictionIntent(AfflictionType.Intangible, 2), new EnemyGainAfflictionIntent(AfflictionType.Embolden, 2)));

        AddEnemyBehaviour(() => true, actionMap);
    }
}

public class HolyGrail : Enemy
{
    public override string Name => "Holy Grail";
    public override EnemyType EnemyType => EnemyType.HolyGrail;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(100, 105);
    protected override int basicAttackDamage => 3;

    protected override void SetUpBehaviour()
    {
        PercentageMap<EnemyAction> actionMap = new PercentageMap<EnemyAction>();
        actionMap.AddOption(MakeOption(50, null, new EnemySingleAttackIntent(5, DamageType.Default), new EnemyWardIntent(6)));
        actionMap.AddOption(MakeOption(20, null, new EnemySingleAttackIntent(15, DamageType.Default)));
        actionMap.AddOption(MakeOption(10, null, new EnemyWardIntent(6), new EnemyGainAfflictionIntent(AfflictionType.Protection, 2)));
        actionMap.AddOption(MakeOption(10, null, new EnemyWardIntent(10), new EnemyGainAfflictionIntent(AfflictionType.Regeneration, 6)));
        actionMap.AddOption(MakeOption(10, null, new EnemyHealIntent(Mathf.RoundToInt(GetMaxHP() * 0.5f))));

        AddEnemyBehaviour(() => true, actionMap);
    }
}

public class EnchantedMace : Enemy
{
    public override string Name => "Enchanted Mace";
    public override EnemyType EnemyType => EnemyType.EnchantedMace;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(100, 105);
    protected override int basicAttackDamage => 4;

    private void ApplyBattleFrenzied()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.BattleFrenzied, 1, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyBattleFrenzied;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyBattleFrenzied;

        // Turn 1
        PercentageMap<EnemyAction> turn1Map = new PercentageMap<EnemyAction>();
        turn1Map.AddOption(MakeOption(100, null, new EnemySingleAttackIntent(15, DamageType.Default), new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 1)));

        // Turn 2
        PercentageMap<EnemyAction> turn2Map = new PercentageMap<EnemyAction>();
        turn2Map.AddOption(MakeOption(50, null, new EnemyGainAfflictionIntent(AfflictionType.Power, 5), new EnemyWardIntent(10)));
        turn2Map.AddOption(MakeOption(50, null, new EnemySingleAttackIntent(8, DamageType.Default)));

        // then back to turn 1
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 2 == 0, turn1Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 2 == 1, turn2Map);
    }
}

public class AncientDaggerSet : Enemy
{
    public override string Name => "Ancient Dagger Set";
    public override EnemyType EnemyType => EnemyType.AncientDaggerSet;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(65, 75);
    protected override int basicAttackDamage => 4;

    private int numAttacksLessAttack = 2;
    private int numAttacksMoreAttack = 3;

    private void ApplyPoisonCoated()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.PoisonCoated, 1, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyPoisonCoated;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyPoisonCoated;

        // Turn 1 & 3
        PercentageMap<EnemyAction> turn1And3Map = new PercentageMap<EnemyAction>();
        turn1And3Map.AddOption(MakeOption(75, null, new EnemyMultiAttackIntent(2, () => numAttacksMoreAttack, DamageType.Default)));
        turn1And3Map.AddOption(MakeOption(25, null, new EnemyMultiAttackIntent(3, () => numAttacksLessAttack, DamageType.Default)));

        // Turn 2
        PercentageMap<EnemyAction> turn2Map = new PercentageMap<EnemyAction>();
        turn2Map.AddOption(MakeOption(100, null, new EnemyGainAfflictionIntent(AfflictionType.PoisonCoated, 1)));

        // Turn 4
        PercentageMap<EnemyAction> turn4Map = new PercentageMap<EnemyAction>();
        turn4Map.AddOption(MakeOption(100, delegate
        {
            numAttacksLessAttack++;
            numAttacksMoreAttack++;
        }, new EnemyGainAfflictionIntent(AfflictionType.Power, 1)));

        // Add behaviours
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 0 || (CombatManager._Instance.TurnCount - 1) % 4 == 2, turn1And3Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 1, turn2Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 3, turn4Map);
    }
}

public class TheScienceExperiment : Enemy
{
    public override string Name => "The Science Experiment";
    public override EnemyType EnemyType => EnemyType.TheScienceExperiment;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(60, 70);
    protected override int basicAttackDamage => 2;

    private int buffer;

    private void ApplyThorns()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Thorns, 1, Target.Enemy);
    }

    private void ConsumeThornsStack()
    {
        if (buffer == 0)
        {
            buffer++;
            return;
        }
        CombatManager._Instance.ConsumeAfflictionStack(AfflictionType.Thorns, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyThorns;
        CombatManager._Instance.OnEnemyTurnStart -= ConsumeThornsStack;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyThorns;
        CombatManager._Instance.OnEnemyTurnStart += ConsumeThornsStack;

        // Turn 1s
        PercentageMap<EnemyAction> turn1Map = new PercentageMap<EnemyAction>();
        turn1Map.AddOption(MakeOption(50, null, new EnemyWardIntent(10)));
        turn1Map.AddOption(MakeOption(50, null, new EnemyWardIntent(6), new EnemySingleAttackIntent(4, DamageType.Default)));

        // Turn 2s
        PercentageMap<EnemyAction> turn2Map = new PercentageMap<EnemyAction>();
        turn2Map.AddOption(MakeOption(30, null, new EnemyGainAfflictionIntent(AfflictionType.Thorns, 1)));
        turn1Map.AddOption(MakeOption(70, null, new EnemyWardIntent(12)));

        // Add behaviours
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 2 == 0, turn1Map);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 2 == 1, turn2Map);
    }
}

public class SpiritOfContempt : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfContempt;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(60, 70);
    protected override int basicAttackDamage => 4;

    private bool canBigAttack;

    private void ApplyGhostly()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Ghostly, 5, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyGhostly;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyGhostly;

        // Actions
        EnemyAction attackAndVulnerable = MakeEnemyAction(() => canBigAttack = true, new EnemySingleAttackIntent(6, DamageType.Evil), new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 3));

        PercentageMap<EnemyAction> nonBigAttackMap = new PercentageMap<EnemyAction>();
        nonBigAttackMap.AddOption(MakeOption(100, attackAndVulnerable));

        // 
        PercentageMap<EnemyAction> bigAttackActionMap = new PercentageMap<EnemyAction>();
        bigAttackActionMap.AddOption(MakeOption(50, attackAndVulnerable));
        bigAttackActionMap.AddOption(MakeOption(50, () => canBigAttack = false, new EnemySingleAttackIntent(13, DamageType.Evil)));

        // Add behaviours
        AddEnemyBehaviour(() => !canBigAttack, nonBigAttackMap);
        AddEnemyBehaviour(() => canBigAttack, bigAttackActionMap);
    }
}

public class SpiritsTombGolem : Enemy
{
    public override string Name => "Spirits Tomb Golem";
    public override EnemyType EnemyType => EnemyType.SpiritsTombGolem;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(110, 120);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Turn 1s
        PercentageMap<EnemyAction> turn1sMap = new PercentageMap<EnemyAction>();
        turn1sMap.AddOption(MakeOption(100, null, new EnemyApplyAfflictionIntent(AfflictionType.Weak, 5), new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 1), new EnemyWardIntent(8)));

        // Turn 2s
        PercentageMap<EnemyAction> turn2sMap = new PercentageMap<EnemyAction>();
        turn2sMap.AddOption(MakeOption(100, null, new EnemyApplyAfflictionIntent(AfflictionType.Paralyze, 3), new EnemyWardIntent(8)));

        // Turn 3s
        PercentageMap<EnemyAction> turn3sMap = new PercentageMap<EnemyAction>();
        turn3sMap.AddOption(MakeOption(50, null, new EnemySingleAttackIntent(10, DamageType.Fire), new EnemyApplyAfflictionIntent(AfflictionType.Burn, 3)));
        turn3sMap.AddOption(MakeOption(50, null, new EnemySingleAttackIntent(17, DamageType.Evil), new EnemyApplyAfflictionIntent(AfflictionType.Weak, 3)));

        // Turn 4s
        PercentageMap<EnemyAction> turn4sMap = new PercentageMap<EnemyAction>();
        turn4sMap.AddOption(MakeOption(100, null, new EnemyGainAfflictionIntent(AfflictionType.Protection, 3), new EnemyGainAfflictionIntent(AfflictionType.Power, 3), new EnemyWardIntent(8)));

        // then back to turn 1

        // Add to Behaviour tree
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 0, turn1sMap);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 1, turn2sMap);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 2, turn3sMap);
        AddEnemyBehaviour(() => (CombatManager._Instance.TurnCount - 1) % 4 == 3, turn4sMap);
    }
}

public class SpiritOfPride : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfPride;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(50, 60);
    protected override int basicAttackDamage => 4;

    private bool canBuff;

    private void ApplyGhostly()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Ghostly, 5, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyGhostly;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyGhostly;

        EnemyAction attackAndWard = MakeEnemyAction(() => canBuff = true, new EnemySingleAttackIntent(6, DamageType.Evil), new EnemyWardIntent(6));

        // Actions
        PercentageMap<EnemyAction> canBuffMap = new PercentageMap<EnemyAction>();
        canBuffMap.AddOption(MakeOption(60, attackAndWard));
        canBuffMap.AddOption(MakeOption(10, () => canBuff = false, new EnemyApplyAfflictionIntent(AfflictionType.Power, 3)));
        canBuffMap.AddOption(MakeOption(30, () => canBuff = false, new EnemyGainAfflictionIntent(AfflictionType.Power, 1),
            new EnemyGainAfflictionIntent(AfflictionType.Embolden, 3), new EnemyWardIntent(6)));

        PercentageMap<EnemyAction> cannotBuffMap = new PercentageMap<EnemyAction>();
        cannotBuffMap.AddOption(MakeOption(100, attackAndWard));

        // Add behaviours
        AddEnemyBehaviour(() => canBuff, canBuffMap);
        AddEnemyBehaviour(() => !canBuff, cannotBuffMap);
    }
}

public class SpiritOfWar : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfWar;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(60, 70);
    protected override int basicAttackDamage => 4;

    private int turnsSinceBuff;

    private void ApplyGhostly()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Ghostly, 5, Target.Enemy);
    }

    private void ApplyBattleFrenzied()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.BattleFrenzied, 1, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyGhostly;
        CombatManager._Instance.OnCombatStart -= ApplyBattleFrenzied;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyGhostly;
        CombatManager._Instance.OnCombatStart += ApplyBattleFrenzied;

        // Actions
        PercentageMap<EnemyAction> attackMap = new PercentageMap<EnemyAction>();
        attackMap.AddOption(MakeOption(70, () => turnsSinceBuff++, new EnemySingleAttackIntent(6, DamageType.Evil), new EnemyWardIntent(6)));
        attackMap.AddOption(MakeOption(30, () => turnsSinceBuff++, new EnemySingleAttackIntent(10, DamageType.Evil)));

        // 
        PercentageMap<EnemyAction> buffMap = new PercentageMap<EnemyAction>();
        buffMap.AddOption(MakeOption(75, () => turnsSinceBuff = 0, new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 2)));
        buffMap.AddOption(MakeOption(25, () => turnsSinceBuff = 0, new EnemyGainAfflictionIntent(AfflictionType.Power, 2), new EnemyGainAfflictionIntent(AfflictionType.Protection, 2)));

        // Add behaviours
        AddEnemyBehaviour(() => true, attackMap);
        AddEnemyBehaviour(() => turnsSinceBuff > 1, buffMap);
    }
}

public class SpiritOfDebilitation : Enemy
{
    public override string Name => "Spirit of Debilitation";
    public override EnemyType EnemyType => EnemyType.SpritOfDebilitation;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(75, 85);
    protected override int basicAttackDamage => 4;

    private int turnsSinceDebuff;

    private void ApplyGhostly()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.Ghostly, 5, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyGhostly;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyGhostly;

        PercentageMap<EnemyAction> turn1Map = new PercentageMap<EnemyAction>();
        turn1Map.AddOption(MakeOption(100, null, new EnemyApplyAfflictionIntent(AfflictionType.Blight, 3)));

        // Actions
        PercentageMap<EnemyAction> attackMap = new PercentageMap<EnemyAction>();
        attackMap.AddOption(MakeOption(100, () => turnsSinceDebuff++, new EnemySingleAttackIntent(6, DamageType.Evil), new EnemyApplyAfflictionIntent(AfflictionType.Weak, 3)));
        attackMap.AddOption(MakeOption(100, () => turnsSinceDebuff++, new EnemySingleAttackIntent(10, DamageType.Evil), new EnemyWardIntent(5)));

        // 
        PercentageMap<EnemyAction> debuffMap = new PercentageMap<EnemyAction>();
        debuffMap.AddOption(MakeOption(75, () => turnsSinceDebuff = 0, new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 2), new EnemyApplyAfflictionIntent(AfflictionType.Weak, 4)));
        debuffMap.AddOption(MakeOption(25, () => turnsSinceDebuff = 0, new EnemyApplyAfflictionIntent(AfflictionType.Power, -1), new EnemyApplyAfflictionIntent(AfflictionType.Protection, -1)));

        // Add behaviours
        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount == 1, turn1Map);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount > 1, attackMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount > 1 && turnsSinceDebuff > 1, debuffMap);
    }
}


public class MimicChest : Enemy
{
    public override string Name => "Mimic Chest";
    public override EnemyType EnemyType => EnemyType.MimicChest;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(140, 150);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
    }
}

public class PanickedWizard : Enemy
{
    public override string Name => "Panicked Wizard";
    public override EnemyType EnemyType => EnemyType.PanickedWizard;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(175, 185);
    protected override int basicAttackDamage => 5;

    protected override void SetUpBehaviour()
    {
    }
}


public class EnthralledServant : Enemy
{
    public override string Name => "Enthralled Servant";
    public override EnemyType EnemyType => EnemyType.EnthralledServant;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 210);
    protected override int basicAttackDamage => 5;

    private bool canBuff = true;

    protected override void SetUpBehaviour()
    {
        PercentageMap<EnemyAction> cannotBuffMap = new PercentageMap<EnemyAction>();
        PercentageMap<EnemyAction> canBuffMap = new PercentageMap<EnemyAction>();

        EnemyAction singleAttack = MakeEnemyAction(() => canBuff = true, new EnemySingleAttackIntent(10, DamageType.Default), new EnemyWardIntent(8));
        EnemyAction multiAttack = MakeEnemyAction(() => canBuff = true, new EnemyMultiAttackIntent(5, 2, DamageType.Default), new EnemyWardIntent(8));
        EnemyAction buff = MakeEnemyAction(() => canBuff = false,
            new EnemyWardIntent(20), new EnemyGainAfflictionIntent(AfflictionType.Power, 2), new EnemyGainAfflictionIntent(AfflictionType.Protection, 2));

        cannotBuffMap.AddOption(MakeOption(50, singleAttack));
        cannotBuffMap.AddOption(MakeOption(50, multiAttack));

        canBuffMap.AddOption(MakeOption(80, buff));
        canBuffMap.AddOption(MakeOption(10, singleAttack));
        canBuffMap.AddOption(MakeOption(10, multiAttack));

        AddEnemyBehaviour(() => canBuff, canBuffMap);
        AddEnemyBehaviour(() => !canBuff, cannotBuffMap);
    }
}

public class InfestedRatPack : Enemy
{
    public override string Name => "Infested Rat Pack";
    public override EnemyType EnemyType => EnemyType.InfestedRatPack;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 210);
    protected override int basicAttackDamage => 3;

    private int numAttacks = 4;
    private bool canBuff = true;

    private void ApplyPoisonCoated()
    {
        CombatManager._Instance.AddAffliction(AfflictionType.PoisonCoated, 1, Target.Enemy);
    }

    public override void OnDeath()
    {
        CombatManager._Instance.OnCombatStart -= ApplyPoisonCoated;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnCombatStart += ApplyPoisonCoated;

        PercentageMap<EnemyAction> firstTurnMap = new PercentageMap<EnemyAction>();
        PercentageMap<EnemyAction> attackMap = new PercentageMap<EnemyAction>();
        PercentageMap<EnemyAction> buffMap = new PercentageMap<EnemyAction>();

        firstTurnMap.AddOption(MakeOption(100, null, new EnemyApplyAfflictionIntent(AfflictionType.Blight, 5),
            new EnemyApplyAfflictionIntent(AfflictionType.Vulnerable, 3), new EnemyApplyAfflictionIntent(AfflictionType.Weak, 3)));

        attackMap.AddOption(MakeOption(80, () => canBuff = true, new EnemyMultiAttackIntent(2, () => numAttacks, DamageType.Default)));
        attackMap.AddOption(MakeOption(20, () => canBuff = true, new EnemySingleAttackIntent(8, DamageType.Default), new EnemyApplyAfflictionIntent(AfflictionType.Weak, 5)));

        buffMap.AddOption(MakeOption(50, () => canBuff = false, new EnemyGainAfflictionIntent(AfflictionType.PoisonCoated, 2), new EnemyWardIntent(10)));
        buffMap.AddOption(MakeOption(50, delegate
        {
            numAttacks++;
            canBuff = false;
        }, new EnemyGainAfflictionIntent(AfflictionType.Power, 1), new EnemyWardIntent(10)));

        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount == 1, firstTurnMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount > 1, attackMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnCount > 1 && canBuff, buffMap);
    }
}

public class FacelessWitch : Enemy
{
    public override string Name => "Faceless Witch";
    public override EnemyType EnemyType => EnemyType.FacelessWitch;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(250, 250);
    protected override int basicAttackDamage => 6;

    protected override void SetUpBehaviour()
    {
    }
}