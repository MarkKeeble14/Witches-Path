using UnityEngine;
using UnityEngine.UI;

public class BookDisplay : ItemDisplay
{
    [SerializeField] private Transform pipDisplay;
    [SerializeField] private Image pip;
    [SerializeField] private Image mainImage;
    private Image[] pips;
    private Book repBook;

    public override void SetItem(PowerupItem i)
    {
        base.SetItem(i);

        // Set Rep Book
        repBook = (Book)i;

        // Set New Pips
        SetPips(repBook.MaxCharge);
    }

    private void SetPips(int numPips)
    {
        // Clear Old Pips
        if (pips != null)
        {
            foreach (Image oldPip in pips)
            {
                Destroy(oldPip.gameObject);
            }
        }

        // Set New Pips
        pips = new Image[numPips];
        for (int i = 0; i < numPips; i++)
        {
            pips[i] = Instantiate(pip, pipDisplay);
        }
    }

    private new void Update()
    {
        // Call base Update
        base.Update();

        // Update Color of Pips Depending on Books Current Charge
        for (int i = 0; i < pips.Length; i++)
        {
            pips[i].color = (i >= repBook.currentCharge ? Color.grey : Color.black);
        }

        // Set color of main book
        mainImage.color = (repBook.CanActivate ? Color.red : Color.white);

        // Use Book On Press Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            repBook.TryActivate();
        }
    }
}
