module SpringCollab2020RetractSpinner

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/RetractSpinner" RetractSpinner(x::Integer, y::Integer, attachToSolid::Bool=false)

const placements = Ahorn.PlacementDict(
    "Retract Spinner (Spring Collab 2020)" => Ahorn.EntityPlacement(
        RetractSpinner
    )
)

sprite = "danger/SpringCollab2020/retractspinner/urchin_harm00.png"

function Ahorn.selection(entity::RetractSpinner)
    x, y = Ahorn.position(entity)
    return Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RetractSpinner, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end