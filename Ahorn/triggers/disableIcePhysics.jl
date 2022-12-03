module SpringCollab2020DisableIcePhysicsTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/DisableIcePhysicsTrigger" DisableIcePhysicsTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, disableIcePhysics::Bool=true)

const placements = Ahorn.PlacementDict(
    "Disable Ice Physics (Spring Collab 2020)" => Ahorn.EntityPlacement(
        DisableIcePhysicsTrigger,
        "rectangle"
    )
)

end