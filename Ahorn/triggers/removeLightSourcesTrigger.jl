module SpringCollab2020RemoveLightSources

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/RemoveLightSourcesTrigger" RemoveLightSourcesTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Remove Light Sources (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RemoveLightSourcesTrigger,
        "rectangle",
    ),
)

end