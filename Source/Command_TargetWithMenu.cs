using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
public class Command_TargetWithMenu : Command_Target {
    public Map map;

    public Func<IEnumerable<FloatMenuOption>> menu;

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions 
        => (menu ?? ((map != null) ? ForMap : Enumerable.Empty<FloatMenuOption>))();

    private IEnumerable<FloatMenuOption> ForMap() 
        => map.spawnedThings
            .Where(x => targetingParams.CanTarget(x))
            .Select(x => new FloatMenuOption(x.LabelCap, () => action(x)));
}
