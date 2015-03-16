using System;
using System.Activities;
using System.Diagnostics;

namespace OctopusTFS
{
    public sealed class CreateOctopusRelease : CodeActivity
    {
        public InArgument<bool> PushDeployment { get; set; }
        public InOutArgument<bool> DeployFailed { get; set; }
        public InArgument<string> OctopusExe { get; set; }
        public InArgument<string> OctopusServerUrl { get; set; }
        public InArgument<int> TimeoutMilliseconds { get; set; }
        public InArgument<string> Project { get; set; }
        public InArgument<string> Version { get; set; }
        public InArgument<string> DeployEnvironment { get; set; }
        public InArgument<bool> WaitForDeployment { get; set; }
        public InArgument<string> ApiKey { get; set; }
        public InArgument<string> NugetPackageId { get; set; }
        public InArgument<string> ProjectVariables { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string octoExe = context.GetValue(OctopusExe);
            string octopusServerUrl = context.GetValue(OctopusServerUrl);
            string project = context.GetValue(Project);
            string deployEnvironment = context.GetValue(DeployEnvironment);
            string apiKey = context.GetValue(ApiKey);
            bool wait = context.GetValue(WaitForDeployment);
            int timeout = context.GetValue(TimeoutMilliseconds);
            string version = context.GetValue(Version);
            bool pushDeployment = context.GetValue(PushDeployment);
            string projectVariables = context.GetValue(ProjectVariables);

            if(apiKey == string.Empty)
                throw new InvalidOperationException("Octopus requires an API Key to be specified.");

            if(version == string.Empty)
                throw new InvalidOperationException("Version must be specified.");

            var buildDetail = context.GetExtension<IBuildDetail>();
            string buildNumber = buildDetail.BuildNumber;
            string args =
                string.Format(
                    @"create-release -server={0} -project=""{1}""{2}{3}{4}{5}{6} --apiKey={7}"
                    , octopusServerUrl
                    , project
                    , version == string.Empty ? "" : string.Format(" -version={0}", version)
                    , version == string.Empty ? "" : string.Format(" -packageVersion={0}", version)
                    , pushDeployment ? string.Format(" -deployto={0}", deployEnvironment) : ""
                    , wait ? " -waitfordeployment" : ""
                    , projectVariables == string.Empty ? "" : string.Format(@" -v ""{0}""", projectVariables)
                    , apiKey);


            try
            {
                using (Process nugetProcess = new Process())
                {
                    nugetProcess.StartInfo.FileName = octoExe == string.Empty
                        ? "octo"
                        : octoExe;

                    nugetProcess.StartInfo.Arguments = args;
                    nugetProcess.StartInfo.RedirectStandardError = true;
                    nugetProcess.StartInfo.RedirectStandardOutput = true;
                    nugetProcess.StartInfo.UseShellExecute = false;
                    nugetProcess.StartInfo.CreateNoWindow = true;

                    string exeCmd = string.Format("{0} {1}", nugetProcess.StartInfo.FileName, args);

                    context.WriteBuildMessage("Starting Octopus exe: " + exeCmd, BuildMessageImportance.High);
                    
                    if(pushDeployment)
                        context.WriteBuildMessage(string.Format("Project will be auto-deployed to environment {0}.", deployEnvironment) , BuildMessageImportance.High);
                    else
                        context.WriteBuildMessage("Project will NOT be auto-deployed but will be made into a release within Octopus.", BuildMessageImportance.High);

                    nugetProcess.Start();

                    if (wait)
                        context.WriteBuildMessage(string.Format("Waiting for Octopus deployment ({0} milliseconds)",
                            timeout), BuildMessageImportance.High);

                    nugetProcess.WaitForExit(timeout);
                    context.WriteBuildMessage(nugetProcess.StandardOutput.ReadToEnd(), BuildMessageImportance.High);

                    if (!nugetProcess.HasExited)
                    {
                        throw new Exception(string.Format("Octopus deploy for {0} timed out.", project));
                    }
                    if (nugetProcess.ExitCode != 0)
                    {
                        string err = nugetProcess.StandardError.ReadToEnd();
                        if (err == string.Empty) err = nugetProcess.StandardOutput.ReadToEnd();

                        throw new Exception("Octopus deploy failed: " + err);
                    }
                }
            }
            catch (Exception ex)
            {
                context.SetValue(DeployFailed, true);
                context.WriteBuildMessage(string.Format("Nuget args: {0}", args), BuildMessageImportance.High);
                context.TrackBuildError(ex.ToString());
            }
        }
    }
}
