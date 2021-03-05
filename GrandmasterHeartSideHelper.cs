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
        private static Hook hookLevelExitToLobby;

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
                        // "return to lobby" but it actually returns to the new GM heart side.
                        return () => {
                            hookLevelExitToLobby = new Hook(
                                typeof(CollabUtils2.UI.LevelExitToLobby).GetMethod("Begin"),
                                typeof(GrandmasterHeartSideHelper).GetMethod("modLevelExitToLobby", BindingFlags.NonPublic | BindingFlags.Static));
                            On.Celeste.LevelEnter.Go += modLevelEnter;

                            Engine.Scene = new CollabUtils2.UI.LevelExitToLobby(LevelExit.Mode.Completed, self.Session);
                        };
                    } else {
                        // do the usual.
                        return orig;
                    }
                });
            }
        }

        private static void modLevelExitToLobby(Action<CollabUtils2.UI.LevelExitToLobby> orig, CollabUtils2.UI.LevelExitToLobby self) {
            // back up return location.
            string bakSID = CollabUtils2.CollabModule.Instance.Session.LobbySID;
            string bakRoom = CollabUtils2.CollabModule.Instance.Session.LobbyRoom;
            float bakX = CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointX;
            float bakY = CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointY;

            // modify it to the new gmhs.
            CollabUtils2.CollabModule.Instance.Session.LobbySID = "SpringCollab2020/5-Grandmaster/ZZ-NewHeartSide";
            CollabUtils2.CollabModule.Instance.Session.LobbyRoom = null;
            CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointX = 0;
            CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointY = 0;

            // run collab utils code.
            orig(self);

            // restore old values.
            CollabUtils2.CollabModule.Instance.Session.LobbySID = bakSID;
            CollabUtils2.CollabModule.Instance.Session.LobbyRoom = bakRoom;
            CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointX = bakX;
            CollabUtils2.CollabModule.Instance.Session.LobbySpawnPointY = bakY;

            // undo the hook so that future returns to lobby behave normally.
            hookLevelExitToLobby?.Dispose();
            hookLevelExitToLobby = null;
        }

        private static void modLevelEnter(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData) {
            // This hook is only applied when returning from the old GMHS.
            // We know we are returning to the start of new GMHS, so set up the session properly for that.
            session.FirstLevel = true;
            session.StartedFromBeginning = true;
            new DynData<Session>(session)["pauseTimerUntilAction"] = false;

            orig(session, fromSaveData);

            // and undo the hook.
            On.Celeste.LevelEnter.Go -= modLevelEnter;
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

                        MTN.FileSelect["heart"].Draw(self.Position + new Vector2(-580f, 130f));
                    }
                });
            }
        }
    }
}
