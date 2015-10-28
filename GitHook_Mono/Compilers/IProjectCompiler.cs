//#define TESTING
using System;
using Newtonsoft.Json;
using System.Diagnostics;
using GitHook_Mono.Data;
using GitHook_Mono.GitHub;
using System.Linq;
using GitHook_Mono.Config.Build;

namespace GitHook_Mono.Compilers
{
	public class CompilerException : Exception
	{
		public int ExitCode { get; set; }

		public CompilerException (int exitCode, string message = "") : base (message)
		{
			this.ExitCode = exitCode;
		}

		public override string ToString ()
		{
			var msg = Message;
			if (!String.IsNullOrEmpty (msg)) msg += "\n";
			return $"{msg}Process failed with exit code {ExitCode}.";
		}
	}

	public abstract class IProjectCompiler
	{
		public const String BuildFile = "gh-build.json";

		private GitHub_Repository _repository { get; set; }

		private int _repoId { get; set; }

		private int _builds { get; set; }

		//		private RepoContext _db;

		const String WorkingDirectory = "Clones";

		/// <summary>
		/// This variable is used to trigger another recompilation after the current operation completes.
		/// The reason for this is if a push is received, followed be another the second push would not
		/// trigger a build.
		/// </summary>
		private static System.Collections.Concurrent.ConcurrentQueue<String> _pending;

		private System.Threading.Thread _processor;

		public IProjectCompiler (GitHub_Repository repository)
		{
			_repository = repository;
			_pending = new System.Collections.Concurrent.ConcurrentQueue<String> ();
		}

		protected string GetProjectPath (string sha)
		{
			#if !TESTING
			return System.IO.Path.Combine (
			Environment.CurrentDirectory, 
			WorkingDirectory, 
			$"{_repository.FullName}_{sha}_{_builds}");
			#else
			return System.IO.Path.Combine (
				Environment.CurrentDirectory, 
				WorkingDirectory, 
				$"{_repository.FullName}_{sha}")
			;
			#endif
		}

		private void Start ()
		{
			#if !DEBUG
			using (var db = new RepoContext ())
			{
				var dbRepo = db.Repositories.SingleOrDefault (x => x.Name == _repository.FullName);
				if (null == dbRepo)
				{
					dbRepo = new GitHook_Mono.Data.Models.Repository () {
						Builds = 0,
						Name = _repository.FullName
					};
					db.Repositories.Add (dbRepo);
					db.SaveChanges ();
				}

				//Set the working repo id
				_repoId = dbRepo.Id;
				_builds = dbRepo.Builds;
			}
			#endif

			if (null == _processor || !_processor.IsAlive) _processor = new System.Threading.Thread (Processor);

			if (!_processor.IsAlive) _processor.Start ();
		}

		public void SheduleCompile (string commitSHA)
		{
			_pending.Enqueue (commitSHA);
			Start ();
		}

		private void Processor ()
		{
			while (_pending.Count > 0)
			{
				string sha;
				if (_pending.TryDequeue (out sha))
				{
					//Generate the clone directory
					var cloneDirectory = GetProjectPath (sha);

					//Load config
					var config = MainClass.LoadConfig<BuildConfig> (System.IO.Path.Combine (cloneDirectory, BuildFile));

					#if !DEBUG
					using (var db = new RepoContext ())
					{
						var commit = db.Commits.SingleOrDefault (x => x.RepositoryId == _repoId && x.SHA == sha);
						if (null == commit)
						{
							commit = new GitHook_Mono.Data.Models.Commit () {
								RepositoryId = _repoId,
								SHA = sha
							};
							db.Commits.Add (commit);
						}

						commit.Status = GitHook_Mono.Data.Models.CommitStatus.Processing;
						db.SaveChanges ();
					#endif
						try
						{
							//Clone
							Console.WriteLine ("Cloning...");
							Clone (config, cloneDirectory, sha);
							Console.WriteLine ("Clone Completed.");

							//Now that we have a clone, we can attempt to load some config
							if (config == null || String.IsNullOrEmpty (config.SolutionFile))
							{
								var sln = System.IO.Directory.GetFiles (cloneDirectory, "*.sln");
								if (sln != null && sln.Length > 0)
								{
									if (sln.Length == 1)
									{
										if (config == null) config = new BuildConfig () {
											SolutionFile = sln.First ()
										};
										else config.SolutionFile = sln.First ();
									}
									else
									{
										throw new CompilerException(1, $"Too many solution files detected, please specify one in your {BuildFile}");
									}
								}
								else
								{
									throw new CompilerException(1, $"No solution file specified in {BuildFile}");
								}
							}

							//Start compilation
							Console.WriteLine ("Compiling...");
							Compile (config, cloneDirectory, sha);
							Console.WriteLine ("Compiling Completed.");

						#if !DEBUG
							commit.Status = GitHook_Mono.Data.Models.CommitStatus.Passed;
						#endif
						}
						catch (CompilerException ex)
					{
						#if !DEBUG
							commit.Status = GitHook_Mono.Data.Models.CommitStatus.Failed;
						#endif
							Console.WriteLine (ex.ToString ());
						}
					#if !DEBUG
						db.Repositories.Single (x => x.Id == _repoId).Builds++;
						_builds++;
						db.SaveChanges ();
					}
					#endif
				}
			}
		}

		public void Run (string command, string args, string workingPath = WorkingDirectory)
		{
			Console.WriteLine ($"{command} {args}");
			var pc = Process.Start (new ProcessStartInfo () {
				FileName = command,
				Arguments = args,
				WorkingDirectory = workingPath
			});

			pc.WaitForExit ();

			if (0 != pc.ExitCode)
			{
				throw new CompilerException (pc.ExitCode);
			}
		}

		protected virtual void Clone (BuildConfig config, string cloneDirectory, string sha)
		{
			//Touch the directory
			if (!System.IO.Directory.Exists (WorkingDirectory))
				System.IO.Directory.CreateDirectory (WorkingDirectory);

			#if !TESTING
			//If for some reason the clone directory exists, then it must be removed for git will crash and burn.
			if (System.IO.Directory.Exists (cloneDirectory))
				System.IO.Directory.Delete (cloneDirectory, true);

			//Clone from github into the target directory
			Run ("git", $"clone --depth=50 --branch=master {_repository.CloneUrl} {cloneDirectory}");
			#endif

			//Checkout the target commit
			Run ("git", $"checkout -qf {sha}", cloneDirectory);

			//Initialise sub modules
			Run ("git", $"submodule init", cloneDirectory);
		}

		protected abstract void Compile (BuildConfig config, string cloneDirectory, string sha);
	}
}

