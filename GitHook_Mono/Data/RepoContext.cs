using System;
using Microsoft.Data.Entity;

namespace GitHook_Mono.Data
{
	public enum SqliteCopyResult
	{
		Ok,
		DirectoryNotFound,
		FileMissing
	}

	public static class DbContextExtensions
	{
		/// <summary>
		/// Attempt to copy sqlite files from Data/Sqlite/[x86/x64]/* to where they should be.
		/// </summary>
		public static SqliteCopyResult TryCopySqliteDependencies () //(this DbContext ctx)
		{
			var path = System.IO.Path.Combine (Environment.CurrentDirectory, "Sqlite", Environment.Is64BitProcess ? "x64" : "x86");
			if (!System.IO.Directory.Exists (path)) return SqliteCopyResult.DirectoryNotFound;

			//Copy the new platform files
			foreach (var file in new string[] { "sqlite3.dll", "sqlite3.def" })
			{
				var fl = System.IO.Path.Combine (path, file);
				if (!System.IO.File.Exists (fl))
					return SqliteCopyResult.FileMissing;

				//Remove the existing, incase the platform changed
				//This actually can even occur when you run in debug mode
				//on a x64 machine, vshost can be x86.
				if (System.IO.File.Exists (file))
					System.IO.File.Delete (file);

				System.IO.File.Copy (fl, file);
			}

			return SqliteCopyResult.Ok;
		}
	}

	#if !DEBUG
	public class RepoContext : DbContext
	{
		public DbSet<Models.Repository> Repositories { get; set; }

		public DbSet<Models.Commit> Commits { get; set; }

		protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder)
		{
//			this.TryCopySqliteDependencies ();
			optionsBuilder.UseSqlite ("Filename=repos.db");
		}
	}
	#endif
}

