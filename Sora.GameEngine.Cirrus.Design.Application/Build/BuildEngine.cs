using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    public class BuildEngine : IBuildEngine
    { 
        public PackageBuilder Builder { get; private set; }

        public BuildEngine(PackageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            Builder = builder;
        }


        #region IBuildEngine Members

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public int ColumnNumberOfTaskNode
        {
            get { return 0; }
        }

        public bool ContinueOnError
        {
            get { return false; }
        }

        public int LineNumberOfTaskNode
        {
            get { return 0; }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Builder.InternalBuild_Message(e.Message, e.SenderName, BuildMessageSeverity.Information);

            if (Builder.cancellationPending)
                CancelBreak();
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Builder.InternalBuild_Message(e.Message, e.SenderName, BuildMessageSeverity.Error);

            if (Builder.cancellationPending)
                CancelBreak();
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Builder.InternalBuild_Message(e.Message, e.SenderName, BuildMessageSeverity.Information);

            if (Builder.cancellationPending)
                CancelBreak();
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Builder.InternalBuild_Message(e.Message, e.SenderName, BuildMessageSeverity.Warning);

            if (Builder.cancellationPending)
                CancelBreak();
        }

        private void CancelBreak()
        {
            throw new Exception("The build operation was cancelled");
        }

        public string ProjectFileOfTaskNode
        {
            get { return "Cirrus"; }
        }

        #endregion
    }
}
