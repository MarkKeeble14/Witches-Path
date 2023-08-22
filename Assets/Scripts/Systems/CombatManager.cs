using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Pool;
using TMPro;

public enum Turn
{
    Player, Enemy
}

public enum Target
{
    Character,
    Enemy,
}

public enum AfflictionType
{
    Embolden,
    Weak,
    Vulnerable,
    OnGuard,
    Bandages,
    Protection,
    Intangible,
    Echo,
    Poison,
    Blight,
    Burn,
    Paralyze,
    Thorns,
    Power,
    Regeneration
}

public enum DamageType
{
    Default,
    Poison,
    Electricity,
    Fire,
    Heal,
    Evil
}

public enum DamageSource
{
    ActiveSpell,
    PassiveSpell,
    BasicAttack,
    Book
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
    [SerializeField] private Circle circlePrefab; // Circle Object
    private ObjectPool<Circle> circlePool;
    private List<Circle> circleList = new List<Circle>(); // Circles List
    [SerializeField] private Transform parentNoteCirclesTo;

    // Other stuff
    [SerializeField] private LayerMask noteLayer;
    [SerializeField] private Image background;

    // Enemy Stuff
    private AudioClip hitSound;
    private AudioClip missSound;
    private Enemy currentEnemy;
    private int currentEnemyHP;
    private int maxEnemyHP;

    // Ward
    private int enemyWard;
    private int characterWard;

    [Header("References")]
    [SerializeField] private CombatentHPBar characterHPBar;
    [SerializeField] private CombatentHPBar enemyHPBar;
    [SerializeField] private IntentDisplay enemyIntentDisplay;
    [SerializeField] private TextMeshProUGUI enemyNameText;

    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private Image enemyCombatSprite;
    [SerializeField] private CanvasGroup characterCombatSpriteCV;
    [SerializeField] private CanvasGroup enemyCombatSpriteCV;
    [SerializeField] private TurnDisplay turnDisplay;
    [SerializeField] private TextMeshProUGUI effectivenessMultiplierText;

    [Header("Game")]
    [SerializeField] private PopupText popupTextPrefab;

    [Header("Audio")]
    [SerializeField] private bool playSFXOnHit;
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

    public int NumFreeSpells { get; set; }
    public bool InCombat { get; private set; }
    public bool CanCastSpells { get; private set; }
    private bool hasCastQueue;
    private bool isCastingQueue;
    [SerializeField] private string castButtonTextEndPlayerTurn = "Cast";
    [SerializeField] private string castButtonTextWhileCasting = "Casting";
    [SerializeField] private string castButtonTextPostCasting = "End Turn";
    [SerializeField] private string castButtonTextEnemyTurn = "Enemy Turn";
    [SerializeField] private TextMeshProUGUI castButtonText;

    private bool playerTurnEnded;
    private Turn currentTurn;
    private int turnCount;

    [Header("Spell Queue")]
    [SerializeField] private SpellQueueDisplay spellQueueDisplayPrefab;
    [SerializeField] private Transform spellQueueDisplayList;
    private List<InCastQueueActiveSpell> spellQueue = new List<InCastQueueActiveSpell>();
    [SerializeField] private AudioSource spellSFXSource;

    private float effectivenessMultiplier = 0;
    [SerializeField] private float decreaseEffectivenessMultiplierOnMiss = 0.25f;
    [SerializeField] private float increaseEffectivenessMultiplierOnHit = 0.1f;
    [SerializeField] private float defaultEffectivenessMultiplier = 1;
    [SerializeField] private float maxEffectivenessMultiplier;
    private List<Circle> spawnedCircles = new List<Circle>();

    public void SetPlayerTurnEnded(bool b)
    {
        playerTurnEnded = b;
    }

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        // Set Up Combat

        // Enemy Stuff
        // Reset enemy sprite CV from last Combat Dying
        enemyCombatSpriteCV.alpha = 1;

        // Set Current Variables
        currentEnemy = combat.Enemy;
        maxEnemyHP = currentEnemy.GetMaxHP();
        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();
        enemyNameText.text = currentEnemy.Name;
        // Hired Hand Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.HiredHand))
        {
            currentEnemyHP = Mathf.CeilToInt(maxEnemyHP * (HiredHand.PercentHP / 100));
            GameManager._Instance.AnimateArtifact(ArtifactLabel.HiredHand);
        }
        else
        {
            currentEnemyHP = maxEnemyHP;
        }
        enemyHPBar.Set(currentEnemyHP, maxEnemyHP);

        // Player Stuff
        characterCombatSprite.sprite = GameManager._Instance.GetCharacter().GetCombatSprite();
        characterHPBar.Set(GameManager._Instance.GetCurrentCharacterHP(), GameManager._Instance.GetMaxPlayerHP());

        // Combat stuff
        effectivenessMultiplier = defaultEffectivenessMultiplier;

        // Set music source
        // Read Circle Data (.osu)
        // Set Hit Sound
        // Set Miss Sound
        musicSource.clip = combat.MainMusic;
        musicSource.Play();

