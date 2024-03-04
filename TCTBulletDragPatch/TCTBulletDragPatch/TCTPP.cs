using System;
using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using TurretsPhysicsPatch;
using Unity.Collections;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace TurretsPhysicsPatch
{
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class TurretsPhysicsPatch : BaseUnityPlugin
    {
        private const string ModId = "TheCampingTurret.Rounds.TCTPP.patch";
        private const string ModName = "TurretsPhysicsPatch";
        public const string Version = "0.2.0";

        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            
            new Harmony(ModId).PatchAll(Assembly.GetExecutingAssembly());

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
            float dtt = 1f / 70f;
            if (t > dtt)
            {
                float dt = dtt * simspeed;
                float time_since_started = __instance.GetAdditionalData().time_since_started;
                Vector2 V0 = __instance.velocity;
                Vector2 P0 = __instance.transform.position;
                int grav_sim = __instance.simulateGravity;
                float grav = __instance.gravity;
                float mult = __instance.multiplier;
                float cd = __instance.drag * 0.0075f;
                float ag;
                if (grav_sim == 0) { ag = -grav * mult; } else { ag = 0; }
                if (cd < 0.000001f && !__instance.GetAdditionalData().accelerationeffect)
                {
                    while (t > dtt)
                    {

                        Vector2 newx = TurretsPhysicsExtensions.rungekutta4(new Vector2(P0.x, V0.x), time_since_started, dtt, 0);
                        Vector2 newy = TurretsPhysicsExtensions.rungekutta4(new Vector2(P0.y, V0.y), time_since_started, dtt, ag);
                        Vector2 newpos = new Vector2(newx.x, newy.x);
                        __instance.distanceTravelled += Vector2.Distance(P0, newpos);
                        __instance.transform.position = newpos;
                        __instance.velocity = new Vector3(newx.y, newy.y);
                        __instance.transform.rotation = Quaternion.LookRotation(__instance.velocity, Vector3.forward);
                        P0 = __instance.transform.position;
                        V0 = __instance.velocity;
                        t = t - dtt;
                        time_since_started += dtt;
                    }
                }
                else
                {
                    Func<float, float, float> drag = (t, v) => -cd * Math.Abs(v) * v;

                    Func<float, float, float> Ydir;
                    Func<float, float, float> Xdir;

                    if (__instance.GetAdditionalData().accelerationeffect)
                    {
                        Ydir = (t, v) => { float x = drag(t, v) + ag; foreach (Func<float, float, float> eq in __instance.GetAdditionalData().a_y) { x += eq(t, v); } return x; };
                        Xdir = (t, v) => { float x = drag(t, v); foreach (Func<float, float, float> eq in __instance.GetAdditionalData().a_x) { x += eq(t, v); } return x; };
                    }

                    else
                    {
                        Ydir = (t, v) => drag(t, v) + ag;
                        Xdir = (t, v) => drag(t, v);
                    }



                    while (t > dtt)
                    {
                        Vector2 newx = TurretsPhysicsExtensions.rungekutta4(new Vector2(P0.x, V0.x), time_since_started, dtt, Xdir);
                        Vector2 newy = TurretsPhysicsExtensions.rungekutta4(new Vector2(P0.y, V0.y), time_since_started, dtt, Ydir);
                        Vector2 newpos = new Vector2(newx.x, newy.x);
                        __instance.distanceTravelled += Vector2.Distance(P0, newpos);
                        __instance.transform.position = newpos;
                        __instance.velocity = new Vector3(newx.y, newy.y);
                        __instance.transform.rotation = Quaternion.LookRotation(__instance.velocity, Vector3.forward);
                        P0 = __instance.transform.position;
                        V0 = __instance.velocity;
                        t = t - dtt;
                        time_since_started += dtt;
                    }
                }
                __instance.GetAdditionalData().time_since_started = time_since_started;
            }
            __instance.GetAdditionalData().msleft = t;
            return false;
        }

    }

    
    [Serializable]
    public class MoveTransformAdditionalData
    {
        public float msleft;
        public float time_since_started;
        public bool accelerationeffect;
        public List<Func<float,float,float>> a_x;
        public List<Func<float, float, float>> a_y;

        public MoveTransformAdditionalData()
        {
            msleft = 0;
            accelerationeffect = false;
            time_since_started = 0;
            a_x = new List<Func<float, float, float>>() {};
            a_y = new List<Func<float, float, float>>() {};
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
    
    public static class TurretsPhysicsExtensions
    {

        public static Vector2 rungekutta4(Vector2 dir, float t, float dtt, Func<float, float, float> func)
        {
            float v = dir.y;
            Vector2 f1 = new Vector2(v, func(t, v));
            Vector2 f2 = new Vector2(v + dtt / 2 * f1.y, func(t + dtt / 2, v + dtt / 2 * f1.y));
            Vector2 f3 = new Vector2(v + dtt / 2 * f2.y, func(t + dtt / 2, v + dtt / 2 * f2.y));
            Vector2 f4 = new Vector2(v + dtt * f3.y, func(t + dtt, v + dtt * f3.y));
            Vector2 next = dir + dtt / 6 * (f1 + 2 * f2 + 2 * f3 + f4);
            return next;
        }
        public static Vector2 rungekutta4(Vector2 dir, float t, float dtt, float func)
        {
            return new Vector2(dir.x + dtt*dir.y + dtt * dtt * func / 2, dir.y + dtt * func);
        }


        /// <summary>
        /// Adds a constant acceleration to the bullet, applied on the starttime since the bullet spawned.
        /// </summary>
        /// <param name="accelerations"></param>
        /// <param name="starttime"></param>
        public static void add_constant_acceleration(this MoveTransform movetransform, Vector3 accelerations, float starttime)
            
        {
            //UnityEngine.Debug.Log(movetransform.GetAdditionalData().a_x.ToString());
            movetransform.GetAdditionalData().a_x.Add((t, v) => { if (t > starttime) { return accelerations.x; } else return 0; });
            movetransform.GetAdditionalData().a_y.Add((t, v) => { if (t > starttime) { return accelerations.y; } else return 0; });
            movetransform.GetAdditionalData().accelerationeffect = true;
            
        }
        /// <summary>
        /// Adds an equation for acceleration to the bullet, applied on the starttime since the bullet spawned.
        /// The equation must be in the same form as the drag.
        /// 
        /// Drag:  Func<float, float, float> drag = (t, v) => -cd * Math.Abs(v) * v / 2;
        /// </summary>
        /// <param name="List_of_equations"></param>
        /// <param name="starttime"></param>
        public static void add_variate_acceleration(this MoveTransform movetransform, List<Func<float,float,float>> List_of_equations, float starttime)

        {
            movetransform.GetAdditionalData().a_x.Add((t, v) => { if (t > starttime) { return List_of_equations[0](t, v); } else return 0; });
            movetransform.GetAdditionalData().a_y.Add((t, v) => { if (t > starttime) { return List_of_equations[1](t, v); } else return 0; });
            movetransform.GetAdditionalData().accelerationeffect = true;
        }
        /// <summary>
        /// Adds an equation for acceleration to the bullet, applied on the starttime since the bullet spawned.
        /// The equation must be in the same form as the drag.
        /// 
        /// Drag:  Func<float, float, float> drag = (t, v) => -cd * Math.Abs(v) * v / 2;
        /// 
        /// direction: 'x' for x, 'y' for y
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="direction"></param>
        /// <param name="starttime"></param>
        public static void add_variate_acceleration(this MoveTransform movetransform, Func<float, float, float> equation, char direction, float starttime)
        {
            if (direction == 'x')
            {
                movetransform.GetAdditionalData().a_x.Add((t, v) => { if (t > starttime) { return equation(t, v); } else return 0; });
                movetransform.GetAdditionalData().accelerationeffect = true;
            }
            else if (direction == 'y')
            {
                movetransform.GetAdditionalData().a_y.Add((t, v) => { if (t > starttime) { return equation(t, v); } else return 0; });
                movetransform.GetAdditionalData().accelerationeffect = true;
            }
            else
            {
                throw new Exception("Incorrect direction set");
            }
        }
    }
}



