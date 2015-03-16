using System.Activities;
using System.Activities.Tracking;

namespace OctopusTFS
{
    internal static class BuildContextExtensions
    {
        public static void WriteBuildMessage(this CodeActivityContext context, string message, BuildMessageImportance messageImportance)
        {
            CodeActivityContext codeActivityContext = context;
            BuildInformationRecord<BuildMessage> informationRecord1 = new BuildInformationRecord<BuildMessage>();
            BuildInformationRecord<BuildMessage> informationRecord2 = informationRecord1;
            BuildMessage buildMessage1 = new BuildMessage();
            buildMessage1.Importance = messageImportance;
            buildMessage1.Message = message;
            BuildMessage buildMessage2 = buildMessage1;
            informationRecord2.Value = buildMessage2;
            BuildInformationRecord<BuildMessage> informationRecord3 = informationRecord1;
            codeActivityContext.Track((CustomTrackingRecord)informationRecord3);
        }
    }
}