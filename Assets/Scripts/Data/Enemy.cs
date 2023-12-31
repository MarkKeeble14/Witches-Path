﻿using System;
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
    SpritOfDebilitation,
    TheFamilyPet,
    Slimer
}

public abstract class Enemy
{
    public abstract string Name { get; }
    public abstract EnemyType EnemyType { get; }
    public Sprite[] CombatSprites => Resources.LoadAll<Sprite>("EnemySprites/" + EnemyType.ToString());
    private int maxHP;
    protected abstract Vector2Int minMaxHPAmount { get; }
    protected abstract int basicAttackDamage { get; }

    // Enemy Actions are Containers for Spell Effects without Spells
    private List<EnemyAction> onCombatStartActions = new List<EnemyAction>();
    private List<EnemyAction> onTurnStartActions = new List<EnemyAction>();
    private List<EnemyAction> onTurnEndActions = new List<EnemyAction>();
    private Dictionary<string, EnemyAction> enemyActionDict = new Dictionary<string, EnemyAction>();

    private List<CombatEffect> onCombatStartSpellEffects = new List<CombatEffect>();

    public virtual string AdditionalInfoText => "";

    // Enemy Behaviour Dict Determines what EnemyActions are Available on any given turn
    private Dictionary<Func<bool>, PercentageMap<string>> enemyBehaviourDict = new Dictionary<Func<bool>, PercentageMap<string>>();

    public Enemy()
    {
        maxHP = RandomHelper.RandomIntInclusive(minMaxHPAmount);
        SetUpBehaviour();
    }

    protected abstract void SetUpBehaviour();

    public virtual void OnDeath()
    {
        //
    }

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

    protected void AddOnCombatStartSpellEffects(params CombatEffect[] spellEffects)
    {
        onCombatStartSpellEffects.AddRange(spellEffects);
    }

    public List<CombatEffect> GetOnCombatStartSpellEffects()
    {
        return onCombatStartSpellEffects;
    }

    protected void AddEnemyAction(string key, EnemyAction action)
    {
        enemyActionDict.Add(key, action);
    }

    protected void AddToEnemyAction(string key, params Spell[] toAdd)
    {
        enemyActionDict[key].AddSpellsToAction(toAdd);
    }

    protected void AddEnemyBehaviour(Func<bool> condition, PercentageMap<string> behaviour)
    {
        enemyBehaviourDict.Add(condition, behaviour);
    }

    public int GetMaxHP()
    {
        return maxHP;
    }

    public int GetBasicAttackDamage()
    {
        return basicAttackDamage;
    }

    public Sprite[] GetCombatSprite()
    {
        return CombatSprites;
    }

