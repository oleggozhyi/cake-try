public class Settings {
    public string Target = "Default";
    public TasksFilterSettings TasksFilter = new TasksFilterSettings();
    public BuildSettings Build = new BuildSettings();
    public TestSettings Test = new TestSettings();
}

public class TasksFilterSettings {
    public string TasksToRun = "";
    public string TasksToSkip = "";
}

public class BuildSettings {
    public string Configuration = "Release";
    public string BinariesDir = "./.outputDir/binaries";
    public string OutDir = "./.outputDir";
    public Verbosity Verbosity = Verbosity.Minimal;

    public MSBuildSettings ToMSBuildSettings(ICakeContext context) {
        var outdirAbs = context.MakeAbsolute(context.Directory(BinariesDir)).FullPath;
        return new MSBuildSettings {
            Configuration = Configuration,
            Verbosity =  Verbosity
        }.WithProperty("OutDir", outdirAbs);;
    }
}

public class TestSettings {
    public string CoverageDir = "./.outputDir/coverage";
}

