module SpringCollab2020HorizontalRoomWrapController

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/HorizontalRoomWrapController" HorizontalRoomWrapController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Horizontal Room Wrap Controller (Spring Collab 2020)" => Ahorn.EntityPlacement(
        HorizontalRoomWrapController
    )
)

function Ahorn.selection(entity::HorizontalRoomWrapController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HorizontalRoomWrapController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/SpringCollab2020/horizontal_room_wrap", 0, 0)

end
