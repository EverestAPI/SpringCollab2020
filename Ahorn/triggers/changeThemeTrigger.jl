module SpringCollab2020ChangeThemeTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/ChangeThemeTrigger" ChangeThemeTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, enable::Bool=true)

const placements = Ahorn.PlacementDict(
    "Change Theme Trigger (Spring Collab 2020)" => Ahorn.EntityPlacement(
        ChangeThemeTrigger,
        "rectangle",
    ),
)

end
