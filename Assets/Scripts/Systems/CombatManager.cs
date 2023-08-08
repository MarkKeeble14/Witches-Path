using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using TMPro;

public enum Target
{
    Character,
    Enemy
}

public enum AfflictionType
{
    Emboldened,
    Weakened,
    Vulnerable,
    Guarded,
    Bandaged,
    Retribution,
    Prepared,
    Parry,
    Echo,
    Blight,
    Burn,
    Paralyzed,
}

public enum AfflictionSetType
{
    Activations,
    Duration
}

public enum DamageType
{
    Default,
    Poison,
    Electricity,
    Fire,
    Heal
}

public enum DamageSource
{
    ActiveSpell,
    PassiveSpell,
    BasicAttack,
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;
    }

    [Header("Objects")]
    [SerializeField] private GameObject Circle; // Circle Object

    [SerializeField] private bool enableSliders = true;

    const int SPAWN = -100; // Spawn coordinates for objects

    private double timer = 0; // Main song timer
    [SerializeField] private int apprRate = 600; // Approach rate (in ms)
    private int delayPos = 0; // Delay song position

    private int noteCount = 0; // Notes played counter
    private int objCount = 0; // Spawned objects counter

    private List<GameObject> circleList; // Circles List
    private static string[] lineParams; // Object Parameters

    [Header("Prefabs")]
    [SerializeField] private PopupText popupTextPrefab;

    // Audio stuff
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    // Other stuff
    [SerializeField] private LayerMask noteLayer;
    [SerializeField] private GameObject cursorTrail;
    [SerializeField] private Image background;
    private Vector3 mousePosition;
    private Ray mainRay;
    private RaycastHit mainHit;

    private AudioClip hitSound;
    private AudioClip missSound;
    private Enemy currentEnemy;
    private float currentEnemyHP;
    private float maxEnemyHP;

    [Header("Settings")]
    [SerializeField] private bool playSFXOnMiss;

    [SerializeField] private TextMeshProUGUI characterHP;
    [SerializeField] private TextMeshProUGUI enemyHP;
    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private Image enemyCombatSprite;

    public Action OnPlayerAttack;
    public Action OnEnemyAttack;
    public Action OnCombatStart;
    public Dictionary<Action, float> OnCombatStartDelayedActionMap = new Dictionary<Action, float>();
    public Dictionary<Action, RepeatData> OnCombatStartRepeatedActionMap = new Dictionary<Action, RepeatData>();
    public List<IEnumerator> OnCombatStartInfinitelyRepeatedActionMap = new List<IEnumerator>();
    public Action OnCombatEnd;
    public Action OnPassiveSpellProc;
    public Action OnActiveSpellActivated;
    public Action OnCharacterGainAffliction;
    public Action OnCharacterLoseAffliction;
    public Action OnEnemyGainAffliction;
    public Action OnEnemyLoseAffliction;
    private List<Coroutine> onStartCombatCoroutines = new List<Coroutine>();

    public bool SetActiveSpellCooldowns { get; set; }
    public bool DuplicatePassiveSpellProcs { get; set; }

    private Dictionary<AfflictionType, Affliction> characterAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, Affliction> enemyAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private List<AfflictionType> toClearFromAffMap = new List<AfflictionType>();

    private Dictionary<AfflictionType, AfflictionIcon> afflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();
    [SerializeField] private AfflictionIcon afflictionIconPrefab;

    [SerializeField] private SerializableDictionary<AfflictionType, AfflictionSign> afflictionSignMap = new SerializableDictionary<AfflictionType, AfflictionSign>();

    [SerializeField] private Transform characterAfflictionList;
    [SerializeField] private Transform enemyAfflictionList;

    public bool InCombat { get; private set; }

    private void Start()
    {
        circleList = new List<GameObject>();
    }

    #region Afflictions

    public void AddAffliction(AfflictionType type, float num, AfflictionSetType setType, Target target)
    {
        switch (target)
        {
            case Target.Character:

                if (type == AfflictionType.Weakened && GameManager._Instance.HasArtifact(ArtifactLabel.SpecialSpinich))
                {
                    return;
                }

                if (type == AfflictionType.Vulnerable && GameManager._Instance.HasArtifact(ArtifactLabel.HolyShield))
                {
                    return;
                }

                switch (setType)
                {
                    case AfflictionSetType.Activations:
                        if (SetAffliction(type, (int)num, target))
                        {
                            // Character was given new Affliction
                            OnCharacterGainAffliction?.Invoke();
                        }
                        break;
                    case AfflictionSetType.Duration:
                        if (SetAffliction(type, num, target))
                        {
                            // Character was given new Affliction
                            OnCharacterGainAffliction?.Invoke();
                        }
                        break;
                }
                break;
            case Target.Enemy:
                switch (setType)
                {
                    case AfflictionSetType.Activations:
                        if (SetAffliction(type, (int)num, target))
                        {
                            // Enemy was given new Affliction
                            OnEnemyGainAffliction?.Invoke();
                        }
                        break;
                    case AfflictionSetType.Duration:
                        if (SetAffliction(type, num, target))
                        {
                            // Enemy was given new Affliction
                            OnEnemyGainAffliction?.Invoke();
                        }
                        break;
                }
                break;
        }
    }

    private bool SetAffliction(AfflictionType type, float duration, Target t)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        Transform parentTo = GetTargetParentAfflictionTo(t);

        //  Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterDuration(duration);
            return false;
        }
        else
        {
            Affliction aff = new Affliction(type, duration);
            map.Add(type, aff);

            AfflictionIcon spawned = Instantiate(afflictionIconPrefab, parentTo);
            spawned.SetAffliction(aff);
            afflictionIconTracker.Add(type, spawned);

            return true;
        }
    }

    private bool SetAffliction(AfflictionType type, int activations, Target t)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        Transform parentTo = GetTargetParentAfflictionTo(t);

        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterActivations(activations);
            return false;
        }
        else
        {
            Affliction aff = new Affliction(type, activations);
            map.Add(type, aff);

            AfflictionIcon spawned = Instantiate(afflictionIconPrefab, parentTo);
            spawned.SetAffliction(aff);
            afflictionIconTracker.Add(type, spawned);

            return true;
        }
    }

    private void UpdateAfflictionMap(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        toClearFromAffMap.Clear();
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            Affliction a = kvp.Value;

            if (a.TickAway)
                a.AlterDuration(-Time.deltaTime);

            if (a.CanBeCleared)
            {
                toClearFromAffMap.Add(kvp.Key);
            }
        }

        foreach (AfflictionType toClear in toClearFromAffMap)
        {
            AfflictionIcon i = afflictionIconTracker[toClear];
            afflictionIconTracker.Remove(toClear);
            Destroy(i.gameObject);

            map.Remove(toClear);

            switch (target)
            {
                case Target.Character:
                    OnCharacterLoseAffliction?.Invoke();
                    break;
                case Target.Enemy:
                    OnEnemyLoseAffliction?.Invoke();
                    break;
            }
        }
    }

    private void UpdateAfflictionMaps()
    {
        UpdateAfflictionMap(characterAfflictionMap, Target.Character);
        UpdateAfflictionMap(enemyAfflictionMap, Target.Enemy);
    }

    private void ClearAfflictionMap(Dictionary<AfflictionType, Affliction> map)
    {
        Dictionary<AfflictionType, Affliction>.KeyCollection keys = map.Keys;
        foreach (AfflictionType type in keys)
        {
            AfflictionIcon icon = afflictionIconTracker[type];
            afflictionIconTracker.Remove(type);
            Destroy(icon.gameObject);
        }
        map.Clear();
    }

    private Dictionary<AfflictionType, Affliction> GetTargetAfflictionMap(Target t)
    {
        return t == Target.Character ? characterAfflictionMap : enemyAfflictionMap;
    }

    private Transform GetTargetParentAfflictionTo(Target t)
    {
        return t == Target.Character ? characterAfflictionList : enemyAfflictionList;
    }

    public bool TargetHasAffliction(AfflictionType type, Target target)
    {
        return GetTargetAfflictionMap(target).ContainsKey(type);
    }

    public void TryConsumeAfflictionStack(AfflictionType type, Target target)
    {
        // Only consumes a stack if there are stacks to be consumed
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Affliction aff = map[type];
        if (!aff.TickAway)
        {
            map[type].AlterActivations(-1);
        }
    }

    private void UpdateTickBasedAfflictions(Dictionary<AfflictionType, Affliction> map, Target t)
    {
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            if (kvp.Key == AfflictionType.Blight)
            {
                AlterCombatentHP(-kvp.Value.RemainingActivations, t, DamageType.Poison);
                map[kvp.Key].AlterActivations(-1);
            }
            else if (kvp.Key == AfflictionType.Burn)
            {
                AlterCombatentHP(-BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount"), t, DamageType.Fire);
                map[kvp.Key].AlterActivations(-1);
            }
        }
    }

    private IEnumerator UpdateTickBasedAfflictions()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            UpdateTickBasedAfflictions(characterAfflictionMap, Target.Character);
            UpdateTickBasedAfflictions(enemyAfflictionMap, Target.Enemy);
        }
    }

    #endregion

    #region Combat

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        // Set Up Combat
        musicSource.clip = combat.MainMusic;
        ReadCircles(AssetDatabase.GetAssetPath(combat.MapFile));
        hitSound = combat.HitSound;
        missSound = combat.MissSound;

        currentEnemy = combat.Enemy;
        maxEnemyHP = currentEnemy.GetMaxHP();

        if (GameManager._Instance.HasBook(BookLabel.CheatersConfessional))
        {
            currentEnemyHP = maxEnemyHP * CheatersConfessional.PercentHP;
            GameManager._Instance.AnimateBook(BookLabel.CheatersConfessional);
        }
        else
        {
            currentEnemyHP = maxEnemyHP;
        }

        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();
        characterCombatSprite.sprite = GameManager._Instance.GetCharacter().GetCombatSprite();

        CallOnStartCombat();

        musicSource.Play();

        InCombat = true;
        SetActiveSpellCooldowns = true;
        DuplicatePassiveSpellProcs = false;

        yield return StartCoroutine(UpdateRoutine());

        if (GameManager._Instance.GetMaxPlayerHP() <= 0)
        {
            GameManager._Instance.GameOver();
        }
        else
        {
            // Bandaged Effect
            if (TargetHasAffliction(AfflictionType.Bandaged, Target.Character))
            {
                GameManager._Instance.AlterPlayerHP(characterAfflictionMap[AfflictionType.Bandaged].RemainingActivations, DamageType.Heal);
            }

            SetActiveSpellCooldowns = false;
            InCombat = false;

            // Reset
            ResetCombat();

            GameManager._Instance.ResolveCurrentEvent();

            Debug.Log("Combat Completed: " + combat);
        }
    }

    private void ResetCombat()
    {
        ClearAfflictionMap(enemyAfflictionMap);
        ClearAfflictionMap(characterAfflictionMap);

        // Clear active spell cooldowns
        GameManager._Instance.TickActiveSpellCooldowns(999);

        foreach (Coroutine c in onStartCombatCoroutines)
        {
            StopCoroutine(c);
        }
        onStartCombatCoroutines.Clear();

        musicSource.Stop();
        musicSource.time = 0;
        objCount = 0;
        noteCount = 0;

        // Destroy Circles
        while (circleList.Count > 0)
        {
            GameObject circle = circleList[0];
            circleList.RemoveAt(0);
            Destroy(circle);
        }
    }

    public void TriggerRandomPassiveSpell()
    {

    }

    public void HalfLitFireworkProc()
    {

    }

    public void MagicRainProc(float damageAmount)
    {

    }

    public void PhantasmalWhispersProc()
    {

    }


    public void WrittenWarningProc(float damage)
    {

    }

    public void OnNoteHit()
    {
        sfxSource.PlayOneShot(hitSound);
        noteCount++;

        PlayerAttack();
    }

    public void OnNoteMiss()
    {
        if (playSFXOnMiss)
            sfxSource.PlayOneShot(missSound);
        noteCount++;

        EnemyAttack();
    }

    private void PlayerAttack()
    {
        // Simple way of attacking for now
        AttackCombatent(-GameManager._Instance.GetBasicAttackDamage(), Target.Character, Target.Enemy, DamageType.Default, DamageSource.BasicAttack);

        OnPlayerAttack?.Invoke();
    }

    private void EnemyAttack()
    {
        // Simple way of attacking for now
        // Damage increase from equipment is built into basic attack damage
        if (!AttackCombatent(-currentEnemy.GetBasicAttackDamage(), Target.Enemy, Target.Character, DamageType.Default, DamageSource.BasicAttack))
        {
            // Player Died
            GameManager._Instance.GameOver();
        }
        else
        {
            OnEnemyAttack?.Invoke();
        }
    }

    public bool AttackCombatent(float amount, Target attacker, Target target, DamageType damageType, DamageSource source)
    {
        // Paralyzed Effect
        if (TargetHasAffliction(AfflictionType.Paralyzed, attacker))
        {
            TryConsumeAfflictionStack(AfflictionType.Paralyzed, attacker);
            return true;
        }

        // Book of Effect Effect
        if (attacker == Target.Character
            && source == DamageSource.ActiveSpell
            && GameManager._Instance.HasBook(BookLabel.BookOfEffect))
        {
            amount *= BookOfEffect.PercentDamageMultiplier;
            GameManager._Instance.AnimateBook(BookLabel.BookOfEffect);
        }

        // Emboldened Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Emboldened, attacker))
        {
            amount *= BalenceManager._Instance.GetValue(AfflictionType.Emboldened, "MultiplyBy");
            TryConsumeAfflictionStack(AfflictionType.Emboldened, attacker);
        }

        // Weakened Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Weakened, attacker))
        {
            amount *= BalenceManager._Instance.GetValue(AfflictionType.Weakened, "MultiplyBy");
            TryConsumeAfflictionStack(AfflictionType.Weakened, attacker);
        }

        // Retribution Effect
        if (TargetHasAffliction(AfflictionType.Retribution, target))
        {
            DamageCombatent(BalenceManager._Instance.GetValue(AfflictionType.Retribution, "DamageAmount"), attacker, target, DamageType.Default);
            TryConsumeAfflictionStack(AfflictionType.Retribution, target);
        }

        // Parry Effect
        if (TargetHasAffliction(AfflictionType.Parry, target))
        {
            TryConsumeAfflictionStack(AfflictionType.Parry, target);
            Target swap = target;
            target = attacker;
            attacker = swap;
        }

        // Then to Deal Damage
        return DamageCombatent(amount, target, attacker, damageType);
    }

    public bool DamageCombatent(float amount, Target combatent, Target attacker, DamageType damageType)
    {
        if (amount < 0 && combatent == Target.Character)
        {
            // Reduce the amount of damage by the players defense added by equipment
            amount += GameManager._Instance.DefenseFromEquipment;
            if (amount > 0)
            {
                amount = 0;
            }
        }

        // Vulnerable Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Vulnerable, Target.Enemy))
        {
            amount *= BalenceManager._Instance.GetValue(AfflictionType.Vulnerable, "MultiplyBy");
            TryConsumeAfflictionStack(AfflictionType.Vulnerable, Target.Enemy);
        }

        // Guarded Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Guarded, Target.Enemy))
        {
            amount -= BalenceManager._Instance.GetValue(AfflictionType.Guarded, "ReduceBy");
            TryConsumeAfflictionStack(AfflictionType.Guarded, Target.Enemy);
            if (amount > 0)
            {
                amount = 0;
            }
        }

        // Alter HP
        return AlterCombatentHP(amount, combatent, damageType);
    }

    private bool AlterCombatentHP(float amount, Target t, DamageType damageType)
    {
        // Prepared Effect
        if (TargetHasAffliction(AfflictionType.Prepared, t))
        {
            amount = 1;
            TryConsumeAfflictionStack(AfflictionType.Prepared, t);
        }

        // Barbarians Tactics Effect
        if (amount < 0 && GameManager._Instance.HasBook(BookLabel.BarbariansTactics))
        {
            amount -= BarbariansTactics.DamageIncrease;
            GameManager._Instance.AnimateBook(BookLabel.ClarksTimeCard);
        }

        // Call the AlterHP function on the appropriate Target
        switch (t)
        {
            case Target.Character:

                PopupText text = Instantiate(popupTextPrefab, characterCombatSprite.transform.position, Quaternion.identity);
                text.Set(Utils.RoundTo(amount, 1).ToString(), GameManager._Instance.GetColorByDamageSource(damageType));

                return GameManager._Instance.AlterPlayerHP(amount, damageType, false);
            case Target.Enemy:
                return AltarEnemyHP(amount, damageType);
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public bool AltarEnemyHP(float amount, DamageType damageType)
    {
        // Doctors Report Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.DoctorsReport) && amount > BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "MustBeOver"))
        {
            AddAffliction(AfflictionType.Bandaged, BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "StackAmount"), AfflictionSetType.Activations, Target.Character);
            GameManager._Instance.AnimateArtifact(ArtifactLabel.DoctorsReport);
        }

        PopupText text = Instantiate(popupTextPrefab, enemyCombatSprite.transform.position, Quaternion.identity);
        text.Set(Utils.RoundTo(amount, 1).ToString(), GameManager._Instance.GetColorByDamageSource(damageType));

        if (currentEnemyHP + amount > maxEnemyHP)
        {
            currentEnemyHP = maxEnemyHP;
        }
        else
        {
            currentEnemyHP += amount;
            if (currentEnemyHP <= 0)
            {
                // Enemy Died
                CallOnEndCombat();
                return false;
            }
        }
        return true;
    }

    public void ClearRandomAffliction(Target t, AfflictionSign sign)
    {
        List<AfflictionType> negativeAfflictions = new List<AfflictionType>();
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            if (afflictionSignMap[kvp.Key] == AfflictionSign.Negative)
            {
                negativeAfflictions.Add(kvp.Key);
            }
        }

        if (negativeAfflictions.Count > 0)
        {
            AfflictionType affToRemove = RandomHelper.GetRandomFromList(negativeAfflictions);
            RemoveAffliction(t, affToRemove);
        }
    }

    public void RemoveAffliction(Target t, AfflictionType type)
    {
        AfflictionIcon icon = afflictionIconTracker[type];
        afflictionIconTracker.Remove(type);
        Destroy(icon.gameObject);
        GetTargetAfflictionMap(t).Remove(type);
    }

    private void CallOnStartCombat()
    {
        OnCombatStart?.Invoke();

        foreach (KeyValuePair<Action, float> kvp in OnCombatStartDelayedActionMap)
        {
            Debug.Log("Starting: " + kvp.Key + ", Delay = " + kvp.Value);
            onStartCombatCoroutines.Add(StartCoroutine(Utils.CallActionAfterDelay(kvp.Key, kvp.Value)));
        }

        foreach (KeyValuePair<Action, RepeatData> kvp in OnCombatStartRepeatedActionMap)
        {
            Debug.Log("Starting: " + kvp.Key + ", Repetitions = " + kvp.Value.Repetitions + ", Delay = " + kvp.Value.Delay);
            for (int i = 1; i <= kvp.Value.Repetitions; i++)
            {
                onStartCombatCoroutines.Add(StartCoroutine(Utils.CallActionAfterDelay(kvp.Key, kvp.Value.Delay * i)));
            }
        }

        foreach (IEnumerator c in OnCombatStartInfinitelyRepeatedActionMap)
        {
            Debug.Log("Starting: " + c);
            onStartCombatCoroutines.Add(StartCoroutine(c));
        }
    }

    private void CallOnEndCombat()
    {
        OnCombatEnd?.Invoke();
    }

    public void AddOnCombatStartDelayedAction(Action a, float delay)
    {
        // Debug.Log("Added: " + a + ", Delay = " + delay);
        OnCombatStartDelayedActionMap.Add(a, delay);
    }

    public void RemoveOnCombatStartDelayedAction(Action a)
    {
        OnCombatStartDelayedActionMap.Remove(a);
    }

    public void AddOnCombatStartRepeatedAction(Action a, RepeatData data)
    {
        // Debug.Log("Added: " + a + ", Repetitions = " + data.Repetitions + ", Delay = " + data.Delay);
        OnCombatStartRepeatedActionMap.Add(a, data);
    }

    public void RemoveOnCombatStartRepeatedAction(Action a)
    {
        OnCombatStartRepeatedActionMap.Remove(a);
    }

    public void AddOnCombatStartInfinitelyRepeatedAction(IEnumerator c)
    {
        // Debug.Log("Added (Infinite): " + a + ", Delay = " + delay);
        OnCombatStartInfinitelyRepeatedActionMap.Add(c);
    }

    public void RemoveOnCombatStartInfinitelyRepeatedAction(IEnumerator c)
    {
        OnCombatStartInfinitelyRepeatedActionMap.Remove(c);
    }

    #endregion

    #region Parsing

    void ReadCircles(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line;

        // Skip to [HitObjects] part
        while (true)
        {
            if (reader.ReadLine() == "[HitObjects]")
                break;
        }

        int TotalLines = 0;

        // Count all lines
        while (!reader.EndOfStream)
        {
            reader.ReadLine();
            TotalLines++;
        }

        reader.Close();

        reader = new StreamReader(path);

        // Skip to [HitObjects] part again
        while (true)
        {
            if (reader.ReadLine() == "[HitObjects]")
                break;
        }

        // Sort objects on load
        int ForeOrder = TotalLines + 2; // Sort foreground layer
        int BackOrder = TotalLines + 1; // Sort background layer
        int ApproachOrder = TotalLines; // Sort approach circles layer

        // Some crazy Z axis modifications for sorting
        string TotalLinesStr = "0.";
        for (int i = 3; i > TotalLines.ToString().Length; i--)
            TotalLinesStr += "0";
        TotalLinesStr += TotalLines.ToString();
        float Z_Index = -(float.Parse(TotalLinesStr));

        while (!reader.EndOfStream)
        {
            // Uncomment to skip sliders
            if (!enableSliders)
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (!line.Contains("|"))
                            break;
                    }
                    else
                        break;
                }
            }

            line = reader.ReadLine();
            if (line == null)
                break;

            lineParams = line.Split(','); // Line parameters (X&Y axis, time position)

            // Sorting configuration
            GameObject circleObject = Instantiate(Circle, new Vector2(SPAWN, SPAWN), Quaternion.identity);
            Circle circle = circleObject.GetComponent<Circle>();
            circle.GetFore().sortingOrder = ForeOrder;
            circle.GetBack().sortingOrder = BackOrder;
            circle.GetAppr().sortingOrder = ApproachOrder;
            circleObject.transform.localPosition += new Vector3((float)circleObject.transform.localPosition.x, (float)circleObject.transform.localPosition.y, (float)Z_Index);
            circleObject.transform.SetAsFirstSibling();
            ForeOrder--; BackOrder--; ApproachOrder--; Z_Index += 0.01f;

            int FlipY = 384 - int.Parse(lineParams[1]); // Flip Y axis

            int AdjustedX = Mathf.RoundToInt(Screen.height * 1.333333f); // Aspect Ratio

            // Padding
            float slices = 8f;
            float paddingX = AdjustedX / slices;
            float paddingY = Screen.height / slices;

            // Resolution set
            float newRangeX = ((AdjustedX - paddingX) - paddingX);
            float newValueX = ((int.Parse(lineParams[0]) * newRangeX) / 512f) + paddingX + ((Screen.width - AdjustedX) / 2f);
            float newRangeY = Screen.height;
            float newValueY = ((FlipY * newRangeY) / 512f) + paddingY;

            Vector3 mainPos = Camera.main.ScreenToWorldPoint(new Vector3(newValueX, newValueY, 0)); // Convert from screen position to world position
            Circle mainCircle = circleObject.GetComponent<Circle>();

            mainCircle.Set(mainPos.x, mainPos.y, circleObject.transform.position.z, int.Parse(lineParams[2]) - apprRate);

            circleList.Add(circleObject);
        }
    }

    private IEnumerator UpdateRoutine()
    {
        Coroutine tickAfflictionsRoutine = StartCoroutine(UpdateTickBasedAfflictions());
        while (true)
        {
            // Show Enemy HP
            enemyHP.text = currentEnemyHP + "/" + maxEnemyHP;
            characterHP.text = GameManager._Instance.GetCurrentCharacterHP() + "/" + GameManager._Instance.GetMaxPlayerHP();
            UpdateAfflictionMaps();

            GameManager._Instance.TickActiveSpellCooldowns(Time.deltaTime);

            if (currentEnemyHP <= 0 || GameManager._Instance.GetCurrentCharacterHP() <= 0)
            {
                break;
            }

            timer = (musicSource.time * 1000); // Convert timer
            delayPos = (circleList[objCount].GetComponent<Circle>().posA);
            mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Spawn object
            if (timer >= delayPos)
            {
                circleList[objCount].GetComponent<Circle>().Spawn();
                objCount++;
            }

            // Check if cursor is over object
            if (Physics.Raycast(mainRay, out mainHit))
            {
                if (LayerMaskHelper.IsInLayerMask(mainHit.collider.gameObject, noteLayer) && timer >= mainHit.collider.gameObject.GetComponent<Circle>().posA + apprRate)
                {
                    mainHit.collider.gameObject.GetComponent<Circle>().Got();
                    mainHit.collider.enabled = false;
                    noteCount++;

                }
            }

            // Cursor trail movement
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorTrail.transform.position = new Vector3(mousePosition.x, mousePosition.y, -9);

            yield return null;
        }
        StopCoroutine(tickAfflictionsRoutine);
    }

    #endregion

    public double GetTimer()
    {
        return timer;
    }

    public int GetApprRate()
    {
        return apprRate;
    }
}
