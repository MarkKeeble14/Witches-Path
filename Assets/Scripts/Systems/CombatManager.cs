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
    Evil,
    Ward
}

public enum DamageSource
{
    ActiveSpell,
    PassiveSpell,
    BasicAttack,
    Book,
    EnemyAttack
}

public partial class CombatManager : MonoBehaviour
{
    public static CombatManager _Instance { get; private set; }

    private AudioClip hitSound;
    private AudioClip missSound;
    private Enemy currentEnemy;
    private int currentEnemyHP;
    private int maxEnemyHP;

    // Ward
    private int enemyWard;
    private int characterWard;

    public float CurrentEnemyBasicAttackDamage => currentEnemy.GetBasicAttackDamage();
    public int NumFreeSpells { get; set; }
    public bool InCombat { get; private set; }
    public bool CanCastSpells { get; private set; }
    private bool hasCastQueue;
    private bool isCastingQueue;
    public bool AllowGameSpaceToolTips => !isCastingQueue;


    private bool playerTurnEnded;
    private Turn currentTurn;
    private int turnCount;

    private ObjectPool<Circle> circlePool;
    private List<Circle> circleList = new List<Circle>(); // Circles List

    [Header("Combat Settings")]
    [SerializeField] private List<DamageType> wardableDamageTypes = new List<DamageType>();

    [Header("Spell Queue")]
    [SerializeField] private Transform spellQueueDisplayList;
    private List<InCastQueueActiveSpell> spellQueue = new List<InCastQueueActiveSpell>();
    [SerializeField] private AudioSource spellSFXSource;

    [Header("Spell Potency")]
    [SerializeField] private SemicircleLayoutGroup castingSpellSemiCircle;
    [SerializeField] private float decreaseEffectivenessMultiplierOnMiss = 0.25f;
    [SerializeField] private float increaseEffectivenessMultiplierOnHit = 0.1f;
    [SerializeField] private float defaultEffectivenessMultiplier = 1;
    [SerializeField] private float maxEffectivenessMultiplier;
    [SerializeField] private float effectivenessMultiplierTextMinScale;
    [SerializeField] private float effectivenessMultiplierTextMaxScale;
    [SerializeField] private float animateEffectivenessTextRectScaleSpeed;
    private float effectivenessMultiplier = 0;
    private List<Circle> spawnedCircles = new List<Circle>();

    private struct InCastQueueActiveSpell
    {
        public SpellPotencyDisplay SpellPotencyDisplay;
        public QueuedActiveSpell QueuedActiveSpell;
    }

    [Header("Afflictions")]
    [SerializeField] private Transform characterAfflictionList;
    [SerializeField] private Transform enemyAfflictionList;
    private Dictionary<AfflictionType, Affliction> characterAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, Affliction> enemyAfflictionMap = new Dictionary<AfflictionType, Affliction>();
    private Dictionary<AfflictionType, AfflictionIcon> characterAfflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();
    private Dictionary<AfflictionType, AfflictionIcon> enemyAfflictionIconTracker = new Dictionary<AfflictionType, AfflictionIcon>();

    [Header("UI")]
    [SerializeField] private string castButtonTextEndPlayerTurn = "Cast";
    [SerializeField] private string castButtonTextWhileCasting = "Casting";
    [SerializeField] private string castButtonTextPostCasting = "End Turn";
    [SerializeField] private string castButtonTextEnemyTurn = "Enemy Turn";

    [Header("General References")]
    [SerializeField] private CombatentHPBar characterHPBar;
    [SerializeField] private CombatentHPBar enemyHPBar;
    [SerializeField] private IntentDisplay enemyIntentDisplay;
    [SerializeField] private TextMeshProUGUI enemyBasicAttackDamageText;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI castButtonText;
    [SerializeField] private TextMeshProUGUI nextTurnManaChangeText;
    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private Image enemyCombatSprite;
    [SerializeField] private CanvasGroup characterCombatSpriteCV;
    [SerializeField] private CanvasGroup enemyCombatSpriteCV;
    [SerializeField] private TurnDisplay turnDisplay;
    [SerializeField] private TextMeshProUGUI effectivenessMultiplierText;
    [SerializeField] private RectTransform effectivenessMultiplierTextRect;
    [SerializeField] private Transform parentNoteCirclesTo;
    [SerializeField] private Image background;

