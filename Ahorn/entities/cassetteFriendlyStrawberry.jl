module SpringCollab2020CassetteFriendlyStrawberry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/CassetteFriendlyStrawberry" CassetteFriendlyStrawberry(x::Integer, y::Integer, winged::Bool=false, moon::Bool=false,
    checkpointID::Integer=-1, order::Integer=-1, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Cassette-Friendly Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CassetteFriendlyStrawberry
    )
)

# winged, has pips, moon
sprites = Dict{Tuple{Bool, Bool, Bool}, String}(
    (false, false, false) => "collectables/strawberry/normal00",
    (true, false, false) => "collectables/strawberry/wings01",
    (false, true, false) => "collectables/ghostberry/idle00",
    (true, true, false) => "collectables/ghostberry/wings01",

    (false, false, true) => "collectables/moonBerry/normal00",
    (true, false, true) => "collectables/moonBerry/ghost00",
    (false, true, true) => "collectables/moonBerry/ghost00",
    (true, true, true) => "collectables/moonBerry/ghost00"
)

seedSprite = "collectables/strawberry/seed00"

fallback = "collectables/strawberry/normal00"

Ahorn.nodeLimits(entity::CassetteFriendlyStrawberry) = 0, -1

function Ahorn.selection(entity::CassetteFriendlyStrawberry)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())
    moon = get(entity.data, "moon", false)
    winged = get(entity.data, "winged", false) || entity.name == "memorialTextController"
    hasPips = length(nodes) > 0

    sprite = sprites[(winged, hasPips, moon)]

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteFriendlyStrawberry)
    x, y = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = node

        Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CassetteFriendlyStrawberry, room::Maple.Room)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", ())
    moon = get(entity.data, "moon", false)
    winged = get(entity.data, "winged", false) || entity.name == "memorialTextController"
    hasPips = length(nodes) > 0

    sprite = sprites[(winged, hasPips, moon)]

    for node in nodes
        nx, ny = node

        Ahorn.drawSprite(ctx, seedSprite, nx, ny)
    end

    Ahorn.drawSprite(ctx, sprite, x, y)
end

end