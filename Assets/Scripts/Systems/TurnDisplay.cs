using System.Collections;
using UnityEngine;
using TMPro;

public class TurnDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private GameObject image;

    [SerializeField] private float scaleAnimationSpeed = 5;

    [SerializeField] private float stayDelay = 1;

    private Vector3 hiddenScale = new Vector3(0, .75f, 1);

    public IEnumerator Show(string turnText, string turnCountText)
    {
        this.turnText.text = turnText;

        if (turnCountText.Length > 0)
        {
            this.turnCountText.gameObject.SetActive(true);
            this.turnCountText.text = turnCountText;
        }
        else
        {
            this.turnCountText.gameObject.SetActive(false);
        }

        while (image.transform.localScale.x < 1)
        {
            image.transform.localScale = Vector3.MoveTowards(image.transform.localScale, Vector3.one, Time.deltaTime * scaleAnimationSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(stayDelay);

        while (image.transform.localScale.x > 0)
        {
            image.transform.localScale = Vector3.MoveTowards(image.transform.localScale, hiddenScale, Time.deltaTime * scaleAnimationSpeed);
            yield return null;
        }
    }
}
