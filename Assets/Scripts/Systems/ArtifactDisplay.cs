public class ArtifactDisplay : ItemDisplay
{
    private Artifact repArtifact;
    public override void SetItem(PowerupItem i)
    {
        base.SetItem(i);
        repArtifact = (Artifact)i;
    }
}
