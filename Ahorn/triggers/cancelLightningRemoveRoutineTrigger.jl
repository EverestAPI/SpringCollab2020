module SpringCollab2020CancelLightningRemoveRoutineTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/CancelLightningRemoveRoutineTrigger" CancelLightningRemoveRoutineTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Cancel Lightning Remove Routine Trigger (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CancelLightningRemoveRoutineTrigger,
        "rectangle",
    ),
)

end
