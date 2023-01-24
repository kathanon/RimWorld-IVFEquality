using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality {
    public static class Strings {
        public const string ID = "kathanon.IVFEquality";
        public const string Name = "IVF Equality";

        public static readonly string InseminationLabel  = (ID + ".InseminationLabel" ).Translate();
        public static readonly string InseminationDesc   = (ID + ".InseminationDesc"  ).Translate();
        public static readonly string FertilizeLabel     = (ID + ".FertilizeLabel"    ).Translate();
        public static readonly string FertilizeDesc      = (ID + ".FertilizeDesc"     ).Translate();
        public static readonly string FertilizeWithLabel = (ID + ".FertilizeWithLabel").Translate();
        public static readonly string FertilizeWithDesc  = (ID + ".FertilizeWithDesc" ).Translate();
        public static readonly string RecentlyExtracted  = (ID + ".RecentlyExtracted" ).Translate();
        public static readonly string NoTargets          = (ID + ".NoTargets"         ).Translate();

        public static string CancelInsemination(Pawn target) 
            => CancelInseminationKey.Translate(target.LabelShort);
        private const string CancelInseminationKey = ID + ".CancelInsemination";

        public static string InseminationOperation(Thing target) 
            => InseminationOperationKey.Translate(target.LabelCap);
        private const string InseminationOperationKey = ID + ".InseminationOperation";

        public static string InseminationFailed(Pawn target, Thing sample) 
            => InseminationFailedKey.Translate(target.LabelShort, sample.LabelCap);
        private const string InseminationFailedKey = ID + ".InseminationFailed";

        public static string InseminationChanceConfirm(Pawn target, Thing sample, float success) 
            => InseminationChanceConfirmKey.Translate(
                target.LabelShort,
                sample.LabelCap,
                success.ToStringPercent());
        private const string InseminationChanceConfirmKey = ID + ".InseminationChanceConfirm";

        public static string InseminationBothConfirm(Pawn target,
                                                     SpermSample sample,
                                                     float success,
                                                     PawnRelationDef relation,
                                                     float inbred) 
            => InseminationBothConfirmKey.Translate(
                target.LabelShort,
                sample.LabelCap,
                success.ToStringPercent(),
                sample.Donor.LabelShort,
                relation.label,
                inbred.ToStringPercent());
        private const string InseminationBothConfirmKey = ID + ".InseminationBothConfirm";

        public static string CancelFertilization(Thing target) 
            => CancelFertilizationKey.Translate(target.LabelCap);
        private const string CancelFertilizationKey = ID + ".CancelFertilization";

        public static string CancelFertilizationWith(Thing target) 
            => CancelFertilizationWithKey.Translate(target.LabelCap);
        private const string CancelFertilizationWithKey = ID + ".CancelFertilizationWith";
    }
}
