namespace GetRepoCmdlet
{
	internal class Enumerations
	{
		internal enum GitParamType
		{
			URL,
			REPO_NAME,
			INVALID
		}

		internal enum GitCommand
		{
			CLONE,
			ABORT
		}
	}

	internal class CmdContainer
	{
		#region Cmdlet Parameters
		internal string URL { get; private set; }

		internal string RepoName { get; private set; }

		internal string Branch { get; private set; }

		internal bool StopExecute { get; private set; }

		internal double? VSVersion { get; private set; }

		internal bool Exit { get; private set; }
		#endregion

		#region Directory Information
		internal string RootPath { get; private set; }

		internal string RepoPath { get; private set; }

		internal string SlnFile { get; set; }
		#endregion

		internal Enumerations.GitCommand GitCmd { get; set; }

		internal CmdContainer(string url, string repoName, string branch, bool stopExecute, double? vsVersion, bool exit, string fsCurrentDirectory)
		{
			URL = url;
			RepoName = repoName;
			Branch = branch;
			StopExecute = stopExecute;
			VSVersion = vsVersion;
			Exit = exit;
			RootPath = fsCurrentDirectory;
			RepoPath = fsCurrentDirectory + "\\" + repoName;
			GitCmd = Enumerations.GitCommand.CLONE;
		}

		internal void HaltExecution()
		{
			StopExecute = true;
		}
	}

	public class InvalidGitRepoException : System.Exception
	{
		public InvalidGitRepoException()
			: base(ConstMgr.InvalidGitRepoExceptionMessage)
		{ }

		public InvalidGitRepoException(string message)
			: base(ConstMgr.InvalidGitRepoExceptionMessage)
		{ }

		public InvalidGitRepoException(string message, System.Exception inner)
			: base(ConstMgr.InvalidGitRepoExceptionMessage, inner)
		{ }
	}
}
