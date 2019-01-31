using System.Collections.Generic;

namespace Paillave.Etl.Recipes.DefineProcess.StreamTypes.Config
{
    public class SimpleConfigStreamType
    {
        public int Divider { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}