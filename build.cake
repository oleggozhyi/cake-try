#addin "Cake.Json"
#addin "Cake.FileHelpers"

#load "./.build-cake-scripts/Settings.cake"
#load "./.build-cake-scripts/ExecutionManager.cake"

var settings = SettingsManager.LoadSettings(Context);
var executionManager = new ExecutionManager(Context, settings.TasksFilter);

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
});

Task("Clean-OutputDirs")
    .Does(() => {
        CleanDirectories(settings.Output.CodeCoverageDir);
        CleanDirectories(settings.Output.BinariesDir);
    });
Task("Nuget-RestorePackages")
    .Does(() => {
        var solutions = GetFiles("./src/*.sln");
        NuGetRestore(solutions, new NuGetRestoreSettings {  });
    });
Task("Default")
    .IsDependentOn("Nuget-RestorePackages");

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
});

RunTarget(settings.Target);