using System;
using System.Collections.Generic;
using UnityEngine;
public class Deck
{
    private List<Card> cards;

    public Deck(GameObject[] cardPrefabs) {
        cards = new List<Card>();
        foreach (GameObject g in cardPrefabs) {
            cards.Add(new Card(g));
        }
    }
    public Deck(GameObject[] cardPrefabs, int numberOfCardDecks) {
        cards = new List<Card>();
        while(numberOfCardDecks-->0)
        {
            foreach (GameObject g in cardPrefabs) {
                cards.Add(new Card(g));
            }
        }
    }

    public Card DrawRandomCard() {
        int index = UnityEngine.Random.Range(0, cards.Count - 1);
        Card chosen = cards[index];
        cards.RemoveAt(index);
        return chosen;
    }
}