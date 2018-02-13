namespace GetRepoCmdlet
{
	internal class ConstMgr
	{
		internal const string MatchURL = "(?:git|ssh|https?|git@[-\\w.]+):(\\/\\/)?(.*?)(\\.git)(\\/?|\\#[-\\d\\w._]+?)$";
		internal const string MatchRepo = "^(\\.?[A-Za-z]+)+$";

		internal const string GitURLStarter = "https://github.com/GreatAmerica/";
		internal const string GitURLEnder = ".git";

		internal const string VSVersionDisplayMessage = "Opening solution in Visual Studio ";

		internal const string VSExecutionPath_9 = @"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_10 = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_11 = @"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_12 = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_14 = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_15 = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe";

		internal const string GitCloneCmd = "git clone ";
		internal const string GitFetchCmd = "git fetch ";

		internal const string BakDirAppendString = "_BACKUP";
		internal const string BakDirAppendString_Multi = "_BACKUP (";

		internal const string InvalidGitRepoExceptionMessage = "An invalid parameter was passed for the repository to clone.";
	}
}
