using HarmonyLib;
using UnityEngine;
using Gear;
using FX_EffectSystem;
using GameData;
using Player;
using Armament.ArmorPiercingFragmentation;
namespace Armament.SentryGuns
{
    [HarmonyPatch]
    internal class SentryGunsPatches
    {
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.FireBullet))]
        [HarmonyPrefix]
        public static bool FireBullet(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged)
        {
            SentryGunInstance_Firing_Bullets.s_weaponRayData = new Weapon.WeaponHitData
            {
                randomSpread = __instance.m_archetypeData.HipFireSpread,
                fireDir = __instance.MuzzleAlign.forward
            };
            if (__instance.m_archetypeData.Sentry_FireTowardsTargetInsteadOfForward && __instance.m_core.TryGetTargetAimPos(out var pos))
            {
                SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir = (pos - __instance.MuzzleAlign.position).normalized;
            }

            Weapon.WeaponHitData weaponRayData = SentryGunInstance_Firing_Bullets.s_weaponRayData;

            if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref weaponRayData, LayerManager.MASK_SENTRYGUN_RAY))
            {
                SentryGunInstance_Firing_Bullets.s_weaponRayData = weaponRayData;
                SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = (__instance.m_archetypeData.GetSentryDamage(SentryGunInstance_Firing_Bullets.s_weaponRayData.owner, SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance, targetIsTagged));
                SentryGunInstance_Firing_Bullets.s_weaponRayData.staggerMulti = __instance.m_archetypeData.GetSentryStaggerDamage(targetIsTagged);
                SentryGunInstance_Firing_Bullets.s_weaponRayData.precisionMulti = __instance.m_archetypeData.PrecisionDamageMulti;
                SentryGunInstance_Firing_Bullets.s_weaponRayData.damageFalloff = (__instance.m_archetypeData.DamageFalloff) * 3.11850312f;
                SentryGunInstance_Firing_Bullets.s_weaponRayData.vfxBulletHit = __instance.m_vfxBulletHit;
                if (__instance.m_archetypeData.PiercingBullets)
                {
                    int num = 5;
                    int num2 = 0;
                    bool flag = false;
                    float num3 = 0f;
                    int num4 = 0;
                    Vector3 originPos = __instance.MuzzleAlign.position;
                    while (!flag && num2 < num && SentryGunInstance_Firing_Bullets.s_weaponRayData.maxRayDist > 0f && num4 < __instance.m_archetypeData.PiercingDamageCountLimit)
                    {
                        if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref weaponRayData, originPos, LayerManager.MASK_SENTRYGUN_RAY))
                        {
                            SentryGunInstance_Firing_Bullets.s_weaponRayData = weaponRayData;
                            if (BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, doDamage))
                            {
                                num4++;
                            }
                            FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
                            flag = !SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.collider.gameObject.IsInLayerMask(LayerManager.MASK_BULLETWEAPON_PIERCING_PASS);
                            originPos = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point + SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir * 0.1f;
                            num3 += SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance;
                            SentryGunInstance_Firing_Bullets.s_weaponRayData.maxRayDist -= SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance;
                        }
                        else
                        {
                            flag = true;
                            FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
                        }
                        num2++;
                    }
                }
                else if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref weaponRayData, LayerManager.MASK_SENTRYGUN_RAY))
                {
                    SentryGunInstance_Firing_Bullets.s_weaponRayData = weaponRayData;
                    BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, doDamage);
                    FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
                }
                else
                {
                    FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
                }
            }
            __instance.OnBulletFired?.Invoke();
            SentryGunInstance_Firing_Bullets.s_tracerPool.AquireEffect().Play(null, __instance.MuzzleAlign.position, Quaternion.LookRotation(SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir));
            __instance.m_muzzleFlash?.Play();
            __instance.m_fireBulletTimer = Clock.Time + __instance.m_archetypeData.GetSentryShotDelay(__instance.m_core.Owner, targetIsTagged);
            return false;
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
        [HarmonyPrefix]
        public static void UpdateFireShotgunSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (!(Clock.Time > __instance.m_fireBulletTimer))
            {
                return;
            }
            __instance.m_muzzleFlash?.Play();
            ShellTypes ejectType = ShellTypes.Shell_12_Gauge;
            float size = 0.8487f;
            float speed = UnityEngine.Random.Range(1.0f, 6.0f);
            Transform align = __instance.ShellEjectAlign;
            Vector3 inheritedVelocity = Vector3.zero;
            bool isFPSContent = false;
            WeaponShellManager.EjectShell(ejectType, size, speed, align, inheritedVelocity, isFPSContent);
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireSemi))]
        [HarmonyPrefix]
        public static void UpdateFireSemi(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (Clock.Time > __instance.m_fireBulletTimer)
            {
                ShellTypes ejectTypeSemi = ShellTypes.Shell_338;
                float sizeSemi = 2.11534f;
                float speedSemi = UnityEngine.Random.Range(1.0f, 5.0f);
                Transform alignSemi = __instance.ShellEjectAlign;
                Vector3 inheritedVelocitySemi = Vector3.zero;
                bool isFPSContentSemi = false;
                WeaponShellManager.EjectShell(ejectTypeSemi, sizeSemi, speedSemi, alignSemi, inheritedVelocitySemi, isFPSContentSemi);
            }
        }
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireAuto))]
        [HarmonyPrefix]
        public static void UpdateFireAuto(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged)
        {
            if (Clock.Time > __instance.m_fireBulletTimer)
            {
                ShellTypes ejectTypeAuto = ShellTypes.Shell_338;
                float sizeAuto = 0.78f;
                float speedAuto = UnityEngine.Random.Range(1.0f, 3.0f);
                Transform alignAuto = __instance.ShellEjectAlign;
                Vector3 inheritedVelocityAuto = Vector3.zero;
                bool isFPSContentAuto = false;
                WeaponShellManager.EjectShell(ejectTypeAuto, sizeAuto, speedAuto, alignAuto, inheritedVelocityAuto, isFPSContentAuto);
            }
        }
        [HarmonyPatch(typeof(ArchetypeDataBlock), "GetSentryDamage")]
        [HarmonyPostfix]
        public static void GetSentryDamage(ArchetypeDataBlock __instance, PlayerAgent owner, float distance, bool targetIsTagged, ref float __result)
        {
            __result = (AgentModifierManager.ApplyModifier(owner, AgentModifier.SentryGunDamage, __result) > 2.1f) ? ((AgentModifierManager.ApplyModifier(owner, AgentModifier.SentryGunDamage, __result) / ArmorPiercingFragmentationPatches.ArmorDmgMult)) * UnityEngine.Random.Range(2.5f, 2.910603f) : AgentModifierManager.ApplyModifier(owner, AgentModifier.SentryGunDamage, __result) * UnityEngine.Random.Range(2.5f, 2.910603f);
            Debug.Log($"Sentry Damage = {__result} | Armor Damage Multiplier = {ArmorPiercingFragmentationPatches.ArmorDmgMult}");
        }
    }
}
