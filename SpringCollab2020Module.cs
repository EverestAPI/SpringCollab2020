using Celeste.Mod.SpringCollab2020.Effects;
using Celeste.Mod.SpringCollab2020.Entities;
using Celeste.Mod.SpringCollab2020.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Module : EverestModule {

        public static SpringCollab2020Module Instance;

        public override Type SessionType => typeof(SpringCollab2020Session);
        public SpringCollab2020Session Session => (SpringCollab2020Session) _Session;

        public SpringCollab2020Module() {
            Instance = this;
        }

        public override void Load() {
            Logger.SetLogLevel("SpringCollab2020", LogLevel.Info);

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
            FlagTouchSwitch.Load();
            DisableIcePhysicsTrigger.Load();
            MultiRoomStrawberrySeed.Load();
            MadelineSilhouetteTrigger.Load();
            BlockJellySpawnTrigger.Load();
            StrawberryIgnoringLighting.Load();
            SeekerCustomColors.Load();
            CameraCatchupSpeedTrigger.Load();
            ColorGradeFadeTrigger.Load();
            SpeedBasedMusicParamTrigger.Load();
            StaticPuffer.Load();
            LeaveTheoBehindTrigger.Load();
            BadelineBounceDirectionTrigger.Load();
            WaterRocketLaunchingComponent.Load();
            Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;

            DecalRegistry.AddPropertyHandler("scale", (decal, attrs) => {
                Vector2 scale = decal.Scale;
                if (attrs["multiply"] != null) {
                    scale *= float.Parse(attrs["multiply"].Value);
                }
                if (attrs["divide"] != null) {
                    scale /= float.Parse(attrs["divide"].Value);
                }
                decal.Scale = scale;
            });
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);
            GlassBerry.LoadContent();
            StrawberryIgnoringLighting.LoadContent();
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
            FlagTouchSwitch.Unload();
            DisableIcePhysicsTrigger.Unload();
            MultiRoomStrawberrySeed.Unload();
            MadelineSilhouetteTrigger.Unload();
            BlockJellySpawnTrigger.Unload();
            StrawberryIgnoringLighting.Unload();
            SeekerCustomColors.Unload();
            CameraCatchupSpeedTrigger.Unload();
            ColorGradeFadeTrigger.Unload();
            SpeedBasedMusicParamTrigger.Unload();
            StaticPuffer.Unload();
            LeaveTheoBehindTrigger.Unload();
            BadelineBounceDirectionTrigger.Unload();
            WaterRocketLaunchingComponent.Unload();
            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above) {
            if (child.Name.Equals("SpringCollab2020/HeatWaveNoColorGrade", StringComparison.OrdinalIgnoreCase)) {
                return new HeatWaveNoColorGrade();
            }
            if (child.Name.Equals("SpringCollab2020/CustomSnow", StringComparison.OrdinalIgnoreCase)) {
                string[] colorsAsStrings = child.Attr("colors").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]);
                }

                return new CustomSnow(colors, child.AttrBool("foreground"));
            }
            return null;
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<SpringCollab2020MapDataProcessor>();
        }
    }
}