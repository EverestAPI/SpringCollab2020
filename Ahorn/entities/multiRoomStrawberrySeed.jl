module SpringCollab2020MultiRoomStrawberrySeed

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/MultiRoomStrawberrySeed" MultiRoomStrawberrySeed(x::Integer, y::Integer,
    strawberryName::String="multi_room_strawberry", sprite::String="strawberry/seed", ghostSprite::String="ghostberry/seed", index::Int=-1)

const placements = Ahorn.PlacementDict(
    "Multi-Room Strawberry Seed (Spring Collab 2020)" => Ahorn.EntityPlacement(
        MultiRoomStrawberrySeed
    )
)


function Ahorn.selection(entity::MultiRoomStrawberrySeed) 
    x, y = Ahorn.position(entity)
    sprite = "collectables/" * get(entity.data, "sprite", "strawberry/seed") * "00"

    return Ahorn.getSpriteRectangle(sprite, x, y)
end    

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MultiRoomStrawberrySeed, room::Maple.Room)
    sprite = "collectables/" * get(entity.data, "sprite", "strawberry/seed") * "00"
    
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
