using Agents;
using FX_EffectSystem;
using Gear;
using HarmonyLib;
using Player;
using System.Numerics;
using System.Runtime.Intrinsics;
using UnityEngine;
using static Weapon;
namespace Armament.MuzzleProjectileAlignment
{
    [HarmonyPatch]
    internal class FirePatches
    {
        private static BulletWeapon? weapon;

        [HarmonyPatch(typeof(Weapon), nameof(Weapon.CastWeaponRay), new Type[]
        {
        typeof(Transform),
        typeof(Weapon.WeaponHitData),
        typeof(UnityEngine.Vector3),
        typeof(int)
        }, new ArgumentType[]
        {
        ArgumentType.Normal,
        ArgumentType.Ref,
        ArgumentType.Normal,
        ArgumentType.Normal
        })]
        [HarmonyPrefix]
        private static void CastWeaponRay(ref Transform alignTransform, ref Weapon.WeaponHitData weaponRayData, ref UnityEngine.Vector3 originPos, int altRayCastMask)
        {
            if (weapon == null) return;
            UnityEngine.Vector3 position = weapon.MuzzleAlign.position;
            position = weapon.Owner.FPSCamera.Position;
            alignTransform = weapon.MuzzleAlign;
            weaponRayData.fireDir = (!weapon.FPItemHolder.ItemAimTrigger) ? weapon.MuzzleAlign.forward : (weapon.Owner.FPSCamera.CameraRayPos - position).normalized;
        }
        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.Fire))]
        [HarmonyPrefix]
        private static void Pre_Fire(BulletWeapon __instance, bool resetRecoilSimilarity = true)
        {
            weapon = __instance;
        }
        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.Fire))]
        [HarmonyPostfix]
        private static void Post_Fire(BulletWeapon __instance)
        {
            weapon = null;
        }
        [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Fire))]
        [HarmonyPrefix]
        private static void Pre_Shotgun_Fire(Shotgun __instance)
        {
            weapon = __instance;
        }
        [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.Fire))]
        [HarmonyPostfix]
        private static void Post_Shotgun_Fire(Shotgun __instance)
        {
            weapon = null;
        }
    }
}

