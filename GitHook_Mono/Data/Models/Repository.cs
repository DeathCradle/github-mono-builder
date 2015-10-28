using System;
using System.ComponentModel.DataAnnotations;

namespace GitHook_Mono.Data.Models
{
	public class Repository
	{
		[Key]
		public int Id { get; set; }

		[MaxLength (500)]
		public string Name { get; set; }

		public int Builds { get; set; }
	}
}

