module SpringCollab2020RainbowSpinnerColorAreaController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/RainbowSpinnerColorAreaController" RainbowSpinnerColorAreaController(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSize::Number=280.0)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Colour Area Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorAreaController,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::RainbowSpinnerColorAreaController) = 8, 8
Ahorn.resizable(entity::RainbowSpinnerColorAreaController) = true, true

Ahorn.selection(entity::RainbowSpinnerColorAreaController) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpinnerColorAreaController, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.4, 0.4, 1.0, 0.4), (0.4, 0.4, 1.0, 1.0))
end

end