    [Header("Prefabs")]
    [SerializeField] private PopupText popupTextPrefab;
    [SerializeField] private AfflictionIcon afflictionIconPrefab;
    [SerializeField] private SpellQueueDisplay spellQueueDisplayPrefab;
    [SerializeField] private SpellPotencyDisplay castingSpellPotencyDisplayPrefab;
    [SerializeField] private Circle circlePrefab; // Circle Object

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

    private void Awake()
    {
        _Instance = this;

        CreateCirclePool();
    }

    #region Combat Loop

    public IEnumerator StartCombat(Combat combat)
    {
        Debug.Log("Combat Started: " + combat);

        // Set Up Combat

        // Enemy Stuff
        // Reset enemy sprite CV from last Combat Dying
        enemyCombatSpriteCV.alpha = 1;

        // Set Current Variables
        currentEnemy = combat.SpawnedEnemy;
        maxEnemyHP = currentEnemy.GetMaxHP();
        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();
        enemyNameText.text = currentEnemy.Name;
        enemyBasicAttackDamageText.text = currentEnemy.GetBasicAttackDamage().ToString();

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
            currentEnemy.OnDeath();

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
            // Set effectiveness multiplier text to be at position zero
            effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

            // Increment Turn Count
            turnCount++;

            // Decide Enemy Intent
            EnemyAction enemyAction = currentEnemy.GetEnemyIntent();
            enemyIntentDisplay.SetEnemyAction(enemyAction);

            // Player Turn
            yield return StartCoroutine(PlayerTurn());

            if (CheckForCombatOver())
            {
                yield break;
            }

            // Enemy Turn
            yield return StartCoroutine(EnemyTurn(enemyAction));

            if (CheckForCombatOver())
            {
                yield break;
            }

            // End of Turn
            yield return new WaitForSeconds(delayAfterEnemyTurn);

            // Reset for Turn
            GameManager._Instance.AlterPlayerMana(GameManager._Instance.GetManaPerTurn());
        }