    public EnemyAction GetAnyEnemyAction()
    {
        List<PercentageMap<string>> viableMaps = new List<PercentageMap<string>>();
        foreach (KeyValuePair<Func<bool>, PercentageMap<string>> kvp in enemyBehaviourDict)
        {
            viableMaps.Add(kvp.Value);
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

    public EnemyAction GetViableEnemyAction()
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
        EnemyAction action = GetViableEnemyAction();
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
            case EnemyType.SpritOfDebilitation:
                return new SpiritOfDebilitation();
            case EnemyType.TheFamilyPet:
                return new TheFamilyPet();
            case EnemyType.Slimer:
                return new Slimer();
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
    protected override int basicAttackDamage => 3;

    private bool levitating => CombatManager._Instance.TargetHasAffliction(AfflictionType.Levitating, Combatent.Enemy);

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("Levitating", MakeEnemyAction(null, new Spells.Levitate(), new Spells.Protect(10)));
        AddEnemyAction("Power", MakeEnemyAction(null, new Spells.StudyPower(), new Spells.StudyPower(), new Spells.StudyPower()));
        AddEnemyAction("SingleAttack", MakeEnemyAction(null, new Spells.GhastlyGrasp(8), new Spells.Protect(4)));
        AddEnemyAction("MultiAttack", MakeEnemyAction(null, new Spells.GhoulishAssault(3, 3), new Spells.Protect(4)));

        // Add On Combat Start Actions
        AddOnCombatStartSpellEffects(new ApplyAfflictionEffect(AfflictionType.Levitating, () => 1, Target.Self));

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

public class HolyGrail : Enemy
{
    public override string Name => "Holy Grail";
    public override EnemyType EnemyType => EnemyType.HolyGrail;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 225);
    protected override int basicAttackDamage => 3;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndWard", MakeEnemyAction(null, new Spells.ScaldingSplash(6, 1), new Spells.Protect(6)));
        AddEnemyAction("Attack", MakeEnemyAction(null, new Spells.ScaldingSplash(10, 2)));
        AddEnemyAction("WardAndProtection", MakeEnemyAction(null, new Spells.Protect(6), new Spells.StudyProtection(1)));
        AddEnemyAction("Regeneration", MakeEnemyAction(null, new Spells.Blessed(5)));
        AddEnemyAction("Heal", MakeEnemyAction(null, new Spells.Recouperate(10)));

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

public class TheFamilyPet : Enemy
{
    public override string Name => "The Family Pet";
    public override EnemyType EnemyType => EnemyType.TheFamilyPet;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(80, 90);
    protected override int basicAttackDamage => 2;

    private bool willBuff = false;
    private bool canMultiAttack = false;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttack", MakeEnemyAction(delegate
        {
            willBuff = RandomHelper.RandomBool();
            canMultiAttack = true;
        }, new Spells.Tackle(6)));
        AddEnemyAction("MultiAttack", MakeEnemyAction(delegate
        {
            willBuff = RandomHelper.RandomBool();
            canMultiAttack = false;
        }, new Spells.Assault(3, 2)));
        AddEnemyAction("Power", MakeEnemyAction(() => willBuff = false, new Spells.Tackle(3), new Spells.StudyPower(1)));

        // Make Map
        PercentageMap<string> singleAttackMap = new PercentageMap<string>();
        singleAttackMap.AddOption(MakeOption(100, "SingleAttack"));

        PercentageMap<string> multiAttackMap = new PercentageMap<string>();
        multiAttackMap.AddOption(MakeOption(100, "MultiAttack"));

        PercentageMap<string> buffMap = new PercentageMap<string>();
        buffMap.AddOption(MakeOption(100, "Power"));

        // Apply Behaviour
        AddEnemyBehaviour(() => !willBuff && !canMultiAttack, singleAttackMap);
        AddEnemyBehaviour(() => !willBuff && canMultiAttack, multiAttackMap);
        AddEnemyBehaviour(() => willBuff, buffMap);
    }
}

public class LivingCandle : Enemy
{
    public override string Name => "Living Candle";
    public override EnemyType EnemyType => EnemyType.LivingCandle;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(90, 100);
    protected override int basicAttackDamage => 2;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttackAndBurn", MakeEnemyAction(null, new Spells.Fireball(3, 2)));
        AddEnemyAction("MultiAttackAndBurn", MakeEnemyAction(null, new Spells.FlamingLashes(2, 2, 2)));
        AddEnemyAction("EmboldenAndPower", MakeEnemyAction(null, new Spells.Excite(1), new Spells.StudyPower(1), new Spells.Excite(1)));
        AddEnemyAction("Embolden", MakeEnemyAction(null, new Spells.Overexcite(5, 2)));

        // Make Map
        PercentageMap<string> actionMap = new PercentageMap<string>();
        actionMap.AddOption(MakeOption(50, "SingleAttackAndBurn"));
        actionMap.AddOption(MakeOption(30, "MultiAttackAndBurn"));
        actionMap.AddOption(MakeOption(10, "EmboldenAndPower"));
        actionMap.AddOption(MakeOption(10, "Embolden"));

        // Apply Behaviour
        AddEnemyBehaviour(() => true, actionMap);
    }
}

public class Slimer : Enemy
{
    public override string Name => "Slimer";
    public override EnemyType EnemyType => EnemyType.Slimer;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(100, 110);
    protected override int basicAttackDamage => 2;

    private bool willWard;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttack", MakeEnemyAction(() => willWard = true, new Spells.Tackle(7)));
        AddEnemyAction("SingleAttackAndWeak", MakeEnemyAction(() => willWard = true, new Spells.WeakeningBlow(5, 2)));
        AddEnemyAction("SingleAttackAndVulnerable", MakeEnemyAction(null, new Spells.BrutalSmash(7, 2)));
        AddEnemyAction("Ward", MakeEnemyAction(() => willWard = false, new Spells.Protect(7)));

