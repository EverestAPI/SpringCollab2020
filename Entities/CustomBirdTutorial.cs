using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/CustomBirdTutorial")]
    [Tracked]
    class CustomBirdTutorial : BirdNPC {
        public string BirdId;
        private bool onlyOnce;
        private bool caw;

        private bool triggered = false;
        private bool flewAway = false;

        private BirdTutorialGui gui;

        private static Dictionary<string, Vector2> directions = new Dictionary<string, Vector2>() {
            { "Left", new Vector2(-1, 0) },
            { "Right", new Vector2(1, 0) },
            { "Up", new Vector2(0, -1) },
            { "Down", new Vector2(0, 1) },
            { "UpLeft", new Vector2(-1, -1) },
            { "UpRight", new Vector2(1, -1) },
            { "DownLeft", new Vector2(-1, 1) },
            { "DownRight", new Vector2(1, 1) }
        };

        public CustomBirdTutorial(EntityData data, Vector2 offset) : base(data, offset) {
            BirdId = data.Attr("birdId");
            onlyOnce = data.Bool("onlyOnce");
            caw = data.Bool("caw");
            Facing = data.Bool("faceLeft") ? Facings.Left : Facings.Right;

            object info;
            object[] controls;

            // parse the info ("title")
            string infoString = data.Attr("info");
            if (GFX.Gui.Has(infoString)) {
                info = GFX.Gui[infoString];
            } else {
                info = Dialog.Clean(infoString);
            }

            int extraAdvance = 0;

            // go ahead and parse the controls. Controls can be textures, VirtualButtons, directions or strings.
            string[] controlsStrings = data.Attr("controls").Split(',');
            controls = new object[controlsStrings.Length];
            for (int i = 0; i < controls.Length; i++) {
                string controlString = controlsStrings[i];

                if (GFX.Gui.Has(controlString)) {
                    // this is a texture.
                    controls[i] = GFX.Gui[controlString];
                } else if (directions.ContainsKey(controlString)) {
                    // this is a direction.
                    controls[i] = directions[controlString];
                } else {
                    FieldInfo matchingInput = typeof(Input).GetField(controlString, BindingFlags.Static | BindingFlags.Public);
                    if (matchingInput?.GetValue(null)?.GetType() == typeof(VirtualButton)) {
                        // this is a button.
                        controls[i] = matchingInput.GetValue(null);
                    } else {
                        // when BirdTutorialGui renders text, it is offset by 1px on the right.
                        // width computation doesn't take this 1px into account, so we should add it back in.
                        extraAdvance++;
                        if (i == 0) {
                            // as the text is rendered 1px to the right, if the first thing is a string, there will be 1px more padding on the left.
                            // we should add that extra px on the right as well.
                            extraAdvance++;
                        }

                        if (controlString.StartsWith("dialog:")) {
                            // treat that as a dialog key.
                            controls[i] = Dialog.Clean(controlString.Substring("dialog:".Length));
                        } else {
                            // treat that as a plain string.
                            controls[i] = controlString;
                        }
                    }
                }
            }

            gui = new BirdTutorialGui(this, new Vector2(0f, -16f), info, controls);

            DynData<BirdTutorialGui> guiData = new DynData<BirdTutorialGui>(gui);
            // if there is no first line, resize the bubble accordingly.
            if (string.IsNullOrEmpty(infoString)) {
                guiData["infoHeight"] = 0f;
            }
            // apply the extra width.
            guiData["controlsWidth"] = guiData.Get<float>("controlsWidth") + extraAdvance;
        }

        public void TriggerShowTutorial() {
            if (!triggered) {
                triggered = true;
                Add(new Coroutine(ShowTutorial(gui, caw)));
            }
        }

        public void TriggerHideTutorial() {
            if (triggered && !flewAway) {
                flewAway = true;

                Add(new Coroutine(HideTutorial()));
                Add(new Coroutine(StartleAndFlyAway()));

                if (onlyOnce) {
                    SceneAs<Level>().Session.DoNotLoad.Add(EntityID);
                }
            }
        }
    }
}
