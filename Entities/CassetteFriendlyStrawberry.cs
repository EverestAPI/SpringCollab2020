using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// Just a strawberry with cassette-friendly strawberry seeds.
    /// </summary>
    [CustomEntity("SpringCollab2020/CassetteFriendlyStrawberry")]
    [RegisterStrawberry(true, false)]
    class CassetteFriendlyStrawberry : Strawberry {
        public CassetteFriendlyStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            bool isGhostBerry = SaveData.Instance.CheckStrawberry(ID);

            // we just want to create cassette-friendly strawberry seeds to replace vanilla seeds.
            if (data.Nodes != null && data.Nodes.Length != 0) {
                Seeds = new List<StrawberrySeed>();
                for (int i = 0; i < data.Nodes.Length; i++) {
                    Seeds.Add(new CassetteFriendlyStrawberrySeed(this, offset + data.Nodes[i], i, isGhostBerry));
                }
            }
        }
    }
}
