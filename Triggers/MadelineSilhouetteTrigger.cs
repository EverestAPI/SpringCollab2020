using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [CustomEntity("SpringCollab2020/MadelineSilhouetteTrigger")]
    class MadelineSilhouetteTrigger : Trigger {
        private static Color silhouetteOutOfStaminaZeroDashBlinkColor = Calc.HexToColor("348DC1");

        public static void Load() {
            On.Celeste.Player.Added += onPlayerAdded;
            IL.Celeste.Player.Render += patchPlayerRender;
            On.Celeste.Player.ResetSprite += onPlayerResetSprite;
        }

        public static void Unload() {
            On.Celeste.Player.Added -= onPlayerAdded;
            IL.Celeste.Player.Render -= patchPlayerRender;
            On.Celeste.Player.ResetSprite -= onPlayerResetSprite;
        }

        private static void onPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene) {
            orig(self, scene);

            if (SpringCollab2020Module.Instance.Session.MadelineIsSilhouette) {
                refreshPlayerSpriteMode(self, true);
            }
        }

        private static void patchPlayerRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // jump to the usage of the Red color
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_Red"))) {
                Logger.Log("SpringCollab2020/MadelineSilhouetteTrigger", $"Patching silhouette hair color at {cursor.Index} in IL code for Player.Render()");

                // when Madeline blinks red, make the hair blink red.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Player, Color>>((color, player) => {
                    if (SpringCollab2020Module.Instance.Session.MadelineIsSilhouette) {
                        if (player.Dashes == 0) {
                            // blink to another shade of blue instead, to avoid red/blue flashes that are hard on the eyes.
                            color = silhouetteOutOfStaminaZeroDashBlinkColor;
                        }
                        player.Hair.Color = color;
                    }
                    return color;
                });
            }

            // jump to the usage of the White color
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("SpringCollab2020/MadelineSilhouetteTrigger", $"Patching silhouette color at {cursor.Index} in IL code for Player.Render()");

                // intercept Color.White (or whatever Max Helping Hand returned) and mod it if required.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, Player, Color>>((orig, self) => {
                    if (SpringCollab2020Module.Instance.Session.MadelineIsSilhouette) {
                        return self.Hair.Color;
                    } else {
                        return orig;
                    }
                });
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

        private static void onPlayerResetSprite(On.Celeste.Player.orig_ResetSprite orig, Player self, PlayerSpriteMode mode) {
            // filter all calls to ResetSprite when MadelineIsSilhouette is enabled, only the ones with Playback will go through.
            // this prevents Madeline from turning back into normal when the Other Self variant is toggled.
            if (!SpringCollab2020Module.Instance.Session.MadelineIsSilhouette || mode == PlayerSpriteMode.Playback) {
                orig(self, mode);
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
