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
    ChoosingDiscard
}

public abstract class SpellDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected Spell Spell { get; private set; }

    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected Image progressBar;
    [SerializeField] protected Image spellIcon;
    [SerializeField] private Image main;

    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;

    [SerializeField] private Vector2 toolTipOffset;
    protected GameObject spawnedToolTip;

    private float targetScale;
    protected bool scaleLocked;

    [SerializeField] protected CanvasGroup mainCV;
    [SerializeField] private float changeScaleSpeed = 1f;

    [SerializeField] private GameObject[] disableWhenEmpty;

    protected SpellDisplayState currentSpellDisplayState = SpellDisplayState.Normal;
    private Tweener shakeTweener;
    public bool IsEmpty { get; private set; }

    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private GameObject lockedContainer;

    private Action onClick;
    private Action onEnter;
    private Action onExit;
    protected bool isMouseOver;

    public void AddOnClick(Action a)
    {
        onClick += a;
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

    protected virtual void Update()
    {
        if ((currentSpellDisplayState == SpellDisplayState.Normal || currentSpellDisplayState == SpellDisplayState.Normal) && !scaleLocked)
        {
            // Allow target scale to fall back to regular scale
            if (targetScale != regularScale)
            {
                targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
            }

            // Set Scale
            main.transform.localScale = targetScale * Vector3.one;
        }
    }

    public void SetSpellDisplayState(SpellDisplayState displayState)
    {
        currentSpellDisplayState = displayState;

        mainCV.blocksRaycasts = displayState != SpellDisplayState.Locked;
        lockedContainer.SetActive(currentSpellDisplayState == SpellDisplayState.Locked);

        if (displayState == SpellDisplayState.Selected)
        {
            shakeTweener = transform.DOShakePosition(1, 3, 10, 90, false, false).SetLoops(-1);
        }
        else
        {
            if (displayState == SpellDisplayState.ChoosingDiscard || displayState == SpellDisplayState.ChoosingExhaust)
            {
                // 
            }
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
        SetEmpty(false);

        onEnter += CallSpawnToolTip;
        onExit += DestroyToolTip;
    }

    public virtual void Unset()
    {
        nameText.text = "";
        text.text = "";
        spellIcon.sprite = defaultSprite;
        progressBar.fillAmount = 1;
        SetEmpty(true);
        Spell = null;

        shakeTweener.Kill();

        onEnter -= CallSpawnToolTip;
        onExit -= DestroyToolTip;
    }

    public void SetEmpty(bool isEmpty)
    {
        IsEmpty = isEmpty;
        foreach (GameObject obj in disableWhenEmpty)
        {
            obj.SetActive(!isEmpty);
        }
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
        // Only spawn ToolTip if spell is set
        if (!IsEmpty)
        {
            SpawnToolTipFunc();
        }
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
