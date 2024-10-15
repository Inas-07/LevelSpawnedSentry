using ExtraObjectiveSetup.Utils;
using GameData;
using LevelGeneration;
using System.Collections.Generic;

namespace EOSExt.NavigationalSpline.Definition
{
    public class Spline
    {
        public Vec3 From { get; set; } = new();

        public Vec3 To { get; set; } = new();
    }

    public class NavigationalSplineDefinition
    {
        public string WorldEventObjectFilter { get; set; } = string.Empty;

        public float RevealSpeedMulti { get; set; } = 1.0f;

        public List<Spline> Splines { get; set; } = new() { new() };
    }
}
