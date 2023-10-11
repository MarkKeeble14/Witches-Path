using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.EventSystems;

public enum SpellDisplayState
{
    Normal,
    Selected,
    Locked,
    InHand,
    ToolTip,
    DraggingWontCast,
    DraggingWillCast,
    Fading,
    ShopOffer,
    Waiting
}

public class SpellDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    protected Spell Spell { get; private set; }


    [Header("Card Settings")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;
    [SerializeField] private float distFromStartToCast = 50;
    [SerializeField] private float rotateSpeedInHand = 5;
    [SerializeField] private float rotateSpeedOnMoveToMiddle = 50;
    [SerializeField] private float moveToCenterSpeed = 500;
    [SerializeField] private float moveUpFromHandSpeed = 1000;
    [SerializeField] private float delayAfterMovingToPosition = .5f;
    [SerializeField] private float upDist = 10;
    [SerializeField] private SerializableDictionary<SpellDisplayState, Vector2Int> sortingOrderDict = new SerializableDictionary<SpellDisplayState, Vector2Int>();
    [SerializeField] private DGDoShakeData onFailCast;
    private Vector3 dragStartPos;
    private bool isDragging;
    private KeyCode keyCode;

    [Header("Color Info")]
    [SerializeField] private Image[] setColorOf;
    [SerializeField] private TextMeshProUGUI[] coloredTexts;
    private float targetScale;
    protected bool scaleLocked;
    protected bool cvLocked;
    protected SpellDisplayState currentSpellDisplayState = SpellDisplayState.Normal;

    [Header("Scale Settings")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float changeScaleSpeed = 1f;
    [SerializeField] private float mouseOverTargetScale = 1.25f;
    [SerializeField] private float lerpScaleRate = 25;

    [Header("References")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private Transform toScale;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private TextMeshProUGUI keyCodeText;
    [SerializeField] private TextMeshProUGUI castTypeText;
    [SerializeField] private TextMeshProUGUI effectInfo;
    [SerializeField] private TextMeshProUGUI cooldownContainerText;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image border;
    [SerializeField] private Image rarityImage;
    [SerializeField] private Image cooldownDisplay;
    [SerializeField] private Image spellIcon;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject cooldownContainer;
    [SerializeField] private GameObject[] disableWhenEmpty;
    [SerializeField] private GameObject lockedContainer;
    [SerializeField] private CanvasGroup mainCV;

    [Header("Additional Info")]
    [SerializeField] private SemiUniversalSpellInfoContainer manaCostInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer cooldownInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer prepTimeInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer numAttacksInfo;
    public bool IsInHand => currentSpellDisplayState == SpellDisplayState.InHand;

    private GameObject spawnedToolTip;
    private bool waitingToCast;

    private Tweener shakeTweener;
    private bool isMouseOver;

    Action onClick;
    Action onEnter;
    Action onExit;

    public void AddOnClick(Action a)
    {
        onClick += a;
    }

    public void RemoveOnClick(Action a)
    {
        onClick -= a;
    }

    public void AddOnEnter(Action a)
    {
        onEnter += a;
    }

    public void RemoveOnEnter(Action a)
    {
        onEnter -= a;
    }

    public void AddOnExit(Action a)
    {
        onExit += a;
    }

    public void RemoveOnExit(Action a)
    {
        onExit -= a;
    }

    private void Start()
    {
        targetScale = regularScale;
    }

    private void OnDestroy()
    {
        if (shakeTweener == null) return;
        shakeTweener.Kill();
    }

    private void Update()
    {
        // Set border Color
        border.color = UIManager._Instance.GetSpellDisplayBorderColor(currentSpellDisplayState);

        // Show Detail of Spells
        // effectInfo.text = Spell.GetToolTipText();

        prepTimeInfo.Text.text = GetPrepTime().ToString();
        numAttacksInfo.Text.text = GetNumNotes().ToString();
        manaCostInfo.Text.text = GetManaCost().ToString();
        if (Spell is ReusableSpell)
        {
            cooldownInfo.Text.text = GetCooldown().ToString();
        }

        // Activate On KeyBindPressed
        if (Input.GetKeyDown(keyCode))
        {
            TryActivate();
        }

        // Update
        if (IsInHand)
        {
            InHandUpdate();
        }
        else if (currentSpellDisplayState == SpellDisplayState.DraggingWontCast)
        {
            // 
        }
        else if (currentSpellDisplayState == SpellDisplayState.DraggingWillCast)
        {
            //
        }
        else
        {
            NotInhandUpdate();
        }

        if (!cvLocked)
        {
            bool hide = (MapManager._Instance.MapOpen || GameManager._Instance.OverlaidUIOpen || CombatManager._Instance.SpellPileScreenOpen) && IsInHand;
            mainCV.blocksRaycasts = !hide;
            mainCV.alpha = hide ? 0 : 1;
        }

        if (currentSpellDisplayState == SpellDisplayState.ToolTip)
        {
            canvas.sortingOrder = sortingOrderDict[currentSpellDisplayState].y;
        }

        if (!scaleLocked)
        {
            // Animate Scale
            if (isMouseOver)
            {
                canvas.sortingOrder = sortingOrderDict[currentSpellDisplayState].y;

                targetScale = Mathf.Lerp(targetScale, mouseOverTargetScale, Time.deltaTime * lerpScaleRate);
            }
            else
            {
                if (targetScale != regularScale)
                {
                    // Allow target scale to fall back to regular scale
                    targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
                }
                else
                {
                    canvas.sortingOrder = sortingOrderDict[currentSpellDisplayState].x;
                }
            }

            // Set Scale
            toScale.transform.localScale = targetScale * Vector3.one;
        }
    }

    public void SetSpell(Spell spell)
    {
        Spell = spell;
        spell.SetEquippedTo(this);

        nameText.text = spell.Name;
        name = spell.Name + "(SpellDisplay)";
        spellIcon.sprite = spell.GetSpellSprite();

        effectInfo.text = Spell.GetToolTipText();
        castTypeText.text = spell.SpellCastType.ToString();

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);

        // Set Card Color
        SpellColorInfo colorInfo = UIManager._Instance.GetSpellColor(spell.Color);
        foreach (Image i in setColorOf)
        {
            i.color = colorInfo.Color;
        }
        foreach (TextMeshProUGUI text in coloredTexts)
        {
            text.color = colorInfo.TextColor;
        }
    }

    public void SetSpellDisplayState(SpellDisplayState displayState)
    {
        currentSpellDisplayState = displayState;

        lockedContainer.SetActive(currentSpellDisplayState == SpellDisplayState.Locked || currentSpellDisplayState == SpellDisplayState.Waiting);

        if (displayState == SpellDisplayState.Selected)
        {
            shakeTweener = transform.DOShakePosition(1, 3, 10, 90, false, false).SetLoops(-1);
        }
        else
        {
            shakeTweener.Kill();
        }
    }

    private void TryActivate()
    {
        if (currentSpellDisplayState == SpellDisplayState.Fading || currentSpellDisplayState == SpellDisplayState.Waiting) return;

        AudioManager._Instance.PlayFromSFXDict("Card_OnUse");

        if (CombatManager._Instance.AwaitingAlterHandSequenceSelections)
        {
            CombatManager._Instance.ClickedSpellForAlterHandSequence(Spell);
        }
        else if (currentSpellDisplayState == SpellDisplayState.InHand && !isDragging)
        {
            // if is sitting in hand
            StartCoroutine(TryCast());
        }
        else if (currentSpellDisplayState == SpellDisplayState.DraggingWillCast)
        {
            // if is being dragged & will cast
            StartCoroutine(TryCast());
        }
        else if (currentSpellDisplayState == SpellDisplayState.DraggingWontCast)
        {
            ReturnToHand();
        }
    }

    public void ReturnToHand()
    {
        CombatManager._Instance.HandLayoutGroup.InsertTransformToHand(transform, transform.GetSiblingIndex());
        SetSpellDisplayState(SpellDisplayState.InHand);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            return;
        }

        onClick?.Invoke();

        TryActivate();
    }

    private IEnumerator WaitToBeAdded()
    {
        waitingToCast = true;
        CombatManager._Instance.QueueWaitingSpellDisplay(this);

        Vector3 targetPos;
        if (currentSpellDisplayState == SpellDisplayState.InHand)
        {
            // Tell Card to Not be in Hand
            CombatManager._Instance.HandLayoutGroup.RemoveTransformFromHand(transform);

            // Move slightly up
            targetPos = transform.localPosition + transform.up * upDist;
            while (transform.localPosition != targetPos)
            {
                // Set Position
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, Time.deltaTime * moveUpFromHandSpeed);

                yield return null;
            }
        }
        SetSpellDisplayState(SpellDisplayState.Waiting);

        // Wait until this visual spell display is no longer waiting it's turn to move
        yield return new WaitUntil(() => !waitingToCast);

        // Move to middle
        targetPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        while (transform.localEulerAngles != Vector3.zero || transform.position != targetPos)
        {
            // Set Rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.zero), rotateSpeedOnMoveToMiddle * Time.deltaTime);

            // Set Position
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveToCenterSpeed);

            yield return null;
        }

        yield return new WaitForSeconds(delayAfterMovingToPosition);
    }

    private IEnumerator TryCast()
    {
        // Only allow for spell casts while in combat
        if (CombatManager._Instance.CanCastSpells)
        {
            if (!Spell.CanCast)
            {
                CombatManager._Instance.HandLayoutGroup.RemoveTransformFromHand(transform);
                onFailCast.DoShakeAnchorPos(rect);
                CombatManager._Instance.HandLayoutGroup.InsertTransformToHand(transform, transform.GetSiblingIndex());

                AudioManager._Instance.PlayFromSFXDict("Card_FailCast");

                yield break;
            }

            yield return StartCoroutine(WaitToBeAdded());

            yield return StartCoroutine(CombatManager._Instance.AddSpellToCastQueue(Spell, Combatent.Character, Combatent.Enemy, true, true));

            // Tick Cooldowns
            CombatManager._Instance.TickHandCooldowns(Spell);

            // Handle Spell Cast (Discard, Exhaust, Etc)
            yield return StartCoroutine(CombatManager._Instance.HandleSpellCast(Spell));
        }
    }

    private void InHandUpdate()
    {
        if (Spell.SpellCastType == SpellCastType.Reusable)
        {
            ReusableSpell activeSpell = (ReusableSpell)Spell;
            if (activeSpell.CurrentCooldown > 0)
            {
                cooldownContainer.SetActive(true);
                cooldownDisplay.fillAmount = ((float)activeSpell.CurrentCooldown / activeSpell.MaxCooldown);
                progressBar.fillAmount = 1 - ((float)activeSpell.CurrentCooldown / activeSpell.MaxCooldown);
                cooldownContainerText.text = "Cooldown:\n" + Utils.RoundTo(activeSpell.CurrentCooldown, 0).ToString();
            }
            else
            {
                cooldownContainer.SetActive(false);
                progressBar.fillAmount = 1;
            }
        }
    }

    private void NotInhandUpdate()
    {
        // 
    }

    private void RotateTowardsZero()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.zero), rotateSpeedInHand * Time.deltaTime);
    }

    public void SetKeyBinding(KeyCode keyCode)
    {
        this.keyCode = keyCode;
        keyCodeText.text = keyCode.ToString().Substring(keyCode.ToString().Length - 1, 1);
        keyCodeText.gameObject.SetActive(true);
    }

    public void SetTargetScale(float v)
    {
        targetScale = v;
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void SetScaleLocked(bool b)
    {
        scaleLocked = b;
    }

    public void SetCVLocked(bool b)
    {
        cvLocked = b;
    }

    public virtual void CallSpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTips)
        {
            if (!isMouseOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }
        // Disallow tool tip under certain conditions
        if (!isMouseOver || currentSpellDisplayState == SpellDisplayState.DraggingWillCast || currentSpellDisplayState == SpellDisplayState.DraggingWontCast
            || currentSpellDisplayState == SpellDisplayState.Fading)
        {
            yield break;
        }

        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(Spell, transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        transform.position = eventData.position;

        RotateTowardsZero();

        float dist = dragStartPos.y - transform.position.y;
        if (Mathf.Abs(dist) > distFromStartToCast && dist < 0)
        {
            SetSpellDisplayState(SpellDisplayState.DraggingWillCast);
        }
        else
        {
            SetSpellDisplayState(SpellDisplayState.DraggingWontCast);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsInHand) return;

        isDragging = true;
        CombatManager._Instance.HandLayoutGroup.RemoveTransformFromHand(transform);
        SetSpellDisplayState(SpellDisplayState.DraggingWontCast);

        DestroyToolTip();

        dragStartPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        TryActivate();

        // Reset
        isDragging = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;

        CallSpawnToolTip();

        AudioManager._Instance.PlayFromSFXDict("Card_OnHover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;

        DestroyToolTip();
    }

    public CanvasGroup GetCanvasGroup()
    {
        return mainCV;
    }

    public void SetWaiting(bool b)
    {
        waitingToCast = b;
    }

    public int GetNumNotes()
    {
        return Spell.GetNumNotes();
    }

    public int GetCooldown()
    {
        return ((ReusableSpell)Spell).MaxCooldown;
    }

    public int GetPrepTime()
    {
        return Spell.PrepTime;
    }

    public int GetManaCost()
    {
        return Spell.ManaCost;
    }
}
