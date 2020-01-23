module SpringCollab2020GlassBerryModule

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/glassBerry" GlassBerry(x::Integer, y::Integer, checkpointID::Integer=-1, order::Integer=-1, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
	"Glass Strawberry (Spring Collab 2020)" => Ahorn.EntityPlacement(
		GlassBerry,
		"point"
	),
)

Ahorn.nodeLimits(entity::GlassBerry) = 0, -1

function Ahorn.selection(entity::GlassBerry)
	x, y = Ahorn.position(entity)

	nodes = get(entity.data, "nodes", ())
	hasPips = length(nodes) > 0

	sprite = "collectables/SpringCollab2020/glassBerry/idle00"
	seedSprite = "collectables/strawberry/seed00"

	res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]

	for node in nodes
		nx, ny = node

		push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
	end

	return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::GlassBerry)
	x, y = Ahorn.position(entity)

	for node in get(entity.data, "nodes", ())
		nx, ny = node

		Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
	end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::GlassBerry, room::Maple.Room)
	x, y = Ahorn.position(entity)

	nodes = get(entity.data, "nodes", ())
	hasPips = length(nodes) > 0

	sprite = "collectables/SpringCollab2020/glassBerry/idle00"
	seedSprite = "collectables/strawberry/seed00"

	for node in nodes
		nx, ny = node

		Ahorn.drawSprite(ctx, seedSprite, nx, ny)
	end

	Ahorn.drawSprite(ctx, sprite, x, y)
end

end
