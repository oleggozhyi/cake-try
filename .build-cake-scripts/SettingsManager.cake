#load "./Settings.cake"

public static class SettingsManager {
    public static Settings LoadSettings(ICakeContext context) {
        var settingsFile = context.Argument<string>("settingsFile",  "./build.settings.local.json");
        Settings settings;

        if (settingsFile == null) {
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