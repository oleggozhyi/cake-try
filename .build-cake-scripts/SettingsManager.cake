#addin "Newtonsoft.Json"
#load "./Settings.cake"

using Newtonsoft.Json;

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
            settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
        }

        context.Information("=====BUILD SETTINGS==========================\n");
        context.Information(JsonConvert.SerializeObject(settings, Formatting.Indented));
        context.Information("=============================================");

        return settings;
    }
}