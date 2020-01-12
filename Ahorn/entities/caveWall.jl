module SpringCollab2020CaveWall

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/caveWall" CaveWall(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, tiletype::String="3")

const placements = Ahorn.PlacementDict(
    "Cave Wall (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CaveWall,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
)

Ahorn.editingOptions(entity::CaveWall) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::CaveWall) = 8, 8
Ahorn.resizable(entity::CaveWall) = true, true

Ahorn.selection(entity::CaveWall) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CaveWall, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
