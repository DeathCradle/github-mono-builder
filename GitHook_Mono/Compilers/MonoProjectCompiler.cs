using System;
using GitHook_Mono.Config.Build;

namespace GitHook_Mono.Compilers
{
	public class MonoProjectCompiler : IProjectCompiler
	{
		public static string MonoPath { get; set; } = MainClass.Config.MonoPath ?? "mono";
		public static string XbuildPath { get; set; } = MainClass.Config.XbuildPath ?? "xbuild";

		public MonoProjectCompiler (GitHub.GitHub_Repository repository) : base (repository)
		{
			
		}

		protected override void Compile (BuildLogger logger, BuildConfig config, string cloneDirectory, string sha)
		{
			MainClass.Plugins.ForEachPlugin ((plugin) =>
			{
				plugin.BeforeCompile (this, logger, config, cloneDirectory, sha);
			});

			//Print info
			Run (logger, MonoPath, $"--version", cloneDirectory);
			Run (logger, XbuildPath, $"/version", cloneDirectory);

			//Post build commands - Default: restore packages
			if (config == null || config.PreBuild == null || config.PreBuild.Length == 0)
			{
				Console.WriteLine ("No config pre-build commands");
				Run (logger, "nuget", $"restore \"{config.SolutionFile}\"", cloneDirectory);
			}
			else
			{
				foreach (var command in config.PreBuild)
				{
					var line = command.Replace ("{mono-path}", MonoPath);
					var firstSpace = line.IndexOf (' ');
					var cmd = line.Substring (0, firstSpace);
					var args = line.Remove (0, firstSpace + 1);

					Run (logger, cmd, args, cloneDirectory);
				}
			}

			//Compile
			if (String.IsNullOrEmpty (config.BuildArgs))
			{
				Run (logger, XbuildPath, $"\"{config.SolutionFile}\"", cloneDirectory);
			}
			else
			{
				Run (logger, XbuildPath, config.BuildArgs, cloneDirectory);
			}

			MainClass.Plugins.ForEachPlugin ((plugin) =>
			{
				plugin.AfterCompile (this, logger, config, cloneDirectory, sha);
			});
		}
	}
}

