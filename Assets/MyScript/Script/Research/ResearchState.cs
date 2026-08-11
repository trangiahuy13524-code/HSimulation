

public class ResearchState
{
    public DataJobResearch data;
    public float progress;
    public bool completed;

    public ResearchState(DataJobResearch data)
    {
        this.data = data;
        progress = 0;
        completed = false;
    }
}