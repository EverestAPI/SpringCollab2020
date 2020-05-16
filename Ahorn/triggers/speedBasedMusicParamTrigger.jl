module SpringCollab2020SpeedBasedMusicParamTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/SpeedBasedMusicParamTrigger" SpeedBasedMusicParamTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    paramName::String="fade", minSpeed::Number=0.0, maxSpeed::Number=1.0, activate::Bool=true)

const placements = Ahorn.PlacementDict(
    "Speed-Based Music Param (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SpeedBasedMusicParamTrigger,
        "rectangle"
    )
)

end