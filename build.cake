#tool "XUnit.Runner.Console"
#tool "JetBrains.dotCover.CommandLineTools"

#addin "Cake.FileHelpers"

#load "./.build-cake-scripts/Settings.cake"
#load "./.build-cake-scripts/SettingsManager.cake"
#load "./.build-cake-scripts/ExecutionManager.cake"
#load "./.build-cake-scripts/MsTestHelper.cake"

var settings = SettingsManager.LoadSettings(Context, TeamCity);
var executionManager = new ExecutionManager(Context, settings.TasksFilter);

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
    TeamCity.WriteStartBlock("Step <" + ctx.Task.Name + ">");
});
TaskTeardown(ctx => {
    TeamCity.WriteEndBlock("Step <" + ctx.Task.Name + ">");
});

Task("Clean-OutputDirs").Does(() => CleanDirectories(settings.Build.OutDir));

Task("Nuget-RestorePackages").Does(() => NuGetRestore(File("./src/CakeTry.sln")));

Task("Build-Solution").Does(() => 
    MSBuild("./src/CakeTry.sln", settings.Build.ToMSBuildSettings(Context))
);
Task("XUnitTest-NoCoverage").Does(() =>
     XUnit2(GetFiles(settings.Build.BinariesDir + "/*.Unit.Tests.dll"), new XUnit2Settings { ShadowCopy = false })
);
Task("XUnitTest-Dotcover").Does(() => {
    DotCoverCover(tool =>
        tool.XUnit2(GetFiles(settings.Build.BinariesDir + "/*.Unit.Tests.dll"),
            new XUnit2Settings { ShadowCopy = false}),
        new FilePath(settings.Test.CoverageDir + "/xUnitTestCoverage.dcvr"),
        new DotCoverCoverSettings { LogFile = File(settings.Test.CoverageDir + "/xunit-dotcover-log.txt") }
            .WithFilter("+:CakeTry.*")
            .WithFilter("-:CakeTry.*.*.Test*"));
});
Task("BddTest-NoCoverage").Does(() => {
    MSTest(GetFiles(settings.Build.BinariesDir + "/*.Bdd.Tests.dll"),
        new MSTestSettings {
            ResultsFile = MakeAbsolute(File(settings.Build.OutDir + "/TestResult.trx")).FullPath,
            NoIsolation = false,
            ToolPath = MsTestHelper.GetToolPath(Context)
        });
    TeamCity.ImportData("mstest", settings.Build.OutDir + "/TestResult.trx");
});
Task("BddTest-Dotcover").Does(() => {
    DotCoverCover(tool => 
        tool.MSTest(GetFiles(settings.Build.BinariesDir + "/*.Bdd.Tests.dll"),
            new MSTestSettings {
                ResultsFile = MakeAbsolute(File(settings.Build.OutDir + "/TestResult.trx")).FullPath,
                NoIsolation = false,
                ToolPath = MsTestHelper.GetToolPath(Context)}),
        new FilePath(settings.Test.CoverageDir + "/MSTestCoverage.dcvr"),
        new DotCoverCoverSettings { LogFile = File(settings.Test.CoverageDir + "/mstest-dotcover-log.txt") }
            .WithFilter("+:CakeTry.*")
            .WithFilter("-:CakeTry.*.*.Test*"));
    TeamCity.ImportData("mstest", settings.Build.OutDir + "/TestResult.trx");
});
Task("Merge-TestCoverage").Does(() => 
    DotCoverMerge(GetFiles(settings.Test.CoverageDir + "/*.dcvr"),
            new FilePath(settings.Test.CoverageDir + "/FullCodeCoverage.dcvr"))
);
Task("Report-TestCoverage").Does(() => {
    DotCoverReport(settings.Test.CoverageDir + "/FullCodeCoverage.dcvr",
        new FilePath(settings.Test.CoverageDir + "/sonarCoverageReport.html"),
        new DotCoverReportSettings { ReportType = DotCoverReportType.HTML });
    TeamCity.ImportDotCoverCoverage(settings.Test.CoverageDir + "/FullCodeCoverage.dcvr");
});

Task("Default-Local")
    // .IsDependentOn("Clean-OutputDirs")
    // .IsDependentOn("Nuget-RestorePackages")
    .IsDependentOn("Build-Solution")
    .IsDependentOn("XUnitTest-NoCoverage");
    // .IsDependentOn("BddTest-NoCoverage");

Task("Default-Teamcity")
    .IsDependentOn("Clean-OutputDirs")
    .IsDependentOn("Nuget-RestorePackages")
    .IsDependentOn("Build-Solution")
    .IsDependentOn("XUnitTest-Dotcover")
    .IsDependentOn("BddTest-Dotcover")
    .IsDependentOn("Merge-TestCoverage")
    .IsDependentOn("Report-TestCoverage");

RunTarget(settings.Target);