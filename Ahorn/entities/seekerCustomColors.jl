module SpringCollab2020SeekerCustomColors

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/SeekerCustomColors" SeekerCustomColors(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[],
    attackParticleColor1::String="99e550", attackParticleColor2::String="ddffbc", regenParticleColor1::String="cbdbfc", regenParticleColor2::String="575fd9", trailColor::String="99e550")

const placements = Ahorn.PlacementDict(
    "Seeker (Custom Colors) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SeekerCustomColors,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 32, Int(entity.data["y"]))]
        end
    )
)

Ahorn.nodeLimits(entity::SeekerCustomColors) = 1, -1

function Ahorn.selection(entity::SeekerCustomColors)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

sprite = "characters/monsters/predator73.png"

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::SeekerCustomColors)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerCustomColors, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
