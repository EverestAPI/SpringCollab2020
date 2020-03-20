module SpringCollab2020AnimatedJumpthruPlatform

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/AnimatedJumpthruPlatform" AnimatedJumpthruPlatform(x::Integer, y::Integer, width::Integer=8,
    animationPath::String="SpringCollab2020Example/rainbowwood", animationDelay::Number=0.1, surfaceIndex::Int=-1)

const placements = Ahorn.PlacementDict(
    "Jump Through (Animated) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        AnimatedJumpthruPlatform,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::AnimatedJumpthruPlatform) = Dict{String, Any}(
    "surfaceIndex" => Maple.tileset_sound_ids
)

Ahorn.minimumSize(entity::AnimatedJumpthruPlatform) = 8, 0
Ahorn.resizable(entity::AnimatedJumpthruPlatform) = true, false

function Ahorn.selection(entity::AnimatedJumpthruPlatform)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AnimatedJumpthruPlatform, room::Maple.Room)
    texture = get(entity.data, "animationPath", "wood")

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    
    len = stopX - startX
    for i in 0:len
        Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)00", 8 * i, 0)
    end
end

end