module SpringCollab2020SwapDashTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/SwapDashTrigger" SwapDashTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Swap Dash (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SwapDashTrigger,
        "rectangle"
    )
)

end
