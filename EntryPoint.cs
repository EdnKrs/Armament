using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
namespace Armament
{
    internal static class ModuleInfo
    {
        public const string GUID = "EdnKrs.Armament";
        public const string Name = "Armament";
        public const string Version = "1.1.0";
    }
    [BepInPlugin(ModuleInfo.GUID, ModuleInfo.Name, ModuleInfo.Version)]
    internal class Injection : BasePlugin
    {
        private static Harmony? harmony;
        public override void Load()
        {
            harmony = new Harmony(ModuleInfo.GUID);
            new Harmony("Armament").PatchAll();
        }
    }
    internal static class Logging
    {
        private static readonly ManualLogSource _logger;

        static Logging()
        {
            _logger = new ManualLogSource("Armament");
            Logger.Sources.Add(_logger);
        }

        private static string Format(object msg)
        {
            return msg.ToString()!;
        }

        public static void Info(object data)
        {
            _logger.LogMessage(Format(data));
        }

        public static void Verbose(object data)
        {
        }

        public static void Debug(object data)
        {
            _logger.LogDebug(Format(data));
        }

        public static void Error(object data)
        {
            _logger.LogError(Format(data));
        }
    }
}
