module SpringCollab2020NoRefillField

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/NoRefillField" NoRefillField(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "No Refill Field (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NoRefillField,
        "rectangle",
    ),
)

end