using System;
using Newtonsoft.Json;

namespace GitHook_Mono.Config.Build
{
	public class BuildConfig
	{
		/// <summary>
		/// Gets or sets the name of the build.
		/// </summary>
		/// <value>The build name.</value>
		[JsonProperty ("build-name")]
		public string BuildName { get; set; }

		/// <summary>
		/// Gets or sets the solution file.
		/// </summary>
		/// <value>The solution file.</value>
		[JsonProperty ("solution-file")]
		public string SolutionFile { get; set; }

		/// <summary>
		/// Pre build commands, these will replace the default nuget restore commands.
		/// </summary>
		[JsonProperty ("pre-build")]
		public string[] PreBuild { get; set; }

		/// <summary>
		/// Gets or sets the build arguments sent to xbuild.
		/// </summary>
		[JsonProperty ("build-args")]
		public string BuildArgs { get; set; }

		public override string ToString ()
		{
			return string.Format ("[BuildConfig: SolutionFile={0}, PreBuild={1}, BuildArgs={2}]", 
				SolutionFile ?? "null", 
				PreBuild == null ? 0 : PreBuild.Length, 
				BuildArgs ?? "null");
		}
	}
}

