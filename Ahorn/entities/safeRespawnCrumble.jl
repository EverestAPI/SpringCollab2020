module SpringCollab2020SafeRespawnCrumble

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/safeRespawnCrumble" SafeRespawnCrumble(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth)

const placements = Ahorn.PlacementDict(
    "Safe Respawn Crumble (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SafeRespawnCrumble,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::SafeRespawnCrumble) = 8, 0
Ahorn.resizable(entity::SafeRespawnCrumble) = true, false

function Ahorn.selection(entity::SafeRespawnCrumble)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SafeRespawnCrumble, room::Maple.Room)
    texture = "objects/SpringCollab2020/safeRespawnCrumble/tile"

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))
    tilesWidth = div(width, 8)

    Ahorn.Cairo.save(ctx)

    Ahorn.rectangle(ctx, 0, 0, width, 8)
    Ahorn.clip(ctx)

    for i in 0:ceil(Int, tilesWidth)
        Ahorn.drawImage(ctx, texture, 8 * i, 0, 0, 0, 8, 8)
    end

    Ahorn.restore(ctx)
end

end
