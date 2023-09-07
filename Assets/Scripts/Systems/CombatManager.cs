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

public enum Target
{
    Character,
    Enemy,
}

public enum DamageType
{
    Default,
    Poison,
    Electric,
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

    public Enemy CurrentEnemy => currentEnemy;
    public int NumFreeSpells { get; set; }
    public bool InCombat { get; private set; }
    private bool combatScreenOpen;
    public bool CanCastSpells { get; private set; }
    private bool hasCastQueue;
    private bool isCastingQueue;
    public bool AllowGameSpaceToolTips => !isCastingQueue;

    private bool playerTurnEnded;
    private Turn currentTurn;
    private int turnNumber;
    public int TurnNumber => turnNumber;

    private ObjectPool<Circle> circlePool;
    private List<Circle> circleList = new List<Circle>(); // Circles List

    [Header("Combat Settings")]
    [SerializeField] private List<DamageType> wardableDamageTypes = new List<DamageType>();
    private int handSize;
    public int MaxHandSize => 10;
    private Pile<Spell> passiveSpellPile;
    private Pile<Spell> drawPile;
    private Pile<Spell> discardPile;
    private Pile<Spell> exhaustPile;
    private Pile<Spell> hand;

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
    [SerializeField] private float discardSpellDelay = .15f;
    private List<Spell> alterHandSequenceSelectedSpells = new List<Spell>();
    private SpellDisplayState currentAlterHandSequenceState;
    [SerializeField] private VisualSpellDisplay cardDisplayPrefab;
    [SerializeField] private HandLayoutGroup handLayoutGroup;
    public HandLayoutGroup HandLayoutGroup => handLayoutGroup;

    private List<QueuedSpell> spellQueue = new List<QueuedSpell>();
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
    [SerializeField] private bool setEffectivenessMultiplierToZeroOnMiss;
    private float effectivenessMultiplier = 0;
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
    [SerializeField] private Transform passiveSpellDisplaysList;

    [Header("Enemy")]
    [SerializeField] private CombatentHPBar enemyHPBar;
    [SerializeField] private IntentDisplay enemyIntentDisplay;
    [SerializeField] private TextMeshProUGUI enemyBasicAttackDamageText;
    [SerializeField] private TextMeshProUGUI enemyNameText;
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
    [SerializeField] private SpellPotencyDisplay castingSpellPotencyDisplayPrefab;
    [SerializeField] private Circle circlePrefab; // Circle Object
    [SerializeField] private PassiveSpellDisplay passiveSpellDisplayPrefab;

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
    [SerializeField] private float enemyCombatSpriteAnimationSpeedMultiplierStart = 1;
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
    [SerializeField] private float delayAfterPassiveSpellCast = 0.5f;

    [Header("Discard Animation")]
    [SerializeField] private float discardedCardMinScale = .15f;
    [SerializeField] private float discardedCardChangeScaleRate = 10;
    [SerializeField] private float discardedCardScaleGraceRange = .1f;
    [SerializeField] private float discardCardDownAmount;
    [SerializeField] private float discardSlideDuration = 1;

    [Header("Exhaust Animation")]
    [SerializeField] private float exhaustCardUpAmount;
    [SerializeField] private float exhaustSlideDuration = 1;

    [Header("Passive Spell Animation")]
    [SerializeField] private float powerSpellShrinkDuration;

    [SerializeField] private GameObject[] showOnCombat;

    // Callbacks
    public Action OnTurnStart;

    public Action OnPlayerBasicAttack;
    public Action OnPlayerTurnStart;
    public Action OnPlayerTurnEnd;
    public Action OnPassiveSpellProc;

    public Action OnActiveSpellQueued;
    public Action OnPassiveSpellQueued;
    public Action OnActiveSpellActivated;
    public Action OnOffensiveActiveSpellActivated;
    public Action OnDefensiveActiveSpellActivated;
    public Action OnUtilityActiveSpellActivated;

    public Action OnCharacterGainAffliction;
    public Action OnCharacterLoseAffliction;
    public Action<int> OnPlayerTakeDamage;
    public Action OnPlayerAttack;

    public Action OnEnemyBasicAttack;
    public Action OnEnemyTurnStart;
    public Action OnEnemyTurnEnd;
    public Action OnEnemyGainAffliction;
    public Action OnEnemyLoseAffliction;
    public Action<int> OnEnemyTakeDamage;
    public Action OnEnemyAttack;

    public Action OnExhaustSpell;
    public Action OnSpecificDiscardSpell;
    public Action OnDrawSpell;

    public Action OnCombatStart;
    public Action OnCombatEnd;

    public Action OnResetCombat;

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

    [SerializeField] private SerializableDictionary<Target, SpeechBubbleDataContainer> speechBubbles = new SerializableDictionary<Target, SpeechBubbleDataContainer>();
    [SerializeField] private float dialogueBubbleFadeInRate = 1;
    [SerializeField] private float dialogueBubbleFadeOutRate = 1;
    [SerializeField] private float defaultDialogueDuration = 2.5f;

    private KeyCode[] handHotKeyBindings = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
    private KeyCode GetKeyBindingAtIndex(int index)
    {
        return handHotKeyBindings[index];
    }

    public void CallPlayDialogue(string s, Target target)
    {
        CallPlayDialogue(s, defaultDialogueDuration, target);
    }

    public void CallPlayDialogue(string s, float duration, Target target)
    {
        if (speechBubbles[target].CurrentCoroutine != null) StopCoroutine(speechBubbles[target].CurrentCoroutine);
        speechBubbles[target].CurrentCoroutine = StartCoroutine(PlayDialogue(s, duration, target));
    }

