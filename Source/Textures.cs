using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IVFEquality {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";

        public static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public static readonly Texture2D Inseminate = ContentFinder<Texture2D>.Get(Prefix + "Inseminate");
        public static readonly Texture2D Fertilize  = ContentFinder<Texture2D>.Get(Prefix + "Fertilize");
    }
}
