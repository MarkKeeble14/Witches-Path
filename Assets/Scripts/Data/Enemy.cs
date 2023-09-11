using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyType
{
    PanickedWizard,
    PrisonerOfTheMansion,
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

    // Enemy Actions are Containers for Spell Effects without Spells
    private List<EnemyAction> onCombatStartActions = new List<EnemyAction>();
    private List<EnemyAction> onTurnStartActions = new List<EnemyAction>();
    private List<EnemyAction> onTurnEndActions = new List<EnemyAction>();
    private Dictionary<string, EnemyAction> enemyActionDict = new Dictionary<string, EnemyAction>();
    public virtual string AdditionalInfoText => "";

    // Enemy Behaviour Dict Determines what EnemyActions are Available on any given turn
    private Dictionary<Func<bool>, PercentageMap<string>> enemyBehaviourDict = new Dictionary<Func<bool>, PercentageMap<string>>();

    public List<EnemyAction> GetEnemyActions()
    {
        return enemyActionDict.Values.ToList();
    }

    protected void AddOnCombatStartAction(EnemyAction action)
    {
        onCombatStartActions.Add(action);
    }

    protected void AddOnTurnStartAction(EnemyAction action)
    {
        onTurnStartActions.Add(action);
    }

    protected void AddOnTurnEndAction(EnemyAction action)
    {
        onTurnEndActions.Add(action);
    }

    public List<EnemyAction> GetOnCombatStartActions()
    {
        return onCombatStartActions;
    }

    public List<EnemyAction> GetOnTurnStartActions()
    {
        return onTurnStartActions;
    }

    public List<EnemyAction> GetOnTurnEndActions()
    {
        return onTurnEndActions;
    }

    public virtual void OnDeath()
    {
        //
    }

    protected void AddEnemyAction(string key, EnemyAction action)
    {
        enemyActionDict.Add(key, action);
    }

    protected void AddEnemyBehaviour(Func<bool> condition, PercentageMap<string> behaviour)
    {
        enemyBehaviourDict.Add(condition, behaviour);
    }

    public Enemy()
    {
        maxHP = RandomHelper.RandomIntInclusive(minMaxHPAmount);
        SetUpBehaviour();
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

    protected abstract void SetUpBehaviour();

    public EnemyAction GetEnemyAction()
    {
        // Find all Viable maps
        List<PercentageMap<string>> viableMaps = new List<PercentageMap<string>>();
        foreach (KeyValuePair<Func<bool>, PercentageMap<string>> kvp in enemyBehaviourDict)
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
            return enemyActionDict[RandomHelper.GetRandomFromList(viableMaps).GetOption()];
        }
    }

    public EnemyAction GetEnemyAction(List<EnemyAction> exclude)
    {
        EnemyAction action = GetEnemyAction();
        if (exclude.Contains(action))
        {
            return GetEnemyAction(exclude);
        }
        return action;
    }

    protected SerializableKeyValuePair<string, int> MakeOption(int optionOdds, string enemyActionKey)
    {
        return new SerializableKeyValuePair<string, int>(enemyActionKey, optionOdds);
    }

    protected EnemyAction MakeEnemyAction(Action onActivate, params Spell[] spells)
    {
        return new EnemyAction(onActivate, spells);
    }

    // Static Method
    public static Enemy GetEnemyOfType(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.PrisonerOfTheMansion:
                return new PrisonerOfTheMansion();
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
            case EnemyType.SpiritsTombGolem:
                return new SpiritsTombGolem();
            case EnemyType.SpiritOfContempt:
                return new SpiritOfContempt();
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
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 210);
    protected override int basicAttackDamage => 4;

    private bool levitating => CombatManager._Instance.TargetHasAffliction(AfflictionType.Levitating, Combatent.Enemy);

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("Levitating", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Power", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("SingleAttack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MultiAttack", MakeEnemyAction(null, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> notLevitatingMap = new PercentageMap<string>();
        notLevitatingMap.AddOption(MakeOption(100, "Levitating"));

        PercentageMap<string> levitatingMap = new PercentageMap<string>();
        levitatingMap.AddOption(MakeOption(30, "Power"));
        levitatingMap.AddOption(MakeOption(55, "SingleAttack"));
        levitatingMap.AddOption(MakeOption(15, "MultiAttack"));

        // Apply Behaviour
        AddEnemyBehaviour(() => !levitating, notLevitatingMap);
        AddEnemyBehaviour(() => levitating, levitatingMap);
    }
}

