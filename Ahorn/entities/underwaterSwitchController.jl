module SpringCollab2020UnderwaterSwitchController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/UnderwaterSwitchController" UnderwaterSwitchController(x::Integer, y::Integer, flag::String="underwater_switch")

const placements = Ahorn.PlacementDict(
    "Underwater Switch Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        UnderwaterSwitchController
    )
)

function Ahorn.selection(entity::UnderwaterSwitchController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UnderwaterSwitchController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/SpringCollab2020/underwater_switch", 0, 0)

end
