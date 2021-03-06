﻿namespace GetRepoCmdlet
{
	/// <summary>
	/// Contains constant values to be referenced throughout the cmdlet. 
	/// All fields within this class can be altered and recompiled to 
	/// create dynamic messages.
	/// </summary>
	internal class ConstMgr
	{
		internal const string CmdExecutionApplication = "cmd.exe";
		internal const string MatchURL = "(?:git|ssh|https?|git@[-\\w.]+):(\\/\\/)?(.*?)(\\.git)(\\/?|\\#[-\\d\\w._]+?)$";
		internal const string MatchRepo = "^(\\.?[A-Za-z]+)+$";
		internal const string GitURLStarter = "https://github.com/GreatAmerica/";
		internal const string GitURLEnder = ".git";
		internal const string FileSearchPattern = "*.sln";
		internal const string VSVersionuiMessage = "Opening solution in Visual Studio ";
		internal const string VSExecutionPath_9 = @"C:\Program Files (x86)\Microsoft Visual Studio 9.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_10 = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_11 = @"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_12 = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_14 = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe";
		internal const string VSExecutionPath_15 = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe";
		internal const string GitCloneCmd = "/c git clone ";
		internal const string GitBranchCmd = "/c git clone -b ";
		internal const string BakDirAppendString = "_BACKUP";
		internal const string BakDirAppendString_Multi = "_BACKUP (";
		internal const string InvalidGitRepoExceptionMessage = "An invalid parameter was passed for the repository to clone.";
		internal const string UIMessage_BackupBegin = "Backing up existing repo...";
		internal const string UIMessage_BackupLocation = "BACKUP FILE CREATED AT: ";
		internal const string UIMessage_DeleteBegin = "Deleting existing repo...";
		internal const string UIMessage_DeleteSuccess = "Repo deleted successfully.";
		internal const string UIMessage_DeleteFail = "FATAL: Deletion of old repository failed. Please proceed manually.";
		internal const string UIMessage_RepoExistsAlert = "\nALERT: Repo already exists. Would you like to _Delete or _Backup the current repo before clone, or _Cancel? (default is _Delete)";
		internal const string UIMessage_LaunchingVS = "Launching Visual Studio...";
		internal const string UIMessage_NoSlnFound = "ERROR: No solution file found!";
		internal const string UIMessage_SlnFound = "\n";
		internal const string UIMessage_VSDefaultOpen = "Opening repo in system default application for .sln files...";

		internal static string[] DirectoryActionResponses = { "d", "D", "b", "B", "c", "C" };
		internal const string DirectoryAction_Default = "D";
		internal const int DirectoryAction_MaxLength = 1;

		internal static string[] GitOutputErrors = { "fatal", "remote" };

		/// <summary>
		/// Determines the VS information needed to boot the proper version.
		/// </summary>
		/// <param name="vsVersion">The VS version to invoke. If <c>null</c>, will trigger system default application.</param>
		/// <returns>User interface message to display on screen; VS exe file path</returns>
		internal static (string, string) DetermineVSInfo(double? vsVersion)
		{
			string uiMessage;
			string vsFilePath;
			switch (vsVersion)
			{
				case 2008:
				case 9:
					// execute VS 2008
					uiMessage = VSVersionuiMessage + "2008...";
					vsFilePath = VSExecutionPath_9;
					break;
				case 2010:
				case 10:
					// execute VS 2010
					uiMessage = VSVersionuiMessage + "2010...";
					vsFilePath = VSExecutionPath_10;
					break;
				case 2012:
				case 11:
					// execute VS 2012
					uiMessage = VSVersionuiMessage + "2012...";
					vsFilePath = VSExecutionPath_11;
					break;
				case 2013:
				case 12:
					// execute VS 2013
					uiMessage = VSVersionuiMessage + "2013...";
					vsFilePath = VSExecutionPath_12;
					break;
				case 2015:
				case 14:
					// execute VS 2015
					uiMessage = VSVersionuiMessage + "2015...";
					vsFilePath = VSExecutionPath_14;
					break;
				case 2017:
				case 15:
					// execute VS 2017
					uiMessage = VSVersionuiMessage + "2017...";
					vsFilePath = VSExecutionPath_15;
					break;
				default:
					// could not match a version
					uiMessage = UIMessage_VSDefaultOpen;
					vsFilePath = null;
					break;
			}

			return (uiMessage, vsFilePath);
		}
	}
}