public class HauntedClock : Enemy
{
    public override string Name => "Haunted Clock";
    public override EnemyType EnemyType => EnemyType.HauntedClock;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(190, 200);
    protected override int basicAttackDamage => 3;

    private int numAttacks = 2;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("PowerAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => numAttacks++, new WitchesWill()));

        // Turn 1s
        PercentageMap<string> turn1sMap = new PercentageMap<string>();
        turn1sMap.AddOption(MakeOption(100, "PowerAndWard"));

        // Turn 2s
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(100, "PowerAndWard"));

        // Turn 3s
        PercentageMap<string> turn3sMap = new PercentageMap<string>();
        turn3sMap.AddOption(MakeOption(100, "MultiAttack"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 3 == 1, turn1sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 3 == 2, turn2sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 3 == 0, turn3sMap);
    }
}

public class LivingCandle : Enemy
{
    public override string Name => "Living Candle";
    public override EnemyType EnemyType => EnemyType.LivingCandle;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 175);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttackAndBurn", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MultiAttackAndBurn", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("EmboldenAndPower", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("IntangibleAndEmbolden", MakeEnemyAction(null, new WitchesWill()));

        // Make Map
        PercentageMap<string> actionMap = new PercentageMap<string>();
        actionMap.AddOption(MakeOption(40, "SingleAttackAndBurn"));
        actionMap.AddOption(MakeOption(40, "MultiAttackAndBurn"));
        actionMap.AddOption(MakeOption(10, "EmboldenAndPower"));
        actionMap.AddOption(MakeOption(10, "IntangibleAndEmbolden"));

        // Apply Behaviour
        AddEnemyBehaviour(() => true, actionMap);
    }
}

public class HolyGrail : Enemy
{
    public override string Name => "Holy Grail";
    public override EnemyType EnemyType => EnemyType.HolyGrail;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 225);
    protected override int basicAttackDamage => 3;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Attack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("WardAndProtection", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Regeneration", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Heal", MakeEnemyAction(null, new WitchesWill()));

        // Make Map
        PercentageMap<string> actionMap = new PercentageMap<string>();
        actionMap.AddOption(MakeOption(50, "AttackAndWard"));
        actionMap.AddOption(MakeOption(20, "Attack"));
        actionMap.AddOption(MakeOption(10, "WardAndProtection"));
        actionMap.AddOption(MakeOption(10, "Regeneration"));
        actionMap.AddOption(MakeOption(10, "Heal"));

        // Apply Behaviour
        AddEnemyBehaviour(() => true, actionMap);
    }
}

public class EnchantedMace : Enemy
{
    public override string Name => "Enchanted Mace";
    public override EnemyType EnemyType => EnemyType.EnchantedMace;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(135, 140);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndVulnerable", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Attack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("PowerAndWard", MakeEnemyAction(null, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Turn 1
        PercentageMap<string> turn1sMap = new PercentageMap<string>();
        turn1sMap.AddOption(MakeOption(100, "AttackAndVulnerable"));

        // Turn 2
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(50, "PowerAndWard"));
        turn2sMap.AddOption(MakeOption(50, "Attack"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 1, turn1sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 0, turn2sMap);
    }
}

public class AncientDaggerSet : Enemy
{
    public override string Name => "Ancient Dagger Set";
    public override EnemyType EnemyType => EnemyType.AncientDaggerSet;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(160, 175);
    protected override int basicAttackDamage => 4;

