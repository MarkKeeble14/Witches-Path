using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Pool;
using TMPro;
using DG.Tweening;
using System.Linq;

public enum SpellPileType
{
    Draw,
    Hand,
    Discard,
    Exhaust
}

public enum AlterHandSequenceType
{
    Discard,
    Exhaust
}

public enum Order
{
    Unaltered,
    Shuffled,
    Reversed
}

public enum Turn
{
    Player, Enemy
}

public enum Combatent
{
    Character,
    Enemy,
}

public enum Target
{
    Self,
    Other,
    Both
}

public enum DamageType
{
    Physical,
    Poison,
    Electric,
    Fire,
    Heal,
    Evil,
    Ward,
    Holy
}

public enum DamageSource
{
    Spell,
    BasicAttack,
    Book,
}

public enum CombatBaseCallbackType
{
    OnBasicAttack,
    OnTurnStart,
    OnTurnEnd,
    OnSpellQueued,
    OnSpellCast,
    OnAttack,
}

public enum CombatIntCallbackType
{
    OnTakeDamage
}

public enum CombatSpellCallbackType
{
    OnDraw,
    OnExhaust,
    OnSpecificDiscard,
}

public enum CombatAfflictionCallbackType
{
    OnGain,
    OnApply,
    OnRemove
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

    public Enemy CurrentEnemy => currentEnemy;
    public int NumFreeSpells { get; private set; }
    public bool InCombat { get; private set; }
    private bool combatScreenOpen;
    public bool CanCastSpells { get; private set; }
    private bool hasCastQueue;
    public bool IsCastingQueue;
    public bool AllowGameSpaceToolTips => !IsCastingQueue;

    private bool playerTurnEnded;
    private Turn currentTurn;
    private int turnNumber;
    public int TurnNumber => turnNumber;

    private ObjectPool<Circle> circlePool;
    private List<Circle> circleList = new List<Circle>(); // Circles List

    private int handSize;
    public int MaxHandSize => 10;
    public Pile<Spell> Hand;
    public Pile<Spell> DrawPile;
    public Pile<Spell> DiscardPile;
    public Pile<Spell> ExhaustPile;
    public Pile<Spell> PowerSpellPile;

    [Header("Deck Mechanics")]
    [SerializeField] private Transform discardPileMoveOnto;
    [SerializeField] private Transform discardPileParent;
    [SerializeField] private TextMeshProUGUI discardPileCountText;
    [SerializeField] private TextMeshProUGUI drawPileCountText;
    [SerializeField] private TextMeshProUGUI exhaustPileCountText;
    [SerializeField] private TextMeshProUGUI showSpellPileTitleText;
    private bool closeCurrentlyDisplayedSpellPile;
    [SerializeField] private GameObject showSpellPileScreen;
    [SerializeField] private Transform spawnShowSpellPileDisplaysOn;
    [SerializeField] private VisualSpellDisplay visualSpellDisplayPrefab;
    [SerializeField] private float drawSpellDelay = .15f;
    [SerializeField] private float forceLoseSpellDelay = .15f;
    [SerializeField] private float discardSpellDelay = .15f;
    private List<Spell> alterHandSequenceSelectedSpells = new List<Spell>();
    [SerializeField] private VisualSpellDisplay cardDisplayPrefab;
    [SerializeField] private HandLayoutGroup handLayoutGroup;
    public HandLayoutGroup HandLayoutGroup => handLayoutGroup;

    private List<QueuedSpell> enemyCastQueue = new List<QueuedSpell>();
    private List<QueuedSpell> playerCastQueue = new List<QueuedSpell>();
    [SerializeField] private AudioSource spellSFXSource;

    [Header("Spell Potency")]
    [SerializeField] private SemicircleLayoutGroup playerQueuedSpellsDisplay;
    [SerializeField] private float decreaseEffectivenessMultiplierOnMiss = 0.25f;
    [SerializeField] private float increaseEffectivenessMultiplierOnHit = 0.1f;

    [SerializeField] private float minSpellEffectivenessMultiplier = 0;
    public float MinSpellEffectivenessMultiplier => minSpellEffectivenessMultiplier;
    [SerializeField] private float defaultEffectivenessMultiplier = 1;
    public float DefaultSpellEffectivenessMultiplier => defaultEffectivenessMultiplier;
    public float MaxSpellEffectivenessMultiplier => maxSpellEffectivenessMultiplier;
    [SerializeField] private float maxSpellEffectivenessMultiplier;
    private float currentSpellEffectivenessMultiplier = 1;
    public float CurrentSpellEffectivenessMultiplier => currentSpellEffectivenessMultiplier;
    [SerializeField] private float effectivenessMultiplierTextMinScale;
    [SerializeField] private float effectivenessMultiplierTextMaxScale;
    [SerializeField] private float animateEffectivenessTextRectScaleSpeed;
    private List<Circle> spawnedCircles = new List<Circle>();

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
    [SerializeField] private GameObject alterHandInstructionContainer;
    [SerializeField] private TextMeshProUGUI alterHandInstructionText;
    [SerializeField] private GameObject alterHandBackground;
    public bool SpellPileScreenOpen => showSpellPileScreen.activeInHierarchy;
    public int NumSpellsInDraw => DrawPile == null ? 0 : DrawPile.Count;
    public int NumSpellsInDrawablePlaces => DrawPile == null || DiscardPile == null || Hand == null ? 0 : DrawPile.Count + DiscardPile.Count + Hand.Count;

    public int PlayerCastQueueSize => playerCastQueue.Count;

    [Header("Enemy")]
    [SerializeField] private CombatentHPBar enemyHPBar;
    [SerializeField] private SemicircleLayoutGroup enemyQueuedSpellsDisplay;
    [SerializeField] private TextMeshProUGUI enemyBasicAttackDamageText;
    [SerializeField] private Image enemyCombatSprite;
    [SerializeField] private CanvasGroup enemyCombatSpriteCV;
    [SerializeField] private EffectTextDisplay enemyEffectTextDisplay;

    [Header("Character")]
    [SerializeField] private CombatentHPBar characterHPBar;
    [SerializeField] private Image characterCombatSprite;
    [SerializeField] private TextMeshProUGUI characterBasicAttackDamageText;
    [SerializeField] private CanvasGroup characterCombatSpriteCV;
    [SerializeField] private EffectTextDisplay characterEffectTextDisplay;
    [SerializeField] private TextMeshProUGUI castButtonText;
    [SerializeField] private TextMeshProUGUI nextTurnManaChangeText;
    [SerializeField] private TextMeshProUGUI effectivenessMultiplierText;
    [SerializeField] private RectTransform effectivenessMultiplierTextRect;

    [Header("General References")]
    [SerializeField] private TurnDisplay turnDisplay;
    [SerializeField] private Transform parentNoteCirclesTo;
    [SerializeField] private Image background;
    [SerializeField] private GameObject[] disableWhileCasting;

    [Header("Prefabs")]
    [SerializeField] private PopupText popupTextPrefab;
    [SerializeField] private AfflictionIcon afflictionIconPrefab;
    [SerializeField] private QueuedSpellDisplay castingSpellPotencyDisplayPrefab;
    [SerializeField] private Circle circlePrefab; // Circle Object

    [Header("Audio")]
    [SerializeField] private bool playSFXOnHit;
    [SerializeField] private bool playSFXOnMiss;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Animations")]
    [Header("Enemy Animations")]
    [SerializeField] private RectTransform enemyCombatSpriteRect;
    [SerializeField] private int spriteOffset = 50;
    [SerializeField] private float baseEnemyCombatSpriteAnimationSpeed = 25;
    [SerializeField] private float combatSpriteAnimationMultiplier = 1;
    [SerializeField] private float enemyCombatSpriteAnimationSpeedMultiplierGain = 1;
    [SerializeField] private float combatSpriteAlphaChangeRate = 5;

    [Header("Damage Type Animators")]
    [SerializeField] private DamageTypeAnimator defaultDamageTypeAnimatorPrefab;
    [SerializeField] private DamageTypeAnimator wardDamageTypeAnimatorPrefab;

    [Header("Shake Combatent")]
    [SerializeField] private float shakeCombatentDuration = 1;
    [SerializeField] private float shakeCombatentStrength = 10;
    [SerializeField] private int shakeCombatentVibrato = 10;
    [SerializeField] private float shakeCombatentRandomness = 0;

    [Header("Delays")]
    [SerializeField] private float delayBetweenSpellCasts = 1;
    [SerializeField] private float delayAfterPlayerDeath = 2;
    [SerializeField] private float delayAfterEnemyDeath = 2;
    [SerializeField] private float delayAfterBandagesEffect = 1;
    [SerializeField] private float delayBeforeEnemyAttack = 1;
    [SerializeField] private float delayAfterEnemyTurn = 1;
    [SerializeField] private float delayBetweenAlterHandCalls = 0.05f;
    [SerializeField] private float delayAfterNoteSequence = 0.25f;
    [SerializeField] private float delayBetweenSpellPrepTimeTicks = 0.125f;


    [Header("Discard Animation")]
    [SerializeField] private float discardedCardMinScale = .15f;
    [SerializeField] private float discardedCardChangeScaleRate = 10;
    [SerializeField] private float discardedCardScaleGraceRange = .1f;
    [SerializeField] private float discardCardDownAmount;
    [SerializeField] private float discardSlideDuration = 1;

    [Header("Exhaust Animation")]
    [SerializeField] private float exhaustCardUpAmount;
    [SerializeField] private float exhaustSlideDuration = 1;

    [Header("Power Spell Animation")]
    [SerializeField] private float powerSpellShrinkDuration;

    [SerializeField] private GameObject[] showOnCombat;
    private List<Spell> spellCastThisTurn = new List<Spell>();

    // Callbacks
    public Action OnTurnStart;
    public Action OnExhaustSpell;
    public Action OnSpecificDiscardSpell;
    public Action OnDrawSpell;
    public Action OnCombatStart;
    public Action OnCombatEnd;
    public Action OnResetCombat;


    // Should Contain -
    // Basic Attack
    // Turn Start
    // Turn End
    // Spell Queued
    // Spell Cast
    // Gain Affliction
    // Lose Affliction
    // General Attack
    public Dictionary<Combatent, Dictionary<CombatBaseCallbackType, Action>> CombatentBaseCallbackMap = new Dictionary<Combatent, Dictionary<CombatBaseCallbackType, Action>>();

    // Should Contain -
    // Take Damage
    public Dictionary<Combatent, Dictionary<CombatIntCallbackType, Action<int>>> CombatentIntCallbackMap = new Dictionary<Combatent, Dictionary<CombatIntCallbackType, Action<int>>>();

    // Should Contain -
    // On Draw
    // On Exhaust
    // On Specific Discard
    public Dictionary<Combatent, Dictionary<CombatSpellCallbackType, Action<Spell>>> CombatentSpellCallbackMap = new Dictionary<Combatent, Dictionary<CombatSpellCallbackType, Action<Spell>>>();

    // Should Contain -
    // On Gain
    // On Apply
    public Dictionary<Combatent, Dictionary<CombatAfflictionCallbackType, Action<Affliction>>> CombatentAfflictionCallbackMap
        = new Dictionary<Combatent, Dictionary<CombatAfflictionCallbackType, Action<Affliction>>>();

    private void SetupCombatentBaseCallbackMap(Combatent combatent)
    {
        Dictionary<CombatBaseCallbackType, Action> callbackMap = new Dictionary<CombatBaseCallbackType, Action>();
        callbackMap.Add(CombatBaseCallbackType.OnSpellQueued, null);
        callbackMap.Add(CombatBaseCallbackType.OnSpellCast, null);
        callbackMap.Add(CombatBaseCallbackType.OnAttack, null);
        callbackMap.Add(CombatBaseCallbackType.OnBasicAttack, null);
        callbackMap.Add(CombatBaseCallbackType.OnTurnEnd, null);
        callbackMap.Add(CombatBaseCallbackType.OnTurnStart, null);
        CombatentBaseCallbackMap.Add(combatent, callbackMap);
    }