        // Make Map
        PercentageMap<string> actionMap = new PercentageMap<string>();
        actionMap.AddOption(MakeOption(30, "SingleAttack"));
        actionMap.AddOption(MakeOption(35, "SingleAttackAndWeak"));
        actionMap.AddOption(MakeOption(35, "SingleAttackAndVulnerable"));

        PercentageMap<string> wardMap = new PercentageMap<string>();
        wardMap.AddOption(MakeOption(100, "Ward"));

        // Apply Behaviour
        AddEnemyBehaviour(() => !willWard, actionMap);
        AddEnemyBehaviour(() => willWard, wardMap);
    }
}

public class EnchantedMace : Enemy
{
    public override string Name => "Enchanted Mace";
    public override EnemyType EnemyType => EnemyType.EnchantedMace;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(135, 145);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndVulnerable", MakeEnemyAction(null, new Spells.BrutalSmash(8, 2)));
        AddEnemyAction("Attack", MakeEnemyAction(null, new Spells.Bash(15)));
        AddEnemyAction("PowerAndWard", MakeEnemyAction(null, new Spells.StudyPower(3), new Spells.Protect(10)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.EnterFrenzy(1)));

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
    protected override Vector2Int minMaxHPAmount => new Vector2Int(120, 130);
    protected override int basicAttackDamage => 3;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        Spells.Flurry flurry = new Spells.Flurry(2, 3);

        AddEnemyAction("Flurry", MakeEnemyAction(null, flurry));
        AddEnemyAction("PoisonCoated", MakeEnemyAction(null, new Spells.CoatEdges(1)));
        AddEnemyAction("ExtraAttack", MakeEnemyAction(delegate
        {
            flurry.AlterSpellStat(SpellStat.HitAmount, 1, SpellAlterStatDuration.Combat);
            AddToEnemyAction("Power", new Spells.Protect(2));
        }, new Spells.Protect(2), new Spells.Protect(2), new Spells.Protect(2)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.CoatEdges(1)));

        // Turn 1 & 3
        PercentageMap<string> turn1And3sMap = new PercentageMap<string>();
        turn1And3sMap.AddOption(MakeOption(100, "Flurry"));

        // Turn 2
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(100, "PoisonCoated"));

        // Turn 4
        PercentageMap<string> turn4sMap = new PercentageMap<string>();
        turn4sMap.AddOption(MakeOption(100, "ExtraAttack"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 1 || CombatManager._Instance.TurnNumber % 4 == 3, turn1And3sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 2, turn2sMap);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 4 == 0, turn4sMap);
    }
}
public class SpiritOfPride : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfPride;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(110, 125);
    protected override int basicAttackDamage => 3;

    private bool canBuff;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndWard", MakeEnemyAction(() => canBuff = true, new Spells.StrikeTwice(3), new Spells.StrikeTwice(3)));
        AddEnemyAction("Power", MakeEnemyAction(() => canBuff = false, new Spells.StudyPower(1)));
        AddEnemyAction("PowerEmboldenAndWard", MakeEnemyAction(() => canBuff = false, new Spells.StudyPower(1), new Spells.Excite(3), new Spells.Protect(10)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.Ghost(1)));

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

