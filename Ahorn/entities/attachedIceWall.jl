module SpringCollab2020AttachedIceWall

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/AttachedIceWall" AttachedIceWall(x::Integer, y::Integer, height::Integer=8, left::Bool=false)

const placements = Ahorn.PlacementDict(
    "Attached Ice Wall (Right) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        AttachedIceWall,
        "rectangle",
        Dict{String, Any}(
            "left" => true
        )
    ),
    "Attached Ice Wall (Left) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        AttachedIceWall,
        "rectangle",
        Dict{String, Any}(
            "left" => false
        )
    )
)

Ahorn.minimumSize(entity::AttachedIceWall) = 0, 8
Ahorn.resizable(entity::AttachedIceWall) = false, true

function Ahorn.selection(entity::AttachedIceWall)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, 8, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AttachedIceWall, room::Maple.Room)
    left = get(entity.data, "left", false)

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    height = Int(get(entity.data, "height", 8))
    tileHeight = div(height, 8)

    if left
        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "objects/wallBooster/iceMid00", 0, (i - 1) * 8)
        end

        Ahorn.drawImage(ctx, "objects/wallBooster/iceTop00", 0, 0)
        Ahorn.drawImage(ctx, "objects/wallBooster/iceBottom00", 0, (tileHeight - 1) * 8)

    else
        Ahorn.Cairo.save(ctx)
        Ahorn.scale(ctx, -1, 1)

        for i in 2:tileHeight - 1
            Ahorn.drawImage(ctx, "objects/wallBooster/iceMid00", -8, (i - 1) * 8)
        end

        Ahorn.drawImage(ctx, "objects/wallBooster/iceTop00", -8, 0)
        Ahorn.drawImage(ctx, "objects/wallBooster/iceBottom00", -8, (tileHeight - 1) * 8)

        Ahorn.restore(ctx)
    end
end

# Offset X position so it flips in place
function Ahorn.flipped(entity::AttachedIceWall, horizontal::Bool)
    if horizontal
        entity.left = !entity.left
        entity.x += entity.left ? 8 : -8

        return entity
    end
end

end