    private void SetupCombatentIntCallbackMap(Combatent combatent)
    {
        Dictionary<CombatIntCallbackType, Action<int>> callbackMap = new Dictionary<CombatIntCallbackType, Action<int>>();
        callbackMap.Add(CombatIntCallbackType.OnTakeDamage, null);
        CombatentIntCallbackMap.Add(combatent, callbackMap);
    }

    private void SetupCombatentSpellCallbackMap(Combatent combatent)
    {
        Dictionary<CombatSpellCallbackType, Action<Spell>> callbackMap = new Dictionary<CombatSpellCallbackType, Action<Spell>>();
        callbackMap.Add(CombatSpellCallbackType.OnDraw, null);
        callbackMap.Add(CombatSpellCallbackType.OnExhaust, null);
        callbackMap.Add(CombatSpellCallbackType.OnSpecificDiscard, null);
        CombatentSpellCallbackMap.Add(combatent, callbackMap);
    }

    private void SetupCombatentAfflictionCallbackMap(Combatent combatent)
    {
        Dictionary<CombatAfflictionCallbackType, Action<Affliction>> callbackMap = new Dictionary<CombatAfflictionCallbackType, Action<Affliction>>();
        callbackMap.Add(CombatAfflictionCallbackType.OnApply, null);
        callbackMap.Add(CombatAfflictionCallbackType.OnGain, null);
        callbackMap.Add(CombatAfflictionCallbackType.OnRemove, null);
        CombatentAfflictionCallbackMap.Add(combatent, callbackMap);
    }

    private void SetupCombatCallbackMaps()
    {
        SetupCombatentBaseCallbackMap(Combatent.Character);
        SetupCombatentIntCallbackMap(Combatent.Character);
        SetupCombatentSpellCallbackMap(Combatent.Character);
        SetupCombatentAfflictionCallbackMap(Combatent.Character);

        SetupCombatentBaseCallbackMap(Combatent.Enemy);
        SetupCombatentIntCallbackMap(Combatent.Enemy);
        SetupCombatentSpellCallbackMap(Combatent.Enemy);
        SetupCombatentAfflictionCallbackMap(Combatent.Enemy);
    }

    private Dictionary<Combatent, List<Tween>> combatentShakeTweenDict = new Dictionary<Combatent, List<Tween>>();

    private Coroutine recalculateKeyBindsCoroutine;

    private EnemyAction currentEnemyAction;

    [System.Serializable]
    public class SpeechBubbleDataContainer
    {
        [SerializeField] private TextMeshProUGUI dialogueText;
        public TextMeshProUGUI Text => dialogueText;
        [SerializeField] private CanvasGroup cv;
        public CanvasGroup CV => cv;
        public Coroutine CurrentCoroutine;
    }

    [SerializeField] private SerializableDictionary<Combatent, SpeechBubbleDataContainer> speechBubbles = new SerializableDictionary<Combatent, SpeechBubbleDataContainer>();
    [SerializeField] private float dialogueBubbleFadeInRate = 1;
    [SerializeField] private float dialogueBubbleFadeOutRate = 1;
    [SerializeField] private float defaultDialogueDuration = 2.5f;

    private KeyCode[] handHotKeyBindings = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
    private KeyCode GetKeyBindingAtIndex(int index)
    {
        return handHotKeyBindings[index];
    }

    public void CallPlayDialogue(string s, Combatent target)
    {
        CallPlayDialogue(s, defaultDialogueDuration, target);
    }

    public void CallPlayDialogue(string s, float duration, Combatent target)
    {
        if (speechBubbles[target].CurrentCoroutine != null) StopCoroutine(speechBubbles[target].CurrentCoroutine);
        speechBubbles[target].CurrentCoroutine = StartCoroutine(PlayDialogue(s, duration, target));
    }

