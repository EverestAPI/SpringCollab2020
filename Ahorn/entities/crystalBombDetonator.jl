module SpringCollab2020CrystalBombDetonator

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/crystalBombDetonator" CrystalBombDetonator(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight)

const placements = Ahorn.PlacementDict(
    "Crystal Bomb Detonation Field (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CrystalBombDetonator,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::CrystalBombDetonator) = 8, 8
Ahorn.resizable(entity::CrystalBombDetonator) = true, true

function Ahorn.selection(entity::CrystalBombDetonator)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrystalBombDetonator, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
    
    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.45, 0.0, 0.45, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end