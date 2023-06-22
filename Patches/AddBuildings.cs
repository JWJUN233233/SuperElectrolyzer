using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperElectrolyzer.Patches
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    public class AddBuildingsPatches
    {
        public static void Prefix()
        {
            BuildingManager.RegisterBuilding("SuperElectrolyzer", "Oxygen", "超级电解器");
        }
    }
    public static class BuildingManager
    {
        public static void RegisterName(string id, string name)
        {
            Strings.Add(new string[] { $"STRINGS.BUILDINGS.PREFABS.{id.ToUpper()}.NAME", name });
        }
        public static void RegisterEffect(string id, string context)
        {
            Strings.Add(new string[] { $"STRINGS.BUILDINGS.PREFABS.{id.ToUpper()}.EFFECT", context });
        }
        public static void RegisterDesc(string id, string context)
        {
            Strings.Add(new string[] { $"STRINGS.BUILDINGS.PREFABS.{id.ToUpper()}.DESC", context });
        }
        public static void RegisterBuilding(string id, string menuLocation, string name = "", string effect = "", string desc = "")
        {
            RegisterName(id, name);
            RegisterEffect(id, effect);
            RegisterDesc(id, desc);
            ModUtil.AddBuildingToPlanScreen(menuLocation, id);
        }
    }
}
