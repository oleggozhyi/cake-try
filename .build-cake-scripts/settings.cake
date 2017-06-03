public class Settings 
{
    public string Target = "Default";
    public string Configuration = "Release";
    public TasksFilterSettings TasksFilter = new TasksFilterSettings();
}

public class TasksFilterSettings
{
    public string TasksToRun = "";
    public string TasksToSkip = "";
}
