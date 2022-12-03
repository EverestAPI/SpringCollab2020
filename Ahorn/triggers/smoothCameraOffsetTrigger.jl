module SpringCollab2020SmoothCameraOffsetTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/SmoothCameraOffsetTrigger" SmoothCameraOffsetTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    offsetXFrom::Number=0.0, offsetXTo::Number=0.0, offsetYFrom::Number=0.0, offsetYTo::Number=0.0, positionMode::String="NoEffect", onlyOnce::Bool=false)

const placements = Ahorn.PlacementDict(
    "Smooth Camera Offset Trigger (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SmoothCameraOffsetTrigger,
        "rectangle",
    ),
)

function Ahorn.editingOptions(trigger::SmoothCameraOffsetTrigger)
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

end