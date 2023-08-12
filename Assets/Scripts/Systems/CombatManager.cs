using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Pool;

public enum Turn
{
    Player, Enemy
}

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

public partial class CombatManager : MonoBehaviour
{
    public static CombatManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;

        CreateCirclePool();
    }

    [Header("Rhythm Settings")]
    [SerializeField] private Circle circle; // Circle Object

    [SerializeField] private bool enableSliders = true;
    [SerializeField] private int apprRate = 600; // Approach rate (in ms)

    const int SPAWN = -100; // Spawn coordinates for objects

    private double timer = 0; // Main song timer
    private int delayPos = 0; // Delay song position

    private int noteCount = 0; // Notes played counter
    private int objCount = 0; // Spawned objects counter

    private List<Circle> circleList = new List<Circle>(); // Circles List
    private static string[] lineParams; // Object Parameters
    // Other stuff
    [SerializeField] private LayerMask noteLayer;
    [SerializeField] private GameObject cursorTrail;
    [SerializeField] private Image background;
    private Vector3 mousePosition;
    private Ray mainRay;
    private RaycastHit mainHit;

    // Enemy Stuff
    private AudioClip hitSound;
    private AudioClip missSound;
    private Enemy currentEnemy;
    private int currentEnemyHP;
    private int maxEnemyHP;

    // Ward
    private int enemyWard;
    private int characterWard;

    [Header("Referencess")]
    [SerializeField] private CombatentHPBar characterHPBar;
    [SerializeField] private CombatentHPBar enemyHPBar;
    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private Image enemyCombatSprite;

    [Header("Game")]
    [SerializeField] private PopupText popupTextPrefab;

    [Header("Audio")]
    [SerializeField] private bool playSFXOnMiss;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    // Callbacks
    public Action OnPlayerAttack;
    public Action OnEnemyAttack;
    public Action OnCombatStart;
    public Action OnPlayerTurnStart;
    public Action OnEnemyTurnStart;
    public Action OnCombatEnd;
    public Action OnPassiveSpellProc;
    public Action OnActiveSpellQueued;
    public Action OnActiveSpellActivated;
    public Action OnCharacterGainAffliction;
    public Action OnCharacterLoseAffliction;
    public Action OnEnemyGainAffliction;
    public Action OnEnemyLoseAffliction;

    #region Combat

    public bool SetActiveSpellCooldowns { get; set; }
    public bool DuplicatePassiveSpellProcs { get; set; }
    public bool InCombat { get; private set; }
    private bool playerTurnEnded;
    private Turn currentTurn;

    [Header("Spell Queue")]
    [SerializeField] private SpellQueueDisplay spellQueueDisplayPrefab;
    [SerializeField] private Transform spellQueueDisplayList;
    private List<QueuedActiveSpell> spellQueue = new List<QueuedActiveSpell>();
    [SerializeField] private float delayBetweenSpellCasts = 1;
    [SerializeField] private AudioSource spellSFXSource;

    private Stack<CastActiveGameState> gameStateQueue = new Stack<CastActiveGameState>();

    private void SaveNewCastSpellGameState()
    {
        CastActiveGameState newState = new CastActiveGameState(GameManager._Instance.GetActiveSpells(), GameManager._Instance.GetCurrentPlayerMana());
        gameStateQueue.Push(newState);
    }

    private void LoadPreviousCastSpellGameState()
    {
        CastActiveGameState gameState = gameStateQueue.Pop();

        // Set Cooldowns
        foreach (KeyValuePair<ActiveSpell, int> kvp in gameState.CooldownDict)
        {
            kvp.Key.SetCooldown(kvp.Value);
        }

        // Set Player Mana
        GameManager._Instance.SetPlayerMana(gameState.PlayerMana);
    }

    private struct CastActiveGameState
    {
        public Dictionary<ActiveSpell, int> CooldownDict;
        public int PlayerMana;

        public CastActiveGameState(List<ActiveSpell> activeSpells, int playerMana)
        {
            CooldownDict = new Dictionary<ActiveSpell, int>();
            foreach (ActiveSpell equippedSpell in activeSpells)
            {
                CooldownDict.Add(equippedSpell, equippedSpell.CooldownTracker.x);
            }
            PlayerMana = playerMana;
        }
    }

    public void SetPlayerTurnEnded(bool b)
    {
        playerTurnEnded = b;
    }

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        // Set Up Combat

        // Enemy Stuff
        currentEnemy = combat.Enemy;
        maxEnemyHP = currentEnemy.GetMaxHP();

        // Cheaters Confessional Effect
        if (GameManager._Instance.HasBook(BookLabel.CheatersConfessional))
        {
            currentEnemyHP = Mathf.CeilToInt(maxEnemyHP * CheatersConfessional.PercentHP);
            GameManager._Instance.AnimateBook(BookLabel.CheatersConfessional);
        }
        else
        {
            currentEnemyHP = maxEnemyHP;
        }

        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();
        enemyHPBar.Set(currentEnemyHP, maxEnemyHP);

        // Player Stuff
        characterCombatSprite.sprite = GameManager._Instance.GetCharacter().GetCombatSprite();
        characterHPBar.Set(GameManager._Instance.GetCurrentCharacterHP(), GameManager._Instance.GetMaxPlayerHP());

        // Set music source
        // Read Circle Data (.osu)
        // Set Hit Sound
        // Set Miss Sound
        musicSource.clip = combat.MainMusic;
        musicSource.Play();

        // Set settings
        InCombat = true;
        SetActiveSpellCooldowns = true;
        DuplicatePassiveSpellProcs = false;

        StartCoroutine(UpdateDuringCombat());
        yield return StartCoroutine(CombatLoop());

        if (GameManager._Instance.GetMaxPlayerHP() <= 0)
        {
            GameManager._Instance.GameOver();
        }
        else
        {
            // Bandaged Effect
            if (TargetHasAffliction(AfflictionType.Bandaged, Target.Character))
            {
                GameManager._Instance.AlterPlayerHP(characterAfflictionMap[AfflictionType.Bandaged].GetStacks(), DamageType.Heal);
            }

            SetActiveSpellCooldowns = false;
            InCombat = false;

            // Reset
            ResetCombat();

            GameManager._Instance.ResolveCurrentEvent();

            Debug.Log("Combat Completed: " + combat);
        }
    }

    private IEnumerator CombatLoop()
    {
        while (true)
        {
            yield return StartCoroutine(PlayerTurn());

            yield return StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player Turn Started");
        currentTurn = Turn.Player;

        OnPlayerTurnStart?.Invoke();


        // Reset for Turn
        ResetCombatentWard(Target.Character);
        gameStateQueue.Clear();
        GameManager._Instance.AlterPlayerMana(GameManager._Instance.GetCharacter().GetManaPerTurn());
        SaveNewCastSpellGameState();

        yield return new WaitUntil(() => playerTurnEnded);
        playerTurnEnded = false;

        yield return StartCoroutine(CastQueue());

        GameManager._Instance.ResetActiveSpellCooldowns();


        Debug.Log("Player Turn Ended");
    }

    public void AddSpellToCastQueue(ActiveSpell spell)
    {
        // Ensure only the most recent spell can be removed
        if (spellQueue.Count > 0)
            spellQueue[spellQueue.Count - 1].SetCanBeRemoved(false);

        // Spawn new display
        SpellQueueDisplay spawned = Instantiate(spellQueueDisplayPrefab, spellQueueDisplayList);
        spawned.Set(spell.name, spell.GetSpellSprite(), spellQueue.Count);
        // Ensure only the most recent spell can be removed
        spawned.CanBeRemoved = true;

        spellQueue.Add(new QueuedActiveSpell(spell, spawned, spellQueue.Count));

        // Activate Callback
        OnActiveSpellQueued?.Invoke();

        // Save game state from before taking action
        SaveNewCastSpellGameState();

        // Set Cooldown
        if (SetActiveSpellCooldowns)
        {
            spell.SetOnCooldown();
        }

        // Consume Mana
        GameManager._Instance.AlterPlayerMana(-spell.GetManaCost());

        // Tick other spell cooldowns
        List<ActiveSpell> activeSpells = GameManager._Instance.GetActiveSpells();
        foreach (ActiveSpell activeSpell in activeSpells)
        {
            if (!activeSpell.OnCooldown || activeSpell.Equals(spell)) continue;
            activeSpell.AlterCooldown(-1);
        }
    }

    public void RemoveSpellFromQueue(int index)
    {
        // Remove the spell from the queue and destroy it's display
        QueuedActiveSpell spell = spellQueue[index];
        Destroy(spell.Display.gameObject);
        spellQueue.RemoveAt(index);

        // Allow new oldest spell to cast to be removed
        if (spellQueue.Count > 0)
            spellQueue[spellQueue.Count - 1].SetCanBeRemoved(true);

        // Undo change cooldowns
        LoadPreviousCastSpellGameState();
    }

    private IEnumerator CastQueue()
    {
        foreach (QueuedActiveSpell queuedSpell in spellQueue)
        {
            queuedSpell.SetCanBeRemoved(false);
        }

        while (spellQueue.Count > 0)
        {
            // Update Tick Based Afflictions on Cast Spell
            UpdateTickBasedAfflictions(characterAfflictionMap, Target.Character);
            UpdateTickBasedAfflictions(enemyAfflictionMap, Target.Enemy);

            // Get Spell from Queue
            QueuedActiveSpell spell = spellQueue[0];

            // Remove Spell from Queue
            spellQueue.RemoveAt(0);

            // Remove UI from spell queue
            Destroy(spell.Display.gameObject);

            // Cast Spell
            yield return StartCoroutine(CastSpell(spell.Spell));

            yield return new WaitForSeconds(delayBetweenSpellCasts);
        }
    }

    private IEnumerator CastSpell(ActiveSpell spell)
    {
        // Set SFX source to spell audio clip
        spellSFXSource.clip = spell.AssociatedSoundClip;

        // Read Circles (.osu file/beatmap)
        int numNotes = ReadCircles(AssetDatabase.GetAssetPath(spell.MapFile));

        // Set hit & miss sounds
        hitSound = spell.HitSound;
        missSound = spell.MissSound;

        // Play Sequence
        // yield return StartCoroutine(PlaySpell(spellSFXSource, numNotes));
        yield return null;

        // Apply effects
        spell.Cast();
    }

    private IEnumerator PlaySpell(AudioSource spellAudioSource, int numNotes)
    {
        noteCount = 0;
        spellAudioSource.Play();
        while (noteCount < numNotes)
        {
            timer = (spellAudioSource.time * 1000); // Convert timer
            delayPos = (circleList[objCount].posA);
            mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Spawn object
            if (timer >= delayPos)
            {
                circleList[objCount].Show();
                objCount++;
            }

            // Check if cursor is over object
            if (Physics.Raycast(mainRay, out mainHit))
            {
                if (LayerMaskHelper.IsInLayerMask(mainHit.collider.gameObject, noteLayer) && timer >= mainHit.collider.gameObject.GetComponent<Circle>().posA + apprRate)
                {
                    mainHit.collider.gameObject.GetComponent<Circle>().Got();
                    mainHit.collider.enabled = false;
                }
            }

            // Cursor trail movement
            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorTrail.transform.position = new Vector3(mousePosition.x, mousePosition.y, -9);

            yield return null;
        }
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn Started");
        currentTurn = Turn.Enemy;
        OnEnemyTurnStart?.Invoke();

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        Debug.Log("Enemy Turn Ended");
    }

    private IEnumerator UpdateDuringCombat()
    {
        while (InCombat)
        {
            // Show Enemy HP
            enemyHPBar.SetText(currentEnemyHP + "/" + maxEnemyHP);

            // Show Character HP
            characterHPBar.SetText(GameManager._Instance.GetCurrentCharacterHP() + "/" + GameManager._Instance.GetMaxPlayerHP());

            yield return null;
        }
    }

    private void ResetCombat()
    {
        ClearAfflictionMap(enemyAfflictionMap);
        ClearAfflictionMap(characterAfflictionMap);

        // Clear active spell cooldowns
        GameManager._Instance.ResetActiveSpellCooldowns();

        musicSource.Stop();
        musicSource.time = 0;
        objCount = 0;
        noteCount = 0;

        // Destroy Circles
        ClearCircleList();
    }

    private void CallOnEndCombat()
    {
        OnCombatEnd?.Invoke();
    }

    public Turn GetTurn()
    {
        return currentTurn;
    }

    #endregion

    #region Parsing

    private void ClearCircleList()
    {
        while (circleList.Count > 0)
        {
            Circle c = circleList[0];
            circleList.RemoveAt(0);
            circlePool.Release(c);
        }
    }

    private ObjectPool<Circle> circlePool;
    private void CreateCirclePool()
    {
        circlePool = new ObjectPool<Circle>(() =>
        {
            return Instantiate(circle, null);
        }, circ =>
        {
            circ.gameObject.SetActive(true);
        }, circ =>
        {
            circ.gameObject.SetActive(false);
        }, circ =>
        {
            Destroy(circ.gameObject);
        }, true, 100);
    }

    private int ReadCircles(string path)
    {
        // Reset if neccessary
        ClearCircleList();

        // Generate new Circles
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
            Circle circle = circlePool.Get();
            GameObject circleObject = circle.gameObject;
            circle.transform.position = new Vector2(SPAWN, SPAWN);

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

            circle.Set(mainPos.x, mainPos.y, circleObject.transform.position.z, int.Parse(lineParams[2]) - apprRate);

            circleList.Add(circle);
        }
        return circleList.Count;
    }

    #endregion

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

    #region Afflictions

    [Header("Afflictions")]
    [SerializeField] private AfflictionIcon afflictionIconPrefab;
    [SerializeField] private SerializableDictionary<AfflictionType, AfflictionSign> afflictionSignMap = new SerializableDictionary<AfflictionType, AfflictionSign>();
    [SerializeField] private Transform characterAfflictionList;
    [SerializeField] private Transform enemyAfflictionList;
    private Dictionary<AfflictionType, Affliction> characterAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, Affliction> enemyAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private List<AfflictionType> toClearFromAffMap = new List<AfflictionType>();
    private Dictionary<AfflictionType, AfflictionIcon> afflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();

    public void AddAffliction(AfflictionType type, int num, Target target)
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

                if (SetAffliction(type, num, target))
                {
                    // Character was given new Affliction
                    OnCharacterGainAffliction?.Invoke();
                }
                break;
            case Target.Enemy:
                if (SetAffliction(type, num, target))
                {
                    // Enemy was given new Affliction
                    OnEnemyGainAffliction?.Invoke();
                }
                break;
        }
    }

    private bool SetAffliction(AfflictionType type, int activations, Target t)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        Transform parentTo = GetTargetParentAfflictionTo(t);

        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterStacks(activations);
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

    public void ConsumeAfflictionStack(AfflictionType type, Target target)
    {
        // Only consumes a stack if there are stacks to be consumed
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Affliction aff = map[type];

        // remove a stack
        aff.AlterStacks(-1);

        // there remains at least a stack of the affliction, do not remove
        if (!aff.CanBeCleared) return;

        // there are no stacks of the affliction remaining, remove
        AfflictionIcon i = afflictionIconTracker[type];
        afflictionIconTracker.Remove(type);
        Destroy(i.gameObject);
        map.Remove(type);

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

    private void UpdateTickBasedAfflictions(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        if (map.ContainsKey(AfflictionType.Blight))
        {
            AlterCombatentHP(-map[AfflictionType.Blight].GetStacks(), target, DamageType.Poison);
            ConsumeAfflictionStack(AfflictionType.Blight, target);
        }

        if (map.ContainsKey(AfflictionType.Burn))
        {
            AlterCombatentHP(-BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount"), target, DamageType.Fire);
            ConsumeAfflictionStack(AfflictionType.Burn, target);
        }
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

    #endregion

    #region Attacks

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

    public bool AttackCombatent(int amount, Target attacker, Target target, DamageType damageType, DamageSource source)
    {
        // Paralyzed Effect
        if (TargetHasAffliction(AfflictionType.Paralyzed, attacker))
        {
            ConsumeAfflictionStack(AfflictionType.Paralyzed, attacker);
            return true;
        }

        // Book of Effect Effect
        if (attacker == Target.Character
            && source == DamageSource.ActiveSpell
            && GameManager._Instance.HasBook(BookLabel.BookOfEffect))
        {
            amount = Mathf.CeilToInt(amount * BookOfEffect.PercentDamageMultiplier);
            GameManager._Instance.AnimateBook(BookLabel.BookOfEffect);
        }

        // Emboldened Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Emboldened, attacker))
        {
            amount = Mathf.CeilToInt(amount * BalenceManager._Instance.GetPercentValue(AfflictionType.Emboldened, "MultiplyBy"));
            ConsumeAfflictionStack(AfflictionType.Emboldened, attacker);
        }

        // Weakened Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Weakened, attacker))
        {
            amount = Mathf.CeilToInt(amount * BalenceManager._Instance.GetPercentValue(AfflictionType.Weakened, "MultiplyBy"));
            ConsumeAfflictionStack(AfflictionType.Weakened, attacker);
        }

        // Retribution Effect
        if (TargetHasAffliction(AfflictionType.Retribution, target))
        {
            DamageCombatent(BalenceManager._Instance.GetValue(AfflictionType.Retribution, "DamageAmount"), attacker, target, DamageType.Default);
            ConsumeAfflictionStack(AfflictionType.Retribution, target);
        }

        // Parry Effect
        if (TargetHasAffliction(AfflictionType.Parry, target))
        {
            ConsumeAfflictionStack(AfflictionType.Parry, target);
            Target swap = target;
            target = attacker;
            attacker = swap;
        }

        // Then to Deal Damage
        return DamageCombatent(amount, target, attacker, damageType);
    }

    public bool DamageCombatent(int amount, Target combatent, Target attacker, DamageType damageType)
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
            amount = Mathf.CeilToInt(amount * BalenceManager._Instance.GetPercentValue(AfflictionType.Vulnerable, "MultiplyBy"));
            ConsumeAfflictionStack(AfflictionType.Vulnerable, Target.Enemy);
        }

        // Guarded Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Guarded, Target.Enemy))
        {
            amount -= BalenceManager._Instance.GetValue(AfflictionType.Guarded, "ReduceBy");
            ConsumeAfflictionStack(AfflictionType.Guarded, Target.Enemy);
            if (amount > 0)
            {
                amount = 0;
            }
        }

        // Alter HP
        return AlterCombatentHP(amount, combatent, damageType);
    }

    private bool AlterCombatentHP(int amount, Target t, DamageType damageType)
    {
        // Prepared Effect
        if (TargetHasAffliction(AfflictionType.Prepared, t))
        {
            amount = 1;
            ConsumeAfflictionStack(AfflictionType.Prepared, t);
        }

        // Barbarians Tactics Effect
        if (amount < 0 && GameManager._Instance.HasBook(BookLabel.BarbariansTactics))
        {
            amount -= BarbariansTactics.DamageIncrease;
            GameManager._Instance.AnimateBook(BookLabel.BarbariansTactics);
        }

        // Call the AlterHP function on the appropriate Target
        switch (t)
        {
            case Target.Character:

                // Apply Ward
                if (characterWard > 0 &&
                    damageType == DamageType.Default)
                {
                    int wardUsed = GetWardUsed(characterWard, amount);
                    characterWard -= wardUsed;
                    amount -= wardUsed;
                }

                // Set New Ward Amount
                characterHPBar.SetWard(characterWard);

                PopupText text = Instantiate(popupTextPrefab, characterCombatSprite.transform);
                text.Set(Utils.RoundTo(amount, 1).ToString(), GameManager._Instance.GetColorByDamageSource(damageType));

                bool r = GameManager._Instance.AlterPlayerHP(amount, damageType, false);
                characterHPBar.SetCurrentHP(GameManager._Instance.GetCurrentCharacterHP());
                return r;
            case Target.Enemy:
                return AltarEnemyHP(amount, damageType);
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public bool AltarEnemyHP(int amount, DamageType damageType)
    {
        // Doctors Report Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.DoctorsReport) && amount > BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "MustBeOver"))
        {
            AddAffliction(AfflictionType.Bandaged, (int)BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "StackAmount"), Target.Character);
            GameManager._Instance.AnimateArtifact(ArtifactLabel.DoctorsReport);
        }

        // Apply Ward
        if (enemyWard > 0 &&
            damageType == DamageType.Default)
        {
            int wardUsed = GetWardUsed(enemyWard, amount);
            enemyWard -= wardUsed;
            amount -= wardUsed;
        }
        enemyHPBar.SetWard(enemyWard);

        PopupText text = Instantiate(popupTextPrefab, enemyCombatSprite.transform);
        text.Set(Utils.RoundTo(amount, 1).ToString(), GameManager._Instance.GetColorByDamageSource(damageType));

        if (currentEnemyHP + amount > maxEnemyHP)
        {
            currentEnemyHP = maxEnemyHP;
            enemyHPBar.SetCurrentHP(currentEnemyHP);
        }
        else
        {
            currentEnemyHP += amount;
            enemyHPBar.SetCurrentHP(currentEnemyHP);
            if (currentEnemyHP <= 0)
            {
                // Enemy Died
                CallOnEndCombat();
                return false;
            }
        }
        return true;
    }

    public void GiveCombatentWard(int wardAmount, Target target)
    {
        switch (target)
        {
            case Target.Character:
                characterWard += wardAmount;
                characterHPBar.SetWard(characterWard);
                break;
            case Target.Enemy:
                enemyWard += wardAmount;
                characterHPBar.SetWard(enemyWard);
                break;
        }
    }

    private int GetWardUsed(int availableWard, int damageIncoming)
    {
        if (availableWard > damageIncoming)
        {
            return availableWard - damageIncoming;
        }
        else
        {
            return availableWard;
        }
    }

    public void ResetCombatentWard(Target target)
    {
        switch (target)
        {
            case Target.Character:
                characterWard = 0;
                break;
            case Target.Enemy:
                enemyWard = 0;
                break;
        }
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
