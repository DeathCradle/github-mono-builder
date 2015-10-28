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

		protected override void Compile (BuildConfig config, string cloneDirectory, string sha)
		{
			MainClass.Plugins.ForEachPlugin ((plugin) =>
			{
				plugin.BeforeCompile (this, config, cloneDirectory, sha);
			});

			//Print info
			Run (MonoPath, $"--version", cloneDirectory);
			Run (XbuildPath, $"/version", cloneDirectory);

			//Post build commands - Default: restore packages
			if (config == null || config.PreBuild == null || config.PreBuild.Length == 0)
			{
				Console.WriteLine ("No config pre-build commands");
				Run ("nuget", $"restore \"{config.SolutionFile}\"", cloneDirectory);
			}
			else
			{
				foreach (var command in config.PreBuild)
				{
					var line = command.Replace ("{mono-path}", MonoPath);
					var firstSpace = line.IndexOf (' ');
					var cmd = line.Substring (0, firstSpace);
					var args = line.Remove (0, firstSpace + 1);

					Run (cmd, args, cloneDirectory);
				}
			}
//			Run ("mono", ".nuget/NuGet.exe restore API/packages.config -PackagesDirectory packages/ -source \"https://api.nuget.org/v3/index.json;https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/\"", cloneDirectory);
//			Run ("mono", ".nuget/NuGet.exe restore Patcher/packages.config -PackagesDirectory packages/ -source \"https://api.nuget.org/v3/index.json;https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/\"", cloneDirectory);

			//Compile
			if (String.IsNullOrEmpty (config.BuildArgs))
			{
//				Run (MonoPath + "bin/xbuild", "/p:Configuration=Server-Debug \"Open Terraria API.sln\"", cloneDirectory);
				Run (XbuildPath, $"\"{config.SolutionFile}\"", cloneDirectory);
			}
			else
			{
				Run (XbuildPath, config.BuildArgs, cloneDirectory);
			}

			MainClass.Plugins.ForEachPlugin ((plugin) =>
			{
				plugin.AfterCompile (this, config, cloneDirectory, sha);
			});
		}
	}
}

