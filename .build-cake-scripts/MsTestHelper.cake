public static class MsTestHelper {
    public static FilePath GetToolPath(ICakeContext context)
    {
        var programFiles = context.EnvironmentVariable("ProgramFiles(X86)");
        var msTestExefromVs2017Pro = programFiles + @"\Microsoft Visual Studio\2017\Professional\Common7\IDE\mstest.exe";
        if(context.FileExists(msTestExefromVs2017Pro)) {
            return msTestExefromVs2017Pro;
        }
        var msTestExefromVs2015 = programFiles + @"\Microsoft Visual Studio 14.0\Common7\IDE\mstest.exe";
        if(context.FileExists(msTestExefromVs2015)) {
            return msTestExefromVs2015;
        }
        throw new FileNotFoundException("Couyld not find MSTest.exe. Tried" + 
               "\n2017 Pro : " + msTestExefromVs2017Pro +
               "\n2015     : " + msTestExefromVs2015);
    }
}