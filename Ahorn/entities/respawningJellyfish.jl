module SpringCollab2020RespawningJellyfish

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/RespawningJellyfish" RespawningJellyfish(x::Integer, y::Integer, bubble::Bool=false, tutorial::Bool=false, respawnTime::Number=2.0)

const placements = Ahorn.PlacementDict(
    "Respawning Jellyfish (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RespawningJellyfish
    ),
    "Respawning Jellyfish (Floating) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RespawningJellyfish,
        "point",
        Dict{String, Any}(
            "bubble" => true
        )
    )
)

sprite = "objects/glider/idle0"

function Ahorn.selection(entity::RespawningJellyfish)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RespawningJellyfish, room::Maple.Room)
    Ahorn.drawSprite(ctx, sprite, 0, 0)

    if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end