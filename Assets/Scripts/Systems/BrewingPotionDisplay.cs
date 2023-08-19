using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class BrewingPotionDisplay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TextMeshProUGUI brewButtonTextMesh;
    [SerializeField] private string brewButtonText = "Brew";
    [SerializeField] private string brewButtonReadyEffectTextTag = "shake";

    [Header("References")]
    [SerializeField] private BrewingPotionDisplaySegment baseIngredientDisplaySegment;
    [SerializeField] private BrewingPotionDisplaySegment targeterIngredientDisplaySegment;
    [SerializeField] private BrewingPotionDisplaySegment potencyIngredientDisplaySegment;
    private Potion p;
    private bool potionWasReadyForBrew;

    [SerializeField] private GameObject clearBaseButton;
    [SerializeField] private GameObject clearTargeterButton;
    [SerializeField] private GameObject clearPotencyButton;
    [SerializeField] private Transform brewButton;
    private Image brewButtonImage;

    private void Awake()
    {
        brewButtonImage = brewButton.GetComponent<Image>();
    }

    private GameObject spawnedToolTip;

    public void SetPotion(Potion p)
    {
        this.p = p;
    }

    public void RemoveBase()
    {
        GameManager._Instance.AddPotionIngredient(p.CurPotionBaseIngredient.Type);
        p.ClearPotionBase();
    }

    public void RemoveTargeter()
    {
        GameManager._Instance.AddPotionIngredient(p.CurPotionTargeterIngredient.Type);
        p.ClearPotionTargeter();
    }

    public void RemovePotency()
    {
        GameManager._Instance.AddPotionIngredient(p.CurPotionPotencyIngredient.Type);
        p.ClearPotionPotency();
    }

    public void SpawnToolTip()
    {
        if (p.ReadyForBrew)
        {
            Potion potionCopy = new Potion();
            potionCopy.AddIngredient(p.CurPotionBaseIngredient);
            potionCopy.AddIngredient(p.CurPotionTargeterIngredient);
            potionCopy.AddIngredient(p.CurPotionPotencyIngredient);
            potionCopy.Brew();

            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(potionCopy, brewButton);
        }
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    private List<Tween> brewButtonTweens = new List<Tween>();

    private void Update()
    {
        // Set button text depending on if ready or not
        if (p.ReadyForBrew && !potionWasReadyForBrew)
        {
            potionWasReadyForBrew = true;
            brewButtonTweens.Add(brewButtonImage.DOColor(Color.magenta, 1));
            brewButtonTweens.Add(brewButton.DOShakePosition(1, 3, 10, 90, false, true).SetLoops(-1));
        }
        else if (!p.ReadyForBrew && potionWasReadyForBrew)
        {
            potionWasReadyForBrew = false;

            // Kill Tweens
            while (brewButtonTweens.Count > 0)
            {
                Tween t = brewButtonTweens[0];
                brewButtonTweens.RemoveAt(0);
                t.Kill();
            }
            brewButtonImage.DOColor(Color.white, 1);

            DestroyToolTip();
        }

        // Show/Hide clear Buttons
        clearBaseButton.SetActive(p.CurPotionBaseIngredient != null);
        clearTargeterButton.SetActive(p.CurPotionTargeterIngredient != null);
        clearPotencyButton.SetActive(p.CurPotionPotencyIngredient != null);

        baseIngredientDisplaySegment.SetRepresenting(p.CurPotionBaseIngredient);
        targeterIngredientDisplaySegment.SetRepresenting(p.CurPotionTargeterIngredient);
        potencyIngredientDisplaySegment.SetRepresenting(p.CurPotionPotencyIngredient);
    }
}
