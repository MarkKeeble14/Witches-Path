using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    // Circle parameters
    private float posX = 0;
    private float posY = 0;
    private float posZ = 0;

    [HideInInspector]
    public int posA = 0;

    private Color mainColor, mainColor1, mainColor2; // Circle sprites color
    [SerializeField] private GameObject mainFore, mainBack, mainApproach; // Circle objects
    [SerializeField] private SpriteRenderer fore, back, appr; // Circle sprites

    // Checking stuff
    private bool canRemove = false;
    private bool hasBeenClicked = false;

    // Set circle configuration
    public void Set(float x, float y, float z, int a)
    {
        posX = x;
        posY = y;
        posZ = z;
        posA = a;
        mainColor = appr.color;
        mainColor1 = fore.color;
        mainColor2 = back.color;
    }

    // Spawning the circle
    public void Show()
    {
        gameObject.transform.position = new Vector3(posX, posY, posZ);
        enabled = true;
        StartCoroutine(Checker());
    }

    // If circle wasn't clicked
    public void Remove()
    {
        if (!hasBeenClicked)
        {
            canRemove = true;
            enabled = true;
        }
    }

    // If circle was clicked
    public void Got()
    {
        if (!canRemove)
        {
            hasBeenClicked = true;
            mainApproach.transform.position = new Vector2(-101, -101);

            CombatManager._Instance.OnNoteHit();
            canRemove = false;
            enabled = true;
        }
    }

    // Check if circle wasn't clicked
    private IEnumerator Checker()
    {
        while (true)
        {
            // 75 means delay before removing
            if (CombatManager._Instance.GetTimer() >= posA + (CombatManager._Instance.GetApprRate() + 75) && !hasBeenClicked)
            {
                Remove();
                CombatManager._Instance.OnNoteMiss();
                break;
            }
            yield return null;
        }
    }

    // Main Update
    private void Update()
    {
        // Approach Circle modifier
        if (mainApproach.transform.localScale.x >= 0.9f)
        {
            mainApproach.transform.localScale -= new Vector3(5.166667f, 5.166667f, 0f) * Time.deltaTime;
            mainColor.a += 4f * Time.deltaTime;
            mainColor1.a += 4f * Time.deltaTime;
            mainColor2.a += 4f * Time.deltaTime;
            fore.color = mainColor1;
            back.color = mainColor2;
            appr.color = mainColor;

        }
        // If circle wasn't clicked
        else if (!hasBeenClicked)
        {
            // Remove circle
            if (!canRemove)
            {
                mainApproach.transform.position = new Vector2(-101, -101);
                enabled = false;
            }
            // If circle wasn't clicked
            else
            {
                mainColor1.a -= 10f * Time.deltaTime;
                mainColor2.a -= 10f * Time.deltaTime;
                mainFore.transform.localPosition += (Vector3.down * 2) * Time.deltaTime;
                mainBack.transform.localPosition += Vector3.down * Time.deltaTime;
                fore.color = mainColor1;
                back.color = mainColor2;
                if (mainColor1.a <= 0f)
                {
                    gameObject.transform.position = new Vector2(-101, -101);
                    enabled = false;
                }
            }
        }

        // If circle was clicked
        if (hasBeenClicked)
        {
            mainColor1.a -= 10f * Time.deltaTime;
            mainColor2.a -= 10f * Time.deltaTime;
            mainFore.transform.localScale += new Vector3(2, 2, 0) * Time.deltaTime;
            mainBack.transform.localScale += new Vector3(2, 2, 0) * Time.deltaTime;
            fore.color = mainColor1;
            back.color = mainColor2;
            if (mainColor1.a <= 0f)
            {
                gameObject.transform.position = new Vector2(-101, -101);
                enabled = false;
            }
        }
    }

    public SpriteRenderer GetFore()
    {
        return fore;
    }

    public SpriteRenderer GetBack()
    {
        return back;
    }

    public SpriteRenderer GetAppr()
    {
        return appr;
    }
}