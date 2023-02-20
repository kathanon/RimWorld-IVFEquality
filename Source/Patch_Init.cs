using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
[HarmonyPatch]
public static class Patch_Init {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIRoot_Entry), nameof(UIRoot_Entry.Init))]
    public static void Init() 
        => Main.Settings.Update();
}
