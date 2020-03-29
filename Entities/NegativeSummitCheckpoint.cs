using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.SpringCollab2020.Entities {
    [CustomEntity("SpringCollab2020/NegativeSummitCheckpoint")]
    class NegativeSummitCheckpoint : SummitCheckpoint {
        public NegativeSummitCheckpoint(EntityData data, Vector2 offset) : base(data, offset) {
            // convert the checkpoint number to string. the minus sign is replaced by :, because this is the character after 9 in the chartable.
            // so, : will render the number10.png sprite, which is a minus sign.
            string numberString = Number.ToString().Replace("-", ":");

            // add leading zeroes
            while (numberString.Length < 2) {
                numberString = $"0{numberString}";
            }

            // replace the vanilla number string, which will be rendered.
            new DynData<SummitCheckpoint>(this)["numberString"] = numberString;
        }
    }
}
