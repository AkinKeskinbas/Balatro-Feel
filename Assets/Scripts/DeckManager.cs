using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private readonly List<CardData> drawPile = new();

    public int RemainingCards => drawPile.Count;

    private void Awake()
    {
        BuildStandardDeck();
        Shuffle();
    }

    public void ResetDeck()
    {
        BuildStandardDeck();
        Shuffle();
    }

    private void BuildStandardDeck()
    {
        drawPile.Clear();

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                drawPile.Add(new CardData(rank, suit));
            }
        }
    }

    public void Shuffle()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex = Random.Range(i, drawPile.Count);
            (drawPile[i], drawPile[randomIndex]) = (drawPile[randomIndex], drawPile[i]);
        }
    }

    public bool CanDraw(int amount)
    {
        return drawPile.Count >= amount;
    }

    public CardData DrawOne()
    {
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("Deck is empty.");
            return null;
        }

        CardData topCard = drawPile[0];
        drawPile.RemoveAt(0);
        return topCard;
    }

    public List<CardData> DrawMany(int amount)
    {
        List<CardData> result = new();

        for (int i = 0; i < amount; i++)
        {
            CardData drawn = DrawOne();
            if (drawn == null)
                break;

            result.Add(drawn);
        }

        return result;
    }
}