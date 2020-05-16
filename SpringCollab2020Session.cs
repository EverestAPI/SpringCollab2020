
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Session : EverestModuleSession {
        public class MultiRoomStrawberrySeedInfo {
            public int Index { get; set; }
            public EntityID BerryID { get; set; }
            public string Sprite { get; set; }
            public bool IgnoreLighting { get; set; }
        }

        public bool IcePhysicsDisabled { get; set; } = false;

        public List<MultiRoomStrawberrySeedInfo> CollectedMultiRoomStrawberrySeeds { get; set; } = new List<MultiRoomStrawberrySeedInfo>();

        public bool MadelineIsSilhouette { get; set; } = false;

        public bool LightSourcesDisabled { get; set; } = false;

        public class SpeedBasedMusicParamInfo {
            public float MinimumSpeed { get; set; }
            public float MaximumSpeed { get; set; }
        }

        public Dictionary<string, SpeedBasedMusicParamInfo> ActiveSpeedBasedMusicParams = new Dictionary<string, SpeedBasedMusicParamInfo>();
    }
}
