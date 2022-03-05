using HarmonyLib;

namespace Basements.Patches
{
    [Harmony]
    class EnvMan_Patches
    {
        [HarmonyPatch(typeof(EnvMan), "Awake")]
        [HarmonyPostfix]
        public static void EnvMan_Awake()
        {
            EnvSetup basementEnv = EnvMan.instance.m_environments.Find(x => x.m_name == "Crypt").Clone();
            basementEnv.m_name = "Basement";
            EnvMan.instance.m_environments.Add(basementEnv);
        }
    }
}
