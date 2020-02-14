module SpringCollab2020CustomBirdTutorial

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/CustomBirdTutorial" CustomBirdTutorial(x::Integer, y::Integer,
    birdId::String="birdId", onlyOnce::Bool=false, caw::Bool=true, faceLeft::Bool=true, info::String="tutorial_dreamjump", controls::String="DownRight,+,Dash,tinyarrow,Jump")

const placements = Ahorn.PlacementDict(
    "Custom Bird Tutorial (Spring Collab 2020)" => Ahorn.EntityPlacement(
        CustomBirdTutorial
    )
)

sprite = "characters/bird/crow00"

function Ahorn.selection(entity::CustomBirdTutorial)
    x, y = Ahorn.position(entity)
    scaleX = get(entity.data, "faceLeft", true) ? -1 : 1

    return Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, jx=0.5, jy=1.0)]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomBirdTutorial, room::Maple.Room)
    scaleX = get(entity.data, "faceLeft", true) ? -1 : 1
    
    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, jx=0.5, jy=1.0)
end

end
