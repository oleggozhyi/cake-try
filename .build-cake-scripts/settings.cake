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
    public TasksFilterSettings TasksFilter = new TasksFilterSettings();
    public BuildSettings Build = new BuildSettings();
}

public class TasksFilterSettings {
    public string TasksToRun = "";
    public string TasksToSkip = "";
}

public class BuildSettings {
    public string Configuration = "Release";
    public string BinariesDir = "./outputDir/binaries";
    public Verbosity Verbosity = Verbosity.Minimal;

    public MSBuildSettings ToMSBuildSettings(ICakeContext context) {
        var outdirAbs = context.MakeAbsolute(context.Directory(BinariesDir)).FullPath;
        return new MSBuildSettings()
        {
            Configuration = Configuration,
            Verbosity =  Verbosity
        }.WithProperty("OutDir", outdirAbs);;
    }
}

