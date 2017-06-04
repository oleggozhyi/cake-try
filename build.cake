#tool "XUnit.Runner.Console"
#tool "JetBrains.dotCover.CommandLineTools"

#addin "Cake.Json"
#addin "Cake.FileHelpers"

#load "./.build-cake-scripts/Settings.cake"
#load "./.build-cake-scripts/SettingsManager.cake"
#load "./.build-cake-scripts/ExecutionManager.cake"
#load "./.build-cake-scripts/MsTestHelper.cake"

var settings = SettingsManager.LoadSettings(Context);
var executionManager = new ExecutionManager(Context, settings.TasksFilter);

Information("TeamCity.IsRunningOnTeamCity = " + TeamCity.IsRunningOnTeamCity);

TaskSetup(ctx => {
    executionManager.RunOrSkip(ctx.Task);
    
    TeamCity.WriteStartProgress("Starting " + ctx.Task.Name);
});
TaskTeardown(ctx => {
    TeamCity.WriteEndProgress("Finished " + ctx.Task.Name);
});

Task("Clean-OutputDirs")
    .Does(() => CleanDirectories(settings.Build.OutDir));
Task("Nuget-RestorePackages")
    .Does(() => NuGetRestore(File("./src/CakeTry.sln")));
Task("Build-Solution")
    .IsDependentOn("Clean-OutputDirs")
    .IsDependentOn("Nuget-RestorePackages")
    .Does(() => MSBuild("./src/CakeTry.sln", 
        settings.Build.ToMSBuildSettings(Context))
    ).ReportError(exception => TeamCity.BuildProblem("Build solution failed", "msbuild"));;
Task("XUnitTest-NoCoverage")
    .IsDependentOn("Build-Solution")
    .Does(() => XUnit2(GetFiles(settings.Build.BinariesDir + "/*.Unit.Tests.dll"), 
        new XUnit2Settings { ShadowCopy = false })
    );
Task("XUnitTest-Dotcover")
    .IsDependentOn("Build-Solution")
    .Does(() => {
        DotCoverCover(tool => {
            tool.XUnit2( GetFiles(settings.Build.BinariesDir + "/*.Unit.Tests.dll"),
                new XUnit2Settings { ShadowCopy = false});
            },
            new FilePath(settings.Test.CoverageDir + "/xUnitTestCoverage.dcvr"),
            new DotCoverCoverSettings { LogFile = File(settings.Test.CoverageDir + "/xunit-dotcover-log.txt") }
                .WithFilter("+:CakeTry.*")
                .WithFilter("-:CakeTry.*.*.Test*"));
    });
Task("BddTest-NoCoverage")
    .IsDependentOn("Build-Solution")
    .Does(() => {
        MSTest(GetFiles(settings.Build.BinariesDir + "/*.Bdd.Tests.dll"),
            new MSTestSettings {
                ResultsFile =    MakeAbsolute(Directory(settings.Build.OutDir) + File("TestResult.trx")).FullPath,
                NoIsolation = false,
                ToolPath = MsTestHelper.GetToolPath(Context)
            });
    });
Task("BddTest-Dotcover")
    .IsDependentOn("Build-Solution")
    .Does(() => {
       DotCoverCover(tool => {
            tool.MSTest(GetFiles(settings.Build.BinariesDir + "/*.Bdd.Tests.dll"),
                new MSTestSettings {
                    ResultsFile =    MakeAbsolute(Directory(settings.Build.OutDir) + File("TestResult.trx")).FullPath,
                    NoIsolation = false,
                    ToolPath = MsTestHelper.GetToolPath(Context)});
            },
            new FilePath(settings.Test.CoverageDir + "/MSTestCoverage.dcvr"),
            new DotCoverCoverSettings { LogFile = File(settings.Test.CoverageDir + "/mstest-dotcover-log.txt") }
                .WithFilter("+:CakeTry.*")
                .WithFilter("-:CakeTry.*.*.Test*"));
    });
Task("Merge-TestCoverage")
    .IsDependentOn("XUnitTest-Dotcover")
    .IsDependentOn("BddTest-Dotcover")
    .Does(() => {
        DotCoverMerge(GetFiles(settings.Test.CoverageDir + "/*.dcvr"),
             new FilePath(settings.Test.CoverageDir + "/FullCodeCoverage.dcvr")
        );
    });
Task("Report-TestCoverage")
    .IsDependentOn("Merge-TestCoverage")
    .Does(() => {
       DotCoverReport(settings.Test.CoverageDir + "/FullCodeCoverage.dcvr",
        new FilePath(settings.Test.CoverageDir + "/sonarCoverageReport.html"),
        new DotCoverReportSettings { ReportType = DotCoverReportType.HTML });
    });


Task("Default-Local")
    // .IsDependentOn("XUnitTest-NoCoverage")
    .IsDependentOn("BddTest-NoCoverage");

Task("Default-Teamcity")
    .IsDependentOn("Report-TestCoverage");

RunTarget(settings.Target);