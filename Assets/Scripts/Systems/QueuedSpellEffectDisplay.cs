using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class QueuedSpellEffectDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    private CombatEffect repEffect;

    public void Set(CombatEffect effect)
    {
        switch (effect)
        {
            case ApplyAfflictionEffect applyAff:
                icon.sprite = UIManager._Instance.GetAfflictionIcon(applyAff.AfflictionType);
                break;
            default:
                icon.sprite = effect.Sprite;
                break;
        }
        repEffect = effect;
    }

    private void Update()
    {
        if (repEffect.NumText.Length > 0)
        {
            text.gameObject.SetActive(true);
            text.text = repEffect.NumText;
        }
        else
        {
            text.gameObject.SetActive(false);
        }
    }
}