    private int numAttacksLessAttack = 2;
    private int numAttacksMoreAttack = 3;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("MoreAttacks", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("LessAttacks", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("PoisonCoated", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Power", MakeEnemyAction(delegate
        {
            numAttacksLessAttack++;
            numAttacksMoreAttack++;
        }, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Turn 1 & 3
        PercentageMap<string> turn1And3sMap = new PercentageMap<string>();
        turn1And3sMap.AddOption(MakeOption(75, "MoreAttacks"));
        turn1And3sMap.AddOption(MakeOption(25, "LessAttacks"));

        // Turn 2
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(100, "PoisonCoated"));

        // Turn 4
        PercentageMap<string> turn4sMap = new PercentageMap<string>();
        turn4sMap.AddOption(MakeOption(100, "Power"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 1 || CombatManager._Instance.TurnNumber % 4 == 3, turn1And3sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 2, turn2sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 0, turn4sMap);
    }
}

public class SpiritOfContempt : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfContempt;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 175);
    protected override int basicAttackDamage => 4;

    private bool canBigAttack;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndVulnerable", MakeEnemyAction(() => canBigAttack = true, new WitchesWill()));
        AddEnemyAction("BigAttack", MakeEnemyAction(() => canBigAttack = false, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> nonBigAttackMap = new PercentageMap<string>();
        nonBigAttackMap.AddOption(MakeOption(100, "AttackAndVulnerable"));

        PercentageMap<string> bigAttackActionMap = new PercentageMap<string>();
        bigAttackActionMap.AddOption(MakeOption(50, "AttackAndVulnerable"));
        bigAttackActionMap.AddOption(MakeOption(50, "BigAttack"));

        // Apply Behaviours
        AddEnemyBehaviour(() => !canBigAttack, nonBigAttackMap);
        AddEnemyBehaviour(() => canBigAttack, bigAttackActionMap);
    }
}

public class SpiritsTombGolem : Enemy
{
    public override string Name => "Spirits Tomb Golem";
    public override EnemyType EnemyType => EnemyType.SpiritsTombGolem;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(250, 275);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("WeakVulnerableAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("ParalyzeAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("FireAttack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("EvilAttack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("ProtectionPowerAndWard", MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        // Turn 1s
        PercentageMap<string> turn1sMap = new PercentageMap<string>();
        turn1sMap.AddOption(MakeOption(100, "WeakVulnerableAndWard"));

        // Turn 2s
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(100, "ParalyzeAndWard"));

        // Turn 3s
        PercentageMap<string> turn3sMap = new PercentageMap<string>();
        turn3sMap.AddOption(MakeOption(50, "FireAttack"));
        turn3sMap.AddOption(MakeOption(50, "EvilAttack"));

        // Turn 4s
        PercentageMap<string> turn4sMap = new PercentageMap<string>();
        turn4sMap.AddOption(MakeOption(100, "ProtectionPowerAndWard"));

        // Apply Behaviours
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 1, turn1sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 2, turn2sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 3, turn3sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 0, turn4sMap);
    }
}

public class SpiritOfPride : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfPride;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 175);
    protected override int basicAttackDamage => 4;

    private bool canBuff;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndWard", MakeEnemyAction(() => canBuff = true, new WitchesWill()));
        AddEnemyAction("Power", MakeEnemyAction(() => canBuff = false, new WitchesWill()));
        AddEnemyAction("PowerEmboldenAndWard", MakeEnemyAction(() => canBuff = false, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> canBuffMap = new PercentageMap<string>();
        canBuffMap.AddOption(MakeOption(60, "AttackAndWard"));
        canBuffMap.AddOption(MakeOption(10, "Power"));
        canBuffMap.AddOption(MakeOption(30, "PowerEmboldenAndWard"));

        PercentageMap<string> cannotBuffMap = new PercentageMap<string>();
        cannotBuffMap.AddOption(MakeOption(100, "AttackAndWard"));

        // Apply Behaviours
        AddEnemyBehaviour(() => canBuff, canBuffMap);
        AddEnemyBehaviour(() => !canBuff, cannotBuffMap);
    }
}

