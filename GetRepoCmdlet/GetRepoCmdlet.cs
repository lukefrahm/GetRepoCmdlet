using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using static GetRepoCmdlet.Enumerations;
using static GetRepoCmdlet.ConstMgr;

namespace GetRepoCmdlet
{
	/// <summary>
	/// Cmdlet definition class to create Get-Repo cmdlet.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "RepoTwo")]
	public class GetRepoCmdlet : PSCmdlet
	{
		#region PS Parameters and Instance Variables	
		/// <summary>
		/// Accepts the mandatory parameter to declare the git repo. 
		/// Accepts both a properly formed URL to a git repo, or a 
		/// repo name that is a part of the GreatAmerica collection.
		/// </summary>
		/// <example>
		/// Both of these examples will result in cloning the same repo ("MyRepo"):
		/// URL entry: Get-Repo https://github.com/GreatAmerica/MyRepo
		/// Repo entry: Get-Repo MyRepo
		/// </example>
		[Parameter(Position = 0, Mandatory = true)]
		[Alias("git", "clone", "g", "r", "repo")]
		public string GitRepo { get; set; }

		/// <summary>
		/// Defines a specific branch to clone. Must be a pre-existing branch.
		/// </summary>
		/// <example>
		/// To clone the branch "MyBranch":
		/// Get-Repo https://github.com/Greatamerica/MyRepo -Branch MyBranch
		/// </example>
		[Parameter]
		[Alias("B")]
		public string Branch { get; set; }

		/// <summary>
		/// Parameter flag that stops execution after git command.
		/// </summary>
		/// <remarks>
		/// This parameter takes precedence over VSVersion.</remarks>
		/// <example>
		/// Both of these result in preventing the solution from opening:
		/// Get-Repo https://github.com/Greatamerica/MyRepo -StopExecute
		/// Get-Repo https://github.com/Greatamerica/MyRepo -StopExecute -VSVersion 2017
		/// </example>
		[Parameter]
		[Alias("NoRun", "NoExec", "Halt")]
		public SwitchParameter StopExecute { get; set; }

		/// <summary>
		/// Parameter that collects the version/year of the VS 
		/// to be used in launching the program.
		/// </summary>
		/// <remarks>
		/// If this parameter and the StopExecute flag are both present,
		/// StopExecute takes precedence over VSVersion and will halt execution.
		/// </remarks>
		[Parameter]
		[Alias("VS", "Ver", "Use", "V")]
		[ValidateSet("2008", "9", "2010", "10", "2012", "11", "2013", "12", "2015", "14", "2017", "15")]
		public double? VSVersion { get; set; }

		/// <summary>
		/// Parameter flag that closes the PS instance after completing processing.
		/// </summary>
		[Parameter]
		[Alias("Close", "X")]
		public SwitchParameter Exit { get; set; }

		/// <summary>
		/// Instance variable to hold parameters and processing information.
		/// </summary>
		private CmdContainer _cmdContainer;
		#endregion

		#region PowerShell Override Methods
		/// <summary>
		/// Override of the base method for <code>BeginProcessing()</code>. 
		/// This override method handles instantiation and preparation of the passed 
		/// information in the command.
		/// </summary>
		/// <exception cref="InvalidGitRepoException">
		/// Invalid git parameter entry will trigger this exception and halt the command.
		/// </exception>
		/// <seealso cref="GitParamType"/>
		/// <seealso cref="Processor.ValidateGitParam(string)"/>
		/// <seealso cref="CmdContainer"/>
		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// prepare container
			GitParamType gitType = Processor.ValidateGitParam(GitRepo);
			if (gitType == GitParamType.INVALID)
			{
				throw new InvalidGitRepoException();
			}
			else
			{
				_cmdContainer = new CmdContainer(
					url: (gitType == GitParamType.URL ? GitRepo : Processor.CreateRepoURL(GitRepo)),
					repoName: (gitType == GitParamType.REPO_NAME ? GitRepo : Processor.CreateRepoName(GitRepo)),
					branch: this.Branch,
					stopExecute: this.StopExecute.IsPresent,
					vsVersion: this.VSVersion,
					exit: this.Exit.IsPresent,
					fsCurrentDirectory: SessionState.Path.CurrentFileSystemLocation.ToString()
				);

				Directory.SetCurrentDirectory(_cmdContainer.RootPath);
			}
		}

