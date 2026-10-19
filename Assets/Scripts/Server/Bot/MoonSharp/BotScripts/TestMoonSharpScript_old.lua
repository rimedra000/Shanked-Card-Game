username = "TestMoonSharpScript"
cardSortOrder={
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
    CardValue.Joker,
    CardValue.Two,
    CardValue.Eight,
    CardValue.Ten,
}

cardSortOrder_inverse={
    [CardValue.Three] = "1",
    [CardValue.Four] = "2",
    [CardValue.Five] = "3",
    [CardValue.Six] = "4",
    [CardValue.Seven] = "5",
    [CardValue.Nine] = "6",
    [CardValue.Jack] = "7",
    [CardValue.Queen] = "8",
    [CardValue.King] = "9",
    [CardValue.Ace] = "10",
    [CardValue.Joker] = "11",
    [CardValue.Two] = "12",
    [CardValue.Eight] = "13",
    [CardValue.Ten] = "14",
}


function pick_from_filtered_main_hand(filteredCards, clientState)
    local sorted_cards = sort_cards(filteredCards,cardSortOrder)
    return {sorted_cards[1]}
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
    return mainHand[1],shownCards[1]
end

