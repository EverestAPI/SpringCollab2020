using Monocle;
using System;

namespace Celeste.Mod.SpringCollab2020.Entities {
    /// <summary>
    /// A component that can be attached to an entity to make it (dis)appear depending on a flag.
    /// </summary>
    class FlagToggleComponent : Component {
        public bool Enabled = true;
        private string flag;
        private Action onDisable;
        private Action onEnable;

        public FlagToggleComponent(string flag, Action onDisable = null, Action onEnable = null) : base(true, false) {
            this.flag = flag;
            this.onDisable = onDisable;
            this.onEnable = onEnable;
        }

        public override void Update() {
            base.Update();

            if (SceneAs<Level>().Session.GetFlag(flag) != Enabled) {
                if (Enabled) {
                    // disable the entity.
                    Entity.Visible = Entity.Collidable = false;
                    onDisable?.Invoke();
                    Enabled = false;
                } else {
                    // enable the entity.
                    Entity.Visible = Entity.Collidable = true;
                    onEnable?.Invoke();
                    Enabled = true;
                }
            }
        }
    }
}
