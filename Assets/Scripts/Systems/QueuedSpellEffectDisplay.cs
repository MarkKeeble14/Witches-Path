using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class QueuedSpellEffectDisplay : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    private SpellEffect repEffect;

    public void Set(SpellEffect effect)
    {
        switch (effect)
        {
            case SpellApplyAfflictionEffect applyAff:
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
