using System;
using Newtonsoft.Json;

namespace GitHook_Mono.Config.App
{
	public class AppConfiguration
	{
		/// <summary>
		/// Gets or sets the mono path.
		/// </summary>
		/// <remarks>This can be "mono" or the path to mono</remarks>
		[JsonProperty ("mono-path")]
		public string MonoPath { get; set; }

		/// <summary>
		/// Gets or sets the xbuild path.
		/// </summary>
		/// <remarks>This can be "mono" or the path to mono</remarks>
		[JsonProperty ("xbuild-path")]
		public string XbuildPath { get; set; }

		/// <summary>
		/// The web server binding address to listen for GitHub web hooks.
		/// </summary>
		[JsonProperty ("listen-address")]
		public string ListenAddress { get; set; }
	}
}

