using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

namespace Basements.Patches
{
    [Harmony]
    static class Player_Patches
    {
        const float overlapRadius = 20;

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        [HarmonyPostfix]
        public static void Player_UpdatePlacementGhost(Player __instance, GameObject ___m_placementGhost)
        {
            if (!___m_placementGhost) return;
            var basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (!basementComponent) return;
            if (Basement.allBasements.Count <= 0) return;
            Type type = typeof(Player).Assembly.GetType("Player+PlacementStatus");
            object moreSpace = type.GetField("MoreSpace").GetValue(__instance);
            FieldInfo statusField = __instance.GetType().GetField("m_placementStatus", BindingFlags.NonPublic | BindingFlags.Instance);
            var ol = Basement.allBasements.Where(x => Vector3.Distance(x.transform.position, ___m_placementGhost.transform.position) < overlapRadius).Where(x => x.gameObject != ___m_placementGhost);
            if (ol.Any(x => x.GetComponentInParent<Basement>()) || ___m_placementGhost.transform.position.y > 2500 * Mathf.Max(BasementPlugin.MaxNestedLimit.Value,0) + 2000)
            {
                statusField.SetValue(__instance, moreSpace);
            }

        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions) {
          return new CodeMatcher(instructions)
              .MatchForward(
                  useEnd: false,
                  new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundPiece))),
                  new CodeMatch(OpCodes.Brfalse),
                  new CodeMatch(OpCodes.Ldloc_S),
                  new CodeMatch(OpCodes.Ldnull),
                  new CodeMatch(
                      OpCodes.Call,
                      AccessTools.Method(
                          typeof(UnityEngine.Object), "op_Equality",
                          new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
              .Advance(offset: 4)
              .SetInstructionAndAdvance(
                  Transpilers.EmitDelegate<Func<UnityEngine.Object, UnityEngine.Object, bool>>(
                      OverrideNullEqualityInBasement))
              .MatchForward(
                  useEnd: false,
                  new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundOnly))),
                  new CodeMatch(OpCodes.Brfalse),
                  new CodeMatch(OpCodes.Ldloc_S),
                  new CodeMatch(OpCodes.Ldnull),
                  new CodeMatch(
                      OpCodes.Call,
                      AccessTools.Method(
                          typeof(UnityEngine.Object), "op_Equality",
                          new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) })))
              .Advance(offset: 4)
              .SetInstructionAndAdvance(
                  Transpilers.EmitDelegate<Func<UnityEngine.Object, UnityEngine.Object, bool>>(
                      OverrideNullEqualityInBasement))
              .InstructionEnumeration();
        }

        static bool OverrideNullEqualityInBasement(UnityEngine.Object a, UnityEngine.Object b)
        {
            if (EnvMan.instance.GetCurrentEnvironment().m_name == "Basement")
            {
                return false;
            }
            if (a == null && b == null)
            {
                return false;
            }
            return a == b;
        }
    }
}
