using MonoMod.Cil;
using System;

namespace Celeste.Mod.SpringCollab2020 {
    class AnimatedEmojiInChapterName {
        private static int frameNumber = 0;

        public static void Load() {
            IL.Celeste.OuiChapterPanel.Render += modChapterPanelRender;
        }

        public static void Unload() {
            IL.Celeste.OuiChapterPanel.Render -= modChapterPanelRender;
        }

        private static void modChapterPanelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(Dialog), "Clean"))) {
                Logger.Log("SpringCollab2020/AnimatedEmojiInChapterName", $"Animating emoji in chapter name at {cursor.Index} in IL for OuiChapterPanel.Render");

                cursor.EmitDelegate<Func<string, string>>(mapName => {
                    frameNumber = (frameNumber + 1) % 10;
                    return mapName
                        .Replace(":runeline:", $":runeline0{frameNumber}:")
                        .Replace(":reverseruneline:", $":reverseruneline0{frameNumber}:");
                });
            }
        }
    }
}
