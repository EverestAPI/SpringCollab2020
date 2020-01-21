module SpringCollab2020SidewaysJumpThru

using ..Ahorn, Maple

@mapdef Entity "SpringCollab2020/SidewaysJumpThru" SidewaysJumpThru(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight, 
	allowLeftToRight::Bool=true, texture::String="wood")

textures = ["wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"]
const placements = Ahorn.PlacementDict(
    "Sideways Jump Through ($(uppercasefirst(texture))) (Spring Collab 2020)" => Ahorn.EntityPlacement(
        SidewaysJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture
        )
    ) for texture in textures
)

quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 7) (8, 0, 8, 7) (16, 0, 8, 7);
    (0, 8, 8, 5) (8, 8, 8, 5) (16, 8, 8, 5)
]

Ahorn.editingOptions(entity::SidewaysJumpThru) = Dict{String, Any}(
    "texture" => textures
)

Ahorn.minimumSize(entity::SidewaysJumpThru) = 0, 8
Ahorn.resizable(entity::SidewaysJumpThru) = false, true

function Ahorn.selection(entity::SidewaysJumpThru)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, 8, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SidewaysJumpThru, room::Maple.Room)
    texture = get(entity.data, "texture", "wood")
    texture = texture == "default" ? "wood" : texture

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    height = Int(get(entity.data, "height", 8))
	allowLeftToRight = get(entity.data, "allowLeftToRight", true)
	
    startX = div(x, 8) + 1
    startY = div(y, 8) + 1
    stopY = startY + div(height, 8) - 1
    
    Ahorn.Cairo.save(ctx)
    
    Ahorn.rotate(ctx, pi / 2)
	
	if !allowLeftToRight
		Ahorn.scale(ctx, 1, -1)
	end

    len = stopY - startY
    for i in 0:len
        connected = false
        qx = 2
        if i == 0
            connected = get(room.fgTiles.data, (startY - 1, startX), false) != '0'
            qx = 1

        elseif i == len
            connected = get(room.fgTiles.data, (stopY + 1, startX), false) != '0'
            qx = 3
        end

        quad = quads[2 - connected, qx]
        Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)", 8 * i, allowLeftToRight ? -8 : 0, quad...)
    end
    
    Ahorn.Cairo.restore(ctx)
end

end
