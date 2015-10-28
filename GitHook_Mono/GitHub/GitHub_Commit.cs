using System;
using Newtonsoft.Json;

namespace GitHook_Mono.GitHub
{
	//https://developer.github.com/v3/activity/events/types/#pushevent
	public class GitHub_Commit
	{
		[JsonProperty ("sha")]
		public string SHA { get; set; }

		[JsonProperty ("id")]
		public string Id { get; set; }

		[JsonProperty ("message")]
		public string Message { get; set; }

		[JsonProperty ("author")]
		public GitHub_Author Author { get; set; }

		[JsonProperty ("url")]
		public string Url { get; set; }

		[JsonProperty ("distinct")]
		public bool Distinct { get; set; }

		public string CommitId
		{
			get {
				if (String.IsNullOrEmpty (SHA)) return Id;
				return SHA;
			}
		}
	}
}

