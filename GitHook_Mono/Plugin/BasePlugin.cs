using System;
using GitHook_Mono.Config.Build;
using GitHook_Mono.Compilers;
using GitHook_Mono.GitHub;

namespace GitHook_Mono.Plugin
{
	/// <summary>
	/// The BasePlugin for external assemblies to hook into the compilation process.
	/// </summary>
	public abstract class BasePlugin
	{
		/// <summary>
		/// Called when your plugin is to prepare itself for use
		/// </summary>
		public virtual void Initialise() { }

		/// <summary>
		/// Called before any compilation steps are performed.
		/// </summary>
		/// <param name="compiler">Compiler being used.</param>
		/// <param name="logger">The build log.</param>
		/// <param name="config">Config.</param>
		/// <param name="cloneDirectory">Clone directory.</param>
		/// <param name="commit">Commit details.</param>
		public virtual void BeforeCompile(IProjectCompiler compiler, BuildLogger logger, BuildConfig config, string cloneDirectory, GitHub_Commit commit) { }

		/// <summary>
		/// Called after the build process has successfully ran.
		/// </summary>
		/// <param name="compiler">Compiler being used.</param>
		/// <param name="logger">The build log.</param>
		/// <param name="config">Config.</param>
		/// <param name="cloneDirectory">Clone directory.</param>
		/// <param name="commit">Commit details.</param>
		public virtual void AfterCompile(IProjectCompiler compiler, BuildLogger logger, BuildConfig config, string cloneDirectory, GitHub_Commit commit) { }

		/// <summary>
		/// Called when the build log has been closed.
		/// </summary>
		/// <param name="compiler">Compiler being used.</param>
		/// <param name="filePath">Path to the build log.</param>
		public virtual void LogClosed(IProjectCompiler compiler, string filePath) { }

		public virtual void OnFail(IProjectCompiler compiler, string cloneDirectory, GitHub_Commit commit) { }
		public virtual void OnPass(IProjectCompiler compiler, string cloneDirectory, GitHub_Commit commit) { }
	}
}

