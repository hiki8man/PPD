using System;
using System.IO;
using System.Reflection;

namespace PPDMovie
{
    class Utility
    {
        public static string AppDir
        {
            get
            {
                var location = Assembly.GetExecutingAssembly().Location;
                return string.IsNullOrEmpty(location)
                    ? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    : Path.GetDirectoryName(location);
            }
        }
    }
}