public class HauntedClock : Enemy
{
    public override string Name => "Haunted Clock";
    public override EnemyType EnemyType => EnemyType.HauntedClock;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(150, 160);
    protected override int basicAttackDamage => 3;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        Spells.StudyPower studyPower = new Spells.StudyPower(2);
        AddEnemyAction("PowerAndWard", MakeEnemyAction(null, studyPower, new Spells.Protect(10)));
        Spells.GhoulishAssault ghoulishAssault = new Spells.GhoulishAssault(4, 2);
        AddEnemyAction("MultiAttack",
            MakeEnemyAction(delegate
            {
                AddToEnemyAction("PowerAndWard", studyPower);
            }, ghoulishAssault));

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

public class SpiritsTombGolem : Enemy
{
    public override string Name => "Spirits Tomb Golem";
    public override EnemyType EnemyType => EnemyType.SpiritsTombGolem;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(225, 250);
    protected override int basicAttackDamage => 4;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("WeakVulnerableAndWard", MakeEnemyAction(null, new Spells.BreakSpirit(5, 5), new Spells.Protect(2)));
        AddEnemyAction("ElectricAttack", MakeEnemyAction(null, new Spells.Shock(3, 3), new Spells.Shock(3, 3), new Spells.Protect(4)));
        AddEnemyAction("FireAttack", MakeEnemyAction(null, new Spells.Electrifry(6, 3), new Spells.Protect(6)));
        AddEnemyAction("EvilAttack", MakeEnemyAction(null, new Spells.GhastlyGrasp(10), new Spells.Protect(8)));
        AddEnemyAction("ProtectionPowerAndWard", MakeEnemyAction(null, new Spells.Bash(25)));

        // Make Maps
        // Turn 1s
        PercentageMap<string> turn1sMap = new PercentageMap<string>();
        turn1sMap.AddOption(MakeOption(100, "WeakVulnerableAndWard"));

        // Turn 2s
        PercentageMap<string> turn2sMap = new PercentageMap<string>();
        turn2sMap.AddOption(MakeOption(100, "ElectricAttack"));

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

public class SpiritOfContempt : Enemy
{
    public override string Name => "Spirit of Contempt";
    public override EnemyType EnemyType => EnemyType.SpiritOfContempt;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(175, 200);
    protected override int basicAttackDamage => 4;

    private bool canBigAttack;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("AttackAndVulnerable", MakeEnemyAction(() => canBigAttack = true, new Spells.BrutalSmash(10, 2)));
        AddEnemyAction("BigAttack", MakeEnemyAction(() => canBigAttack = false, new Spells.StrikeTwice(13)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.Ghost(1), new Spells.Ghost(1), new Spells.Ghost(1)));

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
        AddEnemyAction("Blight", MakeEnemyAction(null, new Spells.Belch(1)));
        AddEnemyAction("AttackAndWeak", MakeEnemyAction(() => turnsSinceDebuff++, new Spells.Cripple(6, 3)));
        AddEnemyAction("AttackAndWard", MakeEnemyAction(() => turnsSinceDebuff++, new Spells.Cripple(3, 3), new Spells.Protect(8)));
        AddEnemyAction("VulnerableAndWeak", MakeEnemyAction(() => turnsSinceDebuff = 0, new Spells.BreakSpirit(1, 1), new Spells.BreakSpirit(1, 1), new Spells.BreakSpirit(1, 1)));
        AddEnemyAction("NegativePowerAndProtection", MakeEnemyAction(() => turnsSinceDebuff = 0, new Spells.Protect(5), new Spells.Sap(2), new Spells.Protect(5)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.Ghost(1), new Spells.Ghost(1)));

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
    protected override Vector2Int minMaxHPAmount => new Vector2Int(225, 250);
    protected override int basicAttackDamage => 5;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("Ward", MakeEnemyAction(null, new Spells.Protect(10)));
        AddEnemyAction("WardAndAttack", MakeEnemyAction(null, new Spells.Tackle(20), new Spells.Protect(5)));
        AddEnemyAction("Thorns", MakeEnemyAction(null, new Spells.GrowSpikes(1)));
        AddEnemyAction("MoreWard", MakeEnemyAction(null, new Spells.Harden(1)));

        // Add On Combat Start Actions
        AddOnTurnEndAction(MakeEnemyAction(null, new Spells.GrowSpikes(1)));

        // Turn 1s
        PercentageMap<string> turn1Map = new PercentageMap<string>();
        turn1Map.AddOption(MakeOption(50, "Ward"));
        turn1Map.AddOption(MakeOption(50, "WardAndAttack"));

        // Turn 2s
        PercentageMap<string> turn2Map = new PercentageMap<string>();
        turn2Map.AddOption(MakeOption(10, "Thorns"));
        turn1Map.AddOption(MakeOption(90, "MoreWard"));

        // Apply Behaviour
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 1, turn1Map);
        AddEnemyBehaviour(() => CombatManager._Instance.TurnNumber % 2 == 0, turn2Map);
    }
}

