module SpringCollab2020GlassBlockOriginal

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/GlassBlockOriginal" GlassBlockOriginal(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight)

const placements = Ahorn.PlacementDict(
    "Glass Block (original) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        GlassBlockOriginal,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.minimumSize(entity::GlassBlockOriginal) = 8, 8
Ahorn.resizable(entity::GlassBlockOriginal) = true, true

Ahorn.selection(entity::GlassBlockOriginal) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GlassBlockOriginal, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 1.0, 1.0, 0.5), (1.0, 1.0, 1.0, 0.5))
end

end
