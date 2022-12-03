using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SpringCollab2020.Effects {
    /// <summary>
    /// A heatwave effect that does not affect the colorgrade when it is hidden.
    /// </summary>
    class HeatWaveNoColorGrade : HeatWave {
        private DynData<HeatWave> self = new DynData<HeatWave>();

        public HeatWaveNoColorGrade() : base() {
            self = new DynData<HeatWave>(this);
        }

        public override void Update(Scene scene) {
            Level level = scene as Level;
            bool show = (IsVisible(level) && level.CoreMode != Session.CoreModes.None);

            if (!show) {
                // if not fading out, the heatwave is invisible, so don't even bother updating it.
                if (self.Get<float>("fade") > 0) {
                    // be sure to lock color grading to prevent it from becoming "none".
                    DynData<Level> levelData = new DynData<Level>(level);

                    float colorGradeEase = levelData.Get<float>("colorGradeEase");
                    float colorGradeEaseSpeed = levelData.Get<float>("colorGradeEaseSpeed");
                    string colorGrade = level.Session.ColorGrade;

                    base.Update(scene);

                    levelData["colorGradeEase"] = colorGradeEase;
                    levelData["colorGradeEaseSpeed"] = colorGradeEaseSpeed;
                    level.Session.ColorGrade = colorGrade;

                    if (self.Get<float>("heat") <= 0) {
                        // the heat hit 0, we should now restore the water sine direction
                        // ... because if we don't, waterfalls will flow backwards
                        Distort.WaterSineDirection = 1f;
                    }
                }
            } else {
                // heat wave is visible: update as usual.
                base.Update(scene);
            }
        }
    }
}