public class SpiritOfWar : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfWar;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 175);
    protected override int basicAttackDamage => 4;

    private int turnsSinceBuff;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndWard", MakeEnemyAction(() => turnsSinceBuff++, new WitchesWill()));
        AddEnemyAction("EvilAttack", MakeEnemyAction(() => turnsSinceBuff++, new WitchesWill()));
        AddEnemyAction("Vulnerable", MakeEnemyAction(() => turnsSinceBuff = 0, new WitchesWill()));
        AddEnemyAction("PowerAndProtection", MakeEnemyAction(() => turnsSinceBuff = 0, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> attackMap = new PercentageMap<string>();
        attackMap.AddOption(MakeOption(70, "AttackAndWard"));
        attackMap.AddOption(MakeOption(30, "EvilAttack"));

        PercentageMap<string> buffMap = new PercentageMap<string>();
        buffMap.AddOption(MakeOption(75, "Vulnerable"));
        buffMap.AddOption(MakeOption(25, "PowerAndProtection"));

        // Apply Behaviours
        AddEnemyBehaviour(() => true, attackMap);
        AddEnemyBehaviour(() => turnsSinceBuff > 1, buffMap);
    }
}

public class SpiritOfDebilitation : Enemy
{
    public override string Name => "Spirit of Debilitation";
    public override EnemyType EnemyType => EnemyType.SpritOfDebilitation;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 175);
    protected override int basicAttackDamage => 4;

    private int turnsSinceDebuff;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("Blight", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("AttackAndWeak", MakeEnemyAction(() => turnsSinceDebuff++, new WitchesWill()));
        AddEnemyAction("AttackAndWard", MakeEnemyAction(() => turnsSinceDebuff++, new WitchesWill()));
        AddEnemyAction("VulnerableAndWeak", MakeEnemyAction(() => turnsSinceDebuff = 0, new WitchesWill()));
        AddEnemyAction("NegativePowerAndProtection", MakeEnemyAction(() => turnsSinceDebuff = 0, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> turn1Map = new PercentageMap<string>();
        turn1Map.AddOption(MakeOption(100, "Blight"));

        PercentageMap<string> attackMap = new PercentageMap<string>();
        attackMap.AddOption(MakeOption(75, "AttackAndWeak"));
        attackMap.AddOption(MakeOption(25, "AttackAndWard"));

        PercentageMap<string> debuffMap = new PercentageMap<string>();
        debuffMap.AddOption(MakeOption(75, "VulnerableAndWeak"));
        debuffMap.AddOption(MakeOption(25, "NegativePowerAndProtection"));

        // Apply Behaviours
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber == 1, turn1Map);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber > 1, attackMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber > 1 && turnsSinceDebuff > 1, debuffMap);
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

public class TheScienceExperiment : Enemy
{
    public override string Name => "The Science Experiment";
    public override EnemyType EnemyType => EnemyType.TheScienceExperiment;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(350, 400);
    protected override int basicAttackDamage => 2;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("Ward", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("WardAndAttack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Thorns", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MoreWard", MakeEnemyAction(null, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));
        AddOnTurnStartAction(MakeEnemyAction(null, new WitchesWill()));
        AddOnTurnEndAction(MakeEnemyAction(null, new WitchesWill()));

        // Turn 1s
        PercentageMap<string> turn1Map = new PercentageMap<string>();
        turn1Map.AddOption(MakeOption(50, "Ward"));
        turn1Map.AddOption(MakeOption(50, "WardAndAttack"));

        // Turn 2s
        PercentageMap<string> turn2Map = new PercentageMap<string>();
        turn2Map.AddOption(MakeOption(30, "Thorns"));
        turn1Map.AddOption(MakeOption(70, "MoreWard"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 1, turn1Map);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 0, turn2Map);
    }
}

public class PanickedWizard : Enemy
{
    public override string Name => "Panicked Wizard";
    public override EnemyType EnemyType => EnemyType.PanickedWizard;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(300, 350);
    protected override int basicAttackDamage => 5;

    private PercentageMap<string> actionMap;

    protected override void SetUpBehaviour()
    {
        // Add Callback
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Enemy Actions
        AddEnemyAction("SingleAttackAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("SingleAttackBurnAndWard", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MultiAttack", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Buff", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("Random", MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        actionMap = new PercentageMap<string>();
        actionMap.AddOption(MakeOption(20, "SingleAttackAndWard"));
        actionMap.AddOption(MakeOption(20, "SingleAttackBurnAndWard"));
        actionMap.AddOption(MakeOption(20, "MultiAttack"));
        actionMap.AddOption(MakeOption(20, "Buff"));
        actionMap.AddOption(MakeOption(20, "Random"));


        // Apply Behaviours
        AddEnemyBehaviour(() => true, actionMap);
    }
}


public class EnthralledServant : Enemy
{
    public override string Name => "Enthralled Servant";
    public override EnemyType EnemyType => EnemyType.EnthralledServant;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(340, 350);
    protected override int basicAttackDamage => 5;

    private bool canBuff = true;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttack", MakeEnemyAction(() => canBuff = true, new WitchesWill()));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => canBuff = true, new WitchesWill()));
        AddEnemyAction("WardPowerAndProtection", MakeEnemyAction(() => canBuff = false, new WitchesWill()));

        // Make Maps
        PercentageMap<string> cannotBuffMap = new PercentageMap<string>();
        cannotBuffMap.AddOption(MakeOption(50, "SingleAttack"));
        cannotBuffMap.AddOption(MakeOption(50, "MultiAttack"));

        PercentageMap<string> canBuffMap = new PercentageMap<string>();
        canBuffMap.AddOption(MakeOption(80, "WardPowerAndProtection"));
        canBuffMap.AddOption(MakeOption(10, "SingleAttack"));
        canBuffMap.AddOption(MakeOption(10, "MultiAttack"));

        // Apply Behaviours
        AddEnemyBehaviour(() => canBuff, canBuffMap);
        AddEnemyBehaviour(() => !canBuff, cannotBuffMap);
    }
}

public class InfestedRatPack : Enemy
{
    public override string Name => "Infested Rat Pack";
    public override EnemyType EnemyType => EnemyType.InfestedRatPack;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(300, 325);
    protected override int basicAttackDamage => 3;

    private int numAttacks = 4;
    private bool canBuff = true;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("BlightVulnerableAndWeak", MakeEnemyAction(null, new WitchesWill()));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => canBuff = true, new WitchesWill()));
        AddEnemyAction("SingleAttackAndWeak", MakeEnemyAction(() => canBuff = true, new WitchesWill()));
        AddEnemyAction("PoisonCoatedAndWard", MakeEnemyAction(() => canBuff = false, new WitchesWill()));
        AddEnemyAction("PowerAndNumAttacksUp", MakeEnemyAction(delegate
        {
            numAttacks++;
            canBuff = false;
        }, new WitchesWill()));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Maps
        PercentageMap<string> firstTurnMap = new PercentageMap<string>();
        firstTurnMap.AddOption(MakeOption(100, "BlightVulnerableAndWeak"));

        PercentageMap<string> attackMap = new PercentageMap<string>();
        attackMap.AddOption(MakeOption(80, "MultiAttack"));
        attackMap.AddOption(MakeOption(20, "SingleAttackAndWeak"));

        PercentageMap<string> buffMap = new PercentageMap<string>();
        buffMap.AddOption(MakeOption(50, "PoisonCoatedAndWard"));
        buffMap.AddOption(MakeOption(50, "PowerAndNumAttacksUp"));

        // Apply Behaviours
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber == 1, firstTurnMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber > 1, attackMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber > 1 && canBuff, buffMap);
    }
}

