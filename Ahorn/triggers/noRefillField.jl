module SpringCollab2020NoRefillField

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/NoRefillField" NoRefillField(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "No Refill Field (SpringCollab2020)" => Ahorn.EntityPlacement(
        NoRefillField,
        "rectangle",
    ),
)

end