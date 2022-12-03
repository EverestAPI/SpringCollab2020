module SpringCollab2020MultiNodeMovingPlatform

using ..Ahorn, Maple

@pardef MultiNodeMovingPlatform(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, mode::String="Loop", texture::String="default", moveTime::Number=2.0, pauseTime::Number=0.0, easing::Bool=true) =
    Entity("SpringCollab2020/MultiNodeMovingPlatform", x=x, y=y, nodes=Tuple{Int, Int}[], width=width, mode=mode, texture=texture, moveTime=moveTime, pauseTime=pauseTime, easing=easing)

const placements = Ahorn.PlacementDict()

const modes = ["Loop", "LoopNoPause", "BackAndForth", "BackAndForthNoPause", "TeleportBack"]

for texture in Maple.wood_platform_textures
    placements["Platform (Moving, Multi-Node, $(uppercasefirst(texture))) (Spring Collab 2020)"] = Ahorn.EntityPlacement(
        MultiNodeMovingPlatform,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            width = Int(get(entity.data, "width", 8))
            entity.data["x"], entity.data["y"] = x + width, y
            entity.data["nodes"] = [(x, y)]
        end
    )
end

Ahorn.editingOptions(entity::MultiNodeMovingPlatform) = Dict{String, Any}(
    "texture" => Maple.wood_platform_textures,
    "mode" => modes
)

Ahorn.nodeLimits(entity::MultiNodeMovingPlatform) = 1, -1

Ahorn.resizable(entity::MultiNodeMovingPlatform) = true, false

Ahorn.minimumSize(entity::MultiNodeMovingPlatform) = 8, 0

function Ahorn.selection(entity::MultiNodeMovingPlatform)
    width = Int(get(entity.data, "width", 8))

    nodes = get(entity.data, "nodes", ())
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    rectangles = Ahorn.Rectangle[Ahorn.Rectangle(startX, startY, width, 8)]

    for node in nodes
        nodeX, nodeY = Int.(node)
        push!(rectangles, Ahorn.Rectangle(nodeX, nodeY, width, 8))
    end

    return rectangles
end

outerColor = (30, 14, 25) ./ 255
innerColor = (10, 0, 6) ./ 255

function renderConnection(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, nx::Number, ny::Number, width::Number)
    cx, cy = x + floor(Int, width / 2), y + 4
    cnx, cny = nx + floor(Int, width / 2), ny + 4

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, outerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 3);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.setSourceColor(ctx, innerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)
end

function renderPlatform(ctx::Ahorn.Cairo.CairoContext, texture::String, x::Number, y::Number, width::Number)
    tilesWidth = div(width, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + 8 * (i - 1), y, 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + tilesWidth * 8 - 8, y, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", x + floor(Int, width / 2) - 4, y, 16, 0, 8, 8)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeMovingPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))
    mode = get(entity.data, "mode", "Loop")

    firstNodeX, firstNodeY = Int(entity.data["x"]), Int(entity.data["y"])
    previousNodeX, previousNodeY = firstNodeX, firstNodeY

    texture = get(entity.data, "texture", "default")

    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nodeX, nodeY = Int.(node)
        renderConnection(ctx, previousNodeX, previousNodeY, nodeX, nodeY, width)
        previousNodeX, previousNodeY = nodeX, nodeY
    end

    if mode == "Loop" || mode == "LoopNoPause"
        renderConnection(ctx, previousNodeX, previousNodeY, firstNodeX, firstNodeY, width)
    end

    renderPlatform(ctx, texture, firstNodeX, firstNodeY, width)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeMovingPlatform, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))
    mode = get(entity.data, "mode", "Loop")

    firstNodeX, firstNodeY = Int(entity.data["x"]), Int(entity.data["y"])
    previousNodeX, previousNodeY = firstNodeX, firstNodeY

    texture = get(entity.data, "texture", "default")

    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nodeX, nodeY = Int.(node)
        renderPlatform(ctx, texture, nodeX, nodeY, width)
        Ahorn.drawArrow(ctx, previousNodeX + width / 2, previousNodeY, nodeX + width / 2, nodeY, Ahorn.colors.selection_selected_fc, headLength=6)
        previousNodeX, previousNodeY = nodeX, nodeY
    end

    if mode == "Loop" || mode == "LoopNoPause"
        Ahorn.drawArrow(ctx, previousNodeX + width / 2, previousNodeY, firstNodeX + width / 2, firstNodeY, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
