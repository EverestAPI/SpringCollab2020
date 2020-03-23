module SpringCollab2020StrawberryCollectionField

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/StrawberryCollectionField" StrawberryCollectionField(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    delayBetweenBerries::Bool=true, includeGoldens::Bool=false)

const placements = Ahorn.PlacementDict(
    "Strawberry Collection Field (Spring Collab 2020)" => Ahorn.EntityPlacement(
        StrawberryCollectionField,
        "rectangle"
    )
)

end