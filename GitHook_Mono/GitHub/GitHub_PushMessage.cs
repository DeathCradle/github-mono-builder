using System;
using Newtonsoft.Json;

namespace GitHook_Mono.GitHub
{
	public class GitHub_PushMessage
	{
		[JsonProperty ("repository")]
		public GitHub_Repository Repository { get; set; }

		[JsonProperty ("commits")]
		public GitHub_Commit[] Commits { get; set; }

		public string GitHubLink {
//			get { return $"https://github.com/{Repository.FullName}.git"; }
			get { return Repository.CloneUrl; } 
		}
	}
}

