local drawableSpriteStruct = require("structs.drawable_sprite")
local utils = require("utils")

local safeRespawnCrumble = {}
safeRespawnCrumble.name = "SafeRespawnCrumble/SafeRespawnCrumble"
safeRespawnCrumble.depth = 0
safeRespawnCrumble.placements = {
    {
        name = "default",
        data = {
            width = 16,
            hasOutline = true,
        }
    }
}

safeRespawnCrumble.minimumSize = { 8, 0 }
safeRespawnCrumble.canResize = { true, false }

function safeRespawnCrumble.sprite(room, entity)
    local texture = "objects/SafeRespawnCrumble/tile"

    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8

    local startX = math.floor(x / 8) + 1
    local stopX = startX + math.floor(width / 8) - 1
    local len = stopX - startX

    local sprites = {}

    for i = 0, len do
        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
        sprite:addPosition(i * 8, 0)

        table.insert(sprites, sprite)
    end

    return sprites
end

function safeRespawnCrumble.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return safeRespawnCrumble
