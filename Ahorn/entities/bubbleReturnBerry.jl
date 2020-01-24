module SpringCollab2020BubbleReturnBerry

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/returnBerry" ReturnBerry(x::Integer, y::Integer, order::Integer=-1, checkpointID::Integer=-1, winged::Bool=false, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
	"Strawberry (With Return) (Spring Collab 2020)" => Ahorn.EntityPlacement(
		ReturnBerry
	),
	"Strawberry (Winged, With Return) (Spring Collab 2020)" => Ahorn.EntityPlacement(
		ReturnBerry,
		"point",
		Dict{String,Any}(
			"winged" => true
		)
	)
)

seedSprite = "collectables/strawberry/seed00"

Ahorn.nodeLimits(entity::ReturnBerry) = 0, -1

function getSpriteName(entity::ReturnBerry)
	winged = get(entity.data, "winged", false)

	if winged
		return "collectables/strawberry/wings01"
	end

	return "collectables/strawberry/normal00"
end

function Ahorn.selection(entity::ReturnBerry)
	x, y = Ahorn.position(entity)
	nodes = get(entity.data, "nodes", ())

	res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(getSpriteName(entity), x, y)]

	for node in nodes
		nx, ny = node
		push!(res, Ahorn.getSpriteRectangle(seedSprite, nx, ny))
	end

	return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReturnBerry)
	x, y = Ahorn.position(entity)

	for node in get(entity.data, "nodes", ())
		nx, ny = node

		Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
	end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReturnBerry, room::Maple.Room)
	x, y = Ahorn.position(entity)
	nodes = get(entity.data, "nodes", ())

	for node in nodes
		nx, ny = node

		Ahorn.drawSprite(ctx, seedSprite, nx, ny)
	end

	Ahorn.drawSprite(ctx, getSpriteName(entity), x, y)
end

end
