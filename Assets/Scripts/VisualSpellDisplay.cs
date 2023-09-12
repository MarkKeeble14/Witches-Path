using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class VisualSpellDisplay : SpellDisplay, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Visual Spell Display")]
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;
    [SerializeField] private float distFromStartToCast = 50;
    [SerializeField] private float rotateSpeed = 5;
    private Vector3 dragStartPos;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI castTypeText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject cooldownContainer;
    [SerializeField] private Image cooldownDisplay;
    [SerializeField] private TextMeshProUGUI keyCodeText;
    [SerializeField] private Button button;

    [Header("Spell Dependant")]
    [SerializeField] private Image rarityImage;
    private bool isDragging;

    private KeyCode keyCode;

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

    private bool IsDisplayStateInHand(SpellDisplayState displayState)
    {
        return displayState == SpellDisplayState.ChoosingDiscard || displayState == SpellDisplayState.ChoosingExhaust || displayState == SpellDisplayState.InHand;
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

        // Set Rarity Image Color
        rarityImage.color = UIManager._Instance.GetRarityColor(spell.Rarity);

        switch (spell)
        {
            case ReusableSpell activeSpell:
                castTypeText.text = "Active";
                break;
            case PowerSpell powerSpell:
                castTypeText.text = "Power";
                break;
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (currentSpellDisplayState == SpellDisplayState.InHand && !isDragging)
        {
            TryCast();
        }
        else if (currentSpellDisplayState == SpellDisplayState.ChoosingDiscard || currentSpellDisplayState == SpellDisplayState.ChoosingExhaust)
        {
            CombatManager._Instance.ClickedSpellForAlterHandSequence(Spell);
        }
    }

    protected override void Update()
    {
        base.Update();

        // Set border Color
        border.color = UIManager._Instance.GetSpellDisplayBorderColor(currentSpellDisplayState);

        //
        manaCostText.text = Spell.ManaCost.ToString();

        // Show Detail of Spells
        text.text = Spell.GetToolTipText();

        if (currentSpellDisplayState == SpellDisplayState.InHand)
        {
            // Activate On KeyBindPressed
            if (Input.GetKeyDown(keyCode))
            {
                TryCast();
            }
        }

        if (IsDisplayStateInHand(currentSpellDisplayState))
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


    public void TryCast()
    {
        // Only allow for spell casts while in combat
        if (CombatManager._Instance.CanCastSpells)
        {
            if (!Spell.CanCast)
            {
                if (!Spell.HasMana)
                {
                    GameManager._Instance.PopManaText();
                }
                return;
            }

            // Tick Cooldowns
            CombatManager._Instance.TickHandCooldowns(Spell);

            CombatManager._Instance.AddSpellToCastQueue(Spell, Combatent.Character, Combatent.Enemy);

            if (Spell.SpellCastType == SpellCastType.Power)
            {
                // Automatically remove Power Spells from Hand when Played
                CombatManager._Instance.AddSpellToPowerSpellPile((PowerSpell)Spell);
            }
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
                cooldownText.text = "Cooldown:\n" + Utils.RoundTo(activeSpell.CurrentCooldown, 0).ToString();
            }
            else
            {
                cooldownContainer.SetActive(false);
                progressBar.fillAmount = 1;
            }
        }

        manaCostText.text = Spell.ManaCost.ToString();
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
        if (!IsDisplayStateInHand(currentSpellDisplayState)) return;

        isDragging = true;
        CombatManager._Instance.HandLayoutGroup.RemoveTransformFromHand(transform);
        SetSpellDisplayState(SpellDisplayState.DraggingWontCast);

        DestroyToolTip();

        dragStartPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // Check if should play
        if (currentSpellDisplayState == SpellDisplayState.DraggingWillCast)
        {
            TryCast();
        }

        if (currentSpellDisplayState == SpellDisplayState.Fading) return;

        // Reset
        CombatManager._Instance.HandLayoutGroup.InsertTransformToHand(transform, transform.GetSiblingIndex());
        SetSpellDisplayState(SpellDisplayState.InHand);
        isDragging = false;
    }
}
