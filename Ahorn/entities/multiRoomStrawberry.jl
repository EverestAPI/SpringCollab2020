module SpringCollab2020MultiRoomStrawberry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/MultiRoomStrawberry" MultiRoomStrawberry(x::Integer, y::Integer,
    name::String="multi_room_strawberry", winged::Bool=false, moon::Bool=false, checkpointID::Integer=-1, order::Integer=-1, SpringCollab2020_ignoreLighting::Bool=false)

const placements = Ahorn.PlacementDict(
    "Multi-Room Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
        MultiRoomStrawberry
    )
)

# winged, moon
sprites = Dict{Tuple{Bool, Bool}, String}(
    (false, false) => "collectables/strawberry/normal00",
    (true, false) => "collectables/strawberry/wings01",
    (false, true) => "collectables/moonBerry/normal00",
    (true, true) => "collectables/moonBerry/normal00"
)

function Ahorn.selection(entity::MultiRoomStrawberry)
    x, y = Ahorn.position(entity)

    moon = get(entity.data, "moon", false)
    winged = get(entity.data, "winged", false)
    sprite = sprites[(winged, moon)]

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiRoomStrawberry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    moon = get(entity.data, "moon", false)
    winged = get(entity.data, "winged", false)
    sprite = sprites[(winged, moon)]

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end
