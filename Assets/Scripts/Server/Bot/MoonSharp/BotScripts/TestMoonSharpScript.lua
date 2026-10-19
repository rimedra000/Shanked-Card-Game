username = "TestMoonSharpScript"
local cardSortOrder={
    CardValue.Three,
    CardValue.Four,
    CardValue.Five,
    CardValue.Six,
    CardValue.Seven,
    CardValue.Nine,
    CardValue.Jack,
    CardValue.Queen,
    CardValue.King,
    CardValue.Ace,
    CardValue.Two,
    CardValue.Eight,
    CardValue.Ten,
    CardValue.Joker,
}

cardSortOrder_inverse={}
for key,value in pairs(cardSortOrder) do
    cardSortOrder_inverse[value]=key
end

local function card_struct_table_to_string(card_struct_table)
    local result_table={}
    for i,card_struct in ipairs(card_struct_table) do
        result_table[i]=card_struct.ToString()
    end
    
    return "{"..table.concat(result_table,"},{").."}"
end




function pick_from_filtered_main_hand(filteredCards, clientState)
    local sorted_cards = sort_cards(filteredCards,cardSortOrder)
    local result={}
    for c in sorted_cards do
        if c.value==sorted_cards[1].value then
            table.insert(result,c)
        else
            break
        end
    end
    print(#result)
    return result
end

function pick_from_shown_cards(cards, clientState)
    local filteredCards = {}
    for c in cards do
        if clientState.ValidCard(c.value) then
            table.insert(filteredCards,c)
        end
    end
    if filteredCards[1] ~= nil then
        return filteredCards[1]
    end
    return cards[1]
end

function setup_shown_cards(mainHand, shownCards) -- return (toMainHand,toShownCards)
    local sorted_mainHand = sort_cards(mainHand,cardSortOrder)
    local shown_card_canaidate = sorted_mainHand[#sorted_mainHand]
    local sorted_shownCards = sort_cards(shownCards,cardSortOrder)
    local main_hand_canaidate = sorted_shownCards[1]
    if  cardSortOrder_inverse[shown_card_canaidate.value] > cardSortOrder_inverse[main_hand_canaidate.value] then
        return main_hand_canaidate,shown_card_canaidate
    end
    return mainHand[1],shownCards[1] -- ready
end


