using Celeste.Mod.SpringCollab2020.Entities;
using Celeste.Mod.SpringCollab2020.Triggers;

namespace Celeste.Mod.SpringCollab2020 {
    public class SpringCollab2020Module : EverestModule {

        public static SpringCollab2020Module Instance;
        
        public SpringCollab2020Module() {
            Instance = this;
        }

        public override void Load() {
            NoRefillField.Load();
            FloatierSpaceBlock.Load();
            RemoveLightSourcesTrigger.Load();
        }

        public override void Unload() {
            NoRefillField.Unload();
            FloatierSpaceBlock.Unload();
            RemoveLightSourcesTrigger.Unload();
        }
    }
}