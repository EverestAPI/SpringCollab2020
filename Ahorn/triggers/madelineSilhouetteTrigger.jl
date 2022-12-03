module SpringCollab2020MadelineSilhouetteTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/MadelineSilhouetteTrigger" MadelineSilhouetteTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, enable::Bool=true)

const placements = Ahorn.PlacementDict(
    "Madeline Silhouette (Spring Collab 2020)" => Ahorn.EntityPlacement(
        MadelineSilhouetteTrigger,
        "rectangle"
    )
)

end