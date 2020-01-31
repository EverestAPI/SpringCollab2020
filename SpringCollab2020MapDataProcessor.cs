using System;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020 {
    class SpringCollab2020MapDataProcessor : EverestMapDataProcessor {

        // the structure here is: FlagTouchSwitches[AreaID][ModeID][flagName] = list of entity ids for flag touch switches in this group on this map.
        public static List<List<Dictionary<string, List<EntityID>>>> FlagTouchSwitches = new List<List<Dictionary<string, List<EntityID>>>>();
        private string levelName;

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
                },
                {
                    "entity:SpringCollab2020/FlagTouchSwitch", flagTouchSwitch => {
                        string flag = flagTouchSwitch.Attr("flag");
                        Dictionary<string, List<EntityID>> allTouchSwitchesInMap = FlagTouchSwitches[AreaKey.ID][(int) AreaKey.Mode];

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
                }
            };
        }

        public override void Reset() {
            while (FlagTouchSwitches.Count <= AreaKey.ID) {
                // fill out the empty space before the current map with empty dictionaries. 
                FlagTouchSwitches.Add(new List<Dictionary<string, List<EntityID>>>());
            }
            while (FlagTouchSwitches[AreaKey.ID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries. 
                FlagTouchSwitches[AreaKey.ID].Add(new Dictionary<string, List<EntityID>>());
            }

            // reset the dictionary for the current map and mode.
            FlagTouchSwitches[AreaKey.ID][(int) AreaKey.Mode] = new Dictionary<string, List<EntityID>>();
        }

        public override void End() {
            // nothing required.
        }
    }
}
