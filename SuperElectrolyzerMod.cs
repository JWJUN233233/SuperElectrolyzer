using HarmonyLib;
using KMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperElectrolyzer
{
    public class SuperElectrolyzerMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            Console.WriteLine("Super", GetType().Name, "Loaded");
        }
    }
}
