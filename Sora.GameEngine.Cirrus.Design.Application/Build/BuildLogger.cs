using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    public class BuildLogger : ContentBuildLogger
    {
        public PackageBuilder Builder { get; private set; }

        public BuildLogger(PackageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            Builder = builder;
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            LogMessage(message, messageArgs);
        }

        public override void LogMessage(string message, params object[] messageArgs)
        {
            Builder.InternalBuild_Message(String.Format(message, messageArgs), "BuildLogger", BuildMessageSeverity.Information);
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            Builder.InternalBuild_Message(String.Format(message, messageArgs), "BuildLogger", BuildMessageSeverity.Warning);
        }
    }
}
