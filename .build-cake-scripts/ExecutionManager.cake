#load "./Settings.cake"

public class ExecutionManager {
    private readonly ICakeContext _ctx;
    private readonly HashSet<string> _tasksToRun = new HashSet<string>();
    private readonly HashSet<string> _tasksToSkip = new HashSet<string>();

    private bool TasksToRunSpecified { get { return _tasksToRun.Any(); } }

    public ExecutionManager(ICakeContext ctx, TasksFilterSettings tasksFilterSettings) {
        _ctx = ctx;
        _tasksToRun = ReadParams(tasksFilterSettings.TasksToRun);
        _tasksToSkip = ReadParams(tasksFilterSettings.TasksToSkip);
    }

    public void RunOrSkip(ICakeTaskInfo task) {
        if(_tasksToSkip.Contains(task.Name) || (TasksToRunSpecified && !_tasksToRun.Contains(task.Name))) {
            Skip(task as ActionTask);
        }
    }

    private static HashSet<string> ReadParams(string p) {
         var hs = new HashSet<string>();
         p.Split(',')
            .Select(t => t.Trim())
            .Where(t => !String.IsNullOrEmpty(t))
            .ToList()
            .ForEach(t => hs.Add(t));
        return hs;
    }

    private void Skip(ActionTask task) {
         task.Actions.Clear();
         task.AddAction(_ => _ctx.Warning("-----------------SKIPPING TASK---------------------"));
    }
}