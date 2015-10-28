using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace GitHook_Mono.Plugin
{
	public class PluginManager
	{
		/// <summary>
		/// The directory where plugins are located.
		/// </summary>
		private string _directory;

		/// <summary>
		/// The plugins.
		/// </summary>
		private System.Collections.Concurrent.ConcurrentStack<BasePlugin> _plugins;

		public PluginManager (string directory)
		{
			this._directory = directory;
			_plugins = new System.Collections.Concurrent.ConcurrentStack<BasePlugin> ();
		}

		public void LoadPlugins ()
		{
			//Ensure our working path exists
			if (!Directory.Exists (_directory)) Directory.CreateDirectory (_directory);

			//Load all plugins from the specified directory
			foreach (var file in Directory.GetFiles(_directory, "*.dll"))
			{
				try
				{
					//Load the .NET assembly
					var asm = Assembly.LoadFile (file);

					//Find the plugin types and create them
					foreach (var type in asm.GetTypes().Where(x => typeof(BasePlugin).IsAssignableFrom(x) && !x.IsAbstract))
					{
						try //Catch type exceptions
						{
							var plg = (BasePlugin)Activator.CreateInstance (type);
							_plugins.Push (plg);
						}
						catch (Exception le)
						{
							Console.WriteLine (le);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine (e);
				}
			}

			//Initialise plugins
			ForEachPlugin ((plugin) =>
			{
				plugin.Initialise();
			});
			Console.WriteLine ($"Loaded {_plugins.Count} plugin(s).");
		}

		public void ForEachPlugin(Action<BasePlugin> callback)
		{
			foreach (var plugin in _plugins)
				callback (plugin);
		}
	}
}

