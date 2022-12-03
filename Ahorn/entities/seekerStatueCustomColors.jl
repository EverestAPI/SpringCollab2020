module SpringCollab2020SeekerStatueCustomColors

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/SeekerStatueCustomColors" SeekerStatueCustomColors(x::Integer, y::Integer, hatch::String="Distance", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[],
    breakOutParticleColor1::String="643e73", breakOutParticleColor2::String="3e2854", attackParticleColor1::String="99e550", attackParticleColor2::String="ddffbc",
    regenParticleColor1::String="cbdbfc", regenParticleColor2::String="575fd9", trailColor::String="99e550")

const placements = Ahorn.PlacementDict(
    "Seeker Statue (Custom Colors) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SeekerStatueCustomColors,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 32, Int(entity.data["y"]))]
        end
    )
)

Ahorn.nodeLimits(entity::SeekerStatueCustomColors) = 1, -1

Ahorn.editingOptions(entity::SeekerStatueCustomColors) = Dict{String, Any}(
    "hatch" => Maple.seeker_statue_hatches
)

function Ahorn.selection(entity::SeekerStatueCustomColors)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(statueSprite, x, y)]

    for node in nodes
        nx, ny = node

        push!(res, Ahorn.getSpriteRectangle(monsterSprite, nx, ny))
    end

    return res
end

statueSprite = "decals/5-temple/statue_e.png"
monsterSprite = "characters/monsters/predator73.png"

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::SeekerStatueCustomColors)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, monsterSprite, nx, ny)

        px, py = nx, ny
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerStatueCustomColors, room::Maple.Room) = Ahorn.drawSprite(ctx, statueSprite, 0, 0)

end
