text_trigger = {}

text_trigger.name = "DisplayMessageCommand/TextDisplayTrigger"
text_trigger.depth = 8998
text_trigger.placements = {
    name = "normal",
    data = {
        textID = "0",
        message = "hi",
        scale = 1.0,
        yPosition = 500.0,
        isLeft = false,
        duration = 0.0,
        onlyOnce = true,
        fillColor = "FFFFFF",
        useRawDeltaTime = false
    }
}

text_trigger.fieldInformation = {
    fillColor = {
        fieldType = "color"
    }
}

return text_trigger