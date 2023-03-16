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
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class TurretsPysicsPatch : BaseUnityPlugin
    { 
        private const string ModId = "TheCampingTurret.Rounds.TCTPP.patch";
        private const string ModName = "TurretsPysicsPatch";
        public const string Version = "2.2.4";
 
        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            HarmonyFileLog.Enabled = true;
            new Harmony(ModId).PatchAll();

            // get the MethodBase of the original
            var original = typeof(MoveTransform).GetMethod("Update");
            FileLog.Log("hello4");
            // retrieve all patches
            var patches = Harmony.GetPatchInfo(original);
            FileLog.Log("hello6");
            if (patches is null)
            {
                FileLog.Log("not pached");
                return; // not patched
            }
            FileLog.Log("hello5");
            // get a summary of all different Harmony ids involved
            FileLog.Log("all owners: " + patches.Owners);

            // get info about all Prefixes/Postfixes/Transpilers
            foreach (var patch in patches.Prefixes)
            {
                FileLog.Log("index: " + patch.index);
                FileLog.Log("owner: " + patch.owner);
                FileLog.Log("patch method: " + patch.PatchMethod);
                FileLog.Log("priority: " + patch.priority);
                FileLog.Log("before: " + patch.before);
                FileLog.Log("after: " + patch.after);
            }
        }

    }

    [HarmonyPatch(typeof(MoveTransform), "Update")]
    public class Patch_MoveTransform
    {


        [HarmonyPatch(typeof(MoveTransform), "Update")]
        private static bool Prefix(MoveTransform __instance)
        {
            float simspeed = (float)Traverse.Create(__instance).Field("simulationSpeed").GetValue();
            Debug.Log("hello");
            float t = Time.deltaTime * simspeed + __instance.GetAdditionalData().msleft;
            float dt = 1f / 60f;
            Vector3 av;
            Vector3 nv;
            Vector3 ag;
            Vector3 a;
            Vector3 steppos;
            while (t > dt) {
                
                t = t - dt;
                
                nv = __instance.velocity;
                if (__instance.drag < 0.01)
                {
                    ag = Vector3.down * __instance.gravity * __instance.multiplier;
                    a = ag;
                    nv += ag * dt;
                    steppos = a * dt * dt * 0.5f + __instance.velocity * dt;
                }
                else
                {

                    ag = Vector3.down * __instance.gravity * __instance.multiplier;
                    av = -Vector3.Normalize(__instance.velocity) * __instance.velocity.magnitude * __instance.velocity.magnitude * __instance.multiplier * __instance.drag/200;
                    a = ag+av;
                    nv += a * dt;
                    steppos = a * dt * dt * 0.5f + __instance.velocity * dt;
                    
                }

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


