using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Crosstales.RTVoice.EditorTask
{
    /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
    [InitializeOnLoad]
    public class CompileDefines
    {

        private static readonly string[] symbols = new string[] {
            "CT_RTV",
        };

        static CompileDefines()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(symbols.Except(allDefines));

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)