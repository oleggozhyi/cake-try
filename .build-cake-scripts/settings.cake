public static class SettingsManager {
    public static Settings LoadSettings(ICakeContext context) {
        var settingsFile = context.Argument<string>("settingsFile",  "./settings.local.json") ;
        Settings settings;

        if(settingsFile == null) {
            context.Information("No settings file passed, applying defaults");
            settings = new Settings();
        } else {
            context.Information("Reading settings from " + settingsFile);
            var settingsJson = context.FileReadText(settingsFile);    
            settings = context.DeserializeJson<Settings>(settingsJson);
        }

        context.Information("=====BUILD SETTINGS==========================\n");
        context.Information(context.SerializeJson(settings));
        context.Information("=============================================");

        return settings;
    }
}

public class Settings {
    public string Target = "Default";
    public string Configuration = "Release";
    public TasksFilterSettings TasksFilter = new TasksFilterSettings();
    public OutputSettings Output = new OutputSettings();
}

public class TasksFilterSettings {
    public string TasksToRun = "";
    public string TasksToSkip = "";
}

public class OutputSettings {
    public string OutputDir = "./outputDir";
    public string CodeCoverageDir = "./outputDir/codeCoverage";
    public string BinariesDir = "./outputDir/binaries";
}

public static string AsAbsolutePath(ICakeContext context, string path) {
        return context.MakeAbsolute(new FilePath(path)).FullPath;
}