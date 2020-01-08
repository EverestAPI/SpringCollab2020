using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.SpringCollab2020.Entities {

    [Tracked(false)]
    [CustomEntity("SpringCollab2020/caveWall")]
    public class CaveWall : Entity {

        private char fillTile;

        private TileGrid tiles;

        private bool fadeOut;
        private bool fadeIn = true;

        private EffectCutout cutout;

        private float transitionStartAlpha;

        private bool transitionFade;

        private CaveWall master;

        private bool awake;

        public List<CaveWall> Group;

        public Point GroupBoundsMin;

        public Point GroupBoundsMax;

        public bool HasGroup {
            get;
            private set;
        }

        public bool MasterOfGroup {
            get;
            private set;
        }

        public CaveWall(Vector2 position, char tile, float width, float height)
            : base(position) {
            fillTile = tile;
            Collider = new Hitbox(width, height);
            Depth = -13001;
            Add(cutout = new EffectCutout());
        }

        public CaveWall(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height) {
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            awake = true;
            if (!HasGroup) {
                MasterOfGroup = true;
                Group = new List<CaveWall>();
                GroupBoundsMin = new Point((int) X, (int) Y);
                GroupBoundsMax = new Point((int) Right, (int) Bottom);
                AddToGroupAndFindChildren(this);

                Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> virtualMap = level.SolidsData.Clone();
                foreach (CaveWall item in Group) {
                    int x = (int) (item.X / 8f) - level.Session.MapData.TileBounds.X;
                    int y = (int) (item.Y / 8f - level.Session.MapData.TileBounds.Y);
                    int tilesX = (int) (item.Width / 8f);
                    int tilesY = (int) (item.Height / 8f);
                    for (int i = x; i < x + tilesX; i++) {
                        for (int j = y; j < y + tilesY; j++) {
                            virtualMap[i, j] = fillTile;
                        }
                    }
                }
                // Pretend that the CaveWalls are just part of the map, then draw the CaveWalls on top of it
                // This is a complicated solution to getting the CaveWalls to blend with both the foregroundTiles and each other
                foreach (CaveWall item in Group) {
                    int x = (int) item.X / 8 - tileBounds.Left;
                    int y = (int) item.Y / 8 - tileBounds.Top;
                    int tilesX = (int) item.Width / 8;
                    int tilesY = (int) item.Height / 8;
                    item.tiles = GFX.FGAutotiler.GenerateOverlay(item.fillTile, x, y, tilesX, tilesY, virtualMap).TileGrid;
                    item.Add(item.tiles);
                    item.Add(new TileInterceptor(item.tiles, highPriority: false));
                }
            }
            TryToInitPosition();

            if (CollideCheck<Player>()) {
                tiles.Alpha = 0f;
                fadeOut = true;
                fadeIn = false;
                cutout.Visible = false;
            }

            TransitionListener transitionListener = new TransitionListener();
            transitionListener.OnOut = OnTransitionOut;
            transitionListener.OnOutBegin = OnTransitionOutBegin;
            transitionListener.OnIn = OnTransitionIn;
            transitionListener.OnInBegin = OnTransitionInBegin;
            Add(transitionListener);
        }

        private void TryToInitPosition() {
            if (MasterOfGroup) {
                foreach (CaveWall item in Group) {
                    if (!item.awake) {
                        return;
                    }
                }
            } else {
                master.TryToInitPosition();
            }
        }

        private void AddToGroupAndFindChildren(CaveWall from) {
            if (from.X < GroupBoundsMin.X) {
                GroupBoundsMin.X = (int) from.X;
            }
            if (from.Y < GroupBoundsMin.Y) {
                GroupBoundsMin.Y = (int) from.Y;
            }
            if (from.Right > GroupBoundsMax.X) {
                GroupBoundsMax.X = (int) from.Right;
            }
            if (from.Bottom > GroupBoundsMax.Y) {
                GroupBoundsMax.Y = (int) from.Bottom;
            }
            from.HasGroup = true;
            Group.Add(from);
            if (from != this) {
                from.master = this;
            }
            foreach (CaveWall entity in Scene.Tracker.GetEntities<CaveWall>()) {
                if (!entity.HasGroup && entity.fillTile == fillTile && (Scene.CollideCheck(new Rectangle((int) from.X - 1, (int) from.Y, (int) from.Width + 2, (int) from.Height), entity) || Scene.CollideCheck(new Rectangle((int) from.X, (int) from.Y - 1, (int) from.Width, (int) from.Height + 2), entity))) {
                    AddToGroupAndFindChildren(entity);
                }
            }
        }

        private void OnTransitionOutBegin() {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds)) {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            } else {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent) {
            if (transitionFade) {
                tiles.Alpha = transitionStartAlpha * (1f - percent);
            }
        }

        private void OnTransitionInBegin() {
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value)) {
                transitionFade = true;
                tiles.Alpha = 0f;
            } else {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent) {
            if (transitionFade) {
                tiles.Alpha = percent;
            }
        }

        public override void Update() {
            base.Update();
            if (fadeOut) {
                tiles.Alpha = Calc.Approach(tiles.Alpha, 0f, 3f * Engine.DeltaTime);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha <= 0f) {
                    tiles.Alpha = 0;
                }
            } else if (fadeIn) {
                tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, 3f * Engine.DeltaTime);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha >= 1f) {
                    tiles.Alpha = 1;
                }
            }

            if (MasterOfGroup) {
                Player player = null;
                foreach (CaveWall entity in Group) {
                    player = entity.CollideFirst<Player>();
                    if (player != null) {
                        break;
                    }
                }

                if (player != null && player.StateMachine.State != 9) {
                    fadeOut = true;
                    foreach (CaveWall entity in Group) {
                        entity.fadeOut = true;
                    }
                } else if (fadeOut) {
                    fadeOut = false;
                    fadeIn = true;
                    foreach (CaveWall entity in Group) {
                        entity.fadeOut = false;
                        entity.fadeIn = true;
                    }
                }
            }
        }

        public override void Render() {
            Level level = Scene as Level;
            if (level.ShakeVector.X < 0f && level.Camera.X <= level.Bounds.Left && X <= level.Bounds.Left) {
                tiles.RenderAt(Position + new Vector2(-3f, 0f));
            }
            if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= level.Bounds.Right && X + Width >= level.Bounds.Right) {
                tiles.RenderAt(Position + new Vector2(3f, 0f));
            }
            if (level.ShakeVector.Y < 0f && level.Camera.Y <= level.Bounds.Top && Y <= level.Bounds.Top) {
                tiles.RenderAt(Position + new Vector2(0f, -3f));
            }
            if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= level.Bounds.Bottom && Y + Height >= level.Bounds.Bottom) {
                tiles.RenderAt(Position + new Vector2(0f, 3f));
            }
            base.Render();
        }
    }
}
