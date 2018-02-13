using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GetRepoCmdlet
{
	internal class Processor
	{
		internal static Enumerations.GitParamType ValidateGitParam(string gitParam)
		{
			if (Regex.IsMatch(gitParam, ConstMgr.MatchURL))
			{
				return Enumerations.GitParamType.URL;
			}
			else if (Regex.IsMatch(gitParam, ConstMgr.MatchRepo))
			{
				return Enumerations.GitParamType.REPO_NAME;
			}
			else
			{
				return Enumerations.GitParamType.INVALID;
			}
		}

		internal static string CreateRepoName(string gitRepo_URL)
		{
			return gitRepo_URL.Substring(gitRepo_URL.LastIndexOf('/') + 1).Replace(".git", "");
		}

		internal static string CreateRepoURL(string gitRepo_NAME)
		{
			return ConstMgr.GitURLStarter + gitRepo_NAME + ConstMgr.GitURLEnder;
		}

		internal static bool CheckForExistingDirectory(string directory)
		{
			return (Directory.Exists(directory) && Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Count() > 0) ? true : false;
		}

		internal static string BackupDirectory(string rootPath, string repoPath, string repoName)
		{
			#region Old logic
			//string backupDirectory = repoName + ConstMgr.BakDirAppendString;
			//string backupDirPath = rootPath + "\\" + backupDirectory;

			//DirectoryInfo directoryInto = new DirectoryInfo(rootPath);
			//DirectoryInfo[] directories = directoryInto.GetDirectories(backupDirectory + "*");

			//int maxRecurrance = 0;
			//foreach (DirectoryInfo directory in directories)
			//{
			//	if (directory.Name.Contains("_BACKUP ("))
			//	{
			//		char element = directory.Name.ElementAt(directory.Name.IndexOf("_BACKUP (") + 9);
			//		if (int.TryParse(element.ToString(), out int currItem))
			//		{
			//			maxRecurrance = currItem > maxRecurrance ? currItem : maxRecurrance;
			//		}
			//	}
			//	else
			//	{
			//		maxRecurrance++;
			//	}
			//}

			//if (maxRecurrance > 0)
			//{
			//	// pre-increment so the new file is one greater than the highest val
			//	backupDirPath += " (" + ++maxRecurrance + ")";
			//}

			//Directory.Move(repoPath, backupDirPath);

			//return backupDirPath;
			#endregion

			if (Directory.Exists(repoPath))
			{
				if (Directory.Exists(repoPath + ConstMgr.BakDirAppendString))
				{
					// One backup directory already exists, so set to 1 to reflect this
					int numBackups = 1;

					// pre-increment numBackups to check the next one, loop until the highest backup is reached
					while (Directory.Exists(repoPath + ConstMgr.BakDirAppendString_Multi + ++numBackups + ")")) ;


					return repoPath + ConstMgr.BakDirAppendString_Multi + numBackups + ")";
				}
				else
				{
					return repoPath + ConstMgr.BakDirAppendString;
				}
			}
			else
			{
				return repoPath;
			}
		}

		internal static string LaunchVS(CmdContainer cmdContainer)
		{
			string displayMessage = GetSlnFilePath();
			if (!cmdContainer.StopExecute)
			{
				switch (cmdContainer.VSVersion)
				{
					case 2008:
					case 9:
						// execute VS 2008
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2008...";
						VSFilePath(ConstMgr.VSExecutionPath_9);
						break;
					case 2010:
					case 10:
						// execute VS 2010
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2010...";
						VSFilePath(ConstMgr.VSExecutionPath_10);
						break;
					case 2012:
					case 11:
						// execute VS 2012
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2012...";
						VSFilePath(ConstMgr.VSExecutionPath_11);
						break;
					case 2013:
					case 12:
						// execute VS 2013
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2013...";
						VSFilePath(ConstMgr.VSExecutionPath_12);
						break;
					case 2015:
					case 14:
						// execute VS 2015
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2015...";
						VSFilePath(ConstMgr.VSExecutionPath_14);
						break;
					case 2017:
					case 15:
						// execute VS 2017
						displayMessage = ConstMgr.VSVersionDisplayMessage + "2017...";
						VSFilePath(ConstMgr.VSExecutionPath_15);
						break;
					default:
						// could not match a version
						displayMessage = "Opening repo in system default application for .sln files...";
						VSFilePath(null);
						break;
				}
			}

			return displayMessage;

			string GetSlnFilePath()
			{
				try
				{
					cmdContainer.SlnFile = ((new DirectoryInfo(cmdContainer.RepoPath)).GetFiles("*.sln", SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime).First()).FullName.ToString();
					return "";
				}
				catch
				{
					cmdContainer.HaltExecution();
					return "No solution file found!";
				}
			}

			void VSFilePath(string vsExePath)
			{
				switch (vsExePath)
				{
					case null:
						System.Diagnostics.Process.Start(cmdContainer.SlnFile);
						break;
					default:
						System.Diagnostics.Process.Start(vsExePath, cmdContainer.SlnFile);
						break;
				}
			}
		}

		internal static void DeleteDirectory(string repoPath)
		{
			foreach (string file in Directory.GetFiles(repoPath, "*.*", SearchOption.AllDirectories))
			{
				File.SetAttributes(file, FileAttributes.Normal);
			}
			Directory.Delete(repoPath, true);
		}

		internal static string ExecuteGit(Enumerations.GitCommand gitCmd, string repoURL, string branch)
		{
			switch (gitCmd)
			{
				case Enumerations.GitCommand.CLONE:
					return PrepareCloneCmd();
				default:
					// Reserved for future use
					return "";
			}

			string PrepareCloneCmd()
			{
				if (!string.IsNullOrWhiteSpace(branch))
				{
					return "git clone " + repoURL;
				}
				else
				{
					return "git clone -b " + branch + " " + repoURL;
				}
			}
		}
	}
}
