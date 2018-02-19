namespace GetRepoCmdlet
{
	/// <summary>
	/// Contains enumerations to be used throughout the cmdlet.
	/// </summary>
	internal class Enumerations
	{
		/// <summary>
		/// States the type of git parameter passed.
		/// </summary>
		internal enum GitParamType
		{
			URL,
			REPO_NAME,
			INVALID
		}

		/// <summary>
		/// States the type of git command to be used.
		/// </summary>
		/// <remarks>
		/// See <see cref="Processor.PrepareGitCmd(GitCommand, string, string)"/> for adding
		/// new git commands to the cmdlet.
		/// </remarks>
		/// <seealso cref="Processor.PrepareGitCmd(GitCommand, string, string)"/>
		internal enum GitCommand
		{
			CLONE,
			ABORT
		}
	}

	/// <summary>
	/// Container for information needed to complete cmdlet processing.
	/// </summary>
	internal class CmdContainer
	{
		#region Cmdlet Parameters		
		/// <summary>
		/// Gets the github URL.
		/// </summary>
		/// <value>
		/// The github URL.
		/// </value>
		internal string URL { get; private set; }

		/// <summary>
		/// Gets the name of the repo.
		/// </summary>
		/// <value>
		/// The name of the repo.
		/// </value>
		internal string RepoName { get; private set; }

		/// <summary>
		/// Gets the branch name.
		/// </summary>
		/// <value>
		/// The github branch.
		/// </value>
		internal string Branch { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to skip execution of the solution file.
		/// </summary>
		/// <value>
		///   <c>true</c> if [stop execute] is present; otherwise, <c>false</c>.
		/// </value>
		internal bool StopExecute { get; private set; }

		/// <summary>
		/// Gets the VS version requested for execution.
		/// </summary>
		/// <value>
		/// The VS version. <c>NULL</c> indicates no version selected, 
		/// and will open the default application.
		/// </value>
		internal double? VSVersion { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the process should exit the terminal on 
		/// procedure completion.
		/// </summary>
		/// <value>
		///   <c>true</c> if Exit is present; otherwise, <c>false</c>.
		/// </value>
		internal bool Exit { get; private set; }
		#endregion

		#region Directory Information		
		/// <summary>
		/// Gets the root path where the repo will be added.
		/// </summary>
		/// <value>
		/// The root path.
		/// </value>
		internal string RootPath { get; private set; }

		/// <summary>
		/// Gets the repo file path.
		/// </summary>
		/// <value>
		/// The repo path.
		/// </value>
		internal string RepoPath { get; private set; }

		/// <summary>
		/// Gets or sets the .sln file path.
		/// </summary>
		/// <value>
		/// The solution file.
		/// </value>
		internal string SlnFile { get; set; }
		#endregion

		/// <summary>
		/// Gets or sets the git command type.
		/// </summary>
		/// <value>
		/// The git command.
		/// </value>
		internal Enumerations.GitCommand GitCmd { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CmdContainer"/> class.
		/// </summary>
		/// <param name="url">The github URL.</param>
		/// <param name="repoName">Name of the repo.</param>
		/// <param name="branch">The branch.</param>
		/// <param name="stopExecute">if set to <c>true</c> stop execution.</param>
		/// <param name="vsVersion">The VS version to execute.</param>
		/// <param name="exit">if set to <c>true</c> exit PS instance.</param>
		/// <param name="fsCurrentDirectory">The current file system directory of this PS instance.</param>
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

		/// <summary>
		/// Sets the StopExecute flag to <c>true</c> to prevent VS execution.
		/// </summary>
		internal void HaltExecution()
		{
			StopExecute = true;
		}
	}

	/// <summary>
	/// Custom application exception to handle invalid entries in the GitRepo parameter.
	/// </summary>
	/// <seealso cref="System.Exception" />
	public class InvalidGitRepoException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidGitRepoException"/> class.
		/// </summary>
		/// <seealso cref="ConstMgr.InvalidGitRepoExceptionMessage"/>
		public InvalidGitRepoException()
			: base(ConstMgr.InvalidGitRepoExceptionMessage)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidGitRepoException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <seealso cref="ConstMgr.InvalidGitRepoExceptionMessage"/>
		public InvalidGitRepoException(string message)
			: base(ConstMgr.InvalidGitRepoExceptionMessage)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidGitRepoException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="inner">The inner exception.</param>
		/// <seealso cref="ConstMgr.InvalidGitRepoExceptionMessage"/>
		public InvalidGitRepoException(string message, System.Exception inner)
			: base(ConstMgr.InvalidGitRepoExceptionMessage, inner)
		{ }
	}
}
