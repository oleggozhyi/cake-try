#addin "Cake.Json"
#addin "Cake.FileHelpers"

var settingsFile = Argument<string>("settingsFile",  "./settings.local.json") ;
Settings settings;
string settingsJson;

if(settingsFile == null) {
    Information("No settings file passed, applying defaults");
    settings = new Settings();
    settingsJson = SerializeJson(settings);
} else {
    Information("Reading settings from " + settingsFile);
    settingsJson = FileReadText(settingsFile);    
    settings = DeserializeJson<Settings>(settingsJson);
}

var executionManager = new ExecutionManager(Context, settings.TasksFilter);

Information("=====BUILD SETTINGS==========================\n");
Information(settingsJson);
Information("=============================================");


Task("Task1")
    .Does(() => {
       Debug("<------ Task1 executing "+ SerializeJson(settings));

    });

Task("Task2")
    .IsDependentOn("Task1")
    .Does(() => {
        Debug("<------ Task2 executing");
    });
Task("Task3")
    .IsDependentOn("Task1")
    .Does(() => {
        Debug("<------ Task3 executing");
    });
Task("Task4")
    .IsDependentOn("Task2")
    .IsDependentOn("Task3")
    .Does(() => {
        Debug("<------ Task4 executing");
    });

Task("Default")
    .IsDependentOn("Task4");

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
});

RunTarget(settings.Target);

public class ExecutionManager
{
    private readonly ICakeContext _ctx;
    private readonly HashSet<string> _tasksToRun = new HashSet<string>();
    private readonly HashSet<string> _tasksToSkip = new HashSet<string>();

    private bool TasksToRunSpecified { get { return _tasksToRun.Any(); } }

    public ExecutionManager(ICakeContext ctx, TasksFilterSettings tasksFilterSettings) 
    {
        _ctx = ctx;
        _tasksToRun = ReadParams(tasksFilterSettings.TasksToRun);
        _tasksToSkip = ReadParams(tasksFilterSettings.TasksToSkip);
    }

    private static HashSet<string> ReadParams(string p)
    {
         var hs = new HashSet<string>();
         p.Split(',')
            .Select(t => t.Trim())
            .Where(t => !String.IsNullOrEmpty(t))
            .ToList()
            .ForEach(t => hs.Add(t));
        return hs;
    }

    public void RunOrSkip(ICakeTaskInfo task) 
    {
        if(_tasksToSkip.Contains(task.Name) 
            || (TasksToRunSpecified && !_tasksToRun.Contains(task.Name)))
        {
            Skip(task as ActionTask);
        }
    }
    private void Skip(ActionTask task)
    {
         task.Actions.Clear();
         task.AddAction(_ => _ctx.Warning("-----------------SKIPPING TASK---------------------"));
    }
}

