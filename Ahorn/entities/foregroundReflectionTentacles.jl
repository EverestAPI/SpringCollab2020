module SpringCollab2020ForegroundReflectionTentacles

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/ForegroundReflectionTentacles" ForegroundReflectionTentacles(x::Integer, y::Integer,
    nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], fear_distance::String="", slide_until::Integer=0)

const placements = Ahorn.PlacementDict(
    "Foreground Tentacles (Spring Collab 2020)" => Ahorn.EntityPlacement(
        ForegroundReflectionTentacles,
        "point",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + 32, Int(entity.data["y"]))]
        end
    )
)

# Maple definition doesn't have "Fake keys"
const fearDistance = Dict{String, String}(
    "None" => "",
    "Close" => "close",
    "Medium" => "medium",
    "Far" => "far"
)

Ahorn.nodeLimits(entity::ForegroundReflectionTentacles) = 1, -1

Ahorn.editingOptions(entity::ForegroundReflectionTentacles) = Dict{String, Any}(
    "fear_distance" => fearDistance
)

function Ahorn.selection(entity::ForegroundReflectionTentacles)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.Rectangle(x - 12, y - 12, 24, 24)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx - 12, ny - 12, 24, 24))
    end

    return res
end

function drawTentacleIcon(ctx::Ahorn.Cairo.CairoContext, x, y)
    Ahorn.drawImage(ctx, Ahorn.Assets.tentacle, x - 12, y - 12)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ForegroundReflectionTentacles)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        drawTentacleIcon(ctx, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ForegroundReflectionTentacles, room::Maple.Room)
    x, y = Ahorn.position(entity)
    drawTentacleIcon(ctx, x, y)
end

end
