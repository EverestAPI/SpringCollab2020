module SpringCollab2020SpikeJumpThroughController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/SpikeJumpThroughController" SpikeJumpThroughController(x::Integer, y::Integer, persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "Spiked Jump Through Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SpikeJumpThroughController,
        "point"
    )
)

sprite = "ahorn/SpringCollab2020/spikeJumpThroughController"

function Ahorn.selection(entity::SpikeJumpThroughController)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpikeJumpThroughController, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