        yield return StartCoroutine(CombatLoop());

        if (GameManager._Instance.GameOvered)
        {
            // Player is Dead
            StartCoroutine(Utils.ChangeCanvasGroupAlpha(characterCombatSpriteCV, 0, Time.deltaTime * combatSpriteAlphaChangeRate));

            yield return new WaitForSeconds(delayAfterPlayerDeath);

            yield return GameManager._Instance.GameOverSequence();
        }
        else
        {
            // Enemy is Dead
            // Play Enemy Death Animation
            StartCoroutine(Utils.ChangeCanvasGroupAlpha(enemyCombatSpriteCV, 0, Time.deltaTime * combatSpriteAlphaChangeRate));

            yield return new WaitForSeconds(delayAfterEnemyDeath);

            // Bandaged Effect
            if (TargetHasAffliction(AfflictionType.Bandages, Target.Character))
            {
                int numBandagedStacks = characterAfflictionMap[AfflictionType.Bandages].GetStacks();
                GameManager._Instance.AlterPlayerHP(numBandagedStacks, DamageType.Heal);
                ConsumeAfflictionStack(AfflictionType.Bandages, Target.Character, numBandagedStacks);
                ShowAfflictionProc(AfflictionType.Bandages, Target.Character);
                yield return new WaitForSeconds(delayAfterBandagesEffect);
            }
        }

        InCombat = false;

        Debug.Log("Combat Completed: " + combat);

        // Reset
        ResetCombat();

