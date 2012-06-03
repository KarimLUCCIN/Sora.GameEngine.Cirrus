using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sora.GameEngine.Cirrus.Design.Packages;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Sora.GameEngine.Cirrus.Design
{
    public class CirrusDesignHelper
    {
        public IEnumerable<XmlCirrusXNAReference> GetDefaultXNAReferences()
        {
            yield return new XmlCirrusXNAReference() { Reference = typeof(Mp3Importer).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(EffectImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(FbxImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(TextureImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(WmvImporter).Assembly.FullName };
            yield return new XmlCirrusXNAReference() { Reference = typeof(XImporter).Assembly.FullName };
        }
    }
}
