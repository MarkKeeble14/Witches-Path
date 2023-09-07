using UnityEngine;

public class DisableWhenUIOverlayOpen : MonoBehaviour
{
    [SerializeField] private CanvasGroup cv;

    private void Update()
    {
        bool hide = MapManager._Instance.MapOpen || GameManager._Instance.OverlaidUIOpen;
        cv.blocksRaycasts = !hide;
        cv.alpha = hide ? 0 : 1;
    }
}
