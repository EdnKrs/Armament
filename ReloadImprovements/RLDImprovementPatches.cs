using System;
using Gear;
using HarmonyLib;
using Player;
using UnityEngine;

namespace Armament.ReloadImprovements
{
    [HarmonyPatch]
    internal class RLDImprovementPatches
    {
        [HarmonyPatch(typeof(PlayerAmmoStorage), "GetClipBulletsFromPack")]
        [HarmonyPrefix]
        public static bool GetClipBulletsFromPack(PlayerAmmoStorage __instance, ref int __result, int currentClip, AmmoType ammoType)
        {
            PlayerAmmoStorage __instance2 = __instance;
            InventorySlotAmmo inventorySlotAmmo = __instance2.m_ammoStorage[(int)ammoType];
            float costOfBullet = inventorySlotAmmo.CostOfBullet;
            int num = inventorySlotAmmo.BulletClipSize - currentClip;
            if (num < 1)
            {
                __result = currentClip;
                return false;
            }
            ItemEquippable wieldedItem = PlayerManager.PlayerAgentsInLevel.Find((Func<PlayerAgent, bool>)((PlayerAgent p) => p.PlayerSlotIndex == __instance2.m_playerBackpack.Owner.PlayerSlot.index)).Inventory.WieldedItem;
            if ((UnityEngine.Object)(object)wieldedItem != null && !ShouldExcludeWeapon(wieldedItem.ArchetypeData.persistentID) && currentClip > 0)
            {
                num++;
            }
            float a = (float)num * inventorySlotAmmo.CostOfBullet;
            float ammoInPack = inventorySlotAmmo.AmmoInPack;
            int num2 = Mathf.RoundToInt(Mathf.Min(a, ammoInPack) / costOfBullet);
            int bulletsInPack = inventorySlotAmmo.BulletsInPack;
            inventorySlotAmmo.AmmoInPack -= (float)num2 * costOfBullet;
            inventorySlotAmmo.OnBulletsUpdateCallback?.Invoke(inventorySlotAmmo.BulletsInPack);
            currentClip += Mathf.Min(num2, bulletsInPack);
            __instance2.NeedsSync = true;
            __instance2.UpdateSlotAmmoUI(inventorySlotAmmo, currentClip);
            __result = currentClip;
            return false;
        }
        private static bool ShouldExcludeWeapon(uint persistentID)
        {
            uint[] array = new uint[10] { 23u, 33u, 34u, 37u, 38u, 43u, 45u, 66u, 72u, 81u };
            foreach (uint num in array)
            {
                if (persistentID == num)
                {
                    return true;
                }
            }
            return false;
        }
        [HarmonyPatch(typeof(BulletWeapon), "ClipIsFull")]
        [HarmonyPrefix]
        private static bool ClipIsFull(BulletWeapon __instance, out bool __result)
        {
            __result = __instance.m_clip >= __instance.ClipSize;
            return false;
        }
    }
}
