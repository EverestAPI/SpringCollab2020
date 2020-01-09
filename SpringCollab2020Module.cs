using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.SpringCollab2020.Entities;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Module : EverestModule {

        public static SpringCollab2020Module Instance;
        
        public SpringCollab2020Module() {
            Instance = this;
        }

        public override void Load() {
            DiagonalWingedStrawberry.Load();
        }

        public override void Unload() {
            DiagonalWingedStrawberry.Unload();
        }
    }
}
