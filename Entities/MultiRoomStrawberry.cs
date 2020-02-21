using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/MultiRoomStrawberry")]
    [RegisterStrawberry(true, false)]
    class MultiRoomStrawberry : Strawberry {
        private int seedCount;
        private EntityID id;

        public MultiRoomStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            seedCount = data.Int("seedCount");
            id = gid;
        }

        public override void Added(Scene scene) {
            // trick the berry into thinking it has vanilla seeds, so that it doesn't appear right away
            StrawberrySeed dummySeed = new StrawberrySeed(this, Vector2.Zero, 0, false);
            Seeds = new List<StrawberrySeed> { dummySeed };

            base.Added(scene);

            scene.Remove(dummySeed);
            Seeds = null;
        }

        public override void Update() {
            base.Update();

            if (WaitingOnSeeds) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null) {
                    // look at all the player followers, and filter all the seeds that match our berry.
                    List<StrawberrySeed> matchingSeeds = new List<StrawberrySeed>();
                    foreach (Follower follower in player.Leader.Followers) {
                        if (follower.Entity is MultiRoomStrawberrySeed seed) {
                            if (seed.BerryID.Level == id.Level && seed.BerryID.ID == id.ID) {
                                matchingSeeds.Add(seed);
                            }
                        }
                    }

                    if (matchingSeeds.Count >= seedCount) {
                        // all seeds have been gathered! associate the berry and the seeds, then trigger the cutscene.
                        Seeds = matchingSeeds;
                        foreach (StrawberrySeed seed in matchingSeeds) {
                            seed.Strawberry = this;
                        }

                        Scene.Add(new CSGEN_StrawberrySeeds(this));

                        // also clean up the session, since the seeds are now gone.
                        List<SpringCollab2020Session.MultiRoomStrawberrySeedInfo> seedList = SpringCollab2020Module.Instance.Session.CollectedMultiRoomStrawberrySeeds;
                        for (int i = 0; i < seedList.Count; i++) {
                            if (seedList[i].BerryID.Level == id.Level && seedList[i].BerryID.ID == id.ID) {
                                seedList.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }
    }
}
