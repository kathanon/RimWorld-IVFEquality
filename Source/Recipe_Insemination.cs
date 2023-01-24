using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
public class Recipe_Insemination : Recipe_ImplantEmbryo {
    public const float BaseInseminationChance = 0.75f;

    public static float InseminationChance(Pawn mother, Pawn father) 
        => BaseInseminationChance * PregnancyUtility.PregnancyChanceForPartners(mother, father);

    public override void ApplyOnPawn(Pawn mother, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill) {
        if (!ModsConfig.BiotechActive) {
            return;
        }

        SpermSample sample = ingredients.OfType<SpermSample>().FirstOrDefault();
        if (sample == null) {
            Log.Warning($"Tried to perform insemination on {mother} but no sperm sample was in the ingredients list");
        } else if (!CheckSurgeryFail(billDoer, mother, ingredients, part, bill)) {
            var father = sample.Donor;
            if (Rand.Chance(InseminationChance(mother, father))) {
                var genes = PregnancyUtility.GetInheritedGeneSet(father, mother, out var success);
                if (success) {
                    var hediff = (Hediff_Pregnant) HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, mother);
                    hediff.SetParents(mother, sample.Donor, genes);
                    mother.health.AddHediff(hediff);
                } else if (PawnUtility.ShouldSendNotificationAbout(mother)) {
                    Messages.Message(Strings.InseminationFailed(mother, sample) + " " 
                                     + "CombinedGenesExceedMetabolismLimits".Translate(), 
                        mother, 
                        MessageTypeDefOf.NegativeEvent);
                }
            } else {
                Messages.Message(Strings.InseminationFailed(mother, sample), mother, MessageTypeDefOf.NeutralEvent);
            }
        }
    }

    public override string LabelFromUniqueIngredients(Bill bill) {
        if (bill is Bill_Medical medical) {
            var sample = medical.uniqueRequiredIngredients?.FirstOrDefault();
            if (sample is SpermSample && !sample.Destroyed) {
                return Strings.InseminationOperation(sample);
            }
        }

        return null;
    }
}
