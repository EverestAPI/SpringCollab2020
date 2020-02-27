using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/MadelineSilhouetteTrigger")]
    class MadelineSilhouetteTrigger : Trigger {
        public static void Load() {
            On.Celeste.Player.Added += onPlayerAdded;
            IL.Celeste.Player.Render += patchPlayerRender;
        }

        public static void Unload() {
            On.Celeste.Player.Added -= onPlayerAdded;
            IL.Celeste.Player.Render -= patchPlayerRender;
        }

        private static void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            if (SpringCollab2020Module.Instance.Session.MadelineIsSilhouette) {
                refreshPlayerSpriteMode(self, true);
            }

            orig(self, scene);
        }

        private static void patchPlayerRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the usage of the White color
            if (cursor.TryGotoNext(instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("SpringCollab2020/MadelineSilhouetteTrigger", $"Patching player color at {cursor.Index} in IL code for Player.Render()");

                // instead of calling Color.White, call getMadelineColor just below.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Next.Operand = typeof(MadelineSilhouetteTrigger).GetMethod("GetMadelineColor");
            }
        }

        public static Color GetMadelineColor(Player player) {
            if (SpringCollab2020Module.Instance.Session.MadelineIsSilhouette) {
                return player.Hair.Color;
            } else {
                return Color.White;
            }
        }

        private static void refreshPlayerSpriteMode(Player player, bool enableSilhouette) {
            PlayerSpriteMode targetSpriteMode;
            if (enableSilhouette) {
                targetSpriteMode = PlayerSpriteMode.Playback;
            } else {
                targetSpriteMode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode;
            }

            if (player.Active) {
                player.ResetSpriteNextFrame(targetSpriteMode);
            } else {
                player.ResetSprite(targetSpriteMode);
            }
        }


        private bool enable;

        public MadelineSilhouetteTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enable = data.Bool("enable", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            bool oldValue = SpringCollab2020Module.Instance.Session.MadelineIsSilhouette;
            SpringCollab2020Module.Instance.Session.MadelineIsSilhouette = enable;

            // if the value changed...
            if (oldValue != enable) {
                // switch modes right now. this uses the same way as turning the "Other Self" variant on.
                refreshPlayerSpriteMode(player, enable);
            }
        }
    }
}
