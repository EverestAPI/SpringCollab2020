module SpringCollab2020PauseBadelineBossesTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/PauseBadelineBossesTrigger" PauseBadelineBossesTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Pause Badeline Bosses (Spring Collab 2020)" => Ahorn.EntityPlacement(
        PauseBadelineBossesTrigger,
        "rectangle",
    ),
)

end
