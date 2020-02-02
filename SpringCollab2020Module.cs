using Celeste.Mod.SpringCollab2020.Entities;
using Celeste.Mod.SpringCollab2020.Triggers;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Module : EverestModule {

        public static SpringCollab2020Module Instance;

        public SpringCollab2020Module() {
            Instance = this;
        }

        public override void Load() {
            NoRefillField.Load();
            FloatierSpaceBlock.Load();
            MoveBlockBarrier.Load();
            MoveBlockBarrierRenderer.Load();
            RemoveLightSourcesTrigger.Load();
            SafeRespawnCrumble.Load();
            UpsideDownJumpThru.Load();
            BubbleReturnBerry.Load();
            SidewaysJumpThru.Load();
            CrystalBombDetonatorRenderer.Load();
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            GlassBerry.LoadContent();
        }

        public override void Unload() {
            NoRefillField.Unload();
            FloatierSpaceBlock.Unload();
            MoveBlockBarrier.Unload();
            MoveBlockBarrierRenderer.Unload();
            RemoveLightSourcesTrigger.Unload();
            SafeRespawnCrumble.Unload();
            GlassBerry.Unload();
            UpsideDownJumpThru.Unload();
            BubbleReturnBerry.Unload();
            SidewaysJumpThru.Unload();
            CrystalBombDetonatorRenderer.Unload();
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<SpringCollab2020MapDataProcessor>();
        }
    }
}