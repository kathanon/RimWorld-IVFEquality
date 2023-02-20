using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IVFEquality;
[StaticConstructorOnStartup]
public class Main : Mod {
    static Main() 
        => new Harmony(Strings.ID).PatchAll();

    private static Main instance;

    public Main(ModContentPack content) : base(content)
        => instance = this;

    public static Settings Settings => instance.GetSettings<Settings>();

    public override string SettingsCategory() 
        => Strings.Name;

    public override void DoSettingsWindowContents(Rect inRect) 
        => GetSettings<Settings>().DoGUI(inRect);
}
