﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace nazu.OreRemapping
{
  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
  public class OreRemapping : MySessionComponentBase
  {
    public override void LoadData()
    {
      var allPlanets = MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions();

      foreach (var def in allPlanets)
      {
        var planet = def as MyPlanetGeneratorDefinition;
        var oreList = new List<MyPlanetOreMapping>(planet.OreMappings.ToList());

        for (int i = 0; i < oreList.Count; i++)
        {
          var oreMap = oreList[i];
          //  Tier 1
          //      Easy start
          if (oreMap.Value == 200 && oreMap.Type.Contains("Iron_02")) { oreMap.Depth = 20; }
          if (oreMap.Value == 220 && oreMap.Type.Contains("Nickel_01")) { oreMap.Depth = 20; }
          if (oreMap.Value == 240 && oreMap.Type.Contains("Silicon_01")) { oreMap.Depth = 20; }
          //      Fe 1
          if (oreMap.Value == 1 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 50; oreMap.Depth = 250; }
          if (oreMap.Value == 4 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 72; oreMap.Depth = 228; }
          if (oreMap.Value == 8 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 94; oreMap.Depth = 206; }
          //      Fe 2
          if (oreMap.Value == 12 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 125; oreMap.Depth = 175; }
          if (oreMap.Value == 16 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 158; oreMap.Depth = 142; }
          if (oreMap.Value == 20 && oreMap.Type.Contains("Iron_02")) { oreMap.Start = 181; oreMap.Depth = 119; }
          //      Ni 1
          if (oreMap.Value == 24 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 60; oreMap.Depth = 240; }
          if (oreMap.Value == 28 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 81; oreMap.Depth = 209; }
          if (oreMap.Value == 32 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 110; oreMap.Depth = 190; }
          //      Ni 2
          if (oreMap.Value == 36 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 124; oreMap.Depth = 176; }
          if (oreMap.Value == 40 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 167; oreMap.Depth = 133; }
          if (oreMap.Value == 44 && oreMap.Type.Contains("Nickel_01")) { oreMap.Start = 190; oreMap.Depth = 110; }
          //      Si 1
          if (oreMap.Value == 48 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 49; oreMap.Depth = 251; }
          if (oreMap.Value == 52 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 63; oreMap.Depth = 237; }
          if (oreMap.Value == 56 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 80; oreMap.Depth = 220; }
          //      Si 2
          if (oreMap.Value == 60 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 112; oreMap.Depth = 188; }
          if (oreMap.Value == 64 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 135; oreMap.Depth = 165; }
          if (oreMap.Value == 68 && oreMap.Type.Contains("Silicon_01")) { oreMap.Start = 157; oreMap.Depth = 143; }

          //  Tier 2
          //      Co 1
          if (oreMap.Value == 72 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Start = 210; oreMap.Depth = 390; }
          if (oreMap.Value == 76 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Start = 242; oreMap.Depth = 358; }
          if (oreMap.Value == 80 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Start = 276; oreMap.Depth = 324; }
          //      Co 2
          if (oreMap.Value == 84 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Start = 352; oreMap.Depth = 248; }
          if (oreMap.Value == 88 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Start = 375; oreMap.Depth = 225; }
          if (oreMap.Value == 92 && oreMap.Type.Contains("Cobalt_01")) {oreMap.Start = 392; oreMap.Depth = 208; }
          //      Ag 1
          if (oreMap.Value == 96 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 221; oreMap.Depth = 379; }
          if (oreMap.Value == 100 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 245; oreMap.Depth = 355; }
          if (oreMap.Value == 104 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 271; oreMap.Depth = 329; }
          //      Ag 2
          if (oreMap.Value == 108 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 315; oreMap.Depth = 285; }
          if (oreMap.Value == 112 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 358; oreMap.Depth = 242; }
          if (oreMap.Value == 116 && oreMap.Type.Contains("Silver_01")) { oreMap.Start = 373; oreMap.Depth = 227; }
          //      Mg 1
          if (oreMap.Value == 120 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 423; oreMap.Depth = 405; }
          if (oreMap.Value == 124 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 426; oreMap.Depth = 360; }
          if (oreMap.Value == 128 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 429; oreMap.Depth = 280; }
          //      Mg 2
          if (oreMap.Value == 132 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 553; oreMap.Depth = 307; }
          if (oreMap.Value == 136 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 555; oreMap.Depth = 248; }
          if (oreMap.Value == 140 && oreMap.Type.Contains("Magnesium_01")) { oreMap.Start = 559; oreMap.Depth = 210; }

          //  Tier 3
          //      U 1
          if (oreMap.Value == 144 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 432; oreMap.Depth = 350; }
          if (oreMap.Value == 148 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 435; oreMap.Depth = 300; }
          if (oreMap.Value == 152 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 438; oreMap.Depth = 250; }
          //      U 2
          if (oreMap.Value == 156 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 580; oreMap.Depth = 250; }
          if (oreMap.Value == 160 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 589; oreMap.Depth = 200; }
          if (oreMap.Value == 164 && oreMap.Type.Contains("Uraninite_01")) { oreMap.Start = 596; oreMap.Depth = 110; }
          //      Au 1
          if (oreMap.Value == 168 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 440; oreMap.Depth = 400; }
          if (oreMap.Value == 172 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 442; oreMap.Depth = 320; }
          if (oreMap.Value == 176 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 445; oreMap.Depth = 250; }
          //      Au 2
          if (oreMap.Value == 180 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 500; oreMap.Depth = 300; }
          if (oreMap.Value == 184 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 570; oreMap.Depth = 200; }
          if (oreMap.Value == 188 && oreMap.Type.Contains("Gold_01")) { oreMap.Start = 520; oreMap.Depth = 150; }
          //      Pt 1
          if (oreMap.Value == 192 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 650; oreMap.Depth = 400; }
          if (oreMap.Value == 196 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 651; oreMap.Depth = 300; }
          if (oreMap.Value == 208 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 653; oreMap.Depth = 240; }
          //      Pt 2
          if (oreMap.Value == 212 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 610; oreMap.Depth = 300; }
          if (oreMap.Value == 217 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 614; oreMap.Depth = 200; }
          if (oreMap.Value == 222 && oreMap.Type.Contains("Platinum_01")) { oreMap.Start = 619; oreMap.Depth = 150; }

          // revert the Mg->Ice change
          if (oreMap.Value == 132 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Magnesium_01"; oreMap.Start = 553; oreMap.Depth = 307; }
          if (oreMap.Value == 136 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Magnesium_01"; oreMap.Start = 555; oreMap.Depth = 248; }
          if (oreMap.Value == 140 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Magnesium_01"; oreMap.Start = 559; oreMap.Depth = 210; }
          // revert the U->Ice change
          if (oreMap.Value == 144 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 432; oreMap.Depth = 350; }
          if (oreMap.Value == 148 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 435; oreMap.Depth = 300; }
          if (oreMap.Value == 152 && oreMap.Type.Contains("Ice_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 438; oreMap.Depth = 250; }
          // revert the U->Co change
          if (oreMap.Value == 156 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 580; oreMap.Depth = 250; }
          if (oreMap.Value == 160 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 589; oreMap.Depth = 200; }
          if (oreMap.Value == 164 && oreMap.Type.Contains("Cobalt_01")) { oreMap.Type = "Uraninite_01"; oreMap.Start = 596; oreMap.Depth = 110; }
        }

        planet.OreMappings = oreList.ToArray();
      }
    }
  }
}