    private IEnumerator PlayDialogue(string s, float duration, Target target)
    {
        Debug.Log("Playing Dialogue");
        SpeechBubbleDataContainer data = speechBubbles[target];

        data.Text.text = s;

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(data.CV, 1, dialogueBubbleFadeInRate));

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(data.CV, 0, dialogueBubbleFadeOutRate));
    }

    public void ReduceActiveSpellCDsByPercent(float normalizedPercent)
    {
        // normaliedPercent is some number between 0 and 1
        // 0 = 0%, 1 = 100%
        // .14 = 14%
        // etc
        foreach (ActiveSpell activeSpell in hand.GetEntriesMatching(spell => spell.Type == SpellCastType.Active))
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
        StartCoroutine(ShowSpellPile(exhaustPile, spell => true, Order.Unaltered));
    }

    public void ShowDrawPile()
    {
        showSpellPileTitleText.text = "Draw Pile";
        StartCoroutine(ShowSpellPile(drawPile, spell => true, Order.Shuffled));
    }

    public void ShowDiscardPile()
    {
        showSpellPileTitleText.text = "Discard Pile";
        StartCoroutine(ShowSpellPile(discardPile, spell => true, Order.Unaltered));
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

    public void SetSpellPiles(Pile<Spell> drawPile)
    {
        this.drawPile = drawPile;
        this.drawPile.Shuffle();
        passiveSpellPile = new Pile<Spell>();
        discardPile = new Pile<Spell>();
        hand = new Pile<Spell>();
        exhaustPile = new Pile<Spell>();
        drawPileCountText.text = drawPile.Count.ToString();
    }

    public void SetHandSize(int handSize)
    {
        this.handSize = handSize;
    }

    public IEnumerator DrawHand()
    {
        yield return StartCoroutine(DrawSpells(handSize));
    }

    public void CallDrawSpells(int num = 1)
    {
        StartCoroutine(DrawSpells(num));
    }

    private IEnumerator DrawSpells(int num = 1)
    {
        for (int i = 0; i < num;)
        {
            if (hand.Count >= MaxHandSize)
            {
                CallPlayDialogue("I have no room in my Hand...", Target.Character);
                break;
            }

            // There are absolutely no cards to draw remaining
            if (drawPile.Count == 0 && discardPile.Count == 0)
            {
                CallPlayDialogue("I'm of of Spells...?", Target.Character);
                break;
            }

            // Reshuffle discard pile into draw pile if needed
            yield return StartCoroutine(CheckForReshuffleDiscardIntoDraw());

            DrawSpell();
            i++;

            yield return new WaitForSeconds(drawSpellDelay);
        }
    }

    private IEnumerator CheckForReshuffleDiscardIntoDraw()
    {
        if (drawPile.Count == 0)
        {
            discardPile.TransferEntries(drawPile, true);

            yield return new WaitForSeconds(1);
        }
    }

    [ContextMenu("Test/DrawSpell")]
    public void DrawSpell()
    {
        Spell spell = drawPile.DrawTop();
        hand.Add(spell);

        // Spawn Card
        VisualSpellDisplay spawned = Instantiate(cardDisplayPrefab, null);
        handLayoutGroup.AddChild(spawned.transform);
        spawned.SetSpell(spell);
        spawned.SetSpellDisplayState(SpellDisplayState.InHand);

        spell.OnDraw();

        // Callback
        OnDrawSpell?.Invoke();

        CallRecalculateKeyBinds();
    }


    private IEnumerator DiscardCardAnimation(SpellDisplay card, Action preDestroy)
    {
        RectTransform t = (RectTransform)card.transform;
        card.SetScaleLocked(true);

        t.SetParent(UIManager._Instance.Canvas, true);

        StartCoroutine(Utils.LerpScale(t, Vector3.one * discardedCardMinScale, discardedCardChangeScaleRate, discardedCardScaleGraceRange));

        Tween posTween = t.DOAnchorPos(new Vector2(t.anchoredPosition.x, t.anchoredPosition.y + discardCardDownAmount), discardSlideDuration);
        yield return new WaitUntil(() => !posTween.active);

        // Call any additional Actions
        preDestroy?.Invoke();

        Destroy(card.gameObject);
    }

    private IEnumerator ExhaustCardAnimation(SpellDisplay card, Action preDestroy)
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

        // Call any additional Actions
        preDestroy?.Invoke();

        Destroy(card.gameObject);
    }

    private IEnumerator PlayPassiveSpellAnim(SpellDisplay card, Action preDestroy)
    {
        RectTransform t = (RectTransform)card.transform;
        card.SetScaleLocked(true);

        t.SetParent(UIManager._Instance.Canvas, true);

        Tween scaleTween = t.DOScale(Vector3.zero, powerSpellShrinkDuration);
        yield return new WaitUntil(() => !scaleTween.active);

        // Call any additional Actions
        preDestroy?.Invoke();

        Destroy(card.gameObject);
    }


    private IEnumerator DiscardHand()
    {
        bool done = false;
        // Discard all Spells that are not Passives
        List<Spell> toDiscard = hand.GetEntriesMatching(spell => true);

        while (toDiscard.Count > 0)
        {
            // Remove Spell from Hand
            Spell spell = toDiscard[0];
            hand.Remove(spell);

            SpellDisplay equippedTo = spell.GetEquippedTo();

            handLayoutGroup.RemoveTransformFromHand(spell.GetEquippedTo().transform);
            StartCoroutine(DiscardCardAnimation(equippedTo, delegate
            {
                if (toDiscard.Count == 0)
                {
                    done = true;
                }
            }));

            // Add Spell to Discard Pile
            discardPile.Add(spell);

            // Callback
            spell.OnAnyDiscard();

            // Callback
            spell.OnForceDiscard();

            // Remove Spell from to Remove
            toDiscard.RemoveAt(0);

            yield return new WaitForSeconds(discardSpellDelay);
        }

        yield return new WaitUntil(() => done);
    }

    public void DiscardSpell(Spell spell)
    {
        if (hand.Contains(spell))
        {
            hand.Remove(spell);
            discardPile.Add(spell);

            handLayoutGroup.RemoveTransformFromHand(spell.GetEquippedTo().transform);
            StartCoroutine(DiscardCardAnimation(spell.GetEquippedTo(), delegate
            {
                // Callback
                spell.OnAnyDiscard();

                // Callback
                spell.OnSpecificDiscard();
                OnSpecificDiscardSpell?.Invoke();

                CallRecalculateKeyBinds();
            }));
        }
    }

    public void ExhaustSpell(Spell spell)
    {
        if (hand.Contains(spell))
        {
            Debug.Log("Exhasting: " + spell);
            hand.Remove(spell);
            exhaustPile.Add(spell);

            handLayoutGroup.RemoveTransformFromHand(spell.GetEquippedTo().transform);

            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Fading);

            StartCoroutine(ExhaustCardAnimation(spell.GetEquippedTo(), delegate
            {
                // Callback
                spell.OnExhaust();

                // Callback
                OnExhaustSpell?.Invoke();

                CallRecalculateKeyBinds();
            }));
        }
    }

    public void AddSpellToPassiveSpellPile(PassiveSpell spell)
    {
        if (hand.Contains(spell))
        {
            hand.Remove(spell);
            passiveSpellPile.Add(spell);

            handLayoutGroup.RemoveTransformFromHand(spell.GetEquippedTo().transform);

            spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Fading);

            StartCoroutine(PlayPassiveSpellAnim(spell.GetEquippedTo(), delegate
            {
                Destroy(spell.GetEquippedTo().gameObject);
                CallRecalculateKeyBinds();
            }));

        }
    }

    private PassiveSpellDisplay SpawnPassiveSpellDisplay()
    {
        PassiveSpellDisplay spawned = Instantiate(passiveSpellDisplayPrefab, passiveSpellDisplaysList);
        return spawned;
    }

    public void EquipPassiveSpell(PassiveSpell spell)
    {
        spell.OnEquip();

        // Spawn a display to use
        PassiveSpellDisplay spellDisplay = SpawnPassiveSpellDisplay();
        spellDisplay.SetSpell(spell);

        SpawnEffectText(EffectTextStyle.Fade, spell.Name, UIManager._Instance.GetEffectTextColor("EquipPassiveSpell"), Target.Character);
    }

    public void TriggerRandomPassiveSpell()
    {
        if (InCombat)
        {
            if (passiveSpellPile.Count > 0)
            {
                PassiveSpell spell = (PassiveSpell)RandomHelper.GetRandomFromList(passiveSpellPile.GetEntriesMatching(spell => true));
                spell.Proc(true);
                Debug.Log("Triggered: " + spell);
            }
        }
    }

    [ContextMenu("Test/ExhaustSequence")]
    public void TestExhaustSpellSequence()
    {
        CallExhaustSpellSequence(1);
    }

    [ContextMenu("Test/DiscardSequence")]
    public void TestDiscardSpellSequence()
    {
        CallDiscardSpellSequence(1);
    }

    public void CallExhaustSpellSequence(int numToExhaust, Action onComplete = null)
    {
        StartCoroutine(ExhaustSpellSequence(numToExhaust, onComplete));
    }

    public void CallDiscardSpellSequence(int numToDiscard, Action onComplete = null)
    {
        StartCoroutine(DiscardSpellSequence(numToDiscard, onComplete));
    }

    public void ClickedSpellForAlterHandSequence(Spell spell)
    {
        if (alterHandSequenceSelectedSpells.Contains(spell))
        {
            DeselectSpellForAlterHandSequence(spell);
        }
        else
        {
            SelectSpellForAlterHandSequence(spell);
        }
    }

    private void SelectSpellForAlterHandSequence(Spell spell)
    {
        alterHandSequenceSelectedSpells.Add(spell);
        spell.GetEquippedTo().SetSpellDisplayState(SpellDisplayState.Selected);
    }

    private void DeselectSpellForAlterHandSequence(Spell spell)
    {
        alterHandSequenceSelectedSpells.Remove(spell);
        spell.GetEquippedTo().SetSpellDisplayState(currentAlterHandSequenceState);
    }

    public void ReduceActiveSpellCooldowns(int reduceBy)
    {
        foreach (ActiveSpell spell in hand.GetEntriesMatching(spell => spell.Type == SpellCastType.Active))
        {
            if (spell.OnCooldown)
            {
                spell.AlterCooldown(-reduceBy);
            }
        }
    }

    public void ResetActiveSpellCooldowns()
    {
        foreach (ActiveSpell spell in hand.GetEntriesMatching(spell => spell.Type == SpellCastType.Active))
        {
            if (spell.OnCooldown)
            {
                spell.ResetCooldown();
            }
        }
    }

    private IEnumerator AlterHandSequence(int numToAlter, Action<Spell> callOnSpell, string label, SpellDisplayState choosingState, Action onComplete)
    {
        if (hand.Count < numToAlter)
        {
            hand.ActOnEachSpellInPile(spell => alterHandSequenceSelectedSpells.Add(spell));
        }
        else
        {
            currentAlterHandSequenceState = choosingState;
            alterHandInstructionText.gameObject.SetActive(true);
            alterHandInstructionContainer.SetActive(true);
            alterHandBackground.SetActive(true);
            hand.ActOnEachSpellInPile(spell => spell.GetEquippedTo().SetSpellDisplayState(choosingState));

            while (alterHandSequenceSelectedSpells.Count < numToAlter)
            {
                int numToGo = (numToAlter - alterHandSequenceSelectedSpells.Count);
                alterHandInstructionText.text = "Select " + numToGo + " more " + (numToGo > 1 ? "Spells" : "Spell") + " (" + label + ")";
                yield return null;
            }
        }

        alterHandInstructionContainer.gameObject.SetActive(false);

        while (alterHandSequenceSelectedSpells.Count > 0)
        {
            Spell spell = alterHandSequenceSelectedSpells[0];
            alterHandSequenceSelectedSpells.RemoveAt(0);

            callOnSpell(spell);

            yield return new WaitForSeconds(delayBetweenAlterHandCalls);
        }

        CallRecalculateKeyBinds();
        alterHandBackground.SetActive(false);

        onComplete?.Invoke();
    }

    private void CallRecalculateKeyBinds()
    {
        if (recalculateKeyBindsCoroutine != null) StopCoroutine(recalculateKeyBindsCoroutine);
        recalculateKeyBindsCoroutine = StartCoroutine(RecalculateKeyBinds());
    }

    private Coroutine recalculateKeyBindsCoroutine;

    private IEnumerator RecalculateKeyBinds()
    {
        yield return new WaitForSeconds(.5f);

        int index = 0;
        hand.ActOnEachSpellInPile(spell =>
        {
            // Recalculate Key Bindings
            ((VisualSpellDisplay)spell.GetEquippedTo()).SetKeyBinding(GetKeyBindingAtIndex(index));
            index++;

        });
    }


    public IEnumerator ExhaustSpellSequence(int numToExhaust, Action onComplete = null)
    {
        yield return StartCoroutine(AlterHandSequence(numToExhaust, spell => ExhaustSpell(spell), "Exhaust", SpellDisplayState.ChoosingExhaust, onComplete));
    }

    public IEnumerator DiscardSpellSequence(int numToDiscard, Action onComplete = null)
    {
        yield return StartCoroutine(AlterHandSequence(numToDiscard, spell => DiscardSpell(spell), "Discard", SpellDisplayState.ChoosingDiscard, onComplete));
    }

    private void Awake()
    {
        _Instance = this;

        CreateCirclePool();
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
        enemyNameText.text = currentEnemy.Name;
        enemyBasicAttackDamageText.text = currentEnemy.GetBasicAttackDamage().ToString();
        characterBasicAttackDamageText.text = PlayerBasicAttackDamage.ToString();

        foreach (GameObject obj in showOnCombat)
        {
            obj.SetActive(true);
        }

        if (allowSpellSlection)
        {
            // Get Spells
            yield return StartCoroutine(GameManager._Instance.SelectSpellsForCombat(currentEnemy));
        }

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
            if (TargetHasAffliction(AfflictionType.Bandages, Target.Character))
            {
                int numBandagedStacks = characterAfflictionMap[AfflictionType.Bandages].GetStacks();
                GameManager._Instance.AlterPlayerCurrentHP(numBandagedStacks, DamageType.Heal);
                ConsumeAfflictionStack(AfflictionType.Bandages, Target.Character, numBandagedStacks);
                ShowAfflictionProc(AfflictionType.Bandages, Target.Character);
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
            yield return StartCoroutine(EnemyActOnAction(action));
        }

        StartCoroutine(UpdateDuringCombat());

        while (currentEnemyHP > 0 && GameManager._Instance.GetCurrentCharacterHP() > 0)
        {
            // Turn Begin
            // Set effectiveness multiplier text to be at position zero
            effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

            // Increment Turn Count
            turnNumber++;

            // Call OnTurnStart
            OnTurnStart?.Invoke();

            // Decide Enemy Intent
            currentEnemyAction = currentEnemy.GetEnemyIntent();
            enemyIntentDisplay.SetEnemyAction(currentEnemyAction);

            yield return StartCoroutine(DrawHand());

            // Player Turn
            yield return StartCoroutine(PlayerTurn());

            if (CheckForCombatOver())
            {
                break;
            }

            // Enemy Turn
            yield return StartCoroutine(EnemyTurn());

            if (CheckForCombatOver())
            {
                break;
            }

            // End of Turn
            yield return new WaitForSeconds(delayAfterEnemyTurn);

            // Reset for Turn
            GameManager._Instance.AlterPlayerCurrentMana(GameManager._Instance.GetManaPerTurn());
        }

        // Call On Combat End
        OnCombatEnd?.Invoke();
    }

    public void ReplaceCurrentEnemyAction(EnemyAction newAction)
    {
        currentEnemyAction = newAction;
        enemyIntentDisplay.ClearIntents();
        enemyIntentDisplay.SetEnemyAction(currentEnemyAction);
    }

    public void ReplaceCurrentEnemyAction()
    {
        currentEnemyAction = currentEnemy.GetEnemyIntent(new List<EnemyAction>() { currentEnemyAction });
        enemyIntentDisplay.ClearIntents();
        enemyIntentDisplay.SetEnemyAction(currentEnemyAction);
    }

    public EnemyAction GetCurrentEnemyAction()
    {
        return currentEnemyAction;
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
        yield return StartCoroutine(turnDisplay.Show("Player Turn", turnNumber > 1 ? turnNumber + Utils.GetNumericalSuffix(turnNumber) + " Turn" : ""));

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

        yield return StartCoroutine(DiscardHand());

        foreach (GameObject obj in disableWhileCasting)
        {
            obj.SetActive(false);
        }

        CanCastSpells = false;
        yield return StartCoroutine(CastCharacterQueue());

        foreach (GameObject obj in disableWhileCasting)
        {
            obj.SetActive(true);
        }

        if (CheckForCombatOver())
        {
            yield break;
        }

        ResetActiveSpellCooldowns();

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Target.Character))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Target.Character), Target.Character, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Target.Character);
        }

        // Reset effectiveness multiplier
        effectivenessMultiplier = defaultEffectivenessMultiplier;

        OnPlayerTurnEnd?.Invoke();

        Debug.Log("Player Turn Ended");
    }

    public void TickHandCooldowns(Spell exclude)
    {
        // Tick other active spell cooldowns
        foreach (ActiveSpell activeSpell in hand.GetEntriesMatching(spell => spell.Type == SpellCastType.Active))
        {
            if (activeSpell == exclude) continue;
            if (!activeSpell.OnCooldown) continue;
            activeSpell.AlterCooldown(-1);
        }
    }

    public void AddSpellToCastQueue(Spell spell)
    {
        // Spawn new display
        SpellPotencyDisplay spellPotencyDisplay = Instantiate(castingSpellPotencyDisplayPrefab, castingSpellSemiCircle.transform);
        spellPotencyDisplay.SetSpell(spell);

        QueuedSpell toQueue = new QueuedSpell(spell, spellPotencyDisplay, spellQueue.Count);
        toQueue.SpellQueueDisplay = spellPotencyDisplay;

        // Indicate the first spell to be cast
        if (spellQueue.Count == 0)
        {
            spellPotencyDisplay.SetOutlineColor(Color.red);
        }
        else
        {
            spellPotencyDisplay.SetOutlineColor(Color.black);
        }

        // Spawn casting queue potency display
        if (spell.Type == SpellCastType.Active)
        {
            // Set Potency
            spellPotencyDisplay.SetCurrentPotency(effectivenessMultiplier);
            spellPotencyDisplay.SetMaxPotency(maxEffectivenessMultiplier);

            // Activate Callback
            OnActiveSpellQueued?.Invoke();
        }
        else
        {
            // Set Potency
            spellPotencyDisplay.SetCurrentPotency(defaultEffectivenessMultiplier);
            spellPotencyDisplay.SetMaxPotency(maxEffectivenessMultiplier);

            OnPassiveSpellQueued?.Invoke();
        }

        // Check for Free Spell Casts
        if (NumFreeSpells > 0)
        {
            NumFreeSpells -= 1;
        }
        else
        {
            if (spell.Type == SpellCastType.Active)
            {
                // Set Cooldown
                ((ActiveSpell)spell).SetOnCooldown();
            }
            // Consume Mana
            GameManager._Instance.AlterPlayerCurrentMana(-spell.ManaCost);
        }

        spellQueue.Add(toQueue);

        spell.OnQueue();
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
            QueuedSpell spell = spellQueue[0];

            // Show next up spell cast
            if (spellQueue.Count > 0)
            {
                spellQueue[0].SpellQueueDisplay.SetOutlineColor(Color.red);
            }

            // Remove Spell from Queue
            spellQueue.RemoveAt(0);

            // Remove UI from spell queue & from Spell Potency Display
            Destroy(spell.SpellQueueDisplay.gameObject);

            // Cast Spell
            yield return StartCoroutine(CastSpell(spell));

            // Killed the enemy or died themselves, either way remove the rest of the queued spells
            if (CheckForCombatOver())
            {
                // Killed the Enemy
                if (currentEnemyHP <= 0)
                {
                    // Callback
                    spell.Spell.OnKill();
                }

                // Remove Spell Potency Displays and Queued Spell Displays as Combat is now Over
                while (spellQueue.Count > 0)
                {
                    QueuedSpell cur = spellQueue[0];
                    spellQueue.RemoveAt(0);

                    // Remove UI from Spell Potency Display
                    Destroy(cur.SpellQueueDisplay.gameObject);
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


    private IEnumerator CastSpell(QueuedSpell queuedSpell)
    {
        Spell spell = queuedSpell.Spell;

        // Set SFX source to spell audio clip
        spellSFXSource.clip = spell.AssociatedSoundClip;
        spellSFXSource.Play();

        if (spell.Type == SpellCastType.Passive)
        {
            spell.Cast();
            yield return new WaitForSeconds(delayAfterPassiveSpellCast);
        }
        else
        {
            ActiveSpell activeSpell = (ActiveSpell)queuedSpell.Spell;

            // Set hit & miss sounds
            hitSound = activeSpell.HitSound;
            missSound = activeSpell.MissSound;


            // Set effectiveness multiplier text to be at zero
            effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

            // Play Sequence
            yield return StartCoroutine(PlaySpell(activeSpell, queuedSpell.SpellQueueDisplay));
        }
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn Started");

        currentTurn = Turn.Enemy;

        // Reset Ward
        ResetCombatentWard(Target.Enemy);

        // Show Turn Display
        yield return StartCoroutine(turnDisplay.Show("Enemy Turn", ""));

        OnEnemyTurnStart?.Invoke();

        // Allow Enemy to Act on OnTurnStart Actions
        foreach (EnemyAction action in currentEnemy.GetOnTurnStartActions())
        {
            yield return StartCoroutine(EnemyActOnAction(action));
        }

        // Tick Relevant Afflictions
        ApplyBlightEffectOnMap(enemyAfflictionMap, Target.Enemy);
        ApplyPoisonEffectOnMap(enemyAfflictionMap, Target.Enemy);

        if (CheckForCombatOver())
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeEnemyAttack);

        yield return StartCoroutine(EnemyActOnAction(currentEnemyAction));

        // Clear Enemy Intent
        enemyIntentDisplay.ClearIntents();

        // Regeneration Effect
        if (TargetHasAffliction(AfflictionType.Regeneration, Target.Enemy))
        {
            AlterCombatentHP(GetTargetAfflictionStacks(AfflictionType.Regeneration, Target.Enemy), Target.Enemy, DamageType.Heal);
            ConsumeAfflictionStack(AfflictionType.Regeneration, Target.Enemy);
        }

        // Allow Enemy to Act on OnTurnEnd Actions
        foreach (EnemyAction action in currentEnemy.GetOnTurnEndActions())
        {
            yield return StartCoroutine(EnemyActOnAction(action));
        }

        OnEnemyTurnEnd?.Invoke();

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

            drawPileCountText.text = drawPile.Count.ToString();
            discardPileCountText.text = discardPile.Count.ToString();
            exhaustPileCountText.text = exhaustPile.Count.ToString();

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

    private void ResetCombat()
    {
        // Reset Turn Count
        turnNumber = 0;
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
        ResetActiveSpellCooldowns();

        // Reset Player Mana
        GameManager._Instance.SetCurrentPlayerMana(GameManager._Instance.GetMaxPlayerMana());

        // Reset HP Bars
        characterHPBar.Clear();
        enemyHPBar.Clear();

        enemyIntentDisplay.ClearIntents();

        // Reset
        effectivenessMultiplier = defaultEffectivenessMultiplier;
        effectivenessMultiplierText.text = "x" + Utils.RoundTo(effectivenessMultiplier, 2).ToString();

        // Unequip all Passive Spells
        foreach (PassiveSpell spell in passiveSpellPile.GetSpells())
        {
            spell.OnUnequip();

            PassiveSpellDisplay spellDisplay = (PassiveSpellDisplay)spell.GetEquippedTo();
            Destroy(spellDisplay.gameObject);
        }

        // Put all Spells back into draw Pile so that GameManager can know which Spells were previously used in Combat
        hand.TransferEntries(drawPile, false);
        discardPile.TransferEntries(drawPile, false);
        exhaustPile.TransferEntries(drawPile, false);
        passiveSpellPile.TransferEntries(drawPile, true);

        musicSource.Stop();
        musicSource.time = 0;

        // Clear Spell Queue
        while (spellQueue.Count > 0)
        {
            QueuedSpell spell = spellQueue[0];
            Destroy(spell.SpellQueueDisplay.gameObject);
            spellQueue.RemoveAt(0);
        }

        // Destroy Circles
        ClearCircleList();

        // Set effectiveness multiplier text to be at zero
        effectivenessMultiplierTextRect.anchoredPosition = Vector2.zero;

        // Callback
        OnResetCombat?.Invoke();
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

    private IEnumerator PlaySpell(ActiveSpell queueActiveSpell, SpellPotencyDisplay spellPotencyDisplay)
    {
        Coroutine updatePotencyDisplay = StartCoroutine(UpdateSpellPotencyDisplay(spellPotencyDisplay));
        float t = 0;

        // Foreach Batch in the Spell
        for (int i = 0; i < queueActiveSpell.Batches.Count; i++)
        {
            // Get Current Batch
            SpellNoteBatch currentBatch = queueActiveSpell.Batches[i];

            // Spawn Batch of Circles
            for (int p = 0; p < currentBatch.NumNotes; p++)
            {
                SpellNote currentNote = currentBatch.GetNote(p);
                Circle c = circlePool.Get();
                spawnedCircles.Add(c);
                c.Set(currentNote, UIManager._Instance.GetDamageTypeColor(queueActiveSpell.MainDamageType));

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

        // Stop update potency display coroutine
        StopCoroutine(updatePotencyDisplay);

        // Apply effects
        queueActiveSpell.Cast();
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
            if (setEffectivenessMultiplierToZeroOnMiss)
            {
                effectivenessMultiplier = 0;
            }
            else
            {
                effectivenessMultiplier -= decreaseEffectivenessMultiplierOnMiss;
            }
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

    private int preCorrectedPlayerBasicAttackDamage => GameManager._Instance.DamageFromEquipment + GameManager._Instance.GetBasicAttackDamage();
    public int PlayerBasicAttackDamage => preCorrectedPlayerBasicAttackDamage > 0 ? preCorrectedPlayerBasicAttackDamage : 1;

    // Basic Attack
    private void PlayerBasicAttack()
    {
        // the minimum a basic attack can do is 1
        // Attack the enemy
        AttackCombatent(PlayerBasicAttackDamage, Target.Enemy, Target.Character, DamageType.Default, DamageSource.BasicAttack);

        // Burn Effect
        ApplyBurnEffectOnMap(characterAfflictionMap, Target.Character);

        // Only call this if the combat isn't over
        if (!CheckForCombatOver())
        {
            OnPlayerBasicAttack?.Invoke();
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

        // the minimum a basic attack can do is manually set to 1
        if (attackDamage <= 0)
        {
            attackDamage = 1;
        }

        AttackCombatent(attackDamage, Target.Character, Target.Enemy, DamageType.Default, DamageSource.BasicAttack);

        // Burn Effect
        ApplyBurnEffectOnMap(enemyAfflictionMap, Target.Enemy);

        // Only call on enemy attack if the player is still alive
        if (!CheckForCombatOver())
        {
            OnEnemyBasicAttack?.Invoke();
        }
    }

    private IEnumerator EnemyActOnAction(EnemyAction enemyAction)
    {
        List<EnemyIntent> enemyIntents = enemyAction.GetEnemyIntents();

        bool hasAnimatedSprite = false;
        foreach (EnemyIntent intent in enemyIntents)
        {
            // Paralyze Effect
            if (TargetHasAffliction(AfflictionType.Paralyze, Target.Enemy))
            {
                ConsumeAfflictionStack(AfflictionType.Paralyze, Target.Enemy);
                ShowAfflictionProc(AfflictionType.Paralyze, Target.Enemy);
                ShakeCombatent(Target.Enemy);
                continue;
            }

            switch (intent)
            {
                case EnemySingleAttackIntent singleAttack:

                    if (singleAttack.AttackAnimationStyle != EnemyAttackAnimationStyle.None)
                    {
                        yield return StartCoroutine(AnimateEnemySpriteAttack());
                        hasAnimatedSprite = true;
                    }

                    AttackCombatent(singleAttack.DamageAmount, Target.Character, Target.Enemy, singleAttack.DamageType, DamageSource.EnemyAttack);

                    break;
                case EnemyMultiAttackIntent multiAttack:

                    for (int i = 0; i < multiAttack.NumAttacks; i++)
                    {
                        // Either animate every attack or only the first attack depending on what is set
                        if (multiAttack.AttackAnimationStyle == EnemyAttackAnimationStyle.PerAttack
                            || (multiAttack.AttackAnimationStyle == EnemyAttackAnimationStyle.Once && !hasAnimatedSprite))
                        {
                            yield return StartCoroutine(AnimateEnemySpriteAttack());
                            hasAnimatedSprite = true;
                        }

                        AttackCombatent(multiAttack.DamageAmount, Target.Character, Target.Enemy, multiAttack.DamageType, DamageSource.EnemyAttack);

                        yield return new WaitForSeconds(multiAttack.TimeBetweenAttacks);
                    }

                    break;
                case EnemyWardIntent ward:

                    GiveCombatentWard(ward.WardAmount, Target.Enemy);

                    break;
                case EnemyApplyAfflictionIntent apply:

                    if (!hasAnimatedSprite)
                    {
                        yield return StartCoroutine(AnimateEnemySpriteAttack());
                        hasAnimatedSprite = true;
                    }

                    AddAffliction(apply.AfflictionType, apply.NumStacks, Target.Character);

                    break;
                case EnemyGainAfflictionIntent gain:

                    AddAffliction(gain.AfflictionType, gain.NumStacks, Target.Enemy);

                    break;
                case EnemyHealIntent heal:

                    AlterCombatentHP(heal.HealAmount, Target.Enemy, DamageType.Heal);

                    break;
                case EnemyCleanseAfflictionsIntent cleanse:

                    List<Affliction> toRemove = new List<Affliction>();
                    Debug.Log("Cleanse");
                    foreach (Affliction affliction in GetTargetAfflictionMap(Target.Enemy).Values)
                    {
                        Debug.Log("On Aff: " + affliction);
                        if (cleanse.toCleanse.Contains(affliction.Sign))
                        {
                            toRemove.Add(affliction);
                        }
                    }

                    while (toRemove.Count > 0)
                    {
                        Affliction removingAff = toRemove[0];
                        toRemove.RemoveAt(0);
                        Debug.Log("Removing Aff: " + removingAff);
                        RemoveAffliction(Target.Enemy, removingAff.Type);
                    }
                    break;
                default:
                    throw new UnhandledSwitchCaseException();
            }
        }

        enemyAction.CallOnActivate();
    }

    public void AlterCombatentHP(int amount, Target target, DamageType damageType)
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

                CallDamageTypeAnimation(damageType, target);

                if (amount < 0)
                {
                    // Callback
                    OnPlayerTakeDamage?.Invoke(amount * -1);

                    ShakeCombatent(Target.Character);
                }

                // Finalize player HP damage
                GameManager._Instance.AlterPlayerCurrentHP(amount, damageType, false);
                characterHPBar.SetCurrentHP(GameManager._Instance.GetCurrentCharacterHP());

                break;
            case Target.Enemy:

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

                CallDamageTypeAnimation(damageType, target);

                // tried to heal past max
                if (currentEnemyHP + amount > maxEnemyHP)
                {
                    currentEnemyHP = maxEnemyHP;
                }
                else if (currentEnemyHP + amount < 0) // tried to damage past 0
                {
                    // Callback
                    OnEnemyTakeDamage?.Invoke(amount * -1);

                    ShakeCombatent(Target.Enemy);

                    currentEnemyHP = 0;
                }
                else if (amount < 0)
                {
                    // is Damage
                    // Apply amount
                    currentEnemyHP += amount;

                    // Callbacks
                    OnEnemyTakeDamage?.Invoke(amount * -1);

                    ShakeCombatent(Target.Enemy);
                }
                else
                {
                    // is Either Heal or Zero
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
        // Thorns Effect
        if (TargetHasAffliction(AfflictionType.Thorns, target))
        {
            int thornsDamage = GetTargetAfflictionMap(target)[AfflictionType.Thorns].GetStacks();
            ShowAfflictionProc(AfflictionType.Thorns, target);
            AlterCombatentHP(-thornsDamage, attacker, DamageType.Default);
        }

        // Attempted to Basic Attack for less than 0 (i.e., a Heal)
        if (damageSource == DamageSource.BasicAttack && amount < 0)
        {
            AlterCombatentHP(0, target, damageType);
            return;
        }

        int damage = CalculateDamage(amount, attacker, target, damageType, damageSource, true);

        // Poison Coated Effect
        if (TargetHasAffliction(AfflictionType.PoisonCoated, attacker) && damage > GetCombatentWard(target))
        {
            AddAffliction(AfflictionType.Poison, GetTargetAfflictionStacks(AfflictionType.PoisonCoated, attacker), target);
        }

        // Callback
        switch (attacker)
        {
            case Target.Character:
                OnPlayerAttack?.Invoke();
                break;
            case Target.Enemy:
                OnEnemyAttack?.Invoke();
                break;
        }

        AlterCombatentHP(-damage, target, damageType);
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
            // Make sure that negative Power values don't wind up making a damaging attack heal instead,
            // rather whatever action is trying to do that action is simply zeroed out
            if (amount < 0)
            {
                amount = 0;
            }
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
    public int CalculateWard(int amount, Target applyingTo)
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

    private int UseWard(int amount, Target target, Func<int> getFunc, Action<int> alterFunc)
    {
        // Levitating Effect
        switch (target)
        {
            case Target.Character:
                if (TargetHasAffliction(AfflictionType.Levitating, Target.Enemy))
                {
                    return 0;
                }
                break;
            case Target.Enemy:
                if (TargetHasAffliction(AfflictionType.Levitating, Target.Character))
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

    public void GiveCombatentWard(int wardAmount, Target target)
    {
        wardAmount = CalculateWard(wardAmount, target);
        CallDamageTypeAnimation(DamageType.Ward, target);
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

    public int GetCombatentWard(Target target)
    {
        switch (target)
        {
            case Target.Character:
                return characterWard;
            case Target.Enemy:
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

    private void CallDamageTypeAnimation(DamageType damageType, Target target)
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

    private IEnumerator DefaultDamageTypeAnimation(DamageType damageType, Target target)
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

    private IEnumerator WardDamageTypeAnimation(Target target)
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

    private bool SetAffliction(AfflictionType type, int numStacks, Target target)
    {
        Dictionary<AfflictionType, Affliction> map = GetTargetAfflictionMap(target);
        Transform parentTo = GetTargetParentAfflictionTo(target);

        bool isNewInstance;

        // Affliction Map already contains
        if (map.ContainsKey(type))
        {
            Affliction aff = map[type];
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
            Affliction aff = Affliction.GetAfflictionOfType(type);

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
                return false;
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
                return false;
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

    public Affliction GetTargetAffliction(AfflictionType type, Target target)
    {
        if (TargetHasAffliction(type, target))
        {
            return GetTargetAfflictionMap(target)[type];
        }
        return null;
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
    }

    public void RemoveAffliction(Target target, AfflictionType type)
    {
        Debug.Log("Removing: " + type + " From " + target);

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
        UpdateHPBarAfflictions(type, target);

        // Spawn Effect Text
        SpawnEffectText(EffectTextStyle.Fade, removingAff.GetToolTipLabel() + " Wears Off", UIManager._Instance.GetEffectTextColor("AfflictionRemoved"), target,
            UIManager._Instance.GetAfflictionIcon(removingAff.Type));

        // Callbacks
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
            int numStacks = map[AfflictionType.Blight].GetStacks();
            AlterCombatentHP(-numStacks, target, DamageType.Poison);

            int v = BalenceManager._Instance.GetValue(AfflictionType.Blight, "PercentToIncreaseBy");
            float percentToIncreaseBy = (float)v / 100;

            AddAffliction(AfflictionType.Blight, Mathf.CeilToInt(numStacks * percentToIncreaseBy), target);
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

    public void ClearRandomAffliction(Target t, Sign sign)
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

    public void SpawnEffectText(EffectTextStyle style, string text, Color c, Target owner, Sprite withIcon = null)
    {
        if (!combatScreenOpen) return;
        switch (owner)
        {
            case Target.Character:
                characterEffectTextDisplay.CallSpawnEffectText(style, text, c, withIcon);
                return;
            case Target.Enemy:
                enemyEffectTextDisplay.CallSpawnEffectText(style, text, c, withIcon);
                return;
        }
    }

    public void SpawnEffectIcon(EffectIconStyle style, Sprite sprite, Target owner)
    {
        if (!combatScreenOpen) return;
        switch (owner)
        {
            case Target.Character:
                characterEffectTextDisplay.CallSpawnEffectIcon(style, sprite);
                return;
            case Target.Enemy:
                enemyEffectTextDisplay.CallSpawnEffectIcon(style, sprite);
                return;
        }
    }

    public void ShakeCombatent(Target target)
    {
        RectTransform rect = GetTargetSpriteImage(target).transform as RectTransform;
        rect.DOShakeAnchorPos(shakeCombatentDuration, shakeCombatentStrength, shakeCombatentVibrato, shakeCombatentRandomness, false, true, ShakeRandomnessMode.Harmonic);
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

    #endregion
}
