

public class ResearchState
{
    public JobDataResearch data;
    public float progress;
    public bool completed;

    public ResearchState(JobDataResearch data)
    {
        this.data = data;
        progress = 0;
        completed = false;
    }
}