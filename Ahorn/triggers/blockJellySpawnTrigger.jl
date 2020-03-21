module SpringCollab2020BlockJellySpawnTrigger

using ..Ahorn, Maple

@mapdef Trigger "SpringCollab2020/BlockJellySpawnTrigger" BlockJellySpawnTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Block Jelly Spawn (Spring Collab 2020)" => Ahorn.EntityPlacement(
        BlockJellySpawnTrigger,
        "rectangle"
    )
)

end