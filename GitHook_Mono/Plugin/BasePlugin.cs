using System;
using GitHook_Mono.Config.Build;
using GitHook_Mono.Compilers;

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
		/// <param name="config">Config.</param>
		/// <param name="cloneDirectory">Clone directory.</param>
		/// <param name="sha">Sha.</param>
		public virtual void BeforeCompile(IProjectCompiler compiler, BuildConfig config, string cloneDirectory, string sha) { }

		/// <summary>
		/// Called after the build process has successfully ran.
		/// </summary>
		/// <param name="config">Config.</param>
		/// <param name="cloneDirectory">Clone directory.</param>
		/// <param name="sha">Sha.</param>
		public virtual void AfterCompile(IProjectCompiler compiler, BuildConfig config, string cloneDirectory, string sha) { }
	}
}

