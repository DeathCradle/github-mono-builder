using System;
using Newtonsoft.Json;

namespace GitHook_Mono.Config.Build
{
	public class BuildConfig
	{
		/// <summary>
		/// Gets or sets the solution file.
		/// </summary>
		/// <value>The solution file.</value>
		[JsonProperty("solution-file")]
		public string SolutionFile { get; set; }

		/// <summary>
		/// Pre build commands, these will replace the default nuget restore commands.
		/// </summary>
		[JsonProperty("pre-build")]
		public string[] PreBuild { get; set; }

		/// <summary>
		/// Gets or sets the build arguments sent to xbuild.
		/// </summary>
		[JsonProperty("build-args")]
		public string BuildArgs { get; set; }
	}
}

