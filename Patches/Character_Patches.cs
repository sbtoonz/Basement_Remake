using HarmonyLib;

namespace Basements.Patches
{
    [Harmony]
    class Character_Patches
    {
        [HarmonyPatch(typeof(Character), "InInterior")]
        [HarmonyPostfix]
        static void Character_InInterior(Character __instance, ref bool __result)
        {
            if (Player.m_localPlayer)
            {
                if (__instance == Player.m_localPlayer)
                {
                    if (EnvMan.instance.GetCurrentEnvironment().m_name == "Basement")
                        __result = false;
                }
            }
        }
    }
}