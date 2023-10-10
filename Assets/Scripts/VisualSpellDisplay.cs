﻿using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class VisualSpellDisplay : SpellDisplay, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Visual Settings")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;
    [SerializeField] private float distFromStartToCast = 50;
    [SerializeField] private float rotateSpeed = 5;
    [SerializeField] private DGDoShakeData onFailCast;
    private Vector3 dragStartPos;
    private bool isDragging;
    private KeyCode keyCode;

    [Header("References")]
    [SerializeField] private Image rarityImage;
    [SerializeField] private TextMeshProUGUI keyCodeText;

    [Header("Additional Info")]
    [SerializeField] private SemiUniversalSpellInfoContainer manaCostInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer cooldownInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer prepTimeInfo;
    [SerializeField] private SemiUniversalSpellInfoContainer numAttacksInfo;

    [Header("On Spell")]
    [SerializeField] private TextMeshProUGUI castTypeText;
    [SerializeField] private TextMeshProUGUI effectInfo;
    [SerializeField] private GameObject cooldownContainer;
    [SerializeField] private Image cooldownDisplay;
    [SerializeField] private TextMeshProUGUI cooldownContainerText;


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

        SpawnToolTipFunc();
    }

    public override void CallSpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    private void RotateTowardsZero()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Vector3.zero), rotateSpeed * Time.deltaTime);
    }

    public void SetKeyBinding(KeyCode keyCode)
    {
        this.keyCode = keyCode;
        keyCodeText.text = keyCode.ToString().Substring(keyCode.ToString().Length - 1, 1);
        keyCodeText.gameObject.SetActive(true);
    }

    public override void SetSpell(Spell spell)
    {
        base.SetSpell(spell);

        name = spell.Name + "(VisualSpellDisplay)";
        castTypeText.text = spell.SpellCastType.ToString();

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);
    }

    protected override void Update()
    {
        base.Update();

        // Set border Color
        border.color = UIManager._Instance.GetSpellDisplayBorderColor(currentSpellDisplayState);

        // Show Detail of Spells
        effectInfo.text = Spell.GetToolTipText();

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
    }

    private void TryActivate()
    {
        if (currentSpellDisplayState == SpellDisplayState.Fading) return;

        AudioManager._Instance.PlayFromSFXDict("Card_OnUse");

        // if is sitting in hand or is being dragged & will cast
        if (CombatManager._Instance.AwaitingAlterHandSequenceSelections)
        {
            CombatManager._Instance.ClickedSpellForAlterHandSequence(Spell);
        }
        else if ((currentSpellDisplayState == SpellDisplayState.InHand && !isDragging) || currentSpellDisplayState == SpellDisplayState.DraggingWillCast)
        {
            StartCoroutine(TryCast());
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
        {
            return;
        }

        base.OnPointerClick(eventData);

        TryActivate();
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

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        transform.position = eventData.position;

        float dist = dragStartPos.y - transform.position.y;

        RotateTowardsZero();

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
        CombatManager._Instance.HandLayoutGroup.InsertTransformToHand(transform, transform.GetSiblingIndex());
        SetSpellDisplayState(SpellDisplayState.InHand);
        isDragging = false;
    }
}
