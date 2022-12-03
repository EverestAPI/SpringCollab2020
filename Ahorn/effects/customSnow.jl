module SpringCollab2020CustomSnow

using ..Ahorn, Maple

@mapdef Effect "SpringCollab2020/CustomSnow" CustomSnow(only::String="*", exclude::String="", colors::String="FFFFFF,FFFFFF")

placements = CustomSnow

function Ahorn.canFgBg(effect::CustomSnow)
    return true, true
end

end
