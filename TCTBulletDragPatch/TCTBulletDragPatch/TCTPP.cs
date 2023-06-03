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
using Photon.Compression;
using ExtensionMethods;
using System.Collections.Generic;
using TurretsPhysicsPatch;


namespace TurretsPhysicsPatch
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class TurretsPhysicsPatch : BaseUnityPlugin
    { 
        private const string ModId = "TheCampingTurret.Rounds.TCTPP.patch";
        private const string ModName = "TurretsPhysicsPatch";
        public const string Version = "0.0.6";
 
        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            
            new Harmony(ModId).PatchAll();
             

        }

    }
    /// <summary>
    /// Both function and positionfunction have to be implimented if custom function is defined and used
    /// 
    /// position function is for dS 
    /// function is for dV
    /// 
    /// position function is called first
    /// </summary>
    public class Bullet_Motion_Physiscs_Effect
    {
        public Bullet_Motion_Physiscs_Effect(float t, Vector3 dir,float mag)
        {
            triggertime = t;
            direction = dir;
            magnitude = mag;
        }

        public virtual (Vector3, bool) function(float dt) { return (new Vector3(0, 0, 0), false); }
        public virtual Vector3 positonfunction(float dt) { return new Vector3(0, 0, 0); }

        public float triggertime;
        public Vector3 direction;
        public float magnitude;
    }

    public class Dirac : Bullet_Motion_Physiscs_Effect
    {

        public Dirac(float t, Vector3 dir, float mag) : base(t,dir,mag)
        {

        }
        public override (Vector3,bool) function(float dt)
        {
            return (direction * magnitude ,false);
        }
        public override Vector3 positonfunction(float dt)
        {
            return new Vector3(0, 0, 0);
        }
    }
    public class Square_pulse : Bullet_Motion_Physiscs_Effect
    {

        public Square_pulse(float t, Vector3 dir, float mag, float dur) : base(t, dir, mag)
        {
            duration = dur;
        }
        public override (Vector3, bool) function(float dt)
        {
            float width;
            if(duration > dt) { width = dt; }
            else { width = duration; }
            Vector3 dV = direction * magnitude*width;
            duration -= dt;
            if(duration < 0) { conti = false; }

            return (dV, conti);
        }
        public override Vector3 positonfunction(float dt)
        {
            float width;
            if (duration > dt) { width = dt; }
            else { width = duration; }
            Vector3 dS = direction * magnitude * width * width *0.5f;
            return dS;
        }
        float duration;
        bool conti = true;
    }
    public class Step_pulse : Bullet_Motion_Physiscs_Effect
    {

        public Step_pulse(float t, Vector3 dir, float mag) : base(t, dir, mag)
        {
        }
        public override (Vector3, bool) function(float dt)
        {
            Vector3 dV = direction * magnitude * dt;

            return (dV, true);
        }
        public override Vector3 positonfunction(float dt)
        {
            Vector3 dS = direction * magnitude * dt * dt * 0.5f;
            return dS;
        }
    }

    [HarmonyPatch(typeof(MoveTransform), "Update")]
    public class Patch_MoveTransform
    {


        [HarmonyPatch(typeof(MoveTransform), "Update")]
        private static bool Prefix(MoveTransform __instance)
        {
            float simspeed = (float)Traverse.Create(__instance).Field("simulationSpeed").GetValue();
            float t = Time.deltaTime + __instance.GetAdditionalData().msleft;
            float tsys = Time.time;
            float dtt = 1f / 70f;
            float dt = dtt * simspeed;
            Vector3 Effectstep = new Vector3(0,0,0);
            Vector3 av;
            Vector3 nv;
            Vector3 ag;
            Vector3 a;
            Vector3 steppos;
            
            //Gravity
            if (__instance.simulateGravity == 0) { ag = Vector3.down * __instance.gravity * __instance.multiplier; } else { ag = new Vector3(0, 0, 0); }


            while (t > dtt) {
                
                t = t - dtt;
                tsys = tsys + dtt;
                nv = __instance.velocity;

                //Inputs
                if (__instance.GetAdditionalData().Waitingforeffect)
                {
                    List<Bullet_Motion_Physiscs_Effect> L = __instance.GetAdditionalData().WaitList;
                    foreach (var Effect in L) 
                    {
                        if (Effect.triggertime < tsys)
                        {
                            Effectstep += Effect.positonfunction(dtt);
                            (Vector3 Vinc, bool cont ) = Effect.function(dtt);                           
                            nv += Vinc;
                            if (!cont)
                            {
                                L.Remove(Effect);
                            }
                        }
                    }
                    if (L.Count == 0)
                    {

                    }
                }

                //Drag
                if (__instance.drag < 0.01)
                {
                    av = new Vector3(0, 0, 0);   
                }                
                else
                {
                    av = -__instance.velocity.normalized * __instance.velocity.sqrMagnitude * __instance.multiplier * __instance.drag * 0.015f;
                }


                //Integration
                a = ag + av;
                steppos = a * dt * dt * 0.5f + __instance.velocity * dt;
                nv += a * dt;

                //
                __instance.distanceTravelled += steppos.magnitude + Effectstep.magnitude;
                __instance.transform.position = __instance.transform.position + steppos + Effectstep;
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
        public bool Waitingforeffect;
        public List<Bullet_Motion_Physiscs_Effect> WaitList;

        public MoveTransformAdditionalData()
        {
            float msleft = 0;
            bool Waitingforeffect = false;
            List <Bullet_Motion_Physiscs_Effect> WaitList = new List<Bullet_Motion_Physiscs_Effect>();
            
 
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

namespace ExtensionMethods
{
    public static class MyExtensionMethods
    {
        /// <summary>
        /// Adds a dirac pulse to the velocity, This is not syncronised between players
        /// </summary>
        /// <param name="Impulse"></param>
        public static void Impulse_Dirac_Pulse(this MoveTransform movetransform, Vector3 Impulse)
            
        {

            Dirac Effect = new Dirac(Time.time,Impulse.normalized,Impulse.magnitude);
            movetransform.GetAdditionalData().WaitList.Add(Effect);
            
        }

        /// <summary>
        /// Adds a dirac pulse to the velocity, This is delayed to the time set (semi sync-able) <br></br><br></br>
        ///
        /// Just to be clear, time is compaired with Time.time. It is not a countdown to 0.
        /// </summary>
        /// <param name="Impulse"></param>
        /// <param name="time"></param>
        public static void Impulse_Dirac_Pulse(this MoveTransform movetransform, Vector3 Impulse,float time)
        {

            Dirac Effect = new Dirac(Time.time, Impulse.normalized, Impulse.magnitude);
            movetransform.GetAdditionalData().WaitList.Add(Effect);

        }
        /// <summary>
        /// Adds a square step pulse to the velocity, This is delayed to the time set (semi sync-able), does not account for acceleration during timestep <br></br><br></br>
        ///
        /// Just to be clear, time is compaired with Time.time. It is not a countdown to 0.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="dir"></param>
        /// <param name="magnitude"></param>
        /// <param name="duration"></param>
        public static void Impulse_Square_Pulse(this MoveTransform movetransform,float time, Vector3 dir, float magnitude, float duration)
        {
            Square_pulse Effect = new Square_pulse(Time.time, dir, magnitude, duration);
            movetransform.GetAdditionalData().WaitList.Add(Effect);

        }
        /// <summary>
        /// Adds a step pulse to the velocity, This is delayed to the time set (semi sync-able), does not account for acceleration during timestep <br></br><br></br>
        ///
        /// Just to be clear, time is compaired with Time.time. It is not a countdown to 0.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="dir"></param>
        /// <param name="magnitude"></param>
        public static void Impulse_Step_Pulse(this MoveTransform movetransform, float time, Vector3 dir, float magnitude)
        {
            Step_pulse Effect = new Step_pulse(Time.time, dir, magnitude);
            movetransform.GetAdditionalData().WaitList.Add(Effect);
        }
    }
}



