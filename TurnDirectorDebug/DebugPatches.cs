using BattleTech;
using Harmony;

namespace nl.flukeyfiddler.bt.TurnDirectorDebug
{
    [HarmonyPatch(typeof(AIUtil), "IsExposedToHostileFire")]
    public class AIUtil_IsExposedToHostileFire_Patch_Debug_Force_Fail
    {
        private static bool brokeIt = false;

        public static void Prefix(ref AbstractActor unit)
        {
            if (!brokeIt)
            {
               unit = null;
               //brokeIt = true;
            }
        }
    }
}
