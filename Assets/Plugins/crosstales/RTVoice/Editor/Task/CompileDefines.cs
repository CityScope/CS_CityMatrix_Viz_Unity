using UnityEditor;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
    [InitializeOnLoad]
    public class CompileDefines : Common.EditorTask.BaseCompileDefines
    {

        private static readonly string[] symbols = new string[] {
            "CT_RTV",
        };

        static CompileDefines()
        {
            setCompileDefines(symbols);
        }
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)