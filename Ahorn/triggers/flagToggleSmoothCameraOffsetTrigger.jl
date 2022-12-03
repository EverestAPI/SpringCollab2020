module SpringCollab2020FlagToggleSmoothCameraOffsetTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/FlagToggleSmoothCameraOffsetTrigger" FlagToggleSmoothCameraOffsetTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    offsetXFrom::Number=0.0, offsetXTo::Number=0.0, offsetYFrom::Number=0.0, offsetYTo::Number=0.0, positionMode::String="NoEffect", onlyOnce::Bool=false, flag::String="flag_toggle_smooth_camera_offset", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Flag Toggle Smooth Camera Offset (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleSmoothCameraOffsetTrigger,
        "rectangle",
    ),
)

function Ahorn.editingOptions(trigger::FlagToggleSmoothCameraOffsetTrigger)
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

end
