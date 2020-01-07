module SpringCollab2020DiagonalWingedStrawberry
using ..Ahorn, Maple
@mapdef Entity "SpringCollab2020/diagonalWingedStrawberry" DiagonalWingedStrawberry(x::Integer, y::Integer)

sprite = "collectables/strawberry/wings01"

const placements = Ahorn.PlacementDict(
	"Diagonal Winged Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
		DiagonalWingedStrawberry
	)
)

function Ahorn.selection(entity::DiagonalWingedStrawberry)
	x, y = Ahorn.position(entity)

	return Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DiagonalWingedStrawberry, room::Maple.Room)
	x, y = Ahorn.position(entity)

	Ahorn.drawImage(ctx, sprite, x, y)
end

end