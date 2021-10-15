using System.Collections.Generic;

namespace PostgresDeployer.Contracts
{
    public class ScriptObjectFile
    {
        public string Name { get; set; }
        public List<string> FK { get; set; }
        public string FileName { get; set; }
        public ScriptType scriptType { get; set; }
    }
}
