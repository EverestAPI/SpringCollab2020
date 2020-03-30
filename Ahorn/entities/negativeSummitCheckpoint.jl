module SpringCollab2020NegativeSummitCheckpoint

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/NegativeSummitCheckpoint" NegativeSummitCheckpoint(x::Integer, y::Integer, number::Integer=0)

const placements = Ahorn.PlacementDict(
    "Negative Summit Checkpoint (Spring Collab 2020)" => Ahorn.EntityPlacement(
        NegativeSummitCheckpoint
    )
)

baseSprite = "scenery/summitcheckpoints/base02.png"

function Ahorn.selection(entity::NegativeSummitCheckpoint)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(baseSprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NegativeSummitCheckpoint, room::Maple.Room)
    checkpointIndex = get(entity.data, "number", 0)

    if checkpointIndex < 0
        digit1 = "10"
        digit2 = "0$(- checkpointIndex)"
    else
        digit1 = "0$(floor(Int, checkpointIndex % 100 / 10))"
        digit2 = "0$(checkpointIndex % 10)"
    end

    Ahorn.drawSprite(ctx, baseSprite, 0, 0)
    Ahorn.drawSprite(ctx, "scenery/summitcheckpoints/numberbg$digit1.png", -2, 4)
    Ahorn.drawSprite(ctx, "scenery/summitcheckpoints/number$digit1.png", -2, 4)
    Ahorn.drawSprite(ctx, "scenery/summitcheckpoints/numberbg$digit2.png", 2, 4)
    Ahorn.drawSprite(ctx, "scenery/summitcheckpoints/number$digit2.png", 2, 4)
end

end