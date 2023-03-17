using System;
using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using HarmonyLib;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib.Tools;
using UnityEngine;
using static UnityEngine.Experimental.Rendering.RenderPass;
using System.Runtime.CompilerServices;

namespace TurretsPysicsPatch
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class TurretsPysicsPatch : BaseUnityPlugin
    { 
        private const string ModId = "TheCampingTurret.Rounds.TCTPP.patch";
        private const string ModName = "TurretsPysicsPatch";
        public const string Version = "0.4.5";
 
        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            
            new Harmony(ModId).PatchAll();
             

        }

    }

    [HarmonyPatch(typeof(MoveTransform), "Update")]
    public class Patch_MoveTransform
    {


        [HarmonyPatch(typeof(MoveTransform), "Update")]
        private static bool Prefix(MoveTransform __instance)
        {
            float simspeed = (float)Traverse.Create(__instance).Field("simulationSpeed").GetValue();
            float t = Time.deltaTime * simspeed + __instance.GetAdditionalData().msleft;
            float dt = 1f / 70f;
            Vector3 av;
            Vector3 nv;
            Vector3 ag;
            Vector3 a;
            Vector3 steppos;
            while (t > dt) {
                
                t = t - dt;
                
                nv = __instance.velocity;
                if (__instance.simulateGravity == 0) { ag = Vector3.down * __instance.gravity * __instance.multiplier; } else { ag = new Vector3(0, 0, 0); }
                if (__instance.drag < 0.01)
                {
                    av = new Vector3(0, 0, 0);   
                }                
                else
                {
                    av = -Vector3.Normalize(__instance.velocity) * __instance.velocity.magnitude * __instance.velocity.magnitude * __instance.multiplier * __instance.drag/100;
                }
                a = ag + av;
                steppos = a * dt * dt * 0.5f + __instance.velocity * dt;
                nv += a * dt;

                __instance.distanceTravelled += steppos.magnitude;
                __instance.transform.position = __instance.transform.position + steppos;
                __instance.velocity = nv;
                __instance.transform.rotation = Quaternion.LookRotation(__instance.velocity, Vector3.forward);

            }
            __instance.GetAdditionalData().msleft = t;
            return false;
        }

    }

    
    [Serializable]
    public class MoveTransformAdditionalData
    {
        public float msleft;

        public MoveTransformAdditionalData()
        {
            float msleft = 0;
        }
    }

    public static class MoveTransformExtension
    {
        private static readonly ConditionalWeakTable<MoveTransform, MoveTransformAdditionalData> data =
            new ConditionalWeakTable<MoveTransform, MoveTransformAdditionalData>();

        public static MoveTransformAdditionalData GetAdditionalData(this MoveTransform movetransform)
        {
            return data.GetOrCreateValue(movetransform);
        }

        public static void AddData(this MoveTransform movetransform, MoveTransformAdditionalData value)
        {
            try
            {
                data.Add(movetransform, value);
            }
            catch (Exception) { }
        }
    }
    
}


