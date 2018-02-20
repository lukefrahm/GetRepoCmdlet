using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static GetRepoCmdlet.Enumerations;
using static GetRepoCmdlet.ConstMgr;
using System.Diagnostics;

namespace GetRepoCmdlet
{
	/// <summary>
	/// Internal static class that holds logic for execution of the cmdlet.
	/// </summary>
	internal class Processor
	{
		#region Git-Related Methods
		/// <summary>
		/// Validates the git parameter based on a regex expression to determine
		/// both validity, as well as if it is a URL or repo name.
		/// </summary>
		/// <param name="gitParam">The git parameter passed by the user.</param>
		/// <returns>
		/// Enumeration of the validation result.
		/// </returns>
		/// <example>
		/// If the parameter is valid and a URL, returns <see cref="GitParamType.URL"/>
		/// If the parameter is valid and a repo name, returns <see cref="GitParamType.REPO_NAME"/>
		/// If the parameter is in invalid form, returns <see cref="GitParamType.INVALID"/>
		/// </example>
		/// <seealso cref="GitParamType"/>
		internal static GitParamType ValidateGitParam(string gitParam)
		{
			if (Regex.IsMatch(gitParam, MatchURL))
			{
				return GitParamType.URL;
			}
			else if (Regex.IsMatch(gitParam, MatchRepo))
			{
				return GitParamType.REPO_NAME;
			}
			else
			{
				return GitParamType.INVALID;
			}
		}

		/// <summary>
		/// Prepares the git command.
		/// </summary>
		/// <remarks>
		/// Uses local functions to allow for easy extensibility for 
		/// future git commands.
		/// </remarks>
		/// <param name="gitCmd">The git command to execute.</param>
		/// <param name="repoURL">The repo URL.</param>
		/// <param name="branch">The branch, if specified.</param>
		/// <returns>The command string to execute.</returns>
		/// <seealso cref="GitCommand"/>
		internal static string PrepareGitCmd(GitCommand gitCmd, string repoURL, string branch)
		{
			switch (gitCmd)
			{
				case GitCommand.CLONE:
					return PrepareCloneCmd();
				default:
					// Reserved for future git cmd additions
					return string.Empty;
			}

			string PrepareCloneCmd()
			{
				if (string.IsNullOrWhiteSpace(branch))
				{
					return GitCloneCmd + repoURL;
				}
				else
				{
					return GitBranchCmd + branch + " " + repoURL;
				}
			}
		}

		/// <summary>
		/// Creates the name of the repo from the URL passed.
		/// </summary>
		/// <param name="gitRepo_URL">The git repo URL.</param>
		/// <returns>The repo name, extracted from the URL.</returns>
		/// <seealso cref="ValidateGitParam(string)"/> 
		internal static string CreateRepoName(string gitRepo_URL)
		{
			return gitRepo_URL.Substring(gitRepo_URL.LastIndexOf('/') + 1).Replace(".git", "");
		}

		/// <summary>
		/// Creates the name of the repo from the repo name passed.
		/// </summary>
		/// <param name="gitRepo_NAME">The git repo name.</param>
		/// <returns>The repo name, concatenated with constants to create the git URL.</returns>
		/// <seealso cref="ValidateGitParam(string)"/>
		/// <seealso cref="GitURLStarter"/>
		/// <seealso cref="GitURLEnder"/>
		internal static string CreateRepoURL(string gitRepo_NAME)
		{
			return GitURLStarter + gitRepo_NAME + GitURLEnder;
		}
		#endregion

		#region File System Methods
		/// <summary>
		/// Checks for an existing directory for this repo in the current location.
		/// </summary>
		/// <param name="directory">The directory to validate.</param>
		/// <returns>
		/// <c>true</c> if the directory exists.
		/// <c>false</c> if the directory does not exist.
		/// </returns>
		internal static bool CheckForExistingDirectory(string directory)
		{
			return (Directory.Exists(directory) && Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Count() > 0) ? true : false;
		}

		/// <summary>
		/// Performs a backup of the existing directory.
		/// </summary>
		/// <param name="repoPath">The repo path to backup.</param>
		/// <returns>The location of the backup created by the process.</returns>
		/// <seealso cref="BakDirAppendString"/>
		/// <seealso cref="BakDirAppendString_Multi"/>
		internal static string BackupDirectory(string repoPath)
		{
			if (Directory.Exists(repoPath))
			{
				if (Directory.Exists(repoPath + BakDirAppendString))
				{
					// One backup directory already exists, so set to 1 to reflect this
					int numBackups = 1;

					// pre-increment numBackups to check the next one, loop until the highest backup is reached
					while (Directory.Exists(repoPath + BakDirAppendString_Multi + ++numBackups + ")")) ;


					return repoPath + BakDirAppendString_Multi + numBackups + ")";
				}
				else
				{
					return repoPath + BakDirAppendString;
				}
			}
			else
			{
				return repoPath;
			}
		}

		/// <summary>
		/// Deletes the directory passed to the method.
		/// </summary>
		/// <param name="repoPath">The directory path to delete (the old repo).</param>
		internal static bool DeleteDirectory(string repoPath)
		{
			try
			{
				foreach (string file in Directory.GetFiles(repoPath, "*.*", SearchOption.AllDirectories))
				{
					File.SetAttributes(file, FileAttributes.Normal);
				}
				Directory.Delete(repoPath, true);

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Launches Visual Studio. If no parameter was passed by VSVersion, launches the default application.
		/// </summary>
		/// <param name="cmdContainer">The <see cref="CmdContainer"/> holding the information needed by this process.</param>
		/// <returns>Messages of what this process has done.</returns>
		/// <seealso cref="CmdContainer"/>
		/// <seealso cref="ConstMgr"/>
		/// <seealso cref="DetermineVSInfo(double?)"/>
		internal static string LaunchVS(CmdContainer cmdContainer)
		{
			string displayMessage = GetSlnFilePath(ref cmdContainer);
			if (cmdContainer.StopExecute)
			{
				return displayMessage;
			}
			else
			{
				// Must both have a sln file found, as well as set to execute
				(string vsMessage, string exePath) = DetermineVSInfo(cmdContainer.VSVersion);
				ExecuteVS(cmdContainer, exePath);
				return displayMessage + vsMessage;
			}
		}

		/// <summary>
		/// Collects the sln file path of the repo from its directory.
		/// </summary>
		/// <returns>The message string of the result. If empty string, no message needs passed back (success)</returns>
		private static string GetSlnFilePath(ref CmdContainer cmdContainer)
		{
			try
			{
				cmdContainer.SlnFile = ((new DirectoryInfo(cmdContainer.RepoPath)).GetFiles(FileSearchPattern, SearchOption.AllDirectories).OrderByDescending(f => f.LastWriteTime).First()).FullName.ToString();
				return UIMessage_SlnFound;
			}
			catch
			{
				cmdContainer.HaltExecution();
				return UIMessage_NoSlnFound;
			}
		}

		/// <summary>
		/// Executes the VS path to launch the repo's sln file.
		/// </summary>
		private static void ExecuteVS(CmdContainer cmdContainer, string vsExePath)
		{
			switch (vsExePath)
			{
				case null:
					Process.Start(cmdContainer.SlnFile);
					break;
				default:
					Process.Start(vsExePath, cmdContainer.SlnFile);
					break;
			}
		}
		#endregion
	}
}
