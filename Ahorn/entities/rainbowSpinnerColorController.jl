module SpringCollab2020RainbowSpinnerColorController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/RainbowSpinnerColorController" RainbowSpinnerColorController(x::Integer, y::Integer,
    colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSize::Number=280.0)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Colour Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorController
    )
)

function Ahorn.selection(entity::RainbowSpinnerColorController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpinnerColorController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
