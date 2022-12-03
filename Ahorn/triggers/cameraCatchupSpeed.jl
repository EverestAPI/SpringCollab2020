module SpringCollab2020CameraCatchupSpeedTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/CameraCatchupSpeedTrigger" CameraCatchupSpeedTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    catchupSpeed::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Camera Catchup Speed (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CameraCatchupSpeedTrigger,
        "rectangle"
    )
)

end