		/// <summary>
		/// Override of the base method for <code>ProcessRecord()</code>. 
		/// This override method handles delegating all processing work to the <see cref="Processor"/> class.
		/// </summary>
		/// <seealso cref="Processor.CheckForExistingDirectory(string)"/>
		/// <seealso cref="Processor.BackupDirectory(string, string, string)"/>
		/// <seealso cref="Processor.DeleteDirectory(string)"/>
		/// <seealso cref="Processor.PrepareGitCmd(GitCommand, string, string)"/>
		/// <seealso cref="Processor.LaunchVS(CmdContainer)"/>
		/// <seealso cref="CmdContainer"/>
		protected override void ProcessRecord()
		{
			// check for directory existence 
			if (Processor.CheckForExistingDirectory(_cmdContainer.RepoPath))
			{
				// determine how to proceed if exists
				string input;
				do
				{ 
					// Get user's decision of how to proceed, as the repo is already present
					WriteWarningMessage(UIMessage_RepoExistsAlert);
					string rawInput = Host.UI.ReadLine();
					input = string.IsNullOrWhiteSpace(rawInput) ? DirectoryAction_Default : rawInput;
				} while (Array.IndexOf(DirectoryActionResponses, input) < 0 && input.Length <= DirectoryAction_MaxLength);
				
				//TODO: Cleanup and try to integrate with constants page
				switch (input.ToUpper())
				{
					case "C":
						_cmdContainer.GitCmd = GitCommand.ABORT;
						break;
					case "B":
						WriteInformationMessage(UIMessage_BackupBegin);
						WriteInformationMessage(UIMessage_BackupLocation + Processor.BackupDirectory(_cmdContainer.RepoPath));
						break;
					default:
						_cmdContainer.GitCmd = GitCommand.CLONE;
						if (Processor.DeleteDirectory(_cmdContainer.RepoPath))
						{
							WriteInformationMessage(UIMessage_DeleteSuccess);
						}
						else
						{
							WriteFailMessage(UIMessage_DeleteFail);
						}
						break;
				}
			}

			if (_cmdContainer.GitCmd != GitCommand.ABORT)
			{
				ExecuteGit();

				// launch VS if applicable
				if (!_cmdContainer.StopExecute)
				{
					WriteInformationMessage(UIMessage_LaunchingVS);
					Processor.LaunchVS(_cmdContainer);
				}
			}
		}

		/// <summary>
		/// Override of the base method for <code>EndProcessing()</code>. 
		/// Handles closeout logic, and how to handle the PS command window instance.
		/// </summary>
		/// <seealso cref="CmdContainer"/>
		protected override void EndProcessing()
		{
			if (_cmdContainer.Exit)
			{ // Exit flag is present. Close prompt
				Host.SetShouldExit(0);
			}
		}
		#endregion

		#region Private Methods		
		/// <summary>
		/// Executes and completes execution of necessary git commands. 
		/// Process is created to run in parallel, and uses async calls
		/// to print execution messages back to the invoking PS instance.
		/// </summary>
		private void ExecuteGit()
		{
			List<string> outputList = new List<string>();
			using (Process process = new Process())
			{
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = CmdExecutionApplication;
				process.StartInfo.Arguments = Processor.PrepareGitCmd(_cmdContainer.GitCmd, _cmdContainer.URL, _cmdContainer.Branch);
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.OutputDataReceived += (s, e) => outputList.Add(e.Data);
				process.ErrorDataReceived += (s, e) => outputList.Add(e.Data);
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}

			foreach (var outputMessage in outputList)
			{
				if (!string.IsNullOrWhiteSpace(outputMessage))
				{
					bool hasError = false;
					for (int i = 0; i < GitOutputErrors.Length; i++)
					{
						if (outputMessage.Contains(GitOutputErrors[i]))
						{
							hasError = true;
							i = GitOutputErrors.Length;
						}
					}

					if (hasError)
					{
						WriteFailMessage(outputMessage);
					}
					else
					{
						WriteInformationMessage(outputMessage);
					}
				}
			}
		}

		/// <summary>
		/// Writes the passed failure message to the cmd prompt.
		/// </summary>
		/// <param name="message">The message.</param>
		private void WriteFailMessage(string message)
		{
			Host.UI.WriteLine(System.ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, message);
		}

		/// <summary>
		/// Writes the passed warning message to cmd prompt.
		/// </summary>
		/// <param name="message">The message.</param>
		private void WriteWarningMessage(string message)
		{
			Host.UI.WriteLine(System.ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, message);
		}

		/// <summary>
		/// Writes the passed information message to cmd prompt.
		/// </summary>
		/// <param name="message">The message.</param>
		private void WriteInformationMessage(string message)
		{
			Host.UI.WriteLine(message);
		}
		#endregion
	}
}

