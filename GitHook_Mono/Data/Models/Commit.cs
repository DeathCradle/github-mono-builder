using System;
using System.ComponentModel.DataAnnotations;

namespace GitHook_Mono.Data.Models
{
	public enum CommitStatus
	{
		Processing = 0,
		Passed = 1,
		Failed = 2
	}

	public class Commit
	{
		[Key]
		public int Id { get; set; }

		public int RepositoryId { get; set; }

		public string SHA { get; set; }

		public string Message { get; set; }

		public CommitStatus Status { get; set; }
	}
}

