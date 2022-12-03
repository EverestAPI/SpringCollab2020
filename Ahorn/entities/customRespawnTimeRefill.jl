module SpringCollab2020CustomRespawnTimeRefill

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/CustomRespawnTimeRefill" CustomRespawnTimeRefill(x::Integer, y::Integer, twoDash::Bool=false, respawnTime::Number=2.5)

const placements = Ahorn.PlacementDict(
    "Refill (Custom Respawn Time) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomRespawnTimeRefill
    ),

    "Refill (Two Dashes, Custom Respawn Time) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomRespawnTimeRefill,
        "point",
        Dict{String, Any}(
            "twoDash" => true
        )
    )
)

spriteOneDash = "objects/refill/idle00"
spriteTwoDash = "objects/refillTwo/idle00"

function getSprite(entity::CustomRespawnTimeRefill)
    twoDash = get(entity.data, "twoDash", false)

    return twoDash ? spriteTwoDash : spriteOneDash
end

function Ahorn.selection(entity::CustomRespawnTimeRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomRespawnTimeRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end