using Celeste.Mod.SpringCollab2020.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpringCollab2020.Triggers {
    [Mod.Entities.CustomEntity("SpringCollab2020/CustomBirdTutorialTrigger")]
    class CustomBirdTutorialTrigger : Trigger {
        private string birdId;
        private bool showTutorial;

        public CustomBirdTutorialTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            birdId = data.Attr("birdId");
            showTutorial = data.Bool("showTutorial");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            CustomBirdTutorial matchingBird = (CustomBirdTutorial)
                Scene.Tracker.GetEntities<CustomBirdTutorial>().Find(entity => entity is CustomBirdTutorial bird && bird.BirdId == birdId);

            if (matchingBird != null) {
                if (showTutorial) {
                    matchingBird.TriggerShowTutorial();
                } else {
                    matchingBird.TriggerHideTutorial();
                }
            }
        }
    }
}
