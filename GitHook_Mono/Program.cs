using System;
using System.Threading;
using GitHook_Mono.Config.App;
using GitHook_Mono.Plugin;
using Newtonsoft.Json;
using System.IO;
using System.Web.Http;
using System.Net.Http.Formatting;
using Owin;
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography;

namespace GitHook_Mono
{
	internal class MainClass
	{
		internal static readonly AutoResetEvent Switch = new AutoResetEvent (false);

		internal static AppConfiguration Config { get; private set; }

		internal static PluginManager Plugins;

		internal static T LoadConfig<T> (string filePath) where T : class
		{
			if (File.Exists (filePath))
			{
				return JsonConvert.DeserializeObject<T> (File.ReadAllText (filePath));
			}

			return null;
		}

		public static void Main (string[] args)
		{
			//Load applications the config
			Config = LoadConfig<AppConfiguration> ("config.json");

			//Prepare plugins
			Plugins = new PluginManager ("Plugins");
			Plugins.LoadPlugins ();

			#if !DEBUG
			//Prepare the SQLite DB
			Data.DbContextExtensions.TryCopySqliteDependencies ();
			using (var db = new GitHook_Mono.Data.RepoContext ())
			{
				db.Database.EnsureCreated ();
			}
			#endif

			//Ready to build - start the webserver and wait for GitHub to contact us.
			Console.WriteLine ("Starting web server");
			try
			{
				var baseAddress =
					Config != null && !String.IsNullOrEmpty (Config.ListenAddress) ? Config.ListenAddress : "http://*:7447/";

				using (Microsoft.Owin.Hosting.WebApp.Start<OWINServer> (url: baseAddress as String))
				{
					Console.WriteLine ("Web server started listening on {0}", baseAddress);
					Switch.WaitOne ();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e);
			}
		}
	}

	class OWINServer
	{
		public void Configuration (Owin.IAppBuilder app)
		{
			var config = new System.Web.Http.HttpConfiguration ();

			config.Routes.MapHttpRoute (
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = System.Web.Http.RouteParameter.Optional }
			);

			config.MapHttpAttributeRoutes ();

			config.Formatters.Clear ();
			config.Formatters.Add (new JsonMediaTypeFormatter ());

			app.UseWebApi (config);
		}

		/// <summary>
		/// A mono compatible data protector for use with OWIN
		/// </summary>
		internal class MonoDataProtector : IDataProtector
		{
			private string[] _purposes;
			const String DefaultPurpose = "ota-web-dp";

			public MonoDataProtector ()
			{
				_purposes = null;
			}

			//            public MonoDataProtector(string[] purposes)
			//            {
			//                _purposes = purposes;
			//            }

			public MonoDataProtector (params string[] purposes)
			{
				_purposes = purposes;
			}

			public byte[] Protect (byte[] data)
			{
				return System.Security.Cryptography.ProtectedData.Protect (data, this.GenerateEntropy (), DataProtectionScope.CurrentUser);
			}

			public byte[] Unprotect (byte[] data)
			{
				return System.Security.Cryptography.ProtectedData.Unprotect (data, this.GenerateEntropy (), DataProtectionScope.CurrentUser);
			}

			byte[] GenerateEntropy ()
			{
				using (var hasher = SHA256.Create ())
				{
					using (var ms = new MemoryStream ())
					using (var cr = new CryptoStream (ms, hasher, CryptoStreamMode.Write))
					using (var sw = new StreamWriter (cr))
					{
						//Default purpose 
						sw.Write (DefaultPurpose);

						if (_purposes != null)
							foreach (var purpose in _purposes)
							{
								sw.Write (purpose);
							}
					}

					return hasher.Hash;
				}
			}
		}
	}
}
