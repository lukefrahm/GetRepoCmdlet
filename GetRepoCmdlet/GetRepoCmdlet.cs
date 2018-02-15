/*
 *	TODO: 
 *  - Verbose output
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace GetRepoCmdlet
{
	[Cmdlet(VerbsCommon.Get, "Repo")]
	public class GetRepoCmdlet : PSCmdlet
	{
		#region PS Parameters and Instance Variables		
		[Parameter(Position = 0, Mandatory = true)]
		[Alias("git", "clone", "g", "r", "repo")]
		public string GitRepo { get; set; }

		[Parameter]
		[Alias("B")]
		public string Branch { get; set; }

		[Parameter]
		[Alias("NoRun", "NoExec", "Halt")]
		public SwitchParameter StopExecute { get; set; }

		[Parameter]
		[Alias("VS", "Ver", "Use", "V")]
		[ValidateSet("2008", "9", "2010", "10", "2012", "11", "2013", "12", "2015", "14", "2017", "15")]
		public double? VSVersion { get; set; }

		[Parameter]
		[Alias("Close", "X")]
		public SwitchParameter Exit { get; set; }

		private CmdContainer _cmdContainer;
		#endregion

		protected override void BeginProcessing()
		{
			base.BeginProcessing();

			// prepare container
			Enumerations.GitParamType gitType = Processor.ValidateGitParam(GitRepo);
			if (gitType == Enumerations.GitParamType.INVALID)
			{
				throw new InvalidGitRepoException();
			}
			else
			{
				_cmdContainer = new CmdContainer(
					url: (gitType == Enumerations.GitParamType.URL ? GitRepo : Processor.CreateRepoURL(GitRepo)),
					repoName: (gitType == Enumerations.GitParamType.REPO_NAME ? GitRepo : Processor.CreateRepoName(GitRepo)),
					branch: this.Branch,
					stopExecute: this.StopExecute.IsPresent,
					vsVersion: this.VSVersion,
					exit: this.Exit.IsPresent,
					fsCurrentDirectory: SessionState.Path.CurrentFileSystemLocation.ToString()
				);

				Directory.SetCurrentDirectory(_cmdContainer.RootPath);
			}
		}

		protected override void ProcessRecord()
		{
			// check for directory existence 
			if (Processor.CheckForExistingDirectory(_cmdContainer.RepoPath))
			{
				// determine how to proceed if exists
				string input = "D";
				do
				{
					input = "D";
					// Get user's decision of how to proceed, as the repo is already present
					Host.UI.WriteLine(System.ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "\nALERT: Repo already exists. Would you like to _Delete or _Backup the current repo before clone, or _Cancel? (default is _Delete)");
					input = Host.UI.ReadLine();
				} while (!input.ToUpper().Equals("D") && !input.ToUpper().Equals("B") && !input.ToUpper().Equals("C"));

				switch (input.ToUpper())
				{
					case "C":
						_cmdContainer.GitCmd = Enumerations.GitCommand.ABORT;
						break;
					case "B":
						Host.UI.WriteLine("Backing up existing repo...");
						string bakPath = Processor.BackupDirectory(_cmdContainer.RootPath, _cmdContainer.RepoPath, _cmdContainer.RepoName);
						Host.UI.WriteLine("BACKUP FILE CREATED AT: " + bakPath);
						goto default;
					default:
						_cmdContainer.GitCmd = Enumerations.GitCommand.CLONE;
						Host.UI.WriteLine("Deleting existing repo...");
						Processor.DeleteDirectory(_cmdContainer.RepoPath);
						Host.UI.WriteLine("Repo deleted successfully.");
						break;
				}
			}

			if (_cmdContainer.GitCmd != Enumerations.GitCommand.ABORT)
			{
				Host.UI.WriteLine("Cloning repo now...");
				string cmdString = Processor.PrepareGitCmd(_cmdContainer.GitCmd, _cmdContainer.URL, _cmdContainer.Branch);
				//! Get messages while invoking and display here
				Verbose();
				PowerShell.Create().AddScript(cmdString).Invoke();
				Host.UI.WriteLine("Repo cloned successfully.");

				// launch VS if applicable
				if (!_cmdContainer.StopExecute)
				{
					Host.UI.WriteLine("Launching Visual Studio...");
					Processor.LaunchVS(_cmdContainer);
				}
			}
		}

		protected override void EndProcessing()
		{
			if (_cmdContainer.Exit)
			{ // Exit flag is present. Close prompt
				Host.SetShouldExit(0);
			}
		}

		private void Verbose()
		{
			List<string> messages = new List<string>();
			using (System.Diagnostics.Process process = new System.Diagnostics.Process())
			{
				process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Verb = "runas";
				process.StartInfo.Arguments = Processor.PrepareGitCmd(_cmdContainer.GitCmd, _cmdContainer.URL, _cmdContainer.Branch);
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				//* Set your output and error (asynchronous) handlers
				//process.OutputDataReceived += (s, e) => symLink.Messages.Add(Enumerations.MessageType.SystemError, e.Data);
				//process.ErrorDataReceived += (s, e) => symLink.Messages.Add(Enumerations.MessageType.Info, e.Data);
				process.OutputDataReceived += (s, e) => messages.Add(e.Data);
				process.ErrorDataReceived += (s, e) => messages.Add(e.Data);
				//* Start process and handler
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}

			foreach (var item in messages)
			{
				Host.UI.WriteLine(item);
			}
		}
	}
}

