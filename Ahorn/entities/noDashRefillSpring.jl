module SpringCollab2020NoDashRefillSpring

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/NoDashRefillSpring" NoDashRefillSpring(x::Integer, y::Integer, playerCanUse::Bool=true)
@mapdef Entity "SpringCollab2020/NoDashRefillSpringRight" NoDashRefillSpringRight(x::Integer, y::Integer)
@mapdef Entity "SpringCollab2020/NoDashRefillSpringLeft" NoDashRefillSpringLeft(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "No Dash Refill Spring (Up) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NoDashRefillSpring
    ),
    "No Dash Refill Spring (Left) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NoDashRefillSpringRight
    ),
    "No Dash Refill Spring (Right) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NoDashRefillSpringLeft
    ),
)

function Ahorn.selection(entity::NoDashRefillSpring)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
end

function Ahorn.selection(entity::NoDashRefillSpringLeft)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
end

function Ahorn.selection(entity::NoDashRefillSpringRight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
end

sprite = "objects/SpringCollab2020/noDashRefillSpring/00.png"

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpring, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, -8)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpringLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 24, 0, rot=pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpringRight, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, -8, 16, rot=-pi / 2)

end