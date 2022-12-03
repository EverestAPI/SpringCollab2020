module SpringCollab2020FlagToggleCameraTargetTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/FlagToggleCameraTargetTrigger" FlagToggleCameraTargetTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    lerpStrength::Number=0.0, positionMode::String="NoEffect", xOnly::Bool=false, yOnly::Bool=false, deleteFlag::String="", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[],
    flag::String="flag_toggle_camera_target", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Flag Toggle Camera Target (Spring Collab 2020)" => Ahorn.EntityPlacement(
        FlagToggleCameraTargetTrigger,
        "rectangle",
        Dict{String, Any}(),
        function(trigger)
            trigger.data["nodes"] = [(Int(trigger.data["x"]) + Int(trigger.data["width"]) + 8, Int(trigger.data["y"]))]
        end
    )
)

function Ahorn.editingOptions(trigger::FlagToggleCameraTargetTrigger)
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

function Ahorn.nodeLimits(trigger::FlagToggleCameraTargetTrigger)
    return 1, 1
end

end