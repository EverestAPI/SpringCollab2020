module SpringCollab2020LeaveTheoBehindTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/LeaveTheoBehindTrigger" LeaveTheoBehindTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    enable::Bool=false)

const placements = Ahorn.PlacementDict(
    "Leave Theo Behind (Spring Collab 2020)" => Ahorn.EntityPlacement(
        LeaveTheoBehindTrigger,
        "rectangle",
    ),
)

end
