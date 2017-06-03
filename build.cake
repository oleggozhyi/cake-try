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
        CleanDirectories(settings.Build.BinariesDir);
    });
Task("Nuget-RestorePackages")
    .Does(() => {
        NuGetRestore(File("./src/CakeTry.sln"), new NuGetRestoreSettings {  });
    });
Task("Build-Solution")
    .IsDependentOn("Clean-OutputDirs")
    .IsDependentOn("Nuget-RestorePackages")
    .Does(() =>{
        MSBuild("./src/CakeTry.sln", settings.Build.ToMSBuildSettings(Context));
    });
    
Task("Default")
    .IsDependentOn("Build-Solution");

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
});

RunTarget(settings.Target);