module SpringCollab2020CustomizableGlassBlockController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/CustomizableGlassBlockController" CustomizableGlassBlockController(x::Integer, y::Integer,
    starColors::String="ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77", bgColor::String="302040")

const placements = Ahorn.PlacementDict(
    "Customizable Glass Block Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomizableGlassBlockController
    )
)

function Ahorn.selection(entity::CustomizableGlassBlockController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableGlassBlockController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
