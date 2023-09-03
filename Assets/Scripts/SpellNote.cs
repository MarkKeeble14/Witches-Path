using System.Collections.Generic;
public enum ScreenQuadrant
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Any
}

public class SpellNote
{
    public float DelayAfter { get; private set; }
    public ScreenQuadrant ScreenQuadrant { get; private set; }
    public float ApproachRateMultiplier { get; private set; }
    public SpellNote(float delayAfter, ScreenQuadrant spawnInQuadrant = ScreenQuadrant.Any, float approachRateMultiplier = 1)
    {
        DelayAfter = delayAfter;
        ScreenQuadrant = spawnInQuadrant;
        ApproachRateMultiplier = approachRateMultiplier;
    }
}


public class SpellNoteBatch
{
    private List<SpellNote> spellNotes;

    public SpellNote GetNote(int index)
    {
        return spellNotes[index];
    }

    public int NumNotes => spellNotes.Count;
    public float DelayAfterBatch { get; private set; }

    public SpellNoteBatch(List<SpellNote> notes, float delayAfterBatch)
    {
        spellNotes = notes;
        DelayAfterBatch = delayAfterBatch;
    }

    public SpellNoteBatch(int numNotes, float delayBetweenNotes, float delayAfterBatch, float perNoteDisplayRate = 1)
    {
        spellNotes = new List<SpellNote>();
        for (int i = 0; i < numNotes; i++)
        {
            SpellNote note = new SpellNote(delayBetweenNotes, ScreenQuadrant.Any, perNoteDisplayRate);
            spellNotes.Add(note);
        }
        DelayAfterBatch = delayAfterBatch;
    }
}
