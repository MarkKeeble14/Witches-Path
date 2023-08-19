using UnityEngine;
using TMPro;

public class PotionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private TextMeshProUGUI potencyText;
    [SerializeField] private TextMeshProUGUI baseIngredientText;

    private Potion p;
    public void SetPotion(Potion p)
    {
        this.p = p;
    }

    private void Update()
    {
        targetText.text = p.HasTarget.ToString();
        potencyText.text = p.HasPotency.ToString();
        baseIngredientText.text = p.HasBaseIngredient.ToString();
    }
}
