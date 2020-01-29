module SpringCollab2020CustomSandwichLavaSettingsTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/CustomSandwichLavaSettingsTrigger" CustomSandwichLavaSettingsTrigger(x::Integer, y::Integer, 
    onlyOnce::Bool=false, direction::String="CoreModeBased", speed::Number=20)

const directions = String["AlwaysUp", "AlwaysDown", "CoreModeBased"]

const placements = Ahorn.PlacementDict(
    "Custom Sandwich Lava Settings (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomSandwichLavaSettingsTrigger,
        "rectangle",
    ),
)

Ahorn.editingOptions(entity::CustomSandwichLavaSettingsTrigger) = Dict{String, Any}(
    "direction" => directions
)

end