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
    ChoosingExhaust,
    ChoosingDiscard,
    InHand,
    ToolTip
}

public abstract class SpellDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected Spell Spell { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject[] disableWhenEmpty;
    [SerializeField] protected CanvasGroup mainCV;
    [SerializeField] protected Image spellIcon;
    [SerializeField] private Transform toScale;
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected Image progressBar;
    [SerializeField] private GameObject lockedContainer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Canvas canvas;

    [Header("Color Info")]
    [SerializeField] private Image[] setColorOf;
    [SerializeField] private TextMeshProUGUI[] coloredTexts;
    private float targetScale;
    protected bool scaleLocked;
    protected SpellDisplayState currentSpellDisplayState = SpellDisplayState.Normal;

    [Header("Scale")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float changeScaleSpeed = 1f;
    [SerializeField] private float mouseOverTargetScale = 1.25f;
    [SerializeField] private float lerpScaleRate = 25;

    [SerializeField] private SerializableDictionary<SpellDisplayState, Vector2Int> sortingOrderDict = new SerializableDictionary<SpellDisplayState, Vector2Int>();

    protected GameObject spawnedToolTip;

    private Tweener shakeTweener;
    private Action onClick;
    private Action onEnter;
    private Action onExit;
    protected bool isMouseOver;

    public void AddOnClick(Action a)
    {
        onClick += a;
    }

    protected void RemoveOnClick(Action a)
    {
        onClick -= a;
    }

    public void AddOnEnter(Action a)
    {
        onEnter += a;
    }

    public void AddOnExit(Action a)
    {
        onExit += a;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        onEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        onExit?.Invoke();
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

    bool isInHand => currentSpellDisplayState == SpellDisplayState.ChoosingDiscard || currentSpellDisplayState == SpellDisplayState.ChoosingExhaust || currentSpellDisplayState == SpellDisplayState.InHand;

    protected virtual void Update()
    {
        bool hide = (MapManager._Instance.MapOpen && isInHand) || (MapManager._Instance.MapOpen && currentSpellDisplayState == SpellDisplayState.Locked);
        mainCV.blocksRaycasts = !hide;
        mainCV.alpha = hide ? 0 : 1;

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
                canvas.sortingOrder = sortingOrderDict[currentSpellDisplayState].x;

                if (targetScale != regularScale)
                {
                    // Allow target scale to fall back to regular scale
                    targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
                }
            }

            // Set Scale
            toScale.transform.localScale = targetScale * Vector3.one;
        }
    }

    public virtual void SetSpellDisplayState(SpellDisplayState displayState)
    {
        currentSpellDisplayState = displayState;

        lockedContainer.SetActive(currentSpellDisplayState == SpellDisplayState.Locked);

        if (displayState == SpellDisplayState.Selected)
        {
            shakeTweener = transform.DOShakePosition(1, 3, 10, 90, false, false).SetLoops(-1);
        }
        else
        {
            shakeTweener.Kill();
        }
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public virtual void SetSpell(Spell spell)
    {
        Spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Name;
        spell.SetEquippedTo(this);

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

        // This must be done after Setting the Color of the rest of the Text
        // Debug.Log(spell.Name + " - Has Been Upgraded: " + spell.HasBeenUpgraded);
        if (spell.HasBeenUpgraded)
        {
            nameText.color = UIManager._Instance.GetEffectTextColor("UpgradedSpell");
        }

        onEnter += CallSpawnToolTip;
        onExit += DestroyToolTip;
    }

    public void SetTargetScale(float v)
    {
        targetScale = v;
    }

    public void SetScaleLocked(bool b)
    {
        scaleLocked = b;
    }

    public virtual void CallSpawnToolTip()
    {
        if (currentSpellDisplayState == SpellDisplayState.ToolTip) return;
        SpawnToolTipFunc();
    }

    protected virtual void SpawnToolTipFunc()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(Spell, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
