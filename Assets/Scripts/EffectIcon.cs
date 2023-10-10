using UnityEngine;
using UnityEngine.UI;

public class EffectIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Animator anim;

    public void SetSprite(Sprite s)
    {
        image.sprite = s;
    }

    public void SetAnimation(string triggerString)
    {
        anim.SetTrigger(triggerString);
    }
}