public class PanickedWizard : Enemy
{
    public override string Name => "Panicked Wizard";
    public override EnemyType EnemyType => EnemyType.PanickedWizard;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(200, 225);
    protected override int basicAttackDamage => 5;

    private bool canUseRandomMap = true;

    protected override void SetUpBehaviour()
    {
        // Add Callback
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.LoseResolve(3)));

        // Make Enemy Actions
        AddEnemyAction("SingleAttackAndWard", MakeEnemyAction(() => canUseRandomMap = true, new Spells.Fireball(5, 2), new Spells.Shock(5, 2), new Spells.Toxify(5, 2)));
        AddEnemyAction("SingleAttackBurnAndWard", MakeEnemyAction(() => canUseRandomMap = true, new Spells.Fireball(3, 2), new Spells.Singe(2), new Spells.Protect(10)));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => canUseRandomMap = true, new Spells.GhoulishAssault(4, 2)));
        AddEnemyAction("Buff", MakeEnemyAction(() => canUseRandomMap = true, new Spells.Protect(10), new Spells.StudyPower(1), new Spells.StudyProtection(1),
            new Spells.StudyPower(1), new Spells.StudyProtection(1)));

        AddEnemyAction("Random1", MakeEnemyAction(() => canUseRandomMap = false, Spell.GetSpellOfType(RandomHelper.GetRandomEnumValue<SpellLabel>())));
        AddEnemyAction("Random2", MakeEnemyAction(() => canUseRandomMap = false, Spell.GetSpellOfType(RandomHelper.GetRandomEnumValue<SpellLabel>())));
        AddEnemyAction("Random3", MakeEnemyAction(() => canUseRandomMap = false, Spell.GetSpellOfType(RandomHelper.GetRandomEnumValue<SpellLabel>())));

        // Make Maps
        PercentageMap<string> normalActionMap = new PercentageMap<string>();
        normalActionMap.AddOption(MakeOption(25, "SingleAttackAndWard"));
        normalActionMap.AddOption(MakeOption(25, "SingleAttackBurnAndWard"));
        normalActionMap.AddOption(MakeOption(25, "MultiAttack"));
        normalActionMap.AddOption(MakeOption(25, "Buff"));

        PercentageMap<string> randomActionmap = new PercentageMap<string>();
        randomActionmap.AddOption(MakeOption(34, "Random1"));
        randomActionmap.AddOption(MakeOption(33, "Random2"));
        randomActionmap.AddOption(MakeOption(33, "Random3"));

        // Apply Behaviours
        AddEnemyBehaviour(() => true, normalActionMap);
        AddEnemyBehaviour(() => canUseRandomMap, randomActionmap);
    }
}


public class EnthralledServant : Enemy
{
    public override string Name => "Enthralled Servant";
    public override EnemyType EnemyType => EnemyType.EnthralledServant;
    protected override Vector2Int minMaxHPAmount => new Vector2Int(225, 230);
    protected override int basicAttackDamage => 5;

