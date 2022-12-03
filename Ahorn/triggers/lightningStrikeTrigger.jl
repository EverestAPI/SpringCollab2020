module SpringCollab2020LightningStrikeTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/LightningStrikeTrigger" LightningStrikeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, playerOffset::Number=0.0, verticalOffset::Number=0.0, strikeHeight::Number=0.0, seed::Integer=0, delay::Number=0.0, rain::Bool=true, flash::Bool=true, constant::Bool=false)

const placements = Ahorn.PlacementDict(
    "Lightning Strike (Spring Collab 2020)" => Ahorn.EntityPlacement(
        LightningStrikeTrigger,
        "rectangle",
    ),
)

end