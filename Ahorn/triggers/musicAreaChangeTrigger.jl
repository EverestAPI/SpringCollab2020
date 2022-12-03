module SpringCollab2020MusicAreaChangeTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/MusicAreaChangeTrigger" MusicAreaChangeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    musicParam1::String="", musicParam2::String="", musicParam3::String="", enterValue::Number=1.0, exitValue::Number=0.0)

const placements = Ahorn.PlacementDict(
    "Music Area Change Trigger (Spring Collab 2020)" => Ahorn.EntityPlacement(
        MusicAreaChangeTrigger,
        "rectangle"
    )
)

end