    private bool canBuff = true;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("SingleAttack", MakeEnemyAction(() => canBuff = true, new Spells.Tackle(5), new Spells.Tackle(5), new Spells.Protect(5), new Spells.Protect(5)));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => canBuff = true, new Spells.GhoulishAssault(4, 2)));
        AddEnemyAction("WardPowerAndProtection", MakeEnemyAction(() => canBuff = false, new Spells.StudyPower(2), new Spells.StudyProtection(1), new Spells.Protect(10)));

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
    protected override Vector2Int minMaxHPAmount => new Vector2Int(225, 250);
    protected override int basicAttackDamage => 5;

    private bool canBuff = true;

    protected override void SetUpBehaviour()
    {
        // Make Enemy Actions
        AddEnemyAction("BlightVulnerableAndWeak", MakeEnemyAction(null, new Spells.ViralChomp(7, 1, 3)));
        AddEnemyAction("MultiAttack", MakeEnemyAction(() => canBuff = true, new Spells.Claw(3), new Spells.Claw(3), new Spells.Claw(3)));
        AddEnemyAction("SingleAttackAndWeak", MakeEnemyAction(() => canBuff = true, new Spells.Cripple(5, 5)));
        AddEnemyAction("PoisonCoatedAndWard", MakeEnemyAction(() => canBuff = false, new Spells.CoatEdges(1), new Spells.Protect(3), new Spells.Protect(3), new Spells.Protect(3)));
        AddEnemyAction("PowerAndNumAttacksUp", MakeEnemyAction(delegate
        {
            AddToEnemyAction("MultiAttack", new Spells.Claw(2));
            AddToEnemyAction("PoisonCoatedAndWard", new Spells.Protect(3));
            AddToEnemyAction("PowerAndNumAttacksUp", new Spells.Excite(1));
            canBuff = false;
        }, new Spells.Excite(2)));

        // Add On Combat Start Actions
        AddOnCombatStartAction(MakeEnemyAction(null, new Spells.CoatEdges(1)));

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
    protected override Vector2Int minMaxHPAmount => new Vector2Int(375, 400);
    protected override int basicAttackDamage => 6;

    private bool bufferTurn = true;
    private bool cleansing;
    private bool canAttack;
    private bool canMultiAttack;
    private bool isShackled = true;

    private int numShackledToRemove;
    private int defaultMaxCanRemove = 3;
    private int finalMaxCanRemove;

    private Spells.Struggle struggle;

    private void SetShackledCanRemove()
    {
        // Allow for removing either the default set number or alternatively simply the number of stacks the Enemy still has
        Afflictions.Shackled currentShackled = (Afflictions.Shackled)CombatManager._Instance.GetTargetAffliction(AfflictionType.Shackled, Combatent.Enemy);

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
        struggle.SetSpellStatTo(SpellStat.Aff1StackAmount, numShackledToRemove);
    }

    public override void OnDeath()
    {
        base.OnDeath();
        CombatManager._Instance.OnTurnStart -= SetShackledCanRemove;
    }

    protected override void SetUpBehaviour()
    {
        CombatManager._Instance.OnTurnStart += SetShackledCanRemove;

        // Add Shackled and Other Afflictions on Start
        AddOnCombatStartSpellEffects(new ApplyAfflictionEffect(AfflictionType.Shackled, () => 8, Target.Self), new ApplyAfflictionEffect(AfflictionType.Weak, () => 5, Target.Self),
            new ApplyAfflictionEffect(AfflictionType.Vulnerable, () => 5, Target.Self));

        // Make Enemy Actions
        AddEnemyAction("BufferTurn", MakeEnemyAction(() => bufferTurn = false, new Spells.Overexcite(3, 3), new Spells.BattleFrenzy(1), new Spells.Protect(10)));

        struggle = new Spells.Struggle(-numShackledToRemove, -3);
        AddEnemyAction("Struggle", MakeEnemyAction(delegate
        {
            // Check if this enemy is no longer afflicted by Shackled
            // if so, move on to next phase of fight
            if (!CombatManager._Instance.TargetHasAffliction(AfflictionType.Shackled, Combatent.Enemy))
            {
                cleansing = true;
                isShackled = false;
            }
        }, new Spells.HateFilledStrike(10), new Spells.Protect(20), struggle));

        AddEnemyAction("Cleanse", MakeEnemyAction(() => cleansing = false, new Spells.Unleash(), new Spells.EnterFrenzy(1)));

        AddEnemyAction("MultiAttack", MakeEnemyAction(delegate
        {
            canAttack = false;
            canMultiAttack = false;
        }, new Spells.HateFilledStrike(3), new Spells.HateFilledStrike(3), new Spells.HateFilledStrike(3)));

        AddEnemyAction("SingleAttack", MakeEnemyAction(delegate
        {
            canAttack = false;
            canMultiAttack = true;
        }, new Spells.Cripple(3, 2), new Spells.GhoulishAssault(5, 2), new Spells.Protect(5), new Spells.Protect(5)));

        AddEnemyAction("Debuff", MakeEnemyAction(() => canAttack = true,
            new Spells.BreakSpirit(1, 1), new Spells.Sap(-1), new Spells.BreakSpirit(1, 1), new Spells.Sap(-1), new Spells.BreakSpirit(1, 1)));

        AddEnemyAction("BuffAndWard", MakeEnemyAction(() => canAttack = true, new Spells.Unleash()));

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