        // Call On Combat End
        OnCombatEnd?.Invoke();
    }

    private IEnumerator PlayerTurn()
    {
        Debug.Log("Player Turn Started");

        currentTurn = Turn.Player;

        // Reset Ward
        ResetCombatentWard(Target.Character);

        // Allow player to cast spells
        CanCastSpells = true;
        hasCastQueue = false;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Player Turn", turnCount > 1 ? turnCount + Utils.GetNumericalSuffix(turnCount) + " Turn" : ""));

        OnPlayerTurnStart?.Invoke();

        // Tick Relevant Afflictions
        ApplyBlightEffectOnMap(characterAfflictionMap, Target.Character);
        ApplyPoisonEffectOnMap(characterAfflictionMap, Target.Character);

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
        // Cast Queue
        isCastingQueue = true;

        // Set effectiveness multiplier text to be active
        effectivenessMultiplierText.gameObject.SetActive(true);

        while (spellQueue.Count > 0)
        {
            // Get Spell from Queue
            InCastQueueActiveSpell spell = spellQueue[0];

            // Show next up spell cast
            if (spellQueue.Count > 0)
            {
                spellQueue[0].SpellPotencyDisplay.SetOutlineColor(Color.red);
            }

            // Remove Spell from Queue
            spellQueue.RemoveAt(0);

            // Remove UI from spell queue & from Spell Potency Display
            Destroy(spell.QueuedActiveSpell.Display.gameObject);

            // Cast Spell
            yield return StartCoroutine(CastSpell(spell));

            if (CheckForCombatOver())
            {
                // Killed the enemy or died themselves, either way remove the rest of the queued spells
                while (spellQueue.Count > 0)
                {
                    InCastQueueActiveSpell cur = spellQueue[0];
                    spellQueue.RemoveAt(0);

                    // Remove UI from spell queue & from Spell Potency Display
                    Destroy(cur.QueuedActiveSpell.Display.gameObject);
                    Destroy(cur.SpellPotencyDisplay.gameObject);
                    yield return null;
                }

                yield break;
            }

            yield return new WaitForSeconds(delayBetweenSpellCasts);
        }

        // Set effectiveness multiplier text to be inactive
        effectivenessMultiplierText.gameObject.SetActive(false);

        isCastingQueue = false;
        hasCastQueue = true;
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

        // Set effectiveness multiplier text to be at zero
        effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

        // Play Sequence
        yield return StartCoroutine(PlaySpell(queuedActiveSpell));
    }

    private IEnumerator EnemyTurn(EnemyAction enemyAction)
    {
        Debug.Log("Enemy Turn Started");

        currentTurn = Turn.Enemy;

        // Reset Ward
        ResetCombatentWard(Target.Enemy);

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Enemy Turn", ""));

        OnEnemyTurnStart?.Invoke();

        // Tick Relevant Afflictions
        ApplyBlightEffectOnMap(enemyAfflictionMap, Target.Enemy);
        ApplyPoisonEffectOnMap(enemyAfflictionMap, Target.Enemy);

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeEnemyAttack);

        yield return StartCoroutine(EnemyActOnIntents(enemyAction.GetEnemyIntents()));

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

            // Set effectiveness multiplier text scale
            effectivenessMultiplierTextRect.localScale = Vector3.Lerp(
                effectivenessMultiplierTextRect.localScale,
                Vector3.one * MathHelper.Normalize(effectivenessMultiplier, 0, maxEffectivenessMultiplier,
                effectivenessMultiplierTextMinScale, effectivenessMultiplierTextMaxScale),
                Time.deltaTime * animateEffectivenessTextRectScaleSpeed);

            // Set mana Texts
            nextTurnManaChangeText.text = "+" + GameManager._Instance.GetManaPerTurn();

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
            enemyHPBar.SetText(currentEnemyHP + " / " + maxEnemyHP);

            // Show Character HP
            characterHPBar.SetText(GameManager._Instance.GetCurrentCharacterHP() + " / " + GameManager._Instance.GetMaxPlayerHP());

            yield return null;
        }
    }

    private void CheckForBonusChange(int prevValue, int newValue, Action<int> onChange, Action<int> endAction)
    {
        if (prevValue != newValue) onChange(newValue);
        endAction(newValue);
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

        // Set effectiveness multiplier text to be at zero
        effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;
    }

    public void SetPlayerTurnEnded(bool b)
    {
        playerTurnEnded = b;
    }

    public Turn GetTurn()
    {
        return currentTurn;
    }

    private bool CheckForCombatOver()
    {
        return currentEnemyHP <= 0 || GameManager._Instance.GetCurrentCharacterHP() <= 0;
    }
    #endregion

    #region Spell Gameplay

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
                c.Set(UIManager._Instance.GetDamageTypeColor(spell.MainDamageType));

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

                        Destroy(potencyDisplay.gameObject);

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

    public void OnNoteHit(RectTransform ofNoteHit)
    {
        // Play SFX
        if (playSFXOnHit)
        {
            sfxSource.PlayOneShot(hitSound);
        }

        // Set effectiveness multiplier to follow where the last note was hit
        effectivenessMultiplierTextRect.anchoredPosition = ofNoteHit.anchoredPosition;

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

    public void OnNoteMiss(RectTransform ofNoteMissed)
    {
        // Play SFX
        if (playSFXOnMiss)
        {
            sfxSource.PlayOneShot(missSound);
        }

        // Set effectiveness multiplier to follow where the last note was missed
        effectivenessMultiplierTextRect.anchoredPosition = ofNoteMissed.anchoredPosition;

        // Enemy Attack?
        EnemyBasicAttack();

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
    #endregion

    #region Circle Management
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
            c.name += circlePool.CountAll;
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

    #endregion

    #region Damage Management


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
        // Attack the enemy
        AttackCombatent(GameManager._Instance.DamageFromEquipment + GameManager._Instance.GetBasicAttackDamage(), Target.Enemy, Target.Character, DamageType.Default, DamageSource.BasicAttack);

        // Burn Effect
        ApplyBurnEffectOnMap(characterAfflictionMap, Target.Character);

        // Only call this if the combat isn't over
        if (CheckForCombatOver())
        {
            OnPlayerAttack?.Invoke();
        }
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

    private void EnemyBasicAttack()
    {
        int attackDamage = currentEnemy.GetBasicAttackDamage();

        // Reduce the amount of damage by the players defense added by equipment
        // if the players defense from equipment is negative, consider it zero
        int defenseFromEquipment = GameManager._Instance.DefenseFromEquipment;
        if (defenseFromEquipment < 0)
        {
            defenseFromEquipment = 0;
        }

        // Take away from the damage amount the amount of defense
        attackDamage -= defenseFromEquipment;

        // if doing so makes amount positive, instead just cancel out
        if (attackDamage < 0)
        {
            attackDamage = 0;
        }

        AttackCombatent(attackDamage, Target.Character, Target.Enemy, DamageType.Default, DamageSource.BasicAttack);

        // Burn Effect
        ApplyBurnEffectOnMap(enemyAfflictionMap, Target.Enemy);

        // Only call on enemy attack if the player is still alive
        if (!CheckForCombatOver())
        {
            OnEnemyAttack?.Invoke();
        }
    }

    private IEnumerator EnemyActOnIntents(List<EnemyIntent> enemyIntents)
    {
        foreach (EnemyIntent intent in enemyIntents)
        {
            switch (intent)
            {
                case EnemySingleAttackIntent singleAttack:

                    yield return StartCoroutine(AnimateEnemySpriteAttack());

                    AttackCombatent(singleAttack.DamageAmount, Target.Character, Target.Enemy, singleAttack.DamageType, DamageSource.EnemyAttack);

                    break;
                case EnemyMultiAttackIntent multiAttack:

                    yield return StartCoroutine(AnimateEnemySpriteAttack());

                    for (int i = 0; i < multiAttack.NumAttacks; i++)
                    {
                        AttackCombatent(multiAttack.DamageAmount, Target.Character, Target.Enemy, multiAttack.DamageType, DamageSource.EnemyAttack);
                    }

                    break;
                case EnemyWardIntent ward:

                    GiveCombatentWard(ward.WardAmount, Target.Enemy);

                    break;
                case EnemyApplyAfflictionIntent apply:

                    yield return StartCoroutine(AnimateEnemySpriteAttack());

                    AddAffliction(apply.AfflictionType, apply.NumStacks, Target.Character);

                    break;
                case EnemyGainAfflictionIntent gain:

                    AddAffliction(gain.AfflictionType, gain.NumStacks, Target.Enemy);

                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    public void AlterCombatentHP(int amount, Target target, DamageType damageType)
    {
        // Call the AlterHP function on the appropriate Target
        PopupText text;
        switch (target)
        {
            case Target.Character:

                // Use Ward
                if (amount < 0 && wardableDamageTypes.Contains(damageType))
                {
                    int wardUsed = UseWard(amount, Target.Character, () => characterWard, i => characterWard += i);
                    amount += wardUsed;

                    // Only Spawn Text if Amount is still < 0 after using Ward
                    if (amount < 0)
                    {
                        // Spawn popup Text
                        text = Instantiate(popupTextPrefab, characterCombatSprite.transform);
                        text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));
                    }
                }
                else
                {
                    // No Ward to Complicate things
                    // Spawn popup Text
                    text = Instantiate(popupTextPrefab, characterCombatSprite.transform);
                    text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));
                }

                // Finalize player HP damage
                GameManager._Instance.AlterPlayerHP(amount, damageType, false);
                characterHPBar.SetCurrentHP(GameManager._Instance.GetCurrentCharacterHP());

                break;
            case Target.Enemy:

                // Doctors Report Effect
                if (amount < 0
                    && GameManager._Instance.HasArtifact(ArtifactLabel.DoctorsReport)
                    && (amount * -1) > BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "MustBeOver"))
                {
                    AddAffliction(AfflictionType.Bandages, (int)BalenceManager._Instance.GetValue(ArtifactLabel.DoctorsReport, "StackAmount"), Target.Character);
                    GameManager._Instance.AnimateArtifact(ArtifactLabel.DoctorsReport);
                }

                // Use Ward
                if (amount < 0 && wardableDamageTypes.Contains(damageType))
                {
                    int wardUsed = UseWard(amount, Target.Enemy, () => enemyWard, i => enemyWard += i);
                    amount += wardUsed;

                    // Only Spawn Text if Amount is still < 0 after using Ward
                    if (amount < 0)
                    {
                        // Spawn popup Text
                        text = Instantiate(popupTextPrefab, enemyCombatSprite.transform);
                        text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));
                    }
                }
                else
                {
                    // Spawn Popup text
                    text = Instantiate(popupTextPrefab, enemyCombatSprite.transform);
                    text.Set(Utils.RoundTo(amount, 1).ToString(), UIManager._Instance.GetDamageTypeColor(damageType));
                }

                // tried to heal past max
                if (currentEnemyHP + amount > maxEnemyHP)
                {
                    currentEnemyHP = maxEnemyHP;
                }
                else if (currentEnemyHP + amount < 0) // tried to damage past 0
                {
                    currentEnemyHP = 0;
                }
                else
                {
                    // Apply amount
                    currentEnemyHP += amount;
                }
                // Update HP Bar
                enemyHPBar.SetCurrentHP(currentEnemyHP);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void AttackCombatent(int amount, Target target, Target attacker, DamageType damageType, DamageSource damageSource)
    {
        // Paralyze Effect
        if (TargetHasAffliction(AfflictionType.Paralyze, attacker))
        {
            ConsumeAfflictionStack(AfflictionType.Paralyze, attacker);
            ShowAfflictionProc(AfflictionType.Paralyze, attacker);
            return;
        }

        // Thorns Effect
        if (TargetHasAffliction(AfflictionType.Thorns, attacker))
        {
            int damage = GetTargetAfflictionMap(target)[AfflictionType.Thorns].GetStacks();
            ShowAfflictionProc(AfflictionType.Thorns, target);
            AlterCombatentHP(-damage, attacker, DamageType.Default);
        }

        // Attempted to Basic Attack for less than 0 (i.e., a Heal)
        if (damageSource == DamageSource.BasicAttack && amount < 0)
        {
            AlterCombatentHP(0, target, damageType);
            return;
        }

        AlterCombatentHP(-CalculateDamage(amount, attacker, target, damageType, damageSource, true), target, damageType);
    }

    // Calculation Function
    // Takes a number and spits out the number post effects
    public int CalculateDamage(int amount, Target attacker, Target target, DamageType damageType, DamageSource source, bool consumeAfflictions)
    {
        // Attacker Effects
        // Power Effect
        if (TargetHasAffliction(AfflictionType.Power, attacker)
            && source != DamageSource.BasicAttack)
        {
            amount += GetTargetAfflictionStacks(AfflictionType.Power, attacker);
        }

        // Black Prism Effect
        if (attacker == Target.Character
            && source == DamageSource.ActiveSpell
            && GameManager._Instance.HasArtifact(ArtifactLabel.BlackPrism))
        {
            amount = Mathf.CeilToInt(amount * (BlackPrism.DamageMultiplier / 100));
            if (consumeAfflictions)
            {
                GameManager._Instance.AnimateArtifact(ArtifactLabel.BlackPrism);
            }
        }

        // Embolden Effect
        if (TargetHasAffliction(AfflictionType.Embolden, attacker))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Embolden, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);
            if (consumeAfflictions)
            {
                ConsumeAfflictionStack(AfflictionType.Embolden, attacker);
                ShowAfflictionProc(AfflictionType.Embolden, attacker);
            }
        }

        // Weak Effect
        if (TargetHasAffliction(AfflictionType.Weak, attacker))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Weak, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);

            if (consumeAfflictions)
            {
                ConsumeAfflictionStack(AfflictionType.Weak, attacker);
                ShowAfflictionProc(AfflictionType.Weak, attacker);
            }
        }

        // Reciever Effects
        // Vulnerable Effect
        if (TargetHasAffliction(AfflictionType.Vulnerable, target))
        {
            float value = BalenceManager._Instance.GetValue(AfflictionType.Vulnerable, "MultiplyBy");
            float multiplyBy = value / 100;

            amount = Mathf.CeilToInt(amount * multiplyBy);
            if (consumeAfflictions)
            {
                ConsumeAfflictionStack(AfflictionType.Vulnerable, target);
                ShowAfflictionProc(AfflictionType.Vulnerable, target);
            }
        }

        // OnGuard Effect
        if (TargetHasAffliction(AfflictionType.OnGuard, target))
        {
            amount -= BalenceManager._Instance.GetValue(AfflictionType.OnGuard, "ReduceBy");
            // Make sure guarded doesn't make what should be damage instead be a heal
            if (amount > 0)
            {
                amount = 0;
            }

            if (consumeAfflictions)
            {
                ConsumeAfflictionStack(AfflictionType.OnGuard, target);
                ShowAfflictionProc(AfflictionType.OnGuard, target);
            }
        }

        // General
        // Intangible Effect
        if (TargetHasAffliction(AfflictionType.Intangible, target))
        {
            amount = 1;
            if (consumeAfflictions)
            {
                ConsumeAfflictionStack(AfflictionType.Intangible, target);
                ShowAfflictionProc(AfflictionType.Intangible, target);
            }
        }

        // Barbarians Tactics Effect
        if (GameManager._Instance.HasArtifact(ArtifactLabel.BarbariansBlade))
        {
            amount += BarbariansBlade.DamageIncrease;
            GameManager._Instance.AnimateArtifact(ArtifactLabel.BarbariansBlade);
        }

        return amount;
    }

    // Calculation Function
    // Takes a number and spits out the number post effects
    public int CalculateWard(int amount, Target applyingTo)
    {
        if (TargetHasAffliction(AfflictionType.Protection, applyingTo))
        {
            amount += GetTargetAfflictionStacks(AfflictionType.Protection, applyingTo);
        }
        return amount;
    }

    private int UseWard(int amount, Target target, Func<int> getFunc, Action<int> alterFunc)
    {
        // Apply Ward
        int currentWard = getFunc();

        // if target has ward
        if (currentWard > 0)
        {
            // Get the amount of ward needed to be used
            int wardUsed = GetWardUsed(currentWard, amount);

            // AlterFunc will change the appropriate ward variable by the amount of ward used
            alterFunc(-wardUsed);

            // Set HP Bar
            GetCombatentHPBar(target).SetWard(getFunc());

            if (wardUsed > 0)
            {
                // Spawn Text
                PopupText wardText = Instantiate(popupTextPrefab, GetTargetSpriteImage(target).transform);
                wardText.Set(Utils.RoundTo(-wardUsed, 1).ToString(), UIManager._Instance.GetDamageTypeColor(DamageType.Ward));
            }

            // return the amount ward used;
            return wardUsed;
        }
        else
        {
            // Target has no ward, just return 0
            return 0;
        }
    }

    public void GiveCombatentWard(int wardAmount, Target target)
    {
        wardAmount = CalculateWard(wardAmount, target);
        switch (target)
        {
            case Target.Character:
                characterWard += wardAmount;
                characterHPBar.SetWard(characterWard);
                break;
            case Target.Enemy:
                enemyWard += wardAmount;
                enemyHPBar.SetWard(enemyWard);
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

    public int GetPowerBonus(Target owner)
    {
        int powerBonus = 0;
        if (TargetHasAffliction(AfflictionType.Power, owner))
        {
            powerBonus = GetAffliction(AfflictionType.Power, owner).GetStacks();
        }
        return powerBonus;
    }

    public int GetProtectionBonus(Target owner)
    {
        int protectionBonus = 0;
        if (TargetHasAffliction(AfflictionType.Protection, owner))
        {
            protectionBonus = GetAffliction(AfflictionType.Protection, owner).GetStacks();
        }
        return protectionBonus;
    }

    private Image GetTargetSpriteImage(Target target)
    {
        switch (target)
        {
            case Target.Character:
                return characterCombatSprite;
            case Target.Enemy:
                return enemyCombatSprite;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    #endregion

    #region Afflictions

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
            Affliction aff = Affliction.GetAfflictionOfType(type);
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
        int v;
        switch (type)
        {
            case AfflictionType.Poison:
                v = (TargetHasAffliction(AfflictionType.Poison, target) ? GetTargetAfflictionStacks(AfflictionType.Poison, target) : 0);
                // Debug.Log(v);
                GetCombatentHPBar(target).SetDamageFromPoison(v);
                break;
            case AfflictionType.Burn:
                v = (TargetHasAffliction(AfflictionType.Burn, target) ? BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount") : 0);
                // Debug.Log(v);
                GetCombatentHPBar(target).SetDamageFromBurn(v);
                break;
            case AfflictionType.Blight:
                v = (TargetHasAffliction(AfflictionType.Blight, target) ? GetTargetAfflictionStacks(AfflictionType.Blight, target) : 0);
                // Debug.Log(v);
                GetCombatentHPBar(target).SetDamageFromBlight(v);
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

        // there remains at least a stack of the affliction, do not remove
        if (!aff.CanBeCleared) return;

        // there are no stacks of the affliction remaining, remove
        Dictionary<AfflictionType, AfflictionIcon> displays = GetTargetAfflictionDisplays(target);
        AfflictionIcon i = displays[type];
        displays.Remove(type);
        Destroy(i.gameObject);
        map.Remove(type);

        // update hp bar
        UpdateHPBarAfflictions(type, target);

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
}
