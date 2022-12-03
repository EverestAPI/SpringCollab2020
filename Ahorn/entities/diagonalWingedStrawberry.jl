module SpringCollab2020DiagonalWingedStrawberry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/diagonalWingedStrawberry" DiagonalWingedStrawberry(x::Integer, y::Integer, order::Integer=-1, checkpointID::Integer=-1)

const placements = Ahorn.PlacementDict(
    "Diagonal Winged Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
        DiagonalWingedStrawberry
    )
)

function Ahorn.selection(entity::DiagonalWingedStrawberry)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("collectables/strawberry/wings01", x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DiagonalWingedStrawberry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    Ahorn.drawSprite(ctx, "collectables/strawberry/wings01", x, y)
end

end
