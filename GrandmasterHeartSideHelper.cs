using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Reflection;
using System.Linq;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using Mono.Cecil;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020 {
    // The new Grandmaster Heart Side is called ZZ-NewHeartSide and the old one is hidden at ZZ-HeartSide.
    // This requires some specific handling from the Collab Utils:
    // - hiding old GMHS from the journals, and not making it behave like a heart side
    // - making new GMHS behave like a heart side
    // We also want the ending heart of old GMHS to send back to the new GMHS, instead of displaying an endscreen.
    static class GrandmasterHeartSideHelper {
        private static Hook hookIsHeartSide;
        private static ILHook hookLobbyJournal;
        private static ILHook hookOverworldJournal;
        private static ILHook hookPoemColors;

        public static void Load() {
            Assembly collabUtils = typeof(CollabUtils2.CollabModule).Assembly;

            hookIsHeartSide = new Hook(
                collabUtils.GetType("Celeste.Mod.CollabUtils2.LobbyHelper").GetMethod("IsHeartSide"),
                typeof(GrandmasterHeartSideHelper).GetMethod("modIsHeartSide", BindingFlags.NonPublic | BindingFlags.Static));

            hookOverworldJournal = new ILHook(
                collabUtils.GetType("Celeste.Mod.CollabUtils2.UI.OuiJournalCollabProgressInOverworld").GetConstructor(new Type[] { typeof(OuiJournal) }),
                modOverworldJournal);

            hookLobbyJournal = new ILHook(
                collabUtils.GetType("Celeste.Mod.CollabUtils2.UI.OuiJournalCollabProgressInLobby").GetMethod("GeneratePages"),
                modLobbyJournal);

            IL.Celeste.Level.CompleteArea_bool_bool_bool += modLevelComplete;
            IL.Celeste.OuiChapterPanel.Render += renderOldGMHSCompletionStamp;

            // we are looking for a lambda in Celeste.Mod.CollabUtils2.LobbyHelper.modJournalPoemHeartColors...
            // except it is located in Celeste.Mod.CollabUtils2.LobbyHelper.<>c.<modJournalPoemHeartColors>b__{someRandomNumber}_0.
            // find it by bruteforcing it a bit.
            Type innerType = collabUtils.GetType("Celeste.Mod.CollabUtils2.LobbyHelper").GetNestedType("<>c", BindingFlags.NonPublic);
            for (int i = 0; i < 100; i++) {
                MethodInfo innerMethod = innerType.GetMethod($"<modJournalPoemHeartColors>b__{i}_0", BindingFlags.NonPublic | BindingFlags.Instance);
                if (innerMethod != null) {
                    // found it!
                    hookPoemColors = new ILHook(innerMethod, modifyGMHSeartColor);
                    break;
                }
            }
        }

        public static void Unload() {
            hookIsHeartSide?.Dispose();
            hookIsHeartSide = null;

            hookLobbyJournal?.Dispose();
            hookLobbyJournal = null;

            hookOverworldJournal?.Dispose();
            hookOverworldJournal = null;

            IL.Celeste.Level.CompleteArea_bool_bool_bool -= modLevelComplete;
            IL.Celeste.OuiChapterPanel.Render -= renderOldGMHSCompletionStamp;

            hookPoemColors?.Dispose();
            hookPoemColors = null;
        }

        private static bool modIsHeartSide(Func<string, bool> orig, string sid) {
            if (sid == "SpringCollab2020/5-Grandmaster/ZZ-HeartSide") {
                return false; // old GMHS
            } else if (sid == "SpringCollab2020/5-Grandmaster/ZZ-NewHeartSide") {
                return true; // new GMHS
            }

            return orig(sid); // ... not GMHS
        }

        private static void modLobbyJournal(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SaveData>("get_Areas_Safe"))) {
                Logger.Log("SpringCollab2020/GrandmasterHeartSideHelper", $"Filtering out old GMHS from journal at {cursor.Index} in IL for OuiJournalProgressInLobby.GeneratePages");
                cursor.EmitDelegate<Func<List<AreaStats>, List<AreaStats>>>(orig => orig.Where(area => area.SID != "SpringCollab2020/5-Grandmaster/ZZ-HeartSide").ToList());
            }
        }

        private static void modOverworldJournal(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelSetStats>("Areas"))) {
                Logger.Log("SpringCollab2020/GrandmasterHeartSideHelper", $"Filtering out old GMHS from journal at {cursor.Index} in IL for OuiJournalCollabProgressInOverworld ctor");
                cursor.EmitDelegate<Func<List<AreaStats>, List<AreaStats>>>(orig => orig.Where(area => area.SID != "SpringCollab2020/5-Grandmaster/ZZ-HeartSide").ToList());
            }
        }


        private static void modLevelComplete(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.OpCode == OpCodes.Ldftn && ((instr.Operand as MethodReference)?.Name.StartsWith("<CompleteArea>") ?? false),
                instr => instr.MatchNewobj<Action>())) {

                Logger.Log("SpringCollab2020/GrandmasterHeartSideHelper", $"Redirecting old GMHS ending to new GMHS at {cursor.Index} in IL for Level.CompleteArea");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Action, Level, Action>>((orig, self) => {
                    if (self.Session.Area.GetSID() == "SpringCollab2020/5-Grandmaster/ZZ-HeartSide") {
                        // "return to lobby" (new GMHS) instead of returning to map.
                        return () => {
                            Engine.Scene = new CollabUtils2.UI.LevelExitToLobby(LevelExit.Mode.Completed, self.Session);
                        };
                    } else {
                        // do the usual.
                        return orig;
                    }
                });
            }
        }

        private static void renderOldGMHSCompletionStamp(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // draw the stamp just after the chapter card.
            if (cursor.TryGotoNext(instr => instr.MatchStfld<OuiChapterPanel>("card"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<MTexture>("Draw"))) {

                Logger.Log("SpringCollab2020/GrandmasterHeartSideHelper", $"Injecting GMHS stamp rendering at {cursor.Index} in IL for OuiChapterPanel.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<OuiChapterPanel>>(self => {
                    // draw it only if the player beat old grandmaster heart side, and we're actually looking at it.
                    if (self.Area.GetSID() == "SpringCollab2020/5-Grandmaster/ZZ-NewHeartSide"
                        && (SaveData.Instance.GetAreaStatsFor(AreaData.Get("SpringCollab2020/5-Grandmaster/ZZ-HeartSide").ToKey())?.Modes[0].Completed ?? false)) {

                        GFX.Gui["SpringCollab2020/OldGMHSStamp"].Draw(self.Position + new Vector2(40f, 150f));
                    }
                });
            }
        }

        private static void modifyGMHSeartColor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("_ZZ_HeartSide_A"), instr => instr.MatchCall<string>("Concat"))) {
                Logger.Log("SpringCollab2020/GrandmasterHeartSideHelper", $"Modifying new GMHS heart color at {cursor.Index} in IL for {il.Method.FullName}");

                cursor.Emit(OpCodes.Ldarg_2); // "poem" argument
                cursor.EmitDelegate<Func<string, string, string>>((orig, poem) => {
                    // alter the check so that it matches on the new GMHS like it does for old GMHS.
                    if (orig == "poem_SpringCollab2020/5-Grandmaster_ZZ_HeartSide_A" && poem == Dialog.Clean("poem_SpringCollab2020_5_Grandmaster_ZZ_NewHeartSide_A")) {
                        return "poem_SpringCollab2020_5_Grandmaster_ZZ_NewHeartSide_A";
                    }
                    return orig;
                });
            }
        }
    }
}
