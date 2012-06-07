using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sora.GameEngine.Cirrus
{
    public class CirrusContentManager
    {
        /// <summary>
        /// Prefix happened to the output path
        /// </summary>
        /// <remarks>This prevents clearing a special folder if for exemple the output path is set on desktop</remarks>
        public static string OutputDirectorySuffix
        {
            get
            {
                return "BuiltContent";
            }
        }

        public static string ContentFileExtention
        {
            get
            {
                return ".xnb";
            }
        }
    }
}