        GameManager._Instance.ResolveCurrentEvent();
    }

    private IEnumerator CombatLoop()
    {
        // Allow player to cast spells
        CanCastSpells = true;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Combat Start", ""));

        // Call OnCombatStart
        OnCombatStart?.Invoke();

        // Set settings
        InCombat = true;

        StartCoroutine(UpdateDuringCombat());

        while (currentEnemyHP > 0 && GameManager._Instance.GetCurrentCharacterHP() > 0)
        {
            // Turn Begin

            // Increment Turn Count
            turnCount++;

            // Remove Ward from all Combatents
            enemyWard = 0;
            enemyHPBar.SetWard(enemyWard);
            characterWard = 0;
            characterHPBar.SetWard(characterWard);

            // Decide Enemy Intent
            // ?
            // Set Intent Display

            // Increase damage by Power
            int damageIncrease = GetPowerBonus(Target.Enemy);
            if (damageIncrease > 0)
            {
                ShowAfflictionProc(AfflictionType.Power, Target.Enemy);
            }
            enemyIntentDisplay.SetIntent(IntentType.Attack);
            enemyIntentDisplay.SetIntentText((currentEnemy.GetBasicAttackDamage() + damageIncrease).ToString());

            // Player Turn
            yield return StartCoroutine(PlayerTurn());

            if (CheckForCombatOver())
            {
                yield break;
            }

            // Enemy Turn
            yield return StartCoroutine(EnemyTurn());

            if (CheckForCombatOver())
            {
                yield break;
            }

            // End of Turn
            yield return new WaitForSeconds(delayAfterEnemyTurn);

            // Reset for Turn
            ResetCombatentWard(Target.Character);
            GameManager._Instance.AlterPlayerMana(GameManager._Instance.GetCharacter().GetManaPerTurn());
        }

        // Call On Combat End
        OnCombatEnd?.Invoke();
    }

    public int GetPowerBonus(Target owner)
    {
        int damageIncrease = 0;
        if (TargetHasAffliction(AfflictionType.Power, owner))
        {
            damageIncrease = GetAffliction(AfflictionType.Power, Target.Enemy).GetStacks();
        }
        return damageIncrease;
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player Turn Started");

        currentTurn = Turn.Player;

        // Allow player to cast spells
        CanCastSpells = true;
        hasCastQueue = false;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Player Turn", turnCount > 1 ? turnCount + Utils.GetNumericalSuffix(turnCount) + " Turn" : ""));

        OnPlayerTurnStart?.Invoke();

        // Tick Relevant Afflictions
        ApplyBurnEffectOnMap(characterAfflictionMap, Target.Character);
        ApplyBlightEffectOnMap(characterAfflictionMap, Target.Character);

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitUntil(() => playerTurnEnded);
        playerTurnEnded = false;

        CanCastSpells = false;

        yield return StartCoroutine(CastCharacterQueue());

        if (CheckForCombatOver())
        {
            yield break;
        }

        GameManager._Instance.ResetActiveSpellCooldowns();

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Target.Character))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Target.Character), Target.Character, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Target.Character);
        }

        Debug.Log("Player Turn Ended");
    }

    [SerializeField] private SpellPotencyDisplay castingSpellPotencyDisplayPrefab;
    [SerializeField] private SemicircleLayoutGroup castingSpellSemiCircle;

    private struct InCastQueueActiveSpell
    {
        public SpellPotencyDisplay SpellPotencyDisplay;
        public QueuedActiveSpell QueuedActiveSpell;
    }

    public void AddSpellToCastQueue(ActiveSpell spell)
    {
        // Spawn new display
        SpellQueueDisplay spawned = Instantiate(spellQueueDisplayPrefab, spellQueueDisplayList);
        spawned.Set(spell, spell.GetSpellSprite());

        // Spawn casting queue potency display
        SpellPotencyDisplay spellPotencyDisplay = Instantiate(castingSpellPotencyDisplayPrefab, castingSpellSemiCircle.transform);
        spellPotencyDisplay.SetSpell(spell);

        // Indicate the first spell to be cast
        if (spellQueue.Count == 0)
        {
            spellPotencyDisplay.SetOutlineColor(Color.red);
        }
        else
        {
            spellPotencyDisplay.SetOutlineColor(Color.black);
        }

        InCastQueueActiveSpell toQueue = new InCastQueueActiveSpell();
        toQueue.SpellPotencyDisplay = spellPotencyDisplay;
        toQueue.QueuedActiveSpell = new QueuedActiveSpell(spell, spawned, spellQueue.Count);
        spellQueue.Add(toQueue);

        // Test
        spellPotencyDisplay.SetCurrentPotency(effectivenessMultiplier);
        spellPotencyDisplay.SetMaxPotency(maxEffectivenessMultiplier);

        // Check for Free Spell Casts
        if (NumFreeSpells > 0)
        {
            NumFreeSpells -= 1;
        }
        else
        {
            // Set Cooldown
            spell.SetOnCooldown();

            // Consume Mana
            GameManager._Instance.AlterPlayerMana(-spell.GetManaCost());
        }

        // Activate Callback
        OnActiveSpellQueued?.Invoke();

        // Tick other spell cooldowns
        List<ActiveSpell> activeSpells = GameManager._Instance.GetActiveSpells();
        foreach (ActiveSpell activeSpell in activeSpells)
        {
            if (!activeSpell.OnCooldown || activeSpell.Equals(spell)) continue;
            activeSpell.AlterCooldown(-1);
        }
    }

    private IEnumerator CastCharacterQueue()
    {
        // Disallow tool tips from being spawned while casting is underway
        foreach (InCastQueueActiveSpell inQueue in spellQueue)
        {
            inQueue.SpellPotencyDisplay.SetCanShowToolTips(false);
        }

        // Cast Queue
        isCastingQueue = true;

        // Set effectiveness multiplier text to be active
        effectivenessMultiplierText.gameObject.SetActive(true);

        while (spellQueue.Count > 0)
        {
            // Apply Poison Effect
            ApplyPoisonEffectOnMap(enemyAfflictionMap, Target.Enemy);

            // Get Spell from Queue
            InCastQueueActiveSpell spell = spellQueue[0];

            // Remove Spell from Queue
            spellQueue.RemoveAt(0);

            // Show next up spell cast
            if (spellQueue.Count > 0)
            {
                spellQueue[0].SpellPotencyDisplay.SetOutlineColor(Color.red);
            }

            // Remove UI from spell queue & from Spell Potency Display
            Destroy(spell.QueuedActiveSpell.Display.gameObject);

            // Cast Spell
            yield return StartCoroutine(CastSpell(spell));

            if (CheckForCombatOver())
            {
                yield break;
            }

            yield return new WaitForSeconds(delayBetweenSpellCasts);
        }

        // Set effectiveness multiplier text to be inactive
        effectivenessMultiplierText.gameObject.SetActive(false);

        isCastingQueue = false;
        hasCastQueue = true;
    }

    private bool CheckForCombatOver()
    {
        return currentEnemyHP <= 0 || GameManager._Instance.GetCurrentCharacterHP() <= 0;
    }

    private IEnumerator CastSpell(InCastQueueActiveSpell queuedActiveSpell)
    {
        ActiveSpell spell = queuedActiveSpell.QueuedActiveSpell.Spell;

        // Set SFX source to spell audio clip
        spellSFXSource.clip = spell.AssociatedSoundClip;

        // Set hit & miss sounds
        hitSound = spell.HitSound;
        missSound = spell.MissSound;

        spellSFXSource.Play();

        // Play Sequence
        yield return StartCoroutine(PlaySpell(queuedActiveSpell));
    }

    public float GetActiveSpellEffectivenessMultiplier()
    {
        return effectivenessMultiplier;
    }

    private IEnumerator UpdateSpellPotencyDisplay(SpellPotencyDisplay display)
    {
        while (true)
        {
            display.SetCurrentPotency(effectivenessMultiplier);
            yield return null;
        }
    }

    private IEnumerator PlaySpell(InCastQueueActiveSpell queueActiveSpell)
    {
        ActiveSpell spell = queueActiveSpell.QueuedActiveSpell.Spell;
        SpellPotencyDisplay potencyDisplay = queueActiveSpell.SpellPotencyDisplay;
        Coroutine updatePotencyDisplay = StartCoroutine(UpdateSpellPotencyDisplay(potencyDisplay));
        float t = 0;

        for (int i = 0; i < spell.Batches; i++)
        {
            // Spawn Batch of Circles
            for (int p = 0; p < RandomHelper.RandomIntInclusive(spell.MinMaxNotesPerBatch); p++)
            {
                Circle c = circlePool.Get();
                spawnedCircles.Add(c);
                c.Set();

                t = 0;
                while (t < delayBetweenSpellNotes)
                {
                    t += Time.deltaTime;

                    // Check if Killed the enemy during Spell Sequence
                    if (CheckForCombatOver())
                    {
                        // if so, cancel the rest of the spell
                        while (spawnedCircles.Count > 0)
                        {
                            Circle spawned = spawnedCircles[0];
                            spawned.Cancel();
                        }
                        yield break;
                    }
                    yield return null;
                }
            }

            // Depending on how many are hit, spell power is increased
            t = 0;
            while (t < delayBetweenSpellBatches)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        // Stop update potency display coroutine
        StopCoroutine(updatePotencyDisplay);

        // Apply effects
        spell.Cast();
        Destroy(potencyDisplay.gameObject);

        // Reset
        effectivenessMultiplier = defaultEffectivenessMultiplier;
    }

    public void ReleaseCircle(Circle circle)
    {
        circlePool.Release(circle);
        spawnedCircles.Remove(circle);
    }


    private void ClearCircleList()
    {
        while (circleList.Count > 0)
        {
            Circle c = circleList[0];
            circleList.RemoveAt(0);
            circlePool.Release(c);
        }
    }

    private void CreateCirclePool()
    {
        circlePool = new ObjectPool<Circle>(() =>
        {
            Circle c = Instantiate(circlePrefab, parentNoteCirclesTo);
            c.ResetCircle();
            return c;
        }, circ =>
        {
            circ.gameObject.SetActive(true);
        }, circ =>
        {
            circ.gameObject.SetActive(false);
            circ.ResetCircle();
        }, circ =>
        {
            Destroy(circ.gameObject);
        }, true, 100);
    }

    public void OnNoteHit()
    {
        // Play SFX
        if (playSFXOnHit)
        {
            sfxSource.PlayOneShot(hitSound);
        }

        // Player Attack
        PlayerBasicAttack();

        // Increase effectiveness Multiplier
        if (effectivenessMultiplier + increaseEffectivenessMultiplierOnHit > maxEffectivenessMultiplier)
        {
            effectivenessMultiplier = maxEffectivenessMultiplier;
        }
        else
        {
            effectivenessMultiplier += increaseEffectivenessMultiplierOnHit;
        }
    }

    public void OnNoteMiss()
    {
        // Play SFX
        if (playSFXOnMiss)
        {
            sfxSource.PlayOneShot(missSound);
        }

        // Enemy Attack?

        // Decrease effectiveness Multiplier
        if (effectivenessMultiplier - decreaseEffectivenessMultiplierOnMiss < 0)
        {
            effectivenessMultiplier = 0;
        }
        else
        {
            effectivenessMultiplier -= decreaseEffectivenessMultiplierOnMiss;
        }
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn Started");

        currentTurn = Turn.Enemy;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Enemy Turn", ""));

        OnEnemyTurnStart?.Invoke();

        // Tick Relevant Afflictions
        ApplyBurnEffectOnMap(enemyAfflictionMap, Target.Enemy);
        ApplyBlightEffectOnMap(enemyAfflictionMap, Target.Enemy);

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeEnemyAttack);

        ResetCombatentWard(Target.Enemy);

        yield return StartCoroutine(EnemyAttack());

        // Clear Enemy Intent
        enemyIntentDisplay.ClearIntents();

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Target.Enemy))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Target.Enemy), Target.Enemy, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Target.Enemy);
        }

        Debug.Log("Enemy Turn Ended");
    }

    private IEnumerator UpdateDuringCombat()
    {
        while (InCombat)
        {
            // Set effectiveness multiplier text
            effectivenessMultiplierText.text = "x" + Utils.RoundTo(effectivenessMultiplier, 2).ToString();

            // Change the text of the cast Button depending on what's happening
            if (currentTurn == Turn.Enemy)
            {
                castButtonText.text = castButtonTextEnemyTurn;
            }
            else if (currentTurn == Turn.Player)
            {
                if (hasCastQueue)
                {
                    castButtonText.text = castButtonTextPostCasting;
                }
                else if (isCastingQueue)
                {
                    castButtonText.text = castButtonTextWhileCasting;
                }
                else
                {
                    castButtonText.text = castButtonTextEndPlayerTurn;
                }
            }

            // Show Enemy HP
            enemyHPBar.SetText(currentEnemyHP + "/" + maxEnemyHP);

            // Show Character HP
            characterHPBar.SetText(GameManager._Instance.GetCurrentCharacterHP() + "/" + GameManager._Instance.GetMaxPlayerHP());

            yield return null;
        }
    }

    private void ResetCombat()
    {
        // Reset Turn Count
        turnCount = 0;
        // Reset Num Free Spells
        NumFreeSpells = 0;

        isCastingQueue = false;
        hasCastQueue = false;

        // Clear Afflictions
        ClearAfflictionMap(enemyAfflictionMap, Target.Enemy);
        ClearAfflictionMap(characterAfflictionMap, Target.Character);

        ResetCombatentWard(Target.Character);
        ResetCombatentWard(Target.Enemy);

        // Clear active spell cooldowns
        GameManager._Instance.ResetActiveSpellCooldowns();

        // Reset Player Mana
        GameManager._Instance.SetPlayerMana(GameManager._Instance.GetMaxPlayerMana());

        // Reset HP Bars
        characterHPBar.Clear();
        enemyHPBar.Clear();

        musicSource.Stop();
        musicSource.time = 0;

        // Clear Spell Queue
        while (spellQueue.Count > 0)
        {
            InCastQueueActiveSpell spell = spellQueue[0];
            Destroy(spell.QueuedActiveSpell.Display.gameObject);
            Destroy(spell.SpellPotencyDisplay.gameObject);
            spellQueue.RemoveAt(0);
        }

        // Destroy Circles
        ClearCircleList();
    }

    public Turn GetTurn()
    {
        return currentTurn;
    }

    #endregion

    public void HalfLitFireworkProc()
    {

    }

    public void MagicRainProc(float damageAmount)
    {

    }

    public void PhantasmalWhispersProc()
    {

    }


    public void BoulderProc(float damage)
    {

    }

    #region Afflictions

    [Header("Afflictions")]
    [SerializeField] private AfflictionIcon afflictionIconPrefab;
    [SerializeField] private Transform characterAfflictionList;
    [SerializeField] private Transform enemyAfflictionList;
    private Dictionary<AfflictionType, Affliction> characterAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, Affliction> enemyAfflictionMap = new Dictionary<AfflictionType, Affliction>();

    private Dictionary<AfflictionType, AfflictionIcon> characterAfflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();
    private Dictionary<AfflictionType, AfflictionIcon> enemyAfflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();

    public void AddAffliction(AfflictionType type, int num, Target target)
    {
        switch (target)
        {
            case Target.Character:

                if (type == AfflictionType.Weak && GameManager._Instance.HasArtifact(ArtifactLabel.SpecialSpinich))
                {
                    GameManager._Instance.AnimateArtifact(ArtifactLabel.SpecialSpinich);
                    return;
                }

                if (type == AfflictionType.Vulnerable && GameManager._Instance.HasArtifact(ArtifactLabel.HolyShield))
                {
                    GameManager._Instance.AnimateArtifact(ArtifactLabel.HolyShield);
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

    public Affliction GetAffliction(AfflictionType type, Target owner)
    {
        if (TargetHasAffliction(type, owner))
        {
            return GetTargetAfflictionMap(owner)[type];
        }
        else
        {
            return null;
        }
    }

    public void ShowAfflictionProc(AfflictionType type, Target t)
    {
        Dictionary<AfflictionType, AfflictionIcon> map = GetTargetAfflictionDisplays(t);
        if (map.ContainsKey(type))
        {
            map[type].AnimateScale();
        }
    }

    public Affliction GetAfflictionOfType(AfflictionType type)
    {
        switch (type)
        {
            case AfflictionType.Bandages:
                return new Bandages();
            case AfflictionType.Blight:
                return new Blight();
            case AfflictionType.Burn:
                return new Burn();
            case AfflictionType.Echo:
                return new Echo();
            case AfflictionType.Embolden:
                return new Embolden();
            case AfflictionType.Intangible:
                return new Intangible();
            case AfflictionType.OnGuard:
                return new OnGuard();
            case AfflictionType.Paralyze:
                return new Paralyze();
            case AfflictionType.Poison:
                return new Poison();
            case AfflictionType.Power:
                return new Power();
            case AfflictionType.Protection:
                return new Protection();
            case AfflictionType.Thorns:
                return new Thorns();
            case AfflictionType.Vulnerable:
                return new Vulnerable();
            case AfflictionType.Weak:
                return new Weak();
            case AfflictionType.Regeneration:
                return new Regeneration();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private bool SetAffliction(AfflictionType type, int activations, Target target)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Transform parentTo = GetTargetParentAfflictionTo(target);

        bool isNewInstance;

        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            map[type].AlterStacks(activations);
            ShowAfflictionProc(type, target);
            isNewInstance = false;
        }
        else
        {
            Affliction aff = GetAfflictionOfType(type);
            aff.SetStacks(activations);
            aff.SetOwner(target);
            map.Add(type, aff);

            AfflictionIcon spawned = Instantiate(afflictionIconPrefab, parentTo);
            spawned.SetAffliction(aff);
            GetTargetAfflictionDisplays(target).Add(type, spawned);
            ShowAfflictionProc(type, target);
            isNewInstance = true;
        }

        // Update hp bar
        UpdateHPBarAfflictions(type, target);
        return isNewInstance;
    }

    private void UpdateHPBarAfflictions(AfflictionType type, Target target)
    {
        switch (type)
        {
            case AfflictionType.Poison:
                GetCombatentHPBar(target).SetDamageFromPoison(GetTargetAfflictionStacks(AfflictionType.Poison, target));
                break;
            case AfflictionType.Burn:
                GetCombatentHPBar(target).SetDamageFromBurn(BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount"));
                break;
            case AfflictionType.Blight:
                GetCombatentHPBar(target).SetDamageFromBlight(GetTargetAfflictionStacks(AfflictionType.Blight, target));
                break;
            default:
                break;
        }
    }

    private void ClearAfflictionMap(Dictionary<AfflictionType, Affliction> map, Target t)
    {
        Dictionary<AfflictionType, Affliction>.KeyCollection keys = map.Keys;
        Dictionary<AfflictionType, AfflictionIcon> displays = GetTargetAfflictionDisplays(t);
        foreach (AfflictionType type in keys)
        {
            AfflictionIcon icon = displays[type];
            displays.Remove(type);
            Destroy(icon.gameObject);
        }
        map.Clear();
    }

    private Dictionary<AfflictionType, Affliction> GetTargetAfflictionMap(Target t)
    {
        return t == Target.Character ? characterAfflictionMap : enemyAfflictionMap;
    }

    private Dictionary<AfflictionType, AfflictionIcon> GetTargetAfflictionDisplays(Target t)
    {
        return t == Target.Character ? characterAfflictionIconTracker : enemyAfflictionIconTracker;
    }

    private Transform GetTargetParentAfflictionTo(Target t)
    {
        return t == Target.Character ? characterAfflictionList : enemyAfflictionList;
    }

    public bool TargetHasAffliction(AfflictionType type, Target target)
    {
        return GetTargetAfflictionMap(target).ContainsKey(type);
    }

    public void ConsumeAfflictionStack(AfflictionType type, Target target, int toConsume = 1)
    {
        // Only consumes a stack if there are stacks to be consumed
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Affliction aff = map[type];

        // remove a stack
        aff.AlterStacks(-toConsume);

        // update hp bar
        UpdateHPBarAfflictions(type, target);

        // there remains at least a stack of the affliction, do not remove
        if (!aff.CanBeCleared) return;

        // there are no stacks of the affliction remaining, remove
        Dictionary<AfflictionType, AfflictionIcon> displays = GetTargetAfflictionDisplays(target);
        AfflictionIcon i = displays[type];
        displays.Remove(type);
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

    private int GetTargetAfflictionStacks(AfflictionType type, Target target)
    {
        if (TargetHasAffliction(type, target))
        {
            return GetTargetAfflictionMap(target)[type].GetStacks();
        }
        else
        {
            return 0;
        }
    }

    private CombatentHPBar GetCombatentHPBar(Target target)
    {
        switch (target)
        {
            case Target.Character:
                return characterHPBar;
            case Target.Enemy:
                return enemyHPBar;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private void ApplyPoisonEffectOnMap(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        if (map.ContainsKey(AfflictionType.Poison))
        {
            AlterCombatentHP(-map[AfflictionType.Poison].GetStacks(), target, DamageType.Poison);
            int v = BalenceManager._Instance.GetValue(AfflictionType.Poison, "PercentToReduceBy");
            float percentToMultiplyBy = (float)v / 100;
            ConsumeAfflictionStack(AfflictionType.Poison, target, Mathf.RoundToInt(GetAffliction(AfflictionType.Poison, target).GetStacks() * percentToMultiplyBy));
            ShowAfflictionProc(AfflictionType.Poison, target);
        }

    }

    private void ApplyBlightEffectOnMap(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        if (map.ContainsKey(AfflictionType.Blight))
        {
            AlterCombatentHP(-map[AfflictionType.Blight].GetStacks(), target, DamageType.Poison);
            AddAffliction(AfflictionType.Blight, 1, target);
            ShowAfflictionProc(AfflictionType.Blight, target);
        }
    }

    private void ApplyBurnEffectOnMap(Dictionary<AfflictionType, Affliction> map, Target target)
    {
        if (map.ContainsKey(AfflictionType.Burn))
        {
            AlterCombatentHP(-BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount"), target, DamageType.Fire);
            ConsumeAfflictionStack(AfflictionType.Burn, target);
            ShowAfflictionProc(AfflictionType.Burn, target);
        }
    }

    public void ClearRandomAffliction(Target t, AfflictionSign sign)
    {
        List<AfflictionType> negativeAfflictions = new List<AfflictionType>();
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            if (kvp.Value.Sign == AfflictionSign.Negative)
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
        Dictionary<AfflictionType, AfflictionIcon> displays = GetTargetAfflictionDisplays(t);
        AfflictionIcon icon = displays[type];
        displays.Remove(type);
        Destroy(icon.gameObject);
        GetTargetAfflictionMap(t).Remove(type);
    }

    #endregion

    #region Attacks

    [Header("Enemy Animations")]
    [SerializeField] private RectTransform enemyCombatSpriteRect;
    [SerializeField] private int spriteOffset = 50;
    [SerializeField] private float baseEnemyCombatSpriteAnimationSpeed = 25;
    [SerializeField] private float enemyCombatSpriteAnimationSpeedMultiplierStart = 1;
    [SerializeField] private float enemyCombatSpriteAnimationSpeedMultiplierGain = 1;
    [SerializeField] private float combatSpriteAlphaChangeRate = 5;

    [Header("Delays")]
    [SerializeField] private float delayBetweenSpellCasts = 1;
    [SerializeField] private float delayAfterPlayerDeath = 2;
    [SerializeField] private float delayAfterEnemyDeath = 2;
    [SerializeField] private float delayAfterBandagesEffect = 1;
    [SerializeField] private float delayBeforeEnemyAttack = 1;
    [SerializeField] private float delayAfterEnemyTurn = 1;
    // These should maybe go in Spells
    [SerializeField] private float delayBetweenSpellBatches = .5f;
    [SerializeField] private float delayBetweenSpellNotes = .25f;

    // Basic Attack
    private void PlayerBasicAttack()
    {
        // Get damage bonus from the Power Affliction
        int damageIncrease = GetPowerBonus(Target.Character);
        if (damageIncrease > 0)
        {
            ShowAfflictionProc(AfflictionType.Power, Target.Character);
        }

        // Increase the players basic attack damage depending on their damage bonus from equipment
        int damage = GameManager._Instance.DamageFromEquipment + damageIncrease + GameManager._Instance.GetBasicAttackDamage();

        // if the players damage after all this is negative, instead consider it zero
        // this is done in order to prevent the players basic attacks from healing the enemy, which would suck
        // Attack the enemy
        if (damage > 0)
        {
            AttackCombatent(-(damage), Target.Character, Target.Enemy, DamageType.Default, DamageSource.BasicAttack);
        }
        else
        {
            AttackCombatent(0, Target.Character, Target.Enemy, DamageType.Default, DamageSource.BasicAttack);
        }

        OnPlayerAttack?.Invoke();
    }

    private IEnumerator AnimateEnemySpriteAttack()
    {
        Vector2 originalPos = enemyCombatSpriteRect.anchoredPosition;
        Vector2 newPos = originalPos - new Vector2(spriteOffset, 0);
        float animationSpeedMultiplier = enemyCombatSpriteAnimationSpeedMultiplierStart;
        while (enemyCombatSpriteRect.anchoredPosition.x > newPos.x)
        {
            enemyCombatSpriteRect.anchoredPosition = Vector2.MoveTowards(enemyCombatSpriteRect.anchoredPosition, newPos, Time.deltaTime * baseEnemyCombatSpriteAnimationSpeed * animationSpeedMultiplier);
            animationSpeedMultiplier += Time.deltaTime * enemyCombatSpriteAnimationSpeedMultiplierGain;
            yield return null;
        }

        while (enemyCombatSpriteRect.anchoredPosition.x < originalPos.x)
        {
            enemyCombatSpriteRect.anchoredPosition = Vector2.MoveTowards(enemyCombatSpriteRect.anchoredPosition, originalPos, Time.deltaTime * baseEnemyCombatSpriteAnimationSpeed * animationSpeedMultiplier);
            yield return null;
        }
    }

    private IEnumerator EnemyAttack()
    {
        // Simple way of attacking for now
        // Damage increase from equipment is built into basic attack damage

        yield return StartCoroutine(AnimateEnemySpriteAttack());

        if (AttackCombatent(-currentEnemy.GetBasicAttackDamage(), Target.Enemy, Target.Character, DamageType.Default, DamageSource.BasicAttack))
        {
            // Only call on enemy attack if the player is still alive
            OnEnemyAttack?.Invoke();
        }
    }

    public bool AttackCombatent(int amount, Target attacker, Target target, DamageType damageType, DamageSource source)
    {
        // Paralyze Effect
        if (TargetHasAffliction(AfflictionType.Paralyze, attacker))
        {
            ConsumeAfflictionStack(AfflictionType.Paralyze, attacker);
            ShowAfflictionProc(AfflictionType.Paralyze, attacker);
            return true;
        }

        // Black Prism Effect
        if (attacker == Target.Character
            && source == DamageSource.ActiveSpell
            && GameManager._Instance.HasArtifact(ArtifactLabel.BlackPrism))
        {
            amount = Mathf.CeilToInt(amount * (BlackPrism.DamageMultiplier / 100));
            GameManager._Instance.AnimateArtifact(ArtifactLabel.BlackPrism);
        }

        // Thorns Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Thorns, target))
        {
            int damage = GetTargetAfflictionMap(target)[AfflictionType.Thorns].GetStacks();
            ShowAfflictionProc(AfflictionType.Thorns, target);
            AlterCombatentHP(-damage, attacker, DamageType.Default);
        }

        // Embolden Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Embolden, attacker))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Embolden, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);
            ConsumeAfflictionStack(AfflictionType.Embolden, attacker);
            ShowAfflictionProc(AfflictionType.Embolden, attacker);
        }

        // Weak Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Weak, attacker))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Weak, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);
            ConsumeAfflictionStack(AfflictionType.Weak, attacker);
            ShowAfflictionProc(AfflictionType.Weak, attacker);
        }

        // Then to Deal Damage
        return DamageCombatent(amount, target, attacker, damageType);
    }

    public bool DamageCombatent(int amount, Target combatent, Target attacker, DamageType damageType)
    {
        if (amount < 0 && combatent == Target.Character)
        {
            // Reduce the amount of damage by the players defense added by equipment

            // if the players defense from equipment is negative, consider it zero
            int defenseFromEquipment = GameManager._Instance.DefenseFromEquipment;
            if (defenseFromEquipment < 0)
            {
                defenseFromEquipment = 0;
            }

            // Take away from the damage amount the amount of defense
            amount += defenseFromEquipment;

            // if doing so makes amount positive, instead just cancel out
            if (amount > 0)
            {
                amount = 0;
            }
        }

        // Vulnerable Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Vulnerable, combatent))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Vulnerable, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);
            ConsumeAfflictionStack(AfflictionType.Vulnerable, combatent);
            ShowAfflictionProc(AfflictionType.Vulnerable, combatent);
        }

        // OnGuard Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.OnGuard, combatent))
        {
            amount -= BalenceManager._Instance.GetValue(AfflictionType.OnGuard, "ReduceBy");
            ConsumeAfflictionStack(AfflictionType.OnGuard, combatent);
            ShowAfflictionProc(AfflictionType.OnGuard, combatent);

            // Make sure guarded doesn't make what should be damage instead be a heal
            if (amount > 0)
            {
                amount = 0;
            }
        }

        // Alter HP
        return AlterCombatentHP(amount, combatent, damageType);
    }

    private bool AlterCombatentHP(int amount, Target combatent, DamageType damageType)
    {
        // Intangible Effect
        if (amount < 0 && TargetHasAffliction(AfflictionType.Intangible, combatent))
        {
            amount = -1;
            ConsumeAfflictionStack(AfflictionType.Intangible, combatent);
            ShowAfflictionProc(AfflictionType.Intangible, combatent);
        }

        // Barbarians Tactics Effect
        if (amount < 0 && GameManager._Instance.HasArtifact(ArtifactLabel.BarbariansBlade))
        {
            amount -= BarbariansBlade.DamageIncrease;
            GameManager._Instance.AnimateArtifact(ArtifactLabel.BarbariansBlade);
        }

        // Call the AlterHP function on the appropriate Target
        switch (combatent)
        {
            case Target.Character:

                // Apply Ward
                if (characterWard > 0 &&
                    damageType == DamageType.Default
                    && amount < 0)
                {
                    int wardUsed = GetWardUsed(characterWard, amount);
                    characterWard -= wardUsed;
                    amount += wardUsed;
                }
                // Set New Ward Amount
                characterHPBar.SetWard(characterWard);

                PopupText text = Instantiate(popupTextPrefab, characterCombatSprite.transform);
                text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));

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
        if (amount < 0
            && GameManager._Instance.HasArtifact(ArtifactLabel.DoctorsReport)
            && (amount * -1) > BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "MustBeOver"))
        {
            AddAffliction(AfflictionType.Bandages, (int)BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "StackAmount"), Target.Character);
            GameManager._Instance.AnimateArtifact(ArtifactLabel.DoctorsReport);
        }

        // Apply Ward
        if (enemyWard > 0 &&
            damageType == DamageType.Default
            && amount < 0)
        {
            int wardUsed = GetWardUsed(enemyWard, amount);
            enemyWard -= wardUsed;
            amount += wardUsed;
        }
        // Set new Ward Amount
        enemyHPBar.SetWard(enemyWard);

        PopupText text = Instantiate(popupTextPrefab, enemyCombatSprite.transform);
        text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));

        if (currentEnemyHP + amount > maxEnemyHP)
        {
            currentEnemyHP = maxEnemyHP;
            enemyHPBar.SetCurrentHP(currentEnemyHP);
        }
        else
        {
            currentEnemyHP += amount;
            if (currentEnemyHP < 0)
                currentEnemyHP = 0;
            enemyHPBar.SetCurrentHP(currentEnemyHP);
            if (currentEnemyHP <= 0)
            {
                // Enemy Died
                return false;
            }
        }
        return true;
    }

    public void GiveCombatentWard(int wardAmount, Target target)
    {
        int increaseWardBy = 0;
        if (TargetHasAffliction(AfflictionType.Protection, target))
        {
            increaseWardBy = GetAffliction(AfflictionType.Protection, target).GetStacks();
            ShowAfflictionProc(AfflictionType.Protection, target);
        }
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
        damageIncoming *= -1;
        if (availableWard > damageIncoming)
        {
            return damageIncoming;
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
                characterHPBar.SetWard(characterWard);
                break;
            case Target.Enemy:
                enemyWard = 0;
                enemyHPBar.SetWard(enemyWard);
                break;
        }
    }

    #endregion
}