public class PrisonerOfTheMansion : Enemy
{
    public override string Name => "Prisoner of the Mansion";
    public override EnemyType EnemyType => EnemyType.PrisonerOfTheMansion;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(475, 499);
    protected override int basicAttackDamage => 6;

    private bool bufferTurn = true;
    private bool cleansing;
    private bool canAttack;
    private bool canMultiAttack;
    private bool isShackled = true;

    private int numShackledToRemove;
    private int defaultMaxCanRemove = 3;
    private int finalMaxCanRemove;

    private void SetShackledCanRemove()
    {
        // Allow for removing either the default set number or alternatively simply the number of stacks the Enemy still has
        Shackled currentShackled = (Shackled)CombatManager._Instance.GetTargetAffliction(AfflictionType.Shackled, Combatent.Enemy);

        Debug.Log("Called Shackle Can Remove");

        // Technically we could remove the Callback here, but it's cleaner to allow the OnDeath Call to handle that imo
        if (currentShackled == null)
        {
            return;
        }

        int maxCanRemove = currentShackled.GetStacks();
        if (maxCanRemove > defaultMaxCanRemove)
        {
            finalMaxCanRemove = defaultMaxCanRemove;
        }
        else
        {
            finalMaxCanRemove = maxCanRemove;
        }

        numShackledToRemove = RandomHelper.RandomIntInclusive(1, finalMaxCanRemove);
        Debug.Log("Set Shackle Can Remove: " + maxCanRemove + ", " + finalMaxCanRemove + ", " + numShackledToRemove);
    }

