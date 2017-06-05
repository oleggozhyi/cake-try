#addin "Newtonsoft.Json"
#load "./Settings.cake"

using Newtonsoft.Json;

public static class SettingsManager {
    public static Settings LoadSettings(ICakeContext context,  ITeamCityProvider teamcity) {
        var settingsFile = context.Argument<string>("settingsFile",  "./build.settings.local.json");
        Settings settings;
        
        teamcity.WriteStartBlock("<Build Settings>");

        if (settingsFile == null) {
            context.Information("No settings file passed, applying defaults");
            settings = new Settings();
        } else {
            context.Information("Reading settings from " + settingsFile);
            var settingsJson = context.FileReadText(settingsFile);    
            settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
        }

        
        context.Information(JsonConvert.SerializeObject(settings, Formatting.Indented));
        teamcity.WriteEndBlock("<Build Settings>");

        return settings;
    }
}