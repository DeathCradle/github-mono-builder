using System;
using Newtonsoft.Json;

namespace GitHook_Mono.GitHub
{
	public class GitHub_Author
	{
		[JsonProperty ("name")]
		public string Name { get; set; }

		[JsonProperty ("email")]
		public string Email { get; set; }
	}
}

