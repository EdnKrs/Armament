using HarmonyLib;
using Player;
using UnityEngine;
using Agents;
namespace Armament.ArmorPiercingFragmentation
{
    [HarmonyPatch]
    public class ArmorPiercingFragmentationPatches
    {
        public static ItemEquippable? wieldedItem = null;
        public static bool fromSentry = false;
        public static float ArmorDmgMult;

        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireSemi))]
        [HarmonyPrefix]
        public static void PreUpdateFireSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (Clock.Time > __instance.m_fireBulletTimer)
            {
                fromSentry = true;
            }
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireSemi))]
        [HarmonyPostfix]
        public static void PostUpdateFireSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            fromSentry = false;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireAuto))]
        [HarmonyPrefix]
        public static void PreUpdateFireAuto(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (Clock.Time > __instance.m_fireBulletTimer)
            {
                fromSentry = true;
            }
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireBurst))]
        [HarmonyPrefix]
        public static bool UpdateFireBurst(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (!(Clock.Time > __instance.m_burstTimer))
            {
                return true;
            }
            fromSentry = true;
            if (__instance.m_burstClipCurr > 0)
            {
                if (__instance.m_burstClipCurr == __instance.m_archetypeData.BurstShotCount)
                {
                    __instance.TriggerBurstFireAudio();
                }
                if (Clock.Time > __instance.m_fireBulletTimer && __instance.m_core.Ammo >= __instance.m_core.CostOfBullet)
                {
                    __instance.FireBullet(isMaster, targetIsTagged);
                    __instance.UpdateAmmo(-1);
                    __instance.m_burstClipCurr--;
                    ShellTypes ejectTypeBurst = ShellTypes.Shell_338;
                    float sizeBurst = 1.0f;
                    float speedBurst = UnityEngine.Random.Range(1.0f, 4.0f);
                    Transform alignBurst = __instance.ShellEjectAlign;
                    Vector3 inheritedVelocityBurst = Vector3.zero;
                    bool isFPSContentBurst = false;
                    WeaponShellManager.EjectShell(ejectTypeBurst, sizeBurst, speedBurst, alignBurst, inheritedVelocityBurst, isFPSContentBurst);
                }
            }
            else
            {
                __instance.m_burstTimer = Clock.Time + __instance.m_archetypeData.BurstDelay;
                __instance.m_burstClipCurr = __instance.m_archetypeData.BurstShotCount;
            }
            return false;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireAuto))]
        [HarmonyPostfix]
        public static void PostUpdateFireAuto(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            fromSentry = false;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireBurst))]
        [HarmonyPostfix]
        public static void PostUpdateFireBurst(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            fromSentry = false;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
        [HarmonyPrefix]
        public static void PreUpdateFireShotgunSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (!(Clock.Time > __instance.m_fireBulletTimer))
            {
                return;
            }
            fromSentry = true;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
        [HarmonyPostfix]
        public static void PostUpdateFireShotgunSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            fromSentry = false;
        }
        private static bool ShouldSkipSentryLogic()
        {
            return fromSentry;
        }
        [HarmonyPatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.BulletDamage))]
        [HarmonyPrefix]
        public static void Pre_BulletDamage(float dam, Agent sourceAgent, Vector3 position, Vector3 direction, Vector3 normal, bool allowDirectionalBonus, float staggerMulti, float precisionMulti)
        {
            if (ShouldSkipSentryLogic()) return;
            wieldedItem = sourceAgent.TryCast<PlayerAgent>()!.Inventory.WieldedItem;
        }
        [HarmonyPatch(typeof(Dam_EnemyDamageLimb), nameof(Dam_EnemyDamageLimb.BulletDamage))]
        [HarmonyPostfix]
        public static void Post_BulletDamage(Dam_EnemyDamageLimb __instance, float dam, Agent sourceAgent, Vector3 position, Vector3 direction, Vector3 normal, bool allowDirectionalBonus, float staggerMulti, float precisionMulti)
        {
            if (wieldedItem != null)
            {
                bool flag = sourceAgent != null && sourceAgent.IsLocallyOwned;
                if (flag && wieldedItem.ArchetypeData != null && (wieldedItem.ArchetypeData.persistentID == 27 || wieldedItem.ArchetypeData.persistentID == 29 || wieldedItem.ArchetypeData.persistentID == 30 || wieldedItem.ArchetypeData.persistentID == 37 || wieldedItem.ArchetypeData.persistentID == 67 || wieldedItem.ArchetypeData.persistentID == 81))
                {
                    float num = dam;
                    __instance.ShowHitIndicator(num > dam, willDie: false, position, hitArmor: false);
                }
            }
            wieldedItem = null;
        }
        [HarmonyPatch(typeof(Dam_EnemyDamageLimb), "ApplyWeakspotAndArmorModifiers")]
        [HarmonyPrefix]
        public static bool ApplyWeakspotAndArmorModifiers(Dam_EnemyDamageLimb __instance, ref float __result, float dam, float precisionMulti = 1f)
        {
            ArmorDmgMult = __instance.m_armorDamageMulti;
            if (wieldedItem == null || ShouldSkipSentryLogic())
            {
                return true;
            }
            if (wieldedItem.ArchetypeData != null)
            {
                uint persistentID = wieldedItem.ArchetypeData.persistentID;
                if (persistentID == 27 || persistentID == 29|| persistentID == 37 || persistentID == 67 || persistentID == 81) 
                {
                    __result = dam * ((__instance.m_type == eLimbDamageType.Weakspot) ? Mathf.Max(__instance.m_weakspotDamageMulti * precisionMulti, 1f) : __instance.m_weakspotDamageMulti) * (__instance.m_armorDamageMulti / __instance.m_armorDamageMulti);
                    return false; 
                }
                if (persistentID == 30)
                {
                    __result = (dam * (__instance.m_healthMax > 6000 ? (__instance.m_healthMax / dam * 12) : 1));
                    return false;
                }
                else if (persistentID == 22 || persistentID == 49 || persistentID == 59)
                {
                    __result = (UnityEngine.Random.Range(1.0f, 1.15f) * (dam * ((__instance.m_type == eLimbDamageType.Weakspot) ? Mathf.Max(__instance.m_weakspotDamageMulti * precisionMulti, 1f) : __instance.m_weakspotDamageMulti))) * __instance.m_armorDamageMulti;
                    return false;
                }
            }
            return true;
        }
    }
}
