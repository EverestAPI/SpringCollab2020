module SpringCollab2020FlagToggleWater

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/FlagToggleWater" FlagToggleWater(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth,
    height::Integer=Maple.defaultBlockHeight, hasBottom::Bool=false, flag::String="flag_toggle_water", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Flag Toggle Water (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleWater,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::FlagToggleWater) = 8, 8
Ahorn.resizable(entity::FlagToggleWater) = true, true

Ahorn.selection(entity::FlagToggleWater) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagToggleWater, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.0, 0.0, 1.0, 0.4), (0.0, 0.0, 1.0, 1.0))
end

end