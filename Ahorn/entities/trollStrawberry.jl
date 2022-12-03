module SpringCollab2020TrollStrawberry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/trollStrawberry" TrollStrawberry(x::Integer, y::Integer, winged::Bool=false)

const placements = Ahorn.PlacementDict(
    "Troll Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
        TrollStrawberry
    ),

    "Troll Strawberry (Winged) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        TrollStrawberry,
        "point",
        Dict{String, Any}(
            "winged" => true
        )
    ),
)

# name, winged
sprites = Dict{Tuple{String, Bool}, String}(
    ("SpringCollab2020/trollStrawberry", false) => "collectables/strawberry/normal00",
    ("SpringCollab2020/trollStrawberry", true) => "collectables/strawberry/wings01",
)

fallback = "collectables/strawberry/normal00"

Ahorn.nodeLimits(entity::TrollStrawberry) = 0, -1

function Ahorn.selection(entity::TrollStrawberry)
    x, y = Ahorn.position(entity)

    winged = get(entity.data, "winged", false)

    sprite = sprites[(entity.name, winged)]

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollStrawberry)

end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TrollStrawberry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    winged = get(entity.data, "winged", false)

    sprite = sprites[(entity.name, winged)]

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end