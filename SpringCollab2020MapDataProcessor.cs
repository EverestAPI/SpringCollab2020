using System;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020 {
    class SpringCollab2020MapDataProcessor : EverestMapDataProcessor {

        // the structure here is: FlagTouchSwitches[AreaSID][ModeID][flagName] = list of entity ids for flag touch switches in this group on this map.
        public static Dictionary<string, List<Dictionary<string, List<EntityID>>>> FlagTouchSwitches = new Dictionary<string, List<Dictionary<string, List<EntityID>>>>();
        private string levelName;

        // we want to match multi-room strawberry seeds with the strawberry that has the same name.
        private Dictionary<string, List<BinaryPacker.Element>> multiRoomStrawberrySeedsByName = new Dictionary<string, List<BinaryPacker.Element>>();
        private Dictionary<string, EntityID> multiRoomStrawberryIDsByName = new Dictionary<string, EntityID>();
        private Dictionary<string, BinaryPacker.Element> multiRoomStrawberriesByName = new Dictionary<string, BinaryPacker.Element>();

        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "level", level => {
                        // be sure to write the level name down.
                        levelName = level.Attr("name").Split(':')[0];
                        if (levelName.StartsWith("lvl_")) {
                            levelName = levelName.Substring(4);
                        }
                    }
                }, {
                    "entity:SpringCollab2020/FlagTouchSwitch", flagTouchSwitch => {
                        string flag = flagTouchSwitch.Attr("flag");
                        Dictionary<string, List<EntityID>> allTouchSwitchesInMap = FlagTouchSwitches[AreaKey.SID][(int) AreaKey.Mode];

                        // if no dictionary entry exists for this flag, create one. otherwise, get it.
                        List<EntityID> entityIDs;
                        if (!allTouchSwitchesInMap.ContainsKey(flag)) {
                            entityIDs = new List<EntityID>();
                            allTouchSwitchesInMap[flag] = entityIDs;
                        } else {
                            entityIDs = allTouchSwitchesInMap[flag];
                        }

                        // add this flag touch switch to the dictionary.
                        entityIDs.Add(new EntityID(levelName, flagTouchSwitch.AttrInt("id")));
                    }
                },
                {
                    "entity:SpringCollab2020/MultiRoomStrawberrySeed", strawberrySeed => {
                        // auto-attribute indices for seeds, and save them.
                        string berryName = strawberrySeed.Attr("strawberryName");
                        if (multiRoomStrawberrySeedsByName.ContainsKey(berryName)) {
                            if (strawberrySeed.AttrInt("index") < 0) {
                                strawberrySeed.SetAttr("index", multiRoomStrawberrySeedsByName[berryName].Count);
                            }
                            multiRoomStrawberrySeedsByName[berryName].Add(strawberrySeed);
                        } else {
                            if (strawberrySeed.AttrInt("index") < 0) {
                                strawberrySeed.SetAttr("index", 0);
                            }
                            multiRoomStrawberrySeedsByName[berryName] = new List<BinaryPacker.Element>() { strawberrySeed };
                        }
                    }
                },
                {
                    "entity:SpringCollab2020/MultiRoomStrawberry", strawberry => {
                        // save the strawberry IDs.
                        string berryName = strawberry.Attr("name");
                        multiRoomStrawberryIDsByName[berryName] = new EntityID(levelName, strawberry.AttrInt("id"));
                        multiRoomStrawberriesByName[berryName] = strawberry;
                    }
                }
            };
        }

        public override void Reset() {
            if (!FlagTouchSwitches.ContainsKey(AreaKey.SID)) {
                // create an entry for the current map SID.
                FlagTouchSwitches[AreaKey.SID] = new List<Dictionary<string, List<EntityID>>>();
            }
            while (FlagTouchSwitches[AreaKey.SID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries.
                FlagTouchSwitches[AreaKey.SID].Add(new Dictionary<string, List<EntityID>>());
            }

            // reset the dictionary for the current map and mode.
            FlagTouchSwitches[AreaKey.SID][(int) AreaKey.Mode] = new Dictionary<string, List<EntityID>>();
        }

        public override void End() {
            foreach (string strawberryName in multiRoomStrawberrySeedsByName.Keys) {
                if (!multiRoomStrawberryIDsByName.ContainsKey(strawberryName)) {
                    Logger.Log(LogLevel.Warn, "SpringCollab2020MapDataProcessor", $"Multi-room strawberry seeds with name {strawberryName} didn't match any multi-room strawberry");
                } else {
                    // give the strawberry ID to the seeds.
                    EntityID strawberryID = multiRoomStrawberryIDsByName[strawberryName];
                    foreach (BinaryPacker.Element strawberrySeed in multiRoomStrawberrySeedsByName[strawberryName]) {
                        strawberrySeed.SetAttr("berryLevel", strawberryID.Level);
                        strawberrySeed.SetAttr("berryID", strawberryID.ID);
                    }

                    // and give the expected seed count to the strawberry.
                    multiRoomStrawberriesByName[strawberryName].SetAttr("seedCount", multiRoomStrawberrySeedsByName[strawberryName].Count);
                }
            }

            multiRoomStrawberrySeedsByName.Clear();
            multiRoomStrawberryIDsByName.Clear();
        }
    }
}
