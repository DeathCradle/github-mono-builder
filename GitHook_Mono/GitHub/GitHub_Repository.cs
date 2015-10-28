using System;
using Newtonsoft.Json;

namespace GitHook_Mono.GitHub
{
	public class GitHub_Repository
	{
		[JsonProperty ("full_name")]
		public string FullName { get; set; }

		[JsonProperty ("clone_url")]
		public string CloneUrl { get; set; }
	}
}

