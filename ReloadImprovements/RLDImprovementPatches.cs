using System;
using Gear;
using HarmonyLib;
using Player;
using UnityEngine;
using System.Collections.Generic;
using GameData;
using Il2CppInterop.Runtime.InteropTypes;
namespace Armament.ReloadImprovements
{
    [HarmonyPatch]
    internal class RLDImprovementPatches
    {
        private static Dictionary<InventorySlot, float> AmmoMaxCapLookup = new Dictionary<InventorySlot, float>
        {
            {
                InventorySlot.GearStandard,
                0f
            },
            {
                InventorySlot.GearSpecial,
                0f
            }
        };
        private static InventorySlotAmmo? StandardAmmo;
        private static InventorySlotAmmo? SpeacialAmmo;
        private static bool NeedRestoredStandardAmmoMaxCap = false;
        private static bool NeedRestoredSpecialAmmoMaxCap = false;
        private static float AmmoStandardResourcePackMaxCap => GameDataBlockBase<PlayerDataBlock>.GetBlock(1u).AmmoStandardResourcePackMaxCap;
        private static float AmmoSpecialResourcePackMaxCap => GameDataBlockBase<PlayerDataBlock>.GetBlock(1u).AmmoSpecialResourcePackMaxCap;
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
        [HarmonyPatch(typeof(PlayerBackpackManager), "ReceiveAmmoGive")]
        [HarmonyPrefix]
        private static void ReceiveAmmoGive(ref pAmmoGive data)
        {
            bool flag = false;
            bool flag2 = false;
            float num = 0f;
            float num2 = 0f;
            if (data.targetPlayer.TryGetPlayer(out var player) && player.IsLocal)
            {
                if (PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(InventorySlot.GearStandard, out var backpackItem))
                {
                    BulletWeapon bulletWeapon = ((Il2CppObjectBase)(object)backpackItem.Instance).TryCast<BulletWeapon>()!;
                    float num3 = (float)(bulletWeapon.ClipSize - bulletWeapon.m_clip) * bulletWeapon.ArchetypeData.CostOfBullet;
                    StandardAmmo = PlayerBackpackManager.LocalBackpack.AmmoStorage.StandardAmmo;
                    AmmoMaxCapLookup[InventorySlot.GearStandard] = StandardAmmo.AmmoMaxCap;
                    StandardAmmo.AmmoMaxCap += num3;
                    NeedRestoredStandardAmmoMaxCap = true;
                    num = data.ammoStandardRel * AmmoStandardResourcePackMaxCap + StandardAmmo.AmmoInPack - StandardAmmo.AmmoMaxCap;
                    flag = num > 0f;
                }
                if (PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(InventorySlot.GearSpecial, out backpackItem))
                {
                    BulletWeapon bulletWeapon2 = ((Il2CppObjectBase)(object)backpackItem.Instance).TryCast<BulletWeapon>()!;
                    float num4 = (float)(bulletWeapon2.ClipSize - bulletWeapon2.m_clip) * bulletWeapon2.ArchetypeData.CostOfBullet;
                    SpeacialAmmo = PlayerBackpackManager.LocalBackpack.AmmoStorage.SpecialAmmo;
                    AmmoMaxCapLookup[InventorySlot.GearSpecial] = SpeacialAmmo.AmmoMaxCap;
                    SpeacialAmmo.AmmoMaxCap += num4;
                    NeedRestoredSpecialAmmoMaxCap = true;
                    num2 = data.ammoSpecialRel * AmmoSpecialResourcePackMaxCap + SpeacialAmmo.AmmoInPack - SpeacialAmmo.AmmoMaxCap;
                    flag2 = num2 > 0f;
                }
                if (flag && !flag2)
                {
                    data.ammoSpecialRel += num / AmmoStandardResourcePackMaxCap;
                }
                else if (!flag && flag2)
                {
                    data.ammoStandardRel += num2 / AmmoSpecialResourcePackMaxCap;
                }
            }
        }
        [HarmonyPatch(typeof(PlayerBackpackManager), "ReceiveAmmoGive")]
        [HarmonyPostfix]
        private static void ReceiveAmmoGive(pAmmoGive data)
        {
            if (data.targetPlayer.TryGetPlayer(out var player) && player.IsLocal)
            {
                if (PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(InventorySlot.GearStandard, out var backpackItem))
                {
                    StandardAmmo!.AmmoMaxCap = AmmoMaxCapLookup[InventorySlot.GearStandard];
                    PlayerBackpackManager.LocalBackpack.AmmoStorage.UpdateSlotAmmoUI(InventorySlot.GearStandard);
                    NeedRestoredStandardAmmoMaxCap = false;
                }
                if (PlayerBackpackManager.LocalBackpack.TryGetBackpackItem(InventorySlot.GearSpecial, out backpackItem))
                {
                    SpeacialAmmo!.AmmoMaxCap = AmmoMaxCapLookup[InventorySlot.GearSpecial];
                    PlayerBackpackManager.LocalBackpack.AmmoStorage.UpdateSlotAmmoUI(InventorySlot.GearSpecial);
                    NeedRestoredSpecialAmmoMaxCap = false;
                }
            }
        }
    }
}
