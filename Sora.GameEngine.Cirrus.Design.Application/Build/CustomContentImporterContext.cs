using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Sora.GameEngine.Cirrus.Design.Application.Editor;

namespace Sora.GameEngine.Cirrus.Design.Application.Build
{
    public class CustomContentImporterContext : ContentImporterContext
    {
        public PackageBuilder Builder { get; private set; }
        public EditorApplication Editor { get; private set; }

        public CustomContentImporterContext(PackageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("build");

            Builder = builder;
            Editor = Builder.Editor;
        }

        public override void AddDependency(string filename)
        {
            Builder.XNAAddDependency(filename);
        }

        public override string IntermediateDirectory
        {
            get { return Builder.XNAIntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return Builder.XNALogger; }
        }

        public override string OutputDirectory
        {
            get { return Builder.XNAOutputDirectory; }
        }
    }
}