    private IEnumerator PlayDialogue(string s, float duration, Combatent target)
    {
        Debug.Log("Playing Dialogue");
        SpeechBubbleDataContainer data = speechBubbles[target];

        data.Text.text = s;

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(data.CV, 1, dialogueBubbleFadeInRate));

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(data.CV, 0, dialogueBubbleFadeOutRate));
    }

    public void ReduceSpellCDsByPercent(float normalizedPercent)
    {
        // normaliedPercent is some number between 0 and 1
        // 0 = 0%, 1 = 100%
        // .14 = 14%
        // etc
        foreach (ReusableSpell activeSpell in Hand.GetEntriesMatching(spell => spell.SpellCastType == SpellCastType.Reusable))
        {
            if (activeSpell.OnCooldown)
            {
                activeSpell.MultiplyCooldown(normalizedPercent);
            }
        }
    }

    public void CloseCurrentlyDisplayedSpellPile()
    {
        closeCurrentlyDisplayedSpellPile = true;
    }

    public void ShowExhaustPile()
    {
        showSpellPileTitleText.text = "Exhaust Pile";
        StartCoroutine(ShowSpellPile(ExhaustPile, spell => true, Order.Unaltered));
    }

    public void ShowDrawPile()
    {
        showSpellPileTitleText.text = "Draw Pile";
        StartCoroutine(ShowSpellPile(DrawPile, spell => true, Order.Shuffled));
    }

    public void ShowDiscardPile()
    {
        showSpellPileTitleText.text = "Discard Pile";
        StartCoroutine(ShowSpellPile(DiscardPile, spell => true, Order.Unaltered));
    }

    private IEnumerator ShowSpellPile(Pile<Spell> toShow, Func<Spell, bool> viableSpell, Order order)
    {
        showSpellPileScreen.SetActive(true);

        List<Spell> showing = new List<Spell>();
        showing.AddRange(toShow.GetSpells());
        if (order == Order.Shuffled)
        {
            RandomHelper.Shuffle(showing);
        }
        else if (order == Order.Reversed)
        {
            showing.Reverse();
        }

        List<VisualSpellDisplay> spawnedDisplays = new List<VisualSpellDisplay>();

        // Spawn entries
        foreach (Spell spell in showing)
        {
            if (!viableSpell(spell))
            {
                continue;
            }

            VisualSpellDisplay spawned = Instantiate(visualSpellDisplayPrefab, spawnShowSpellPileDisplaysOn);
            spawned.SetSpell(spell);
            spawnedDisplays.Add(spawned);
        }

        yield return new WaitUntil(() => closeCurrentlyDisplayedSpellPile);
        closeCurrentlyDisplayedSpellPile = false;

        showSpellPileScreen.SetActive(false);

        // Closing
        while (spawnedDisplays.Count > 0)
        {
            VisualSpellDisplay cur = spawnedDisplays[0];
            spawnedDisplays.RemoveAt(0);
            Destroy(cur.gameObject);
        }
    }

    public void SetSpellPiles(Pile<Spell> spellbook)
    {
        DrawPile = new Pile<Spell>();
        PowerSpellPile = new Pile<Spell>();
        DiscardPile = new Pile<Spell>();
        Hand = new Pile<Spell>();
        ExhaustPile = new Pile<Spell>();

        foreach (Spell spell in spellbook.GetSpells())
        {
            DrawPile.Add(spell);
        }
        DrawPile.Shuffle();

        drawPileCountText.text = DrawPile.Count.ToString();
    }

    public void SetHandSize(int handSize)
    {
        this.handSize = handSize;
    }

    public IEnumerator DrawHand()
    {
        yield return StartCoroutine(DrawSpells(handSize));
    }

    private IEnumerator ForceDiscardSpells(int num = 1, Action<Spell> doToSelected = null)
    {
        for (int i = 0; i < num;)
        {
            // Empty hand
            if (Hand.Count == 0)
            {
                CallPlayDialogue("I have no Spells to Discard...", Combatent.Character);
                break;
            }

            Spell toDiscard = RandomHelper.GetRandomFromList(Hand.GetSpells());
            doToSelected?.Invoke(toDiscard);
            StartCoroutine(DiscardSpell(toDiscard));

            i++;

            yield return new WaitForSeconds(forceLoseSpellDelay);
        }
    }

    private IEnumerator ForceExhaustSpells(int num = 1, Action<Spell> doToSelected = null)
    {
        for (int i = 0; i < num;)
        {
            // Empty hand
            if (Hand.Count == 0)
            {
                CallPlayDialogue("I have no Spells to Exhaust...", Combatent.Character);
                break;
            }

            Spell toDiscard = RandomHelper.GetRandomFromList(Hand.GetSpells());
            doToSelected?.Invoke(toDiscard);
            StartCoroutine(ExhaustSpell(toDiscard));

            i++;

            yield return new WaitForSeconds(forceLoseSpellDelay);
        }
    }

    private IEnumerator DrawSpells(int num = 1)
    {
        for (int i = 0; i < num;)
        {
            if (Hand.Count >= MaxHandSize)
            {
                CallPlayDialogue("I have no room in my Hand...", Combatent.Character);
                break;
            }

            // There are absolutely no cards to draw remaining
            if (DrawPile.Count == 0 && DiscardPile.Count == 0)
            {
                CallPlayDialogue("I'm out of Spells...?", Combatent.Character);
                break;
            }

            // Reshuffle discard pile into draw pile if needed
            yield return StartCoroutine(CheckForReshuffleDiscardIntoDraw());

            yield return StartCoroutine(DrawSpell());
            i++;

            yield return new WaitForSeconds(drawSpellDelay);
        }
    }

    private IEnumerator CheckForReshuffleDiscardIntoDraw()
    {
        if (DrawPile.Count == 0)
        {
            DiscardPile.TransferEntries(DrawPile, true);

            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator DiscardCardAnimation(SpellDisplay card)
    {
        RectTransform t = (RectTransform)card.transform;
        card.SetScaleLocked(true);

        t.SetParent(UIManager._Instance.Canvas, true);

        StartCoroutine(Utils.LerpScale(t, Vector3.one * discardedCardMinScale, discardedCardChangeScaleRate, discardedCardScaleGraceRange));

        Tween posTween = t.DOAnchorPos(new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + discardCardDownAmount), discardSlideDuration);
        yield return new WaitUntil(() => !posTween.active);
    }

    private IEnumerator ExhaustCardAnimation(SpellDisplay card)
    {
        RectTransform t = (RectTransform)card.transform;
        card.SetScaleLocked(true);

        t.SetParent(UIManager._Instance.Canvas, true);

        Tween scaleTween = t.DOScale(0, exhaustSlideDuration / 2);
        Tween posTween = t.DOAnchorPos(new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + exhaustCardUpAmount), exhaustSlideDuration);
        Tween alphaTween = card.GetCanvasGroup().DOFade(0, exhaustSlideDuration);

        yield return new WaitUntil(() => !alphaTween.active);

        t.DOKill();
        card.DOKill();
    }

    private IEnumerator PowerSpellAnimation(SpellDisplay card)
    {
        RectTransform t = (RectTransform)card.transform;
        card.SetScaleLocked(true);

        t.SetParent(UIManager._Instance.Canvas, true);

        Tween scaleTween = t.DOScale(Vector3.zero, powerSpellShrinkDuration);
        yield return new WaitUntil(() => !scaleTween.active);
    }

    public IEnumerator HandleSpellCast(Spell spell)
    {
        if (spell.SpellCastType == SpellCastType.Power)
        {
            // Automatically remove Power Spells from Hand when Played
            yield return StartCoroutine(AddSpellToPowerSpellPile((PowerSpell)spell));
        }
        else
        {
            switch (spell.QueueDeckAction)
            {
                case SpellQueueDeckAction.Discard:
                    yield return StartCoroutine(DiscardSpell(spell));
                    break;
                case SpellQueueDeckAction.Exhaust:
                    yield return StartCoroutine(ExhaustSpell(spell));
                    break;
                case SpellQueueDeckAction.None:
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }
    }

    private IEnumerator HandleHandAtEndOfTurn()
    {
        if (GameManager._Instance.HasArtifact(ArtifactLabel.AccursedBrand))
        {
            if (NumSpellsInDrawablePlaces > AccursedBrand.MinCards && Hand.Count > 0)
            {
                yield return StartCoroutine(ExhaustSpellSequence(1, null, null));
            }
        }

        List<Spell> toHandle = Hand.GetEntriesMatching(spell => true);
        foreach (Spell spell in toHandle)
        {
            // if at the end of the turn, there are playable curses in hand, play them
            if (spell.Color == SpellColor.Curse)
            {
                if (spell is ReusableSpell)
                {
                    if (!((ReusableSpell)spell).OnCooldown)
                    {
                        yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Character, Combatent.Enemy, false, false));
                    }
                }
                else
                {
                    yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Character, Combatent.Enemy, false, false));
                }
            }

            // Handle End of Turn Deck Action for each Spell
            switch (spell.EndOfTurnDeckAction)
            {
                case SpellEndOfTurnDeckAction.Ethereal:
                    // Exhaust Reusable Spells if they are not on Cooldown and Exhaust all Spells otherwise
                    if (spell is ReusableSpell)
                    {
                        if (!((ReusableSpell)spell).OnCooldown)
                        {
                            yield return StartCoroutine(ExhaustSpell(spell));
                        }
                        else
                        {
                            StartCoroutine(DiscardSpell(spell, false));
                        }
                    }
                    else
                    {
                        yield return StartCoroutine(ExhaustSpell(spell));
                    }
                    break;
                case SpellEndOfTurnDeckAction.Retain:
                    break;
                case SpellEndOfTurnDeckAction.Discard:
                    StartCoroutine(DiscardSpell(spell, false));
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }

            yield return new WaitForSeconds(discardSpellDelay);
        }
    }

    public IEnumerator DrawSpell(Action<Spell> doToDrawnSpell = null)
    {
        yield return StartCoroutine(CheckForReshuffleDiscardIntoDraw());
        if (DrawPile.Count == 0)
        {
            CallPlayDialogue("No more Spells...?", Combatent.Character);
            yield break;
        }

        Spell spell = DrawPile.DrawTop();
        Hand.Add(spell);

        // Spawn Card
        VisualSpellDisplay spawned = Instantiate(cardDisplayPrefab, null);
        handLayoutGroup.AddChild(spawned.transform);
        spawned.SetSpell(spell);
        spawned.SetSpellDisplayState(SpellDisplayState.InHand);

        // Callback
        yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnDraw));

        // Callback
        OnDrawSpell?.Invoke();

        // Callback
        CombatentSpellCallbackMap[Combatent.Character][CombatSpellCallbackType.OnDraw]?.Invoke(spell);

        if (doToDrawnSpell != null)
            doToDrawnSpell(spell);

        RecalculateKeyBinds();
    }

    public IEnumerator DiscardSpell(Spell spell, bool recalculateKeybinds = true)
    {
        if (Hand.Contains(spell))
        {
            Hand.Remove(spell);
            if (recalculateKeybinds)
            {
                RecalculateKeyBinds();
            }
            DiscardPile.Add(spell);

            SpellDisplay spellDisplay = spell.GetEquippedTo();
            handLayoutGroup.RemoveTransformFromHand(spellDisplay.transform);

            yield return StartCoroutine(DiscardCardAnimation(spellDisplay));

            if (spellDisplay == null)
            {
                Debug.Log("Discard Spell Error: Paused Time");
                Time.timeScale = 0;
            }

            Destroy(spellDisplay.gameObject);

            // Callback
            yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnAnyDiscard));

            if (spell is ReusableSpell)
            {
                ((ReusableSpell)spell).ResetCooldown();
            }
        }
    }

    public IEnumerator ExhaustSpell(Spell spell)
    {
        if (Hand.Contains(spell))
        {
            Hand.Remove(spell);
            RecalculateKeyBinds();
            ExhaustPile.Add(spell);

            SpellDisplay spellDisplay = spell.GetEquippedTo();

            handLayoutGroup.RemoveTransformFromHand(spellDisplay.transform);

            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Fading);

            yield return StartCoroutine(ExhaustCardAnimation(spellDisplay));

            Destroy(spellDisplay.gameObject);

            // Callback
            yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnExhaust));

            // Callback
            OnExhaustSpell?.Invoke();

            // Callback
            CombatentSpellCallbackMap[Combatent.Character][CombatSpellCallbackType.OnExhaust]?.Invoke(spell);

            if (spell is ReusableSpell)
            {
                ((ReusableSpell)spell).ResetCooldown();
            }
        }
    }

    public IEnumerator AddSpellToPowerSpellPile(PowerSpell spell)
    {
        if (Hand.Contains(spell))
        {
            Hand.Remove(spell);
            RecalculateKeyBinds();
            PowerSpellPile.Add(spell);

            handLayoutGroup.RemoveTransformFromHand(spell.GetEquippedTo().transform);

            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Fading);

            yield return StartCoroutine(PowerSpellAnimation(spell.GetEquippedTo()));

            Destroy(spell.GetEquippedTo().gameObject);
        }
    }

    [ContextMenu("Test/ExhaustSequence")]
    public void TestExhaustSpellSequence()
    {
        CallExhaustSpellSequence(1, null, null);
    }

    [ContextMenu("Test/DiscardSequence")]
    public void TestDiscardSpellSequence()
    {
        CallDiscardSpellSequence(1, null, null);
    }

    public void CallExhaustSpellSequence(int numToExhaust, Action<Spell> doToSpell, Action onComplete)
    {
        StartCoroutine(ExhaustSpellSequence(numToExhaust, doToSpell, onComplete));
    }

    public void CallDiscardSpellSequence(int numToDiscard, Action<Spell> doToSpell, Action onComplete)
    {
        StartCoroutine(DiscardSpellSequence(numToDiscard, doToSpell, onComplete));
    }

    public void ClickedSpellForAlterHandSequence(Spell spell)
    {
        if (alterHandSequenceSelectedSpells.Contains(spell))
        {
            alterHandSequenceSelectedSpells.Remove(spell);
            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.InHand);
        }
        else
        {
            alterHandSequenceSelectedSpells.Add(spell);
            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Selected);
        }
    }

    public void ReduceActiveSpellCooldowns(int reduceBy)
    {
        foreach (ReusableSpell spell in Hand.GetEntriesMatching(spell => spell.SpellCastType == SpellCastType.Reusable))
        {
            if (spell.OnCooldown)
            {
                spell.AlterCooldown(-reduceBy);
            }
        }
    }

    public void ResetActiveSpellCooldowns()
    {
        foreach (ReusableSpell spell in Hand.GetEntriesMatching(spell => spell.SpellCastType == SpellCastType.Reusable))
        {
            if (spell.OnCooldown)
            {
                spell.ResetCooldown();
            }
        }
    }

    public bool AwaitingAlterHandSequenceSelections = false;

    private IEnumerator AlterHandSequence(int numToAlter, AlterHandSequenceType type, Action<Spell> doToSpell, Action onComplete)
    {
        if (Hand.Count < numToAlter)
        {
            Hand.ActOnEachSpellInPile(spell => alterHandSequenceSelectedSpells.Add(spell));
        }
        else
        {
            alterHandInstructionText.gameObject.SetActive(true);
            alterHandInstructionContainer.SetActive(true);
            alterHandBackground.SetActive(true);

            AwaitingAlterHandSequenceSelections = true;

            while (alterHandSequenceSelectedSpells.Count < numToAlter)
            {
                int numToGo = (numToAlter - alterHandSequenceSelectedSpells.Count);
                alterHandInstructionText.text = "Select " + numToGo + " Spell" + (numToGo > 1 ? "s" : "") + " to " + type.ToString();
                yield return null;
            }

            AwaitingAlterHandSequenceSelections = false;
        }

        // Set Instruction Text inactive
        alterHandInstructionContainer.gameObject.SetActive(false);
        alterHandBackground.SetActive(false);
        alterHandInstructionText.gameObject.SetActive(false);

        while (alterHandSequenceSelectedSpells.Count > 0)
        {
            Spell spell = alterHandSequenceSelectedSpells[0];
            alterHandSequenceSelectedSpells.RemoveAt(0);

            doToSpell?.Invoke(spell);

            if (type == AlterHandSequenceType.Discard)
            {
                // "Callback"
                yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnSpecificDiscard));

                // Callback
                OnSpecificDiscardSpell?.Invoke();

                // Callback
                CombatentSpellCallbackMap[Combatent.Character][CombatSpellCallbackType.OnSpecificDiscard]?.Invoke(spell);

                StartCoroutine(DiscardSpell(spell));
            }
            if (type == AlterHandSequenceType.Exhaust)
            {
                StartCoroutine(ExhaustSpell(spell));
            }

            yield return new WaitForSeconds(delayBetweenAlterHandCalls);
        }

        // Reset
        RecalculateKeyBinds();

        onComplete?.Invoke();
    }

    private void RecalculateKeyBinds()
    {
        List<Spell> inHand = Hand.GetSpells();
        for (int i = 0; i < inHand.Count; i++)
        {
            // Recalculate Key Bindings
            VisualSpellDisplay spellDisplay = ((VisualSpellDisplay)inHand[i].GetEquippedTo());
            if (spellDisplay != null)
            {
                spellDisplay.SetKeyBinding(GetKeyBindingAtIndex(i));
            }
        }
    }

    public IEnumerator ExhaustSpellSequence(int numToExhaust, Action<Spell> doToSpell, Action onComplete)
    {
        yield return StartCoroutine(AlterHandSequence(numToExhaust, AlterHandSequenceType.Exhaust, doToSpell, onComplete));
    }

    public IEnumerator DiscardSpellSequence(int numToDiscard, Action<Spell> doToSpell, Action onComplete)
    {
        yield return StartCoroutine(AlterHandSequence(numToDiscard, AlterHandSequenceType.Discard, doToSpell, onComplete));
    }

    private void Awake()
    {
        _Instance = this;

        // Create Circle Pool 
        CreateCirclePool();

        // Setup Callback Maps
        SetupCombatCallbackMaps();
    }

    #region Combat Loop


    public IEnumerator StartCombat(Combat combat, Action onEndAction, bool allowSpellSlection = true)
    {
        Debug.Log("Combat Started: " + combat);

        foreach (GameObject obj in showOnCombat)
        {
            obj.SetActive(false);
        }

        // Set Current Variables
        currentEnemy = combat.SpawnedEnemy;
        maxEnemyHP = currentEnemy.GetMaxHP();
        enemyCombatSprite.sprite = currentEnemy.GetCombatSprite();

        foreach (GameObject obj in showOnCombat)
        {
            obj.SetActive(true);
        }

        SetSpellPiles(GameManager._Instance.Spellbook);

        // Set Up Combat
        combatScreenOpen = true;

        // Enemy Stuff
        // Reset enemy sprite CV from last Combat Dying
        enemyCombatSpriteCV.alpha = 1;

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
        currentSpellEffectivenessMultiplier = defaultEffectivenessMultiplier;

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

            StartCoroutine(GameManager._Instance.GameOverSequence());

            // Permanantly Stall out Here Until Player Restarts
            yield return new WaitUntil(() => false);
        }
        else
        {
            // Enemy is Dead
            currentEnemy.OnDeath();

            // Play Enemy Death Animation
            StartCoroutine(Utils.ChangeCanvasGroupAlpha(enemyCombatSpriteCV, 0, Time.deltaTime * combatSpriteAlphaChangeRate));

            // Wait until Enemy HP bar is Empty
            yield return new WaitUntil(() => enemyHPBar.Empty);

            yield return new WaitForSeconds(delayAfterEnemyDeath);

            // Bandaged Effect
            if (TargetHasAffliction(AfflictionType.Bandages, Combatent.Character))
            {
                int numBandagedStacks = characterAfflictionMap[AfflictionType.Bandages].GetStacks();
                GameManager._Instance.AlterPlayerCurrentHP(numBandagedStacks, DamageType.Heal);
                ConsumeAfflictionStack(AfflictionType.Bandages, Combatent.Character, numBandagedStacks);
                ShowAfflictionProc(AfflictionType.Bandages, Combatent.Character);
                yield return new WaitForSeconds(delayAfterBandagesEffect);
            }
        }

        InCombat = false;

        Debug.Log("Combat Completed: " + combat);

        // Reset
        ResetCombat();

        combatScreenOpen = false;

        GameManager._Instance.ResolveCurrentEvent();

        onEndAction?.Invoke();
    }

    private IEnumerator CombatLoop()
    {
        // Allow player to cast spells
        CanCastSpells = true;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Combat Start", ""));

        // Set settings
        InCombat = true;

        // Call OnCombatStart
        OnCombatStart?.Invoke();

        // Allow Enemy to Act on OnCombatStart Actions
        foreach (EnemyAction action in currentEnemy.GetOnCombatStartActions())
        {
            foreach (Spell spell in action.GetActionSpells())
            {
                yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Enemy, Combatent.Character, false, true));
            }
        }

        StartCoroutine(UpdateDuringCombat());

        // Call On Combat Start Spell Effects right Away
        yield return StartCoroutine(CallSpellEffects(currentEnemy.GetOnCombatStartSpellEffects(), null, Combatent.Enemy, Combatent.Character, false));

        while (currentEnemyHP > 0 && GameManager._Instance.GetCurrentCharacterHP() > 0)
        {
            // Turn Begin
            // Set effectiveness multiplier text to be at position zero
            effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

            // Increment Turn Count
            turnNumber++;

            // Call OnTurnStart
            OnTurnStart?.Invoke();

            // Queue all Spells of Given Action
            currentEnemyAction = currentEnemy.GetViableEnemyAction();
            foreach (Spell spell in currentEnemyAction.GetActionSpells())
            {
                yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Enemy, Combatent.Character, false, true));
            }

            yield return StartCoroutine(DrawHand());

            // Player Turn
            yield return StartCoroutine(PlayerTurn());

            // Reset Effectiveness Multiplier
            currentSpellEffectivenessMultiplier = defaultEffectivenessMultiplier;

            if (CheckForCombatOver())
            {
                break;
            }

            // Enemy Turn
            yield return StartCoroutine(EnemyTurn());

            // Reset Effectiveness Multiplier
            currentSpellEffectivenessMultiplier = defaultEffectivenessMultiplier;

            if (CheckForCombatOver())
            {
                break;
            }

            // End of Turn
            yield return new WaitForSeconds(delayAfterEnemyTurn);

            // Reset for Turn
            foreach (GameObject obj in disableWhileCasting)
            {
                obj.SetActive(true);
            }

            GameManager._Instance.AlterPlayerCurrentMana(GameManager._Instance.GetManaPerTurn());
        }

        // Call On Combat End
        OnCombatEnd?.Invoke();
    }

    private IEnumerator PlayerTurn()
    {
        //E Debug.Log("Player Turn Started");

        currentTurn = Turn.Player;
        playerTurnEnded = false;

        // Reset Ward
        ResetCombatentWard(Combatent.Character);
        spellCastThisTurn.Clear();

        // Allow player to cast spells
        CanCastSpells = true;
        hasCastQueue = false;

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Player Turn", turnNumber > 1 ? turnNumber + Utils.GetNumericalSuffix(turnNumber) + " Turn" : ""));

        // Callback
        CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnStart]?.Invoke();

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitUntil(() => playerTurnEnded);
        playerTurnEnded = false;

        yield return StartCoroutine(HandleHandAtEndOfTurn());

        foreach (GameObject obj in disableWhileCasting)
        {
            obj.SetActive(false);
        }

        CanCastSpells = false;
        yield return StartCoroutine(CastSpellQueue(playerCastQueue, Combatent.Character, Combatent.Enemy));

        if (CheckForCombatOver())
        {
            yield break;
        }

        ResetActiveSpellCooldowns();

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Combatent.Character))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Combatent.Character), Combatent.Character, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Combatent.Character);
        }

        // Callback
        CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnTurnEnd]?.Invoke();

        // Callback
        foreach (Spell spell in spellCastThisTurn)
        {
            yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnPlayerTurnEnd));
        }

        Debug.Log("Player Turn Ended");
    }

    public void TickHandCooldowns(Spell exclude)
    {
        // Tick other active spell cooldowns
        foreach (ReusableSpell activeSpell in Hand.GetEntriesMatching(spell => spell.SpellCastType == SpellCastType.Reusable))
        {
            if (activeSpell == exclude) continue;
            if (!activeSpell.OnCooldown) continue;
            activeSpell.AlterCooldown(-1);
        }
    }

    public void TickRandomQueuedSpell(List<QueuedSpell> castQueue)
    {
        List<QueuedSpellDisplay> options = new List<QueuedSpellDisplay>();

        foreach (QueuedSpell spell in castQueue)
        {
            if (spell.Spell.SpellCastType == SpellCastType.Power) continue;
            if (spell.SpellQueueDisplay.GetPrepTime() > 1)
            {
                options.Add(spell.SpellQueueDisplay);
            }
        }

        if (options.Count > 0)
        {
            // spell.SpellQueueDisplay.SetPrepTime(currentPrepTime - 1);
            QueuedSpellDisplay selected = RandomHelper.GetRandomFromList(options);
            selected.AlterPrepTime(-1);
        }
    }

    public IEnumerator AddSpellToCastQueue(Spell spell, Combatent adder, Combatent other, bool useMana, bool canEcho, Action<Spell> actOnSpell = null)
    {
        // Echo Effect
        if (TargetHasAffliction(AfflictionType.Echo, adder) && canEcho)
        {
            ConsumeAfflictionStack(AfflictionType.Echo, adder);
            ShowAfflictionProc(AfflictionType.Echo, adder);
            yield return StartCoroutine(AddSpellToCastQueue(spell, adder, other, useMana, false));
        }

        // Overwealming Blaze Effect
        if (TargetHasAffliction(AfflictionType.OverwealmingBlaze, adder) && spell.DoesApplyAfflictionOfType(AfflictionType.Burn) && canEcho)
        {
            yield return StartCoroutine(AddSpellToCastQueue(spell, adder, other, false, false));
        }

        // Spawn new display
        SemicircleLayoutGroup semicircleDisplay = GetQueuedSpellDisplayForTarget(adder);
        QueuedSpellDisplay queuedSpellDisplay = Instantiate(castingSpellPotencyDisplayPrefab, semicircleDisplay.transform);
        semicircleDisplay.SetChildOrder(queuedSpellDisplay.transform);

        queuedSpellDisplay.SetSpell(spell);
        QueuedSpell toQueue = new QueuedSpell(spell, queuedSpellDisplay);

        // Callback
        CombatentBaseCallbackMap[adder][CombatBaseCallbackType.OnSpellQueued]?.Invoke();

        // Stuff to do when specifically the player is adding
        if (adder == Combatent.Character)
        {
            // Check for Free Spell Casts
            if (NumFreeSpells > 0)
            {
                NumFreeSpells -= 1;
            }
            else
            {
                // Set Cooldown
                if (spell.SpellCastType == SpellCastType.Reusable)
                {
                    // Set Cooldown
                    ((ReusableSpell)spell).SetOnCooldown();
                }

                // Consume Mana
                if (useMana)
                {
                    GameManager._Instance.AlterPlayerCurrentMana(-spell.ManaCost);
                }
            }
        }

        switch (adder)
        {
            case Combatent.Character:
                playerCastQueue.Add(toQueue);
                break;
            case Combatent.Enemy:
                enemyCastQueue.Add(toQueue);
                break;
            default: throw new UnhandledSwitchCaseException();
        }

        // Callback
        yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnQueue));

        if (actOnSpell != null)
            actOnSpell(spell);
    }

    private SemicircleLayoutGroup GetQueuedSpellDisplayForTarget(Combatent t)
    {
        switch (t)
        {
            case Combatent.Character:
                return playerQueuedSpellsDisplay;
            case Combatent.Enemy:
                return enemyQueuedSpellsDisplay;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private IEnumerator CastSpellQueue(List<QueuedSpell> castQueue, Combatent caster, Combatent other)
    {
        // Cast Queue
        IsCastingQueue = true;

        // Set effectiveness multiplier text to be active
        effectivenessMultiplierText.gameObject.SetActive(true);

        for (int i = 0; i < castQueue.Count;)
        {
            // Get Spell from Queue
            QueuedSpell spell = castQueue[i];
            spell.SpellQueueDisplay.AlterPrepTime(-1);

            if (spell.SpellQueueDisplay.GetPrepTime() > 0)
            {
                i++;
                yield return new WaitForSeconds(delayBetweenSpellPrepTimeTicks);
                continue;
            }

            // Can proceed to Cast
            spell.SpellQueueDisplay.SetAllowScale(true);

            // Remove Spell from Queue
            castQueue.RemoveAt(i);

            // Cast Spell
            yield return StartCoroutine(CastSpell(spell, caster, other));

            // Remove UI from spell queue & from Spell Potency Display
            Destroy(spell.SpellQueueDisplay.gameObject);

            // Killed the enemy or died themselves, either way remove the rest of the queued spells
            if (CheckForCombatOver())
            {
                ClearCastQueue(playerCastQueue);

                yield break;
            }

            yield return new WaitForSeconds(delayBetweenSpellCasts);
        }

        // Set effectiveness multiplier text to be inactive
        effectivenessMultiplierText.gameObject.SetActive(false);

        IsCastingQueue = false;
        hasCastQueue = true;
    }


    private IEnumerator CastSpell(QueuedSpell queuedSpell, Combatent caster, Combatent other)
    {
        Spell spell = queuedSpell.Spell;
        spellCastThisTurn.Add(spell);

        // Set SFX source to spell audio clip
        spellSFXSource.clip = spell.AssociatedSoundClip;
        spellSFXSource.Play();

        // Set hit & miss sounds
        hitSound = spell.HitSound;
        missSound = spell.MissSound;

        // Set effectiveness multiplier text to be at zero
        effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;


        // Play Sequence
        yield return StartCoroutine(PlaySpell(spell, queuedSpell.SpellQueueDisplay, caster, other));

    }

    private IEnumerator EnemyTurn()
    {
        // Debug.Log("Enemy Turn Started");

        currentTurn = Turn.Enemy;

        // Reset Ward
        ResetCombatentWard(Combatent.Enemy);

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Enemy Turn", ""));

        // Callback
        CombatentBaseCallbackMap[Combatent.Enemy][CombatBaseCallbackType.OnTurnStart]?.Invoke();

        // Allow Enemy to Act on OnTurnStart Actions
        foreach (EnemyAction action in currentEnemy.GetOnTurnStartActions())
        {
            foreach (Spell spell in action.GetActionSpells())
            {
                yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Enemy, Combatent.Character, false, true));
            }
        }

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeEnemyAttack);
        currentEnemyAction.CallOnActivate();

        yield return StartCoroutine(CastSpellQueue(enemyCastQueue, Combatent.Enemy, Combatent.Character));

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Combatent.Enemy))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Combatent.Enemy), Combatent.Enemy, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Combatent.Enemy);
        }

        // Allow Enemy to Act on OnTurnEnd Actions
        foreach (EnemyAction action in currentEnemy.GetOnTurnEndActions())
        {
            foreach (Spell spell in action.GetActionSpells())
            {
                yield return StartCoroutine(AddSpellToCastQueue(spell, Combatent.Enemy, Combatent.Character, false, true));
            }
        }

        // Callback
        CombatentBaseCallbackMap[Combatent.Enemy][CombatBaseCallbackType.OnTurnEnd]?.Invoke();

        Debug.Log("Enemy Turn Ended");
    }

    private IEnumerator UpdateDuringCombat()
    {
        while (InCombat)
        {
            // Set effectiveness multiplier text
            effectivenessMultiplierText.text = "x" + Utils.RoundTo(currentSpellEffectivenessMultiplier, 2).ToString();

            // Set effectiveness multiplier text scale
            effectivenessMultiplierTextRect.localScale = Vector3.Lerp(
                effectivenessMultiplierTextRect.localScale,
                Vector3.one * MathHelper.Normalize(currentSpellEffectivenessMultiplier, 0, maxSpellEffectivenessMultiplier,
                effectivenessMultiplierTextMinScale, effectivenessMultiplierTextMaxScale),
                Time.deltaTime * animateEffectivenessTextRectScaleSpeed);

            // Set mana Texts
            nextTurnManaChangeText.text = "+" + GameManager._Instance.GetManaPerTurn();

            drawPileCountText.text = DrawPile.Count.ToString();
            discardPileCountText.text = DiscardPile.Count.ToString();
            exhaustPileCountText.text = ExhaustPile.Count.ToString();

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
                else if (IsCastingQueue)
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

    private void ResetCombat()
    {
        // Reset Turn Count
        turnNumber = 0;
        // Reset Num Free Spells
        NumFreeSpells = 0;

        IsCastingQueue = false;
        hasCastQueue = false;

        // Clear Afflictions
        ClearAfflictionMap(enemyAfflictionMap, Combatent.Enemy);
        ClearAfflictionMap(characterAfflictionMap, Combatent.Character);

        ResetCombatentWard(Combatent.Character);
        ResetCombatentWard(Combatent.Enemy);

        // Clear active spell cooldowns
        ResetActiveSpellCooldowns();
        foreach (GameObject obj in disableWhileCasting)
        {
            obj.SetActive(true);
        }

        // Reset Player Mana
        GameManager._Instance.SetCurrentPlayerMana(GameManager._Instance.GetMaxPlayerMana());

        // Reset HP Bars
        characterHPBar.Clear();
        enemyHPBar.Clear();

        // Reset
        currentSpellEffectivenessMultiplier = defaultEffectivenessMultiplier;
        effectivenessMultiplierText.text = "x" + Utils.RoundTo(currentSpellEffectivenessMultiplier, 2).ToString();

        // Reset Spells
        Hand.TransferEntries(DiscardPile, false);
        DiscardPile.TransferEntries(ExhaustPile, false);
        ExhaustPile.TransferEntries(PowerSpellPile, false);
        PowerSpellPile.TransferEntries(DrawPile, false);
        DrawPile.ActOnEachSpellInPile(spell => StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnCombatReset)));
        DrawPile.Clear();

        musicSource.Stop();
        musicSource.time = 0;

        // Clear Spell Queue
        ClearCastQueue(playerCastQueue);
        ClearCastQueue(enemyCastQueue);

        // Destroy Circles
        ClearCircleList();

        // Set effectiveness multiplier text to be at zero
        effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

        // Callback
        OnResetCombat?.Invoke();
    }

    private void ClearCastQueue(List<QueuedSpell> castQueue)
    {
        while (castQueue.Count > 0)
        {
            QueuedSpell spell = castQueue[0];
            Destroy(spell.SpellQueueDisplay.gameObject);
            castQueue.RemoveAt(0);
        }
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

    // Used for Jumpy (Affliction) - Scares me a bit due to not being a Coroutine
    public void RandomizeSpell(Combatent combatent)
    {
        switch (combatent)
        {
            case Combatent.Character:

                if (playerCastQueue.Count > 0 && DrawPile.Count > 0)
                {
                    // Get Random Spell
                    QueuedSpell replacingSpell = RandomHelper.GetRandomFromList(playerCastQueue);

                    playerCastQueue.Remove(replacingSpell);
                    Destroy(replacingSpell.SpellQueueDisplay.gameObject);

                    Spell newSpell = RandomHelper.GetRandomFromList(DrawPile.GetSpells());
                    DrawPile.Remove(newSpell);
                    DiscardPile.Add(newSpell);

                    StartCoroutine(AddSpellToCastQueue(newSpell, Combatent.Character, Combatent.Enemy, false, false));
                }

                break;
            case Combatent.Enemy:

                if (enemyCastQueue.Count > 0)
                {
                    // Get Random Spell
                    QueuedSpell replacingSpell = RandomHelper.GetRandomFromList(enemyCastQueue);

                    enemyCastQueue.Remove(replacingSpell);
                    Destroy(replacingSpell.SpellQueueDisplay.gameObject);

                    Spell newSpell = RandomHelper.GetRandomFromList(CurrentEnemy.GetAnyEnemyAction().GetActionSpells());

                    StartCoroutine(AddSpellToCastQueue(newSpell, Combatent.Character, Combatent.Enemy, false, false));
                }

                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    #endregion

    #region Spell Gameplay

    private IEnumerator PlaySpell(Spell spell, QueuedSpellDisplay spellPotencyDisplay, Combatent caster, Combatent other)
    {
        float t = 0;

        // Foreach Batch in the Spell
        for (int i = 0; i < spell.Batches.Count; i++)
        {
            // Get Current Batch
            SpellNoteBatch currentBatch = spell.Batches[i];

            // Spawn Batch of Circles
            for (int p = 0; p < currentBatch.NumNotes; p++)
            {
                SpellNote currentNote = currentBatch.GetNote(p);
                Circle c = circlePool.Get();
                spawnedCircles.Add(c);
                c.Set(currentNote, UIManager._Instance.GetDamageTypeColor(spell.MainDamageType));

                t = 0;
                while (t < currentNote.DelayAfter)
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

                        Destroy(spellPotencyDisplay.gameObject);

                        yield break;
                    }
                    yield return null;
                }
            }

            // Depending on how many are hit, spell power is increased
            t = 0;
            while (t < currentBatch.DelayAfterBatch)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(delayAfterNoteSequence);

        // Callback
        yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnCast));

        // Callback
        CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnSpellCast]?.Invoke();

        // Callback
        if (CheckForCombatOver() && currentEnemyHP <= 0)
        {
            yield return StartCoroutine(spell.CallSpellCallback(SpellCallbackType.OnKill));
        }
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

        // Change effectiveness Multiplier
        switch (currentTurn)
        {
            case Turn.Enemy:
                if (currentSpellEffectivenessMultiplier - increaseEffectivenessMultiplierOnHit < 0)
                {
                    currentSpellEffectivenessMultiplier = 0;
                }
                else
                {
                    currentSpellEffectivenessMultiplier -= increaseEffectivenessMultiplierOnHit;
                }
                break;
            case Turn.Player:
                if (currentSpellEffectivenessMultiplier + increaseEffectivenessMultiplierOnHit > maxSpellEffectivenessMultiplier)
                {
                    currentSpellEffectivenessMultiplier = maxSpellEffectivenessMultiplier;
                }
                else
                {
                    currentSpellEffectivenessMultiplier += increaseEffectivenessMultiplierOnHit;
                }
                break;
            default:
                throw new UnhandledSwitchCaseException();
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

        // Change effectiveness Multiplier
        switch (currentTurn)
        {
            case Turn.Enemy:
                if (currentSpellEffectivenessMultiplier + decreaseEffectivenessMultiplierOnMiss > maxSpellEffectivenessMultiplier)
                {
                    currentSpellEffectivenessMultiplier = maxSpellEffectivenessMultiplier;
                }
                else
                {
                    currentSpellEffectivenessMultiplier += decreaseEffectivenessMultiplierOnMiss;
                }
                break;
            case Turn.Player:
                if (currentSpellEffectivenessMultiplier - decreaseEffectivenessMultiplierOnMiss < 0)
                {
                    currentSpellEffectivenessMultiplier = 0;
                }
                else
                {
                    currentSpellEffectivenessMultiplier -= decreaseEffectivenessMultiplierOnMiss;
                }
                break;
            default:
                throw new UnhandledSwitchCaseException();
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

    public int GetPlayerBasicAttackDamage()
    {
        int preCorrectedPlayerBasicAttackDamage = GameManager._Instance.DamageFromEquipment + GameManager._Instance.GetBasicAttackDamage();
        if (preCorrectedPlayerBasicAttackDamage <= 0)
        {
            return 1;
        }
        return preCorrectedPlayerBasicAttackDamage;
    }

    // Basic Attack
    private void PlayerBasicAttack()
    {
        // the minimum a basic attack can do is 1
        // Attack the enemy
        AttackCombatent(GetPlayerBasicAttackDamage(), Combatent.Enemy, Combatent.Character, DamageType.Physical, DamageSource.BasicAttack);

        // Only call this if the combat isn't over
        if (!CheckForCombatOver())
        {
            // Callback
            CombatentBaseCallbackMap[Combatent.Character][CombatBaseCallbackType.OnBasicAttack]?.Invoke();
        }
    }

    private IEnumerator AnimateSpriteAttack(Combatent attacker)
    {
        RectTransform attackerSpriteRect = GetTargetSpriteImage(attacker).transform as RectTransform;
        Vector2 originalPos = attackerSpriteRect.anchoredPosition;
        Vector2 newPos;
        switch (attacker)
        {
            case Combatent.Character:
                newPos = originalPos + new Vector2(spriteOffset, 0);
                break;
            case Combatent.Enemy:
                newPos = originalPos - new Vector2(spriteOffset, 0);
                break;
            default: throw new Exception();
        }

        float animationSpeedMultiplier = combatSpriteAnimationMultiplier;
        while (attackerSpriteRect.anchoredPosition.x > newPos.x)
        {
            attackerSpriteRect.anchoredPosition = Vector2.MoveTowards(attackerSpriteRect.anchoredPosition, newPos, Time.deltaTime * baseEnemyCombatSpriteAnimationSpeed * animationSpeedMultiplier);
            animationSpeedMultiplier += Time.deltaTime * enemyCombatSpriteAnimationSpeedMultiplierGain;
            yield return null;
        }

        while (attackerSpriteRect.anchoredPosition.x < originalPos.x)
        {
            attackerSpriteRect.anchoredPosition = Vector2.MoveTowards(attackerSpriteRect.anchoredPosition, originalPos, Time.deltaTime * baseEnemyCombatSpriteAnimationSpeed * animationSpeedMultiplier);
            yield return null;
        }
    }

    public int GetEnemyBasicAttackDamage()
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

        // the minimum a basic attack can do is manually set to 1
        if (attackDamage <= 0)
        {
            attackDamage = 1;
        }
        return attackDamage;
    }

    private void EnemyBasicAttack()
    {
        AttackCombatent(GetEnemyBasicAttackDamage(), Combatent.Character, Combatent.Enemy, DamageType.Physical, DamageSource.BasicAttack);

        // Only call on enemy attack if the player is still alive
        if (!CheckForCombatOver())
        {
            // Callback
            CombatentBaseCallbackMap[Combatent.Enemy][CombatBaseCallbackType.OnBasicAttack]?.Invoke();
        }
    }

    private void HandleSpellSingleAttackEffect(SpellSingleAttackEffect singleAttack, Combatent target, Combatent caster)
    {
        if (singleAttack.Target == Target.Other || singleAttack.Target == Target.Both)
        {
            AttackCombatent(singleAttack.DamageAmount, target, caster, singleAttack.DamageType, DamageSource.Spell);
        }
        if (singleAttack.Target == Target.Self || singleAttack.Target == Target.Both)
        {
            AttackCombatent(singleAttack.DamageAmount, caster, caster, singleAttack.DamageType, DamageSource.Spell);
        }
    }

    private void HandleSpellLeechingAttackEffect(SpellLeechingAttackEffect leechingAttack, Combatent target, Combatent caster)
    {
        int damageDealt = 0;
        if (leechingAttack.Target == Target.Other || leechingAttack.Target == Target.Both)
        {
            damageDealt += AttackCombatent(leechingAttack.DamageAmount, target, caster, leechingAttack.DamageType, DamageSource.Spell);
        }
        if (leechingAttack.Target == Target.Self || leechingAttack.Target == Target.Both)
        {
            damageDealt += AttackCombatent(leechingAttack.DamageAmount, caster, caster, leechingAttack.DamageType, DamageSource.Spell);
        }
        AlterCombatentHP(-damageDealt, caster, DamageType.Heal);
    }

    private void HandleSpellMultiAttackEffect(SpellMultiAttackEffect multiAttack, Combatent target, Combatent caster)
    {
        if (multiAttack.Target == Target.Other || multiAttack.Target == Target.Both)
        {
            AttackCombatent(multiAttack.DamageAmount, target, caster, multiAttack.DamageType, DamageSource.Spell);
        }
        if (multiAttack.Target == Target.Self || multiAttack.Target == Target.Both)
        {
            AttackCombatent(multiAttack.DamageAmount, caster, caster, multiAttack.DamageType, DamageSource.Spell);
        }
    }

    private void HandleWardEffect(SpellWardEffect ward, Combatent target, Combatent caster)
    {
        if (ward.Target == Target.Other || ward.Target == Target.Both)
        {
            GiveCombatentWard(ward.WardAmount, target);
        }
        if (ward.Target == Target.Self || ward.Target == Target.Both)
        {
            GiveCombatentWard(ward.WardAmount, caster);
        }
    }

    private void HandleApplyAfflictionEffect(SpellApplyAfflictionEffect apply, Combatent target, Combatent caster)
    {
        if (apply.Target == Target.Other || apply.Target == Target.Both)
        {
            AddAffliction(apply.AfflictionType, apply.NumStacks, target);
        }
        if (apply.Target == Target.Self || apply.Target == Target.Both)
        {
            AddAffliction(apply.AfflictionType, apply.NumStacks, caster);
        }
    }

    private void HandleCleanseAfflictionEffect(SpellCleanseAfflictionsEffect cleanse, Combatent target, Combatent caster)
    {
        if (cleanse.Target == Target.Other || cleanse.Target == Target.Both)
        {
            RemoveAfflictionsOfSignFromMap(target, cleanse.toCleanse);
        }
        if (cleanse.Target == Target.Self || cleanse.Target == Target.Both)
        {
            RemoveAfflictionsOfSignFromMap(caster, cleanse.toCleanse);
        }
    }

    private void HandleAlterHPEffect(SpellAlterHPEffect alterHP, Combatent target, Combatent caster)
    {
        if (alterHP.Target == Target.Other || alterHP.Target == Target.Both)
        {
            AlterCombatentHP(alterHP.HPAmount, target, alterHP.DamageType);
        }
        if (alterHP.Target == Target.Self || alterHP.Target == Target.Both)
        {
            AlterCombatentHP(alterHP.HPAmount, caster, alterHP.DamageType);
        }
    }

    private void HandleAlterMaxHPEffect(SpellAlterPlayerMaxHPEffect alterHP)
    {
        GameManager._Instance.AlterPlayerMaxHP(alterHP.HPAmount);
    }

    private void HandleAlterQueuedSpellEffect(SpellAlterQueuedSpellEffect alterQueuedSpell, Combatent target, Combatent caster)
    {
        if (alterQueuedSpell.Target == Target.Other || alterQueuedSpell.Target == Target.Both)
        {
            AlterQueuedSpellEffect(alterQueuedSpell, target);
        }
        if (alterQueuedSpell.Target == Target.Self || alterQueuedSpell.Target == Target.Both)
        {
            AlterQueuedSpellEffect(alterQueuedSpell, caster);
        }
    }

    private void AlterQueuedSpellEffect(SpellAlterQueuedSpellEffect alterQueuedSpell, Combatent caster)
    {
        Dictionary<SpellStat, List<QueuedSpell>> applicableSpells = GetQueuedSpellsWithStats(caster, alterQueuedSpell.ApplicableStats);
        if (applicableSpells.Count > 0)
        {
            SpellStat alteringStat = RandomHelper.GetRandomFromList(applicableSpells.Keys.ToList());
            List<QueuedSpell> applicableSpellList = applicableSpells[alteringStat];
            QueuedSpell alteringSpell = RandomHelper.GetRandomFromList(applicableSpellList);
            alteringSpell.Spell.AlterSpellStat(alteringStat, alterQueuedSpell.AlterBy, alterQueuedSpell.AlteredStatDuration);
            alteringSpell.SpellQueueDisplay.ShowStatChange(alteringStat, alterQueuedSpell.AlterBy > 0 ? Sign.Positive : Sign.Negative);
        }
    }

    private Dictionary<SpellStat, List<QueuedSpell>> GetQueuedSpellsWithStats(Combatent caster, List<SpellStat> statTypes)
    {
        // Freakshow
        if (statTypes.Contains(SpellStat.AnyAffStackAmount))
        {
            statTypes.Add(SpellStat.Aff1StackAmount);
            statTypes.Add(SpellStat.Aff2StackAmount);
        }

        Dictionary<SpellStat, List<QueuedSpell>> applicableSpells = new Dictionary<SpellStat, List<QueuedSpell>>();
        foreach (QueuedSpell spell in GetTargetSpellQueue(caster))
        {
            foreach (SpellStat stat in statTypes)
            {
                if (spell.Spell.HasSpellStat(stat))
                {
                    if (applicableSpells.ContainsKey(stat))
                    {
                        applicableSpells[stat].Add(spell);
                    }
                    else
                    {
                        applicableSpells.Add(stat, new List<QueuedSpell>() { spell });
                    }
                }
            }
        }
        return applicableSpells;
    }

    private IEnumerator HandleQueueSpellEffect(SpellQueueSpellEffect queueSpell, Combatent target, Combatent caster)
    {
        if (queueSpell.Target == Target.Other || queueSpell.Target == Target.Both)
        {
            yield return StartCoroutine(AddSpellToCastQueue(queueSpell.ToQueue, target, caster, false, true));
        }
        if (queueSpell.Target == Target.Self || queueSpell.Target == Target.Both)
        {
            yield return StartCoroutine(AddSpellToCastQueue(queueSpell.ToQueue, caster, target, false, true));
        }
    }

    private IEnumerator HandlePlayerDrawSpellsSpellEffect(PlayerDrawSpellsSpellEffect draw)
    {
        yield return StartCoroutine(DrawSpells(draw.NumSpells));
    }

    private IEnumerator HandlePlayerDiscardSpellsSpellEffect(PlayerDiscardSpellsSpellEffect discard)
    {
        if (discard.LetChoose)
        {
            //
            yield return StartCoroutine(DiscardSpellSequence(discard.NumSpells, discard.DoToSelected, null));
        }
        else
        {
            yield return StartCoroutine(ForceDiscardSpells(discard.NumSpells, discard.DoToSelected));
        }
    }

    private IEnumerator HandlePlayerExhaustSpellsSpellEffect(PlayerExhaustSpellsSpellEffect exhaust)
    {
        if (exhaust.LetChoose)
        {
            //
            yield return StartCoroutine(ExhaustSpellSequence(exhaust.NumSpells, exhaust.DoToSelected, null));
        }
        else
        {
            yield return StartCoroutine(ForceExhaustSpells(exhaust.NumSpells));
        }
    }

    private void HandleTickPrepTimeSpellEffect(SpellTickPrepTimeEffect tick, Combatent target, Combatent caster)
    {
        if (tick.Target == Target.Other || tick.Target == Target.Both)
        {
            TickRandomQueuedSpell(caster == Combatent.Character ? enemyCastQueue : playerCastQueue);
        }
        if (tick.Target == Target.Self || tick.Target == Target.Both)
        {
            TickRandomQueuedSpell(caster == Combatent.Character ? playerCastQueue : enemyCastQueue);
        }
    }

    private void HandlePlayerRestoreManaSpellEffect(AlterCurrentManaSpellEffect mana)
    {
        GameManager._Instance.AlterPlayerCurrentMana(mana.AlterBy);
    }

    private void HandlePlayerAddSpellToDeckEffect(AddSpellToDeckEffect deck)
    {
        StartCoroutine(GameManager._Instance.ShowAddSpellSequence(deck.ToAdd));

        switch (deck.AddTo)
        {
            case SpellPileType.Discard:
                DiscardPile.Add(deck.ToAdd);
                break;
            case SpellPileType.Draw:
                DrawPile.Add(deck.ToAdd);
                break;
            case SpellPileType.Exhaust:
                ExhaustPile.Add(deck.ToAdd);
                break;
            case SpellPileType.Hand:
                Hand.Add(deck.ToAdd);
                VisualSpellDisplay spawned = Instantiate(cardDisplayPrefab, null);
                handLayoutGroup.AddChild(spawned.transform);
                spawned.SetSpell(deck.ToAdd);
                spawned.SetSpellDisplayState(SpellDisplayState.InHand);
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public IEnumerator CallSpellEffects(List<SpellEffect> spellEffects, Action callOnFinish, Combatent caster, Combatent target, bool allowAnimations = true)
    {
        // if there's an activate tween on the caster, wait until it's finished
        if (combatentShakeTweenDict.ContainsKey(caster))
        {
            yield return new WaitUntil(() => combatentShakeTweenDict[caster].Count == 0);
        }

        // Paralyze Effect
        if (TargetHasAffliction(AfflictionType.Paralyze, caster))
        {
            ConsumeAfflictionStack(AfflictionType.Paralyze, caster);
            ShowAfflictionProc(AfflictionType.Paralyze, caster);
            ShakeCombatent(caster);
            yield break;
        }

        // Go Ahead with Attack
        bool hasAnimatedSprite = false;
        foreach (SpellEffect spellEffect in spellEffects)
        {
            switch (spellEffect)
            {
                case SpellSingleAttackEffect singleAttack:
                    // Animation
                    if (allowAnimations && singleAttack.AttackAnimationStyle != AttackAnimationStyle.None)
                    {
                        yield return StartCoroutine(AnimateSpriteAttack(caster));
                        hasAnimatedSprite = true;
                    }

                    HandleSpellSingleAttackEffect(singleAttack, target, caster);

                    break;
                case SpellLeechingAttackEffect leechingAttack:
                    // Animation
                    if (allowAnimations && leechingAttack.AttackAnimationStyle != AttackAnimationStyle.None)
                    {
                        yield return StartCoroutine(AnimateSpriteAttack(caster));
                        hasAnimatedSprite = true;
                    }

                    HandleSpellLeechingAttackEffect(leechingAttack, target, caster);

                    break;
                case SpellMultiAttackEffect multiAttack:
                    for (int i = 0; i < multiAttack.NumAttacks; i++)
                    {
                        // Either animate every attack or only the first attack depending on what is set
                        // Animation
                        if (allowAnimations && (multiAttack.AttackAnimationStyle == AttackAnimationStyle.PerAttack
                            || (multiAttack.AttackAnimationStyle == AttackAnimationStyle.Once && !hasAnimatedSprite)))
                        {
                            yield return StartCoroutine(AnimateSpriteAttack(caster));
                            hasAnimatedSprite = true;
                        }

                        HandleSpellMultiAttackEffect(multiAttack, target, caster);

                        yield return new WaitForSeconds(multiAttack.TimeBetweenAttacks);
                    }
                    break;
                case SpellWardEffect ward:
                    HandleWardEffect(ward, target, caster);
                    break;
                case SpellApplyAfflictionEffect apply:
                    // Animation
                    if (apply.Target == Target.Other && allowAnimations && !hasAnimatedSprite)
                    {
                        yield return StartCoroutine(AnimateSpriteAttack(caster));
                        hasAnimatedSprite = true;
                    }

                    HandleApplyAfflictionEffect(apply, target, caster);
                    break;
                case SpellCleanseAfflictionsEffect cleanse:
                    HandleCleanseAfflictionEffect(cleanse, target, caster);
                    break;
                case SpellAlterHPEffect alterHP:
                    HandleAlterHPEffect(alterHP, target, caster);
                    break;
                case SpellAlterQueuedSpellEffect alterQueuedSpell:
                    HandleAlterQueuedSpellEffect(alterQueuedSpell, target, caster);
                    break;
                case SpellQueueSpellEffect queueSpell:
                    yield return StartCoroutine(HandleQueueSpellEffect(queueSpell, target, caster));
                    break;
                case PlayerDrawSpellsSpellEffect draw:
                    yield return StartCoroutine(HandlePlayerDrawSpellsSpellEffect(draw));
                    break;
                case PlayerDiscardSpellsSpellEffect discard:
                    yield return StartCoroutine(HandlePlayerDiscardSpellsSpellEffect(discard));
                    break;
                case PlayerExhaustSpellsSpellEffect exhaust:
                    yield return StartCoroutine(HandlePlayerExhaustSpellsSpellEffect(exhaust));
                    break;
                case SpellTickPrepTimeEffect tick:
                    HandleTickPrepTimeSpellEffect(tick, target, caster);
                    break;
                case AlterCurrentManaSpellEffect mana:
                    HandlePlayerRestoreManaSpellEffect(mana);
                    break;
                case AddSpellToDeckEffect deck:
                    HandlePlayerAddSpellToDeckEffect(deck);
                    break;
                case SpellAlterPlayerMaxHPEffect maxHp:
                    HandleAlterMaxHPEffect(maxHp);
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }


        // Call on Finish
        callOnFinish?.Invoke();
    }

    private void RemoveAfflictionsOfSignFromMap(Combatent combatent, List<Sign> toCleanse)
    {
        List<Affliction> toRemove = new List<Affliction>();

        foreach (Affliction affliction in GetTargetAfflictionMap(combatent).Values)
        {
            // Debug.Log("On Aff: " + affliction);
            if (toCleanse.Contains(affliction.Sign))
            {
                toRemove.Add(affliction);
            }
        }

        while (toRemove.Count > 0)
        {
            Affliction removingAff = toRemove[0];
            toRemove.RemoveAt(0);
            RemoveAffliction(combatent, removingAff.Type);
        }
    }

    public int AlterCombatentHP(int amount, Combatent target, DamageType damageType, bool allowUseWard = true)
    {
        if (amount < 0)
        {
            // Intangible Effect
            if (TargetHasAffliction(AfflictionType.Intangible, target) && amount < -1)
            {
                amount = -1;
                ConsumeAfflictionStack(AfflictionType.Intangible, target);
                ShowAfflictionProc(AfflictionType.Intangible, target);
            }
        }

        // Call the AlterHP function on the appropriate Target
        PopupText text;
        switch (target)
        {
            case Combatent.Character:

                // Use Ward
                if (amount < 0 && allowUseWard)
                {
                    int wardUsed = UseWard(amount, Combatent.Character, () => characterWard, i => characterWard += i);
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

                CallDamageTypeAnimation(damageType, target);

                if (amount < 0)
                {
                    // Callback
                    CombatentIntCallbackMap[Combatent.Character][CombatIntCallbackType.OnTakeDamage]?.Invoke(amount * -1);

                    ShakeCombatent(Combatent.Character);
                }

                // Finalize player HP damage
                GameManager._Instance.AlterPlayerCurrentHP(amount, damageType, false);
                characterHPBar.SetCurrentHP(GameManager._Instance.GetCurrentCharacterHP());

                return amount;
            case Combatent.Enemy:

                // Use Ward
                if (amount < 0 && allowUseWard)
                {
                    int wardUsed = UseWard(amount, Combatent.Enemy, () => enemyWard, i => enemyWard += i);
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

                CallDamageTypeAnimation(damageType, target);

                // tried to heal past max
                if (currentEnemyHP + amount > maxEnemyHP)
                {
                    currentEnemyHP = maxEnemyHP;
                }
                else if (currentEnemyHP + amount < 0) // tried to damage past 0
                {
                    // Callback
                    CombatentIntCallbackMap[Combatent.Enemy][CombatIntCallbackType.OnTakeDamage]?.Invoke(amount * -1);

                    ShakeCombatent(Combatent.Enemy);

                    currentEnemyHP = 0;
                }
                else if (amount < 0)
                {
                    // is Damage
                    // Apply amount
                    currentEnemyHP += amount;

                    // Callback
                    CombatentIntCallbackMap[Combatent.Enemy][CombatIntCallbackType.OnTakeDamage]?.Invoke(amount * -1);

                    ShakeCombatent(Combatent.Enemy);
                }
                else
                {
                    // is Either Heal or Zero
                    currentEnemyHP += amount;
                }
                // Update HP Bar
                enemyHPBar.SetCurrentHP(currentEnemyHP);
                return amount;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public int AttackCombatent(int amount, Combatent target, Combatent attacker, DamageType damageType, DamageSource damageSource)
    {
        // Thorns Effect
        if (TargetHasAffliction(AfflictionType.Thorns, target))
        {
            int thornsDamage = GetTargetAfflictionMap(target)[AfflictionType.Thorns].GetStacks();
            ShowAfflictionProc(AfflictionType.Thorns, target);
            AlterCombatentHP(-thornsDamage, attacker, DamageType.Physical);
        }

        // Attempted to Basic Attack for less than 0 (i.e., a Heal)
        if (damageSource == DamageSource.BasicAttack && amount < 0)
        {
            return AlterCombatentHP(0, target, damageType);
        }

        int damage = CalculateDamage(amount, attacker, target, damageType, damageSource, true);

        // Poison Coated Effect
        if (TargetHasAffliction(AfflictionType.PoisonCoated, attacker) && damage > GetCombatentWard(target))
        {
            AddAffliction(AfflictionType.Poison, GetTargetAfflictionStacks(AfflictionType.PoisonCoated, attacker), target);
        }

        // Callback
        CombatentBaseCallbackMap[attacker][CombatBaseCallbackType.OnAttack]?.Invoke();

        return AlterCombatentHP(-damage, target, damageType);
    }

    // Calculation Function
    // Takes a number and spits out the number post effects
    public int CalculateDamage(int amount, Combatent attacker, Combatent target, DamageType damageType, DamageSource source, bool consumeAfflictions)
    {
        // Attacker Effects
        // Power Effect
        if (TargetHasAffliction(AfflictionType.Power, attacker)
            && source != DamageSource.BasicAttack)
        {
            amount += GetTargetAfflictionStacks(AfflictionType.Power, attacker);
            // Make sure that negative Power values don't wind up making a damaging attack heal instead,
            // rather whatever action is trying to do that action is simply zeroed out
            if (amount < 0)
            {
                amount = 0;
            }
        }

        // Black Prism Effect
        if (attacker == Combatent.Character
            && source == DamageSource.Spell
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

        // On Guard Effect
        if (TargetHasAffliction(AfflictionType.OnGuard, target))
        {
            amount -= BalenceManager._Instance.GetValue(AfflictionType.OnGuard, "ReduceBy");
            // Make sure guarded doesn't make what should be damage instead be a heal
            if (amount < 0)
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
    public int CalculateWard(int amount, Combatent applyingTo)
    {
        if (TargetHasAffliction(AfflictionType.Protection, applyingTo))
        {
            amount += GetTargetAfflictionStacks(AfflictionType.Protection, applyingTo);
            // Make sure that negative Protection values don't wind up taking away Ward, instead whatever action is trying to give that ward is simply zeroed out
            if (amount < 0)
            {
                amount = 0;
            }
        }
        return amount;
    }

    private int UseWard(int amount, Combatent target, Func<int> getFunc, Action<int> alterFunc)
    {
        // Levitating Effect
        switch (target)
        {
            case Combatent.Character:
                if (TargetHasAffliction(AfflictionType.Levitating, Combatent.Enemy))
                {
                    return 0;
                }
                break;
            case Combatent.Enemy:
                if (TargetHasAffliction(AfflictionType.Levitating, Combatent.Character))
                {
                    return 0;
                }
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }

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

            // Spawn Text
            SpawnEffectText(EffectTextStyle.UpAndFade, "Warded", UIManager._Instance.GetDamageTypeColor(DamageType.Ward), target);

            // return the amount ward used;
            return wardUsed;
        }
        else
        {
            // Target has no ward, just return 0
            return 0;
        }
    }

    public void GiveCombatentWard(int wardAmount, Combatent target)
    {
        wardAmount = CalculateWard(wardAmount, target);

        // Target is afflicted with ward negation
        if (TargetHasAffliction(AfflictionType.WardNegation, target))
        {
            // Determine how many stacks there are
            int numStacks = GetTargetAffliction(AfflictionType.WardNegation, target).GetStacks();
            // negate that amount off of the wardAmount
            if (numStacks > wardAmount)
            {
                ConsumeAfflictionStack(AfflictionType.WardNegation, target, wardAmount);
                wardAmount = 0;
            }
            else
            {
                ConsumeAfflictionStack(AfflictionType.WardNegation, target, numStacks);
                wardAmount -= numStacks;
            }
            if (wardAmount < 0) wardAmount = 0;
        }
        if (wardAmount == 0) return;

        CallDamageTypeAnimation(DamageType.Ward, target);
        switch (target)
        {
            case Combatent.Character:
                characterWard += wardAmount;
                characterHPBar.SetWard(characterWard);
                break;
            case Combatent.Enemy:
                enemyWard += wardAmount;
                enemyHPBar.SetWard(enemyWard);
                break;
        }
    }

    public int GetCombatentWard(Combatent target)
    {
        switch (target)
        {
            case Combatent.Character:
                return characterWard;
            case Combatent.Enemy:
                return enemyWard;
            default:
                throw new UnhandledSwitchCaseException();
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

    public void ResetCombatentWard(Combatent target)
    {
        switch (target)
        {
            case Combatent.Character:
                if (TargetHasAffliction(AfflictionType.LingeringFlame, target))
                {
                    characterWard -= GetTargetAffliction(AfflictionType.LingeringFlame, target).GetStacks();
                    if (characterWard < 0) characterWard = 0;
                }
                else
                {
                    characterWard = 0;
                }
                characterHPBar.SetWard(characterWard);
                break;
            case Combatent.Enemy:
                if (TargetHasAffliction(AfflictionType.LingeringFlame, target))
                {
                    enemyWard -= GetTargetAffliction(AfflictionType.LingeringFlame, target).GetStacks();
                    if (enemyWard < 0) enemyWard = 0;
                }
                else
                {
                    enemyWard = 0;
                }
                enemyHPBar.SetWard(enemyWard);
                break;
        }
    }

    public int GetPowerBonus(Combatent owner)
    {
        int powerBonus = 0;
        if (TargetHasAffliction(AfflictionType.Power, owner))
        {
            powerBonus = GetAffliction(AfflictionType.Power, owner).GetStacks();
        }
        return powerBonus;
    }

    public int GetProtectionBonus(Combatent owner)
    {
        int protectionBonus = 0;
        if (TargetHasAffliction(AfflictionType.Protection, owner))
        {
            protectionBonus = GetAffliction(AfflictionType.Protection, owner).GetStacks();
        }
        return protectionBonus;
    }

    private void CallDamageTypeAnimation(DamageType damageType, Combatent target)
    {
        switch (damageType)
        {
            case DamageType.Ward:
                StartCoroutine(WardDamageTypeAnimation(target));
                break;
            default:
                StartCoroutine(DefaultDamageTypeAnimation(damageType, target));
                break;
        }
    }

    private IEnumerator DefaultDamageTypeAnimation(DamageType damageType, Combatent target)
    {
        DamageTypeAnimator animator = Instantiate(defaultDamageTypeAnimatorPrefab, GetTargetSpriteImage(target).transform);
        animator.CV.alpha = 0;
        animator.Image.color = UIManager._Instance.GetDamageTypeColor(damageType);

        Tween shake = (animator.transform as RectTransform).DOShakeAnchorPos(1, animator.GetAdditionalParameter("ShakeStrength"),
            (int)animator.GetAdditionalParameter("ShakeVibrato"), animator.GetAdditionalParameter("ShakeRandomness"), false, false, ShakeRandomnessMode.Full).SetLoops(-1);

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(animator.CV, 1, animator.GetAdditionalParameter("FadeInRate")));

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(animator.CV, 0, animator.GetAdditionalParameter("FadeOutRate")));

        shake.Kill();
        Destroy(animator.gameObject);
    }

    private IEnumerator WardDamageTypeAnimation(Combatent target)
    {
        DamageTypeAnimator animator = Instantiate(wardDamageTypeAnimatorPrefab, GetTargetSpriteImage(target).transform);
        animator.CV.alpha = 0;

        Coroutine scaleUp = StartCoroutine(Utils.MoveTowardsScale(animator.transform, animator.transform.localScale * 2, animator.GetAdditionalParameter("ScaleUpRate")));

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(animator.CV, animator.GetAdditionalParameter("AlphaTarget"), animator.GetAdditionalParameter("FadeInRate")));

        yield return new WaitForSeconds(animator.GetAdditionalParameter("Delay"));

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(animator.CV, 0, animator.GetAdditionalParameter("FadeOutRate")));

        StopCoroutine(scaleUp);

        Destroy(animator.gameObject);
    }

    #endregion

    #region Afflictions

    public void AddAffliction(AfflictionType type, int num, Combatent target)
    {
        if (target == Combatent.Character)
        {
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
        }
        SetAffliction(type, num, target);
    }

    public Affliction GetAffliction(AfflictionType type, Combatent owner)
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

    public void ShowAfflictionProc(AfflictionType type, Combatent t)
    {
        Dictionary<AfflictionType, AfflictionIcon> map = GetTargetAfflictionDisplays(t);
        if (map.ContainsKey(type))
        {
            map[type].AnimateScale();
        }
    }

    private void SetAffliction(AfflictionType type, int numStacks, Combatent target)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Transform parentTo = GetTargetParentAfflictionTo(target);

        bool isNewInstance;
        Affliction aff;

        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            aff = map[type];
            aff.AlterStacks(numStacks);

            ShowAfflictionProc(type, target);
            isNewInstance = false;

            // Spawn Effect Text
            SpawnEffectText(EffectTextStyle.UpAndFade, (numStacks > 0 ? "+" : "") + numStacks + " " + aff.GetToolTipLabel(),
                UIManager._Instance.GetEffectTextColor(aff.Sign + "Affliction"), target, UIManager._Instance.GetAfflictionIcon(type));

            // Animate
            if (aff.Sign == Sign.Negative)
            {
                ShakeCombatent(target);
            }
        }
        else
        {
            aff = Affliction.GetAfflictionOfType(type);

            // Nullify Effect
            if (TargetHasAffliction(AfflictionType.Nullify, target) && aff.Sign == Sign.Negative)
            {
                ConsumeAfflictionStack(AfflictionType.Nullify, target);
                SpawnEffectText(EffectTextStyle.UpAndFade, aff.Name + " Nullified", UIManager._Instance.GetEffectTextColor("AfflictionNullified"), target);

                // Can't Animate the Affliction Display if there is no more Nullify Stacks, so make sure to Guard against that
                if (TargetHasAffliction(AfflictionType.Nullify, target))
                {
                    ShowAfflictionProc(AfflictionType.Nullify, target);
                }
                return;
            }

            aff.SetOwner(target);

            // Spawn Effect Text
            SpawnEffectText(EffectTextStyle.UpAndFade, aff.GetToolTipLabel(), UIManager._Instance.GetEffectTextColor(aff.Sign + "Affliction"), target,
                UIManager._Instance.GetAfflictionIcon(type));

            // Animate
            if (aff.Sign == Sign.Negative)
            {
                ShakeCombatent(target);
            }

            // The affliction we tried to apply didn't stick, we do not need to do any of the following
            aff.SetStacks(numStacks);
            if (aff.CanBeCleared)
            {
                return;
            }

            map.Add(type, aff);

            AfflictionIcon spawned = Instantiate(afflictionIconPrefab, parentTo);
            aff.SetAttachedTo(spawned);

            spawned.SetAffliction(aff);
            GetTargetAfflictionDisplays(target).Add(type, spawned);
            aff.UpdateAfflictionDisplay();

            ShowAfflictionProc(type, target);
            isNewInstance = true;

            // Apply
            aff.Apply();
        }

        // Callbacks
        switch (target)
        {
            case Combatent.Character:
                // Enemy is Setting
                CombatentAfflictionCallbackMap[Combatent.Enemy][CombatAfflictionCallbackType.OnApply]?.Invoke(aff);
                if (isNewInstance)
                {
                    CombatentAfflictionCallbackMap[Combatent.Character][CombatAfflictionCallbackType.OnGain]?.Invoke(aff);
                }
                break;
            case Combatent.Enemy:
                CombatentAfflictionCallbackMap[Combatent.Character][CombatAfflictionCallbackType.OnApply]?.Invoke(aff);
                // Character is Setting
                if (isNewInstance)
                {
                    CombatentAfflictionCallbackMap[Combatent.Character][CombatAfflictionCallbackType.OnGain]?.Invoke(aff);
                }
                break;
        }

        // Update hp bar
        UpdateHPBarAfflictions(target);

        return;
    }

    public void UpdateHPBarAfflictions(Combatent target)
    {
        int v;

        CombatentHPBar hpBar = GetCombatentHPBar(target);

        // Burn
        v = (TargetHasAffliction(AfflictionType.Burn, target) ? BalenceManager._Instance.GetValue(AfflictionType.Burn, "DamageAmount") : 0);
        hpBar.SetDamageFromBurn(v);

        // Poison
        v = (TargetHasAffliction(AfflictionType.Poison, target) ? GetTargetAfflictionStacks(AfflictionType.Poison, target) : 0);
        hpBar.SetDamageFromPoison(v);

        // Blight
        v = (TargetHasAffliction(AfflictionType.Blight, target) ? GetTargetAfflictionStacks(AfflictionType.Blight, target) : 0);
        hpBar.SetDamageFromBlight(v);
    }

    private void ClearAfflictionMap(Dictionary<AfflictionType, Affliction> map, Combatent t)
    {
        // Remove Afflictions
        while (map.Count > 0)
        {
            AfflictionType removing = map.Keys.ElementAt(0);
            RemoveAffliction(t, removing);
        }
        map.Clear();
    }

    private Dictionary<AfflictionType, Affliction> GetTargetAfflictionMap(Combatent t)
    {
        return t == Combatent.Character ? characterAfflictionMap : enemyAfflictionMap;
    }

    private Dictionary<AfflictionType, AfflictionIcon> GetTargetAfflictionDisplays(Combatent t)
    {
        return t == Combatent.Character ? characterAfflictionIconTracker : enemyAfflictionIconTracker;
    }

    private Transform GetTargetParentAfflictionTo(Combatent t)
    {
        return t == Combatent.Character ? characterAfflictionList : enemyAfflictionList;
    }

    public bool TargetHasAffliction(AfflictionType type, Combatent target)
    {
        return GetTargetAfflictionMap(target).ContainsKey(type);
    }

    public Affliction GetTargetAffliction(AfflictionType type, Combatent target)
    {
        if (TargetHasAffliction(type, target))
        {
            return GetTargetAfflictionMap(target)[type];
        }
        return null;
    }

    public void ConsumeAfflictionStack(AfflictionType type, Combatent target, int toConsume = 1)
    {
        // Only consumes a stack if there are stacks to be consumed
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);

        Affliction aff = map[type];

        // remove a stack
        aff.AlterStacks(-toConsume);

        // update hp bar
        UpdateHPBarAfflictions(target);
    }

    public void RemoveAffliction(Combatent target, AfflictionType type)
    {
        // Debug.Log("Removing: " + type + " From " + target);

        // Get the Affliction we're Removing
        Affliction removingAff = GetTargetAfflictionMap(target)[type];

        // Unapply
        removingAff.Unapply();

        // Destroy UI
        Dictionary<AfflictionType, AfflictionIcon> displays = GetTargetAfflictionDisplays(target);
        AfflictionIcon icon = displays[type];
        displays.Remove(type);
        Destroy(icon.gameObject);

        // Remove Affliction
        GetTargetAfflictionMap(target).Remove(type);

        // Animate
        if (removingAff.Sign == Sign.Positive)
        {
            ShakeCombatent(target);
        }

        // Update hp bar
        UpdateHPBarAfflictions(target);

        // Spawn Effect Text
        SpawnEffectText(EffectTextStyle.Fade, removingAff.GetToolTipLabel() + " Wears Off", UIManager._Instance.GetEffectTextColor("AfflictionRemoved"), target,
            UIManager._Instance.GetAfflictionIcon(removingAff.Type));

        // Callback
        CombatentAfflictionCallbackMap[target][CombatAfflictionCallbackType.OnRemove]?.Invoke(removingAff);
    }

    private int GetTargetAfflictionStacks(AfflictionType type, Combatent target)
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

    public void ClearRandomAffliction(Combatent t, Sign sign)
    {
        List<AfflictionType> negativeAfflictions = new List<AfflictionType>();
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(t);
        foreach (KeyValuePair<AfflictionType, Affliction> kvp in map)
        {
            if (kvp.Value.Sign == Sign.Negative)
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

    #endregion

    #region UI

    private Image GetTargetSpriteImage(Combatent target)
    {
        switch (target)
        {
            case Combatent.Character:
                return characterCombatSprite;
            case Combatent.Enemy:
                return enemyCombatSprite;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    private List<QueuedSpell> GetTargetSpellQueue(Combatent target)
    {
        switch (target)
        {
            case Combatent.Character:
                return playerCastQueue;
            case Combatent.Enemy:
                return enemyCastQueue;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void SpawnEffectText(EffectTextStyle style, string text, Color c, Combatent owner, Sprite withIcon = null)
    {
        if (!combatScreenOpen) return;
        switch (owner)
        {
            case Combatent.Character:
                characterEffectTextDisplay.CallSpawnEffectText(style, text, c, withIcon);
                return;
            case Combatent.Enemy:
                enemyEffectTextDisplay.CallSpawnEffectText(style, text, c, withIcon);
                return;
        }
    }

    public void SpawnEffectIcon(EffectIconStyle style, Sprite sprite, Combatent owner)
    {
        if (!combatScreenOpen) return;
        switch (owner)
        {
            case Combatent.Character:
                characterEffectTextDisplay.CallSpawnEffectIcon(style, sprite);
                return;
            case Combatent.Enemy:
                enemyEffectTextDisplay.CallSpawnEffectIcon(style, sprite);
                return;
        }
    }

    public void ShakeCombatent(Combatent target)
    {
        RectTransform rect = GetTargetSpriteImage(target).transform as RectTransform;
        Tween shake = rect.DOShakeAnchorPos(shakeCombatentDuration, shakeCombatentStrength, shakeCombatentVibrato, shakeCombatentRandomness, false, true, ShakeRandomnessMode.Harmonic);

        if (combatentShakeTweenDict.ContainsKey(target))
        {
            combatentShakeTweenDict[target].Add(shake);
            StartCoroutine(RemoveShakeTweenWhenDone(shake, target));
        }
        else
        {
            combatentShakeTweenDict.Add(target, new List<Tween>());
        }
    }

    private IEnumerator RemoveShakeTweenWhenDone(Tween tween, Combatent target)
    {
        yield return new WaitUntil(() => !tween.active);

        combatentShakeTweenDict[target].Remove(tween);
    }

    private CombatentHPBar GetCombatentHPBar(Combatent target)
    {
        switch (target)
        {
            case Combatent.Character:
                return characterHPBar;
            case Combatent.Enemy:
                return enemyHPBar;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    #endregion

    [ContextMenu("Test/AddFreeSpellCast")]
    public void AddFreeSpellCast()
    {
        NumFreeSpells++;
        SpawnEffectText(EffectTextStyle.UpAndFade, "Free Spell", UIManager._Instance.GetEffectTextColor("FreeSpell"), Combatent.Character);
    }
}
