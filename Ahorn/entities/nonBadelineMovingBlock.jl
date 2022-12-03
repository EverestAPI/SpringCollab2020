module SpringCollab2020NonBadelineMovingBlock

using ..Ahorn, Maple

@pardef NonBadelineMovingBlock(x1::Integer, y1::Integer, x2::Integer=x1+16, y2::Integer=y1, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, tiletype::String="g", highlightTiletype::String="G") = Entity("SpringCollab2020/nonBadelineMovingBlock", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, tiletype=tiletype, highlightTiletype=highlightTiletype)

const placements = Ahorn.PlacementDict(
    "Non-Badeline Boss Moving Block (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NonBadelineMovingBlock,
        "rectangle",
        Dict{String, Any}(),
        function(entity)
            entity.data["nodes"] = [(Int(entity.data["x"]) + Int(entity.data["width"]) + 8, Int(entity.data["y"]))]
        end
    )
)

Ahorn.editingOptions(entity::NonBadelineMovingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions(),
    "highlightTiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.nodeLimits(entity::NonBadelineMovingBlock) = 1, 1
Ahorn.minimumSize(entity::NonBadelineMovingBlock) = 8, 8
Ahorn.resizable(entity::NonBadelineMovingBlock) = true, true

function Ahorn.selection(entity::NonBadelineMovingBlock)
    if entity.name == "SpringCollab2020/nonBadelineMovingBlock"
        x, y = Ahorn.position(entity)
        nx, ny = Int.(entity.data["nodes"][1])

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::NonBadelineMovingBlock, room::Maple.Room)
    Ahorn.drawTileEntity(ctx, room, entity, material=get(entity.data, "tiletype", "g")[1], blendIn=false)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::NonBadelineMovingBlock, room::Maple.Room)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    if !isempty(nodes)
        nx, ny = Int.(nodes[1])
        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        # Use 'G' instead of 'g', as that is the highlight color of the block (the active color)
        fakeTiles = Ahorn.createFakeTiles(room, nx, ny, width, height, get(entity.data, "highlightTiletype", "G")[1], blendIn=false)
        Ahorn.drawFakeTiles(ctx, room, fakeTiles, room.objTiles, true, nx, ny, clipEdges=true)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end