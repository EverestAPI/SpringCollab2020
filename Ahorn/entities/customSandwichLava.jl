module SpringCollab2020CustomSandwichLava

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/CustomSandwichLava" CustomSandwichLava(x::Integer, y::Integer, 
    direction::String="CoreModeBased", speed::Number=20, sandwichGap::Number=160)

const directions = String["AlwaysUp", "AlwaysDown", "CoreModeBased"]

const placements = Ahorn.PlacementDict(
    "Custom Sandwich Lava (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomSandwichLava
    )
)

Ahorn.editingOptions(entity::CustomSandwichLava) = Dict{String, Any}(
    "direction" => directions
)

function Ahorn.selection(entity::CustomSandwichLava)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomSandwichLava, room::Maple.Room)    
    direction = get(entity.data, "direction", "CoreModeBased")

    if direction == "AlwaysUp"
        Ahorn.drawSprite(ctx, "ahorn/SpringCollab2020/lava_sandwich_up", 0, 0)
    elseif direction == "AlwaysDown"
        Ahorn.drawSprite(ctx, "ahorn/SpringCollab2020/lava_sandwich_down", 0, 0)
    else
        Ahorn.drawImage(ctx, Ahorn.Assets.lavaSanwitch, -12, -12)
    end
end

end
