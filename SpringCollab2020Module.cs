using Celeste.Mod.SpringCollab2020.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Module : EverestModule {

        public static SpringCollab2020Module Instance;
        
        public SpringCollab2020Module() {
            Instance = this;
        }

        public override void Load() {
            Everest.Events.Level.OnExit += onLevelExit;
        }

        public override void Unload() {
            Everest.Events.Level.OnExit -= onLevelExit;
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            if (mode == LevelExit.Mode.SaveAndQuit) {
                // if saving in a No Refill Field, be sure to restore the refills.
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null && player.CollideCheck<NoRefillField>()) {
                    level.Session.Inventory.NoRefills = false;
                }
            }
        }
    }
}
