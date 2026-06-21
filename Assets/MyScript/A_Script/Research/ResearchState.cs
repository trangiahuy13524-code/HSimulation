

public class ResearchState
{
    public ResearchData data;
    public float progress;
    public bool completed;

    public ResearchState(ResearchData data)
    {
        this.data = data;
        progress = 0;
        completed = false;
    }
}