    public override void OnDeath()
    {
        base.OnDeath();
        CombatManager._Instance.OnTurnStart -= SetShackledCanRemove;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnTurnStart += SetShackledCanRemove;

        AddOnCombatStartAction(MakeEnemyAction(null, new WitchesWill()));

        // Make Enemy Actions
        AddEnemyAction("BufferTurn", MakeEnemyAction(() => bufferTurn = false, new WitchesWill()));

        AddEnemyAction("Struggle", MakeEnemyAction(delegate
        {
            // Check if this enemy is no longer afflicted by Shackled
            // if so, move on to next phase of fight
            if (!CombatManager._Instance.TargetHasAffliction(AfflictionType.Shackled, Combatent.Enemy))
            {
                cleansing = true;
                isShackled = false;
            }

        }, new WitchesWill()));

        AddEnemyAction("Cleanse", MakeEnemyAction(() => cleansing = false, new WitchesWill()));

        AddEnemyAction("MultiAttack", MakeEnemyAction(delegate
        {
            canAttack = false;
            canMultiAttack = false;
        }, new WitchesWill()));
        AddEnemyAction("SingleAttack", MakeEnemyAction(delegate
        {
            canAttack = false;
            canMultiAttack = true;
        }, new WitchesWill()));

        AddEnemyAction("Debuff", MakeEnemyAction(() => canAttack = true,
            new WitchesWill()));
        AddEnemyAction("BuffAndWard", MakeEnemyAction(() => canAttack = true, new WitchesWill()));

        // Make Maps
        PercentageMap<string> bufferTurnMap = new PercentageMap<string>();
        bufferTurnMap.AddOption(MakeOption(100, "BufferTurn"));

        PercentageMap<string> isShackledMap = new PercentageMap<string>();
        isShackledMap.AddOption(MakeOption(100, "Struggle"));

        PercentageMap<string> cleanseMap = new PercentageMap<string>();
        cleanseMap.AddOption(MakeOption(100, "Cleanse"));

        PercentageMap<string> postBuffMultiAttackMap = new PercentageMap<string>();
        postBuffMultiAttackMap.AddOption(MakeOption(100, "MultiAttack"));

        PercentageMap<string> postBuffSingleAttackMap = new PercentageMap<string>();
        postBuffSingleAttackMap.AddOption(MakeOption(100, "SingleAttack"));

        PercentageMap<string> postBuffNonAttackMap = new PercentageMap<string>();
        postBuffNonAttackMap.AddOption(MakeOption(50, "Debuff"));
        postBuffNonAttackMap.AddOption(MakeOption(50, "BuffAndWard"));

        // Apply Behaviours
        AddEnemyBehaviour(() => bufferTurn, bufferTurnMap);
        AddEnemyBehaviour(() => !bufferTurn && isShackled, isShackledMap);
        AddEnemyBehaviour(() => !bufferTurn && cleansing, cleanseMap);
        AddEnemyBehaviour(() => !bufferTurn && !isShackled && !cleansing && !canAttack, postBuffNonAttackMap);
        AddEnemyBehaviour(() => !bufferTurn && !isShackled && !cleansing && canAttack && canMultiAttack, postBuffMultiAttackMap);
        AddEnemyBehaviour(() => !bufferTurn && !isShackled && !cleansing && canAttack && !canMultiAttack, postBuffSingleAttackMap);
    }
}