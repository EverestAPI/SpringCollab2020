using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.SpringCollab2020.Entities;
using Celeste.Mod.Core;

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

        public void PrepareMapDataProcessor(MapDataFixup context) {
            context.Add<BerryMapDataProcessor>();
        }
    }

    public class BerryMapDataProcessor : EverestMapDataProcessor {
        public override void Reset() { }
        
        public override void End() { }

        public override Dictionary<string, Action<BinaryPacker.Element>> Init()
            => new Dictionary<string, Action<BinaryPacker.Element>>() {
                {
                    "entity:SpringCollab2020/diagonalWingedStrawberry", entity => {
                        Context.Run("entity:strawberry", entity);
                    }
                }
            };
    }
}
