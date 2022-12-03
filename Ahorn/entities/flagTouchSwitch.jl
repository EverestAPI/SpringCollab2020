module SpringCollab2020FlagTouchSwitch

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/FlagTouchSwitch" FlagTouchSwitch(x::Integer, y::Integer,
    flag::String="flag_touch_switch", icon::String="vanilla", persistent::Bool=false, inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF")

const bundledIcons = String["vanilla", "tall", "triangle", "circle"]

const placements = Ahorn.PlacementDict(
    "Flag Touch Switch (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagTouchSwitch
    )
)

Ahorn.editingOptions(entity::FlagTouchSwitch) = Dict{String,Any}(
    "icon" => bundledIcons
)

function Ahorn.selection(entity::FlagTouchSwitch)
    x, y = Ahorn.position(entity)

    return  Ahorn.Rectangle(x - 7, y - 7, 14, 14)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagTouchSwitch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/touchswitch/container.png", 0, 0)

    icon = get(entity.data, "icon", "vanilla")

    iconPath = "objects/touchswitch/icon00.png"
    if icon != "vanilla"
        iconPath = "objects/SpringCollab2020/flagTouchSwitch/$(icon)/icon00.png"
    end

    Ahorn.drawSprite(ctx, iconPath, 0, 0)
end

end
