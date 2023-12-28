using FX_EffectSystem;
using HarmonyLib;
using UnityEngine;

namespace Armament.ShellcasingsBulletHoles
{
    [HarmonyPatch]
    public class ShellcasingsBulletHolesPatches
    {
        [HarmonyPatch(typeof(WeaponShellManager), nameof(WeaponShellManager.SetupPools))]
        [HarmonyPostfix]
        public static void SetupPools(WeaponShellManager __instance)
        {
            for (int i = 0; i < __instance.m_shellPools.Length; i++)
            {
                __instance.m_shellPools[i] = new GameObjectPool();
            }
            int size = 60;
            __instance.m_shellPools[1].Setup(__instance.m_shell_9mm_prefab, size, GameObjectPoolType.LoopAndReuse, "Shell_9x18mm_Pool");
            __instance.m_shellPools[2].Setup(__instance.m_shell_45_ACP_prefab, size, GameObjectPoolType.LoopAndReuse, "Shell_45_ACP_Pool");
            __instance.m_shellPools[3].Setup(__instance.m_shell_338_prefab, size, GameObjectPoolType.LoopAndReuse, "Shell_338_Pool");
            __instance.m_shellPools[4].Setup(__instance.m_shell_12_Gauge_prefab, size, GameObjectPoolType.LoopAndReuse, "Shell_Mos_590_Pool");
            for (int j = 1; j < __instance.m_shellPools.Length; j++)
            {
                __instance.m_shellPools[j].GetRootObject().transform.SetParent(__instance.m_root.transform);
                List<GameObject> allObjects = new List<GameObject>();
                foreach (var item in __instance.m_shellPools[j].GetAllObjects())
                {
                    allObjects.Add(item);
                }
                for (int k = 0; k < allObjects.Count; k++)
                {
                    allObjects[k].AddComponent<DisableAfterDelay>().delay = 60f;
                }
            }
        }
        [HarmonyPatch(typeof(FX_Decal), nameof(FX_Decal.PostInstantiateInitialize))]
        [HarmonyPostfix]
        public static void PostInstantiateInitialize(FX_Decal __instance)
        {
            __instance.m_uniqueLifeTime = (__instance.m_lifeTime + UnityEngine.Random.value * __instance.m_lifeVariation) * 2.0f;
            if (__instance.m_feedMaterialLife)
            {
                __instance.m_instancedMaterialRef = UnityEngine.Object.Instantiate(__instance.m_decal.m_decalMaterial);
                __instance.m_decal.m_decalMaterial = __instance.m_instancedMaterialRef;
                __instance.m_fadeoutMod = 1f / (__instance.m_uniqueLifeTime) * (1f / __instance.m_feedMaterialLifeStartAt);
                __instance.m_decal.m_decalMaterial.SetFloat(FX_Decal.s_decalLifeIDHash, __instance.m_fadeoutMod);
            }
        }
    }
}
