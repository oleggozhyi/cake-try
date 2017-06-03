Set-Content -Path 'settings.teamcity.json' -Value `
'{ 
    "target" : "Default",
    "configuration": "Release",

    "tasksFilter": {
        "tasksToRun": "Task1,Task2",
        "tasksToSkip": "Task2,Task3"
    }
}'
Invoke-Expression './build.ps1 -ScriptArgs ''-settingsFile="./settings.teamcity.json"'''
