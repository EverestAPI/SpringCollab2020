module SpringCollab2020InstantFallingBlock

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/InstantFallingBlock" InstantFallingBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    tiletype::String="3", climbFall::Bool=true, behind::Bool=false)

const placements = Ahorn.PlacementDict(
    "Instant Falling Block (Spring Collab 2020)" => Ahorn.EntityPlacement(
        InstantFallingBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
)

Ahorn.editingOptions(entity::InstantFallingBlock) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::InstantFallingBlock) = 8, 8
Ahorn.resizable(entity::InstantFallingBlock) = true, true

Ahorn.selection(entity::InstantFallingBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::InstantFallingBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
