using System.Collections.Generic;

namespace PortraitBuilder.Model.Content
{
    public class Mod : Content
    {

        /// <summary>
        /// Name of .mod file, E.g. mymod.mod
        /// </summary>
        public string ModFile { get; set; }

        /// <summary>
        /// Relative path of the mod root content folder, E.g. mod/mymod/
        /// </summary>
        public string ModPath { get; set; }

        /// <summary>
        /// Mod user_dir optional property
        /// </summary>
        public string UserDir { get; set; }

        public List<string> Dependencies { get; set; } = new List<string>();
        public List<string> Extends { get; set; } = new List<string>();
        public List<string> Replaces { get; set; } = new List<string>();
    }
}