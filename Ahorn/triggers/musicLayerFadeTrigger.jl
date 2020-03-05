module SpringCollab2020MusicLayerFadeTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/MusicLayerFadeTrigger" MusicLayerFadeTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    layers::String="", fadeA::Number=0.0, fadeB::Number=1.0, direction::String="LeftToRight")

const placements = Ahorn.PlacementDict(
    "Music Layer Fade (Spring Collab 2020)" => Ahorn.EntityPlacement(
        MusicLayerFadeTrigger,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::MusicLayerFadeTrigger) = Dict{String, Any}(
    "direction" => Maple.trigger_position_modes
)

end