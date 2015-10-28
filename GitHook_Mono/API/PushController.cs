using System;
using System.Web.Http;
using System.Collections.Generic;
using GitHook_Mono.Compilers;
using System.Net.Http;
using System.Net;
using GitHook_Mono.GitHub;

namespace GitHook_Mono.API
{
	public class PushController : ApiController
	{
		private static Dictionary<String, MonoProjectCompiler> _compilers = new Dictionary<String, MonoProjectCompiler> ();

		public HttpResponseMessage Post ([FromBody] GitHub_PushMessage project)
		{
			if (project == null || null == project.Repository) return this.Request.CreateResponse (HttpStatusCode.InternalServerError, new {
				Message = "Failed to parse body"
			});
			if (String.IsNullOrEmpty (project.Repository.FullName)) return this.Request.CreateResponse (HttpStatusCode.InternalServerError, new {
				Message = "Failed to get the project name"
			});
			if (project.Commits == null || project.Commits.Length == 0) return this.Request.CreateResponse (HttpStatusCode.InternalServerError, new {
				Message = "No commits supplied"
			});

			lock (_compilers)
			{
				MonoProjectCompiler compiler;
				if (!_compilers.ContainsKey (project.Repository.FullName))
				{
					compiler = new MonoProjectCompiler (project.Repository);
					_compilers.Add (project.Repository.FullName, compiler);
				}
				else compiler = _compilers [project.Repository.FullName];

				foreach (var commit in project.Commits)
				{
					if (String.IsNullOrEmpty (commit.CommitId))
					{
						return this.Request.CreateResponse (HttpStatusCode.InternalServerError, new {
							Message = "Commit was missing SHA field"
						});
					}
					compiler.SheduleCompile (commit.CommitId);
				}
			}

			return this.Request.CreateResponse (HttpStatusCode.OK, new {
				Message = "Rebuilding"
			});
		}
	}
}

