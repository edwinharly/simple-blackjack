using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private GameObject[] cardPrefabs, playerCardPosition, dealerCardPosition;
	[SerializeField]
	private GameObject backCardPrefab;
	[SerializeField]
	private Button primaryBtn, secondaryBtn, resetBalanceBtn;
	[SerializeField]
	private Slider betSlider;
	[SerializeField]
	private Text textMoney, textBet, textPlayerPoints, textDealerPoints, textPlaceYourBet, textSelectingBet, textWinner;
	[SerializeField]
	private Image resetImgBtn;

	private List<Card> playerCards;
	private List<Card> dealerCards;
	private bool isPlaying;
	private int playerPoints;
	private int actualDealerPoints, displayDealerPoints;
	private int playerMoney;
	private int currentBet;
	private int playerCardPointer, dealerCardPointer;

	private Deck playingDeck;
	
	private void Start() {
		playerMoney = 1000;
		currentBet = 50;
		resetGame();

		primaryBtn.onClick.AddListener(delegate {
			if (isPlaying) {
				playerDrawCard();
			} else {
				startGame();
			}
		});

		secondaryBtn.onClick.AddListener(delegate {
			playerEndTurn();
		});

		betSlider.onValueChanged.AddListener(delegate {
			updateCurrentBet();
		});
		
		resetBalanceBtn.onClick.AddListener(delegate {
			playerMoney = 1000;
			betSlider.maxValue = playerMoney;
		});
	}
	
	private void Update() {
		textMoney.text = "$" + playerMoney.ToString();
	}

	public void startGame() {
		if (playerMoney > 0)
		{
			playerMoney -= currentBet;
			if (playerMoney < 0) {
				playerMoney += currentBet;
				betSlider.maxValue = playerMoney;
				return;
			}

			isPlaying = true;

			// Update UI accordingly
			betSlider.gameObject.SetActive(false);
			textSelectingBet.gameObject.SetActive(false);
			textPlaceYourBet.gameObject.SetActive(false);
			primaryBtn.GetComponentInChildren<Text>().text = "HIT";
			secondaryBtn.gameObject.SetActive(true);
			textBet.text = "Bet: $" + currentBet.ToString();
			resetBalanceBtn.gameObject.SetActive(false);

			// assign the playing deck with 2 deck of cards
			playingDeck = new Deck(cardPrefabs, 2);
			// draw 2 cards for player
			playerDrawCard();
			playerDrawCard();
			updatePlayerPoints();
			// draw 2 cards for dealer
			dealerDrawCard();
			dealerDrawCard();
			updateDealerPoints(true);

			checkIfPlayerBlackjack();
		}
	}

	private void checkIfPlayerBlackjack()
	{
		if (playerPoints == 21)
		{
			playerBlackjack();
		}
	}

	public void endGame() {
		primaryBtn.gameObject.SetActive(false);
		secondaryBtn.gameObject.SetActive(false);
		betSlider.gameObject.SetActive(false);
		textPlaceYourBet.text = "";
		textSelectingBet.text = "";

		resetImgBtn.gameObject.SetActive(true);
		resetImgBtn.GetComponent<Button>().onClick.AddListener(delegate {
			resetGame();
		});
	}

	public void dealerDrawCard() {
		Card drawnCard = playingDeck.DrawRandomCard();
		GameObject prefab;
		dealerCards.Add(drawnCard);
		if (dealerCardPointer <= 0) {
			prefab = backCardPrefab;
		} else {
			prefab = drawnCard.Prefab;
		}
		Instantiate(prefab, dealerCardPosition[dealerCardPointer++].transform);
		updateDealerPoints(false);
	}

	public void playerDrawCard() {
		Card drawnCard = playingDeck.DrawRandomCard();
		playerCards.Add(drawnCard);
		Instantiate(drawnCard.Prefab, playerCardPosition[playerCardPointer++].transform);
		updatePlayerPoints();
		if (playerPoints > 21)
			playerBusted();
	}

	private void playerEndTurn() {
		revealDealersDownFacingCard();
		// dealer start drawing
		while (actualDealerPoints < 17 && actualDealerPoints < playerPoints) {
			dealerDrawCard();
		}
		updateDealerPoints(false);
		if (actualDealerPoints > 21)
			dealerBusted();
		else if (actualDealerPoints > playerPoints)
			dealerWin(false);
		else if (actualDealerPoints == playerPoints)
			gameDraw();
		else
			playerWin(false);
	}

	private void revealDealersDownFacingCard() {
		// reveal the dealer's down-facing card
		Destroy(dealerCardPosition[0].transform.GetChild(0).gameObject);
		Instantiate(dealerCards[0].Prefab, dealerCardPosition[0].transform);
	}

	private void updatePlayerPoints() {
		playerPoints = 0;
		foreach(Card c in playerCards) {
			playerPoints += c.Point;
		}

		// transform ace to 1 if there is any
		if (playerPoints > 21)
		{
			playerPoints = 0;
			foreach(Card c in playerCards) {
				if (c.Point == 11)
					playerPoints += 1;
				else
					playerPoints += c.Point;
			}
		}

		textPlayerPoints.text = playerPoints.ToString();
	}

	private void updateDealerPoints(bool hideFirstCard) {
		actualDealerPoints = 0;
		foreach(Card c in dealerCards) {
			actualDealerPoints += c.Point;
		}

		// transform ace to 1 if there is any
		if (actualDealerPoints > 21)
		{
			actualDealerPoints = 0;
			foreach(Card c in dealerCards) {
				if (c.Point == 11)
					actualDealerPoints += 1;
				else
					actualDealerPoints += c.Point;
			}
		}

		if (hideFirstCard)
			displayDealerPoints = dealerCards[1].Point;
		else
			displayDealerPoints = actualDealerPoints;
		textDealerPoints.text = displayDealerPoints.ToString();
	}

	private void updateCurrentBet() {
		currentBet = (int) betSlider.value;
		textSelectingBet.text = "$" + currentBet.ToString();
	}

	private void playerBusted() {
		dealerWin(true);
	}

	private void dealerBusted() {
		playerWin(true);
	}

	private void playerBlackjack() {
		textWinner.text = "Blackjack !!!";
		playerMoney += currentBet * 2;
		endGame();
	}

	private void playerWin(bool winByBust) {
		if (winByBust)
			textWinner.text = "Dealer Busted\nPlayer Win !!!";
		else
			textWinner.text = "Player Win !!!";
		playerMoney += currentBet * 2;
		endGame();
	}

	private void dealerWin(bool winByBust) {
		if (winByBust)
			textWinner.text = "Player Busted\nDealer Win !!!";
		else
			textWinner.text = "Dealer Win !!!";
		endGame();
	}

	private void gameDraw() {
		textWinner.text = "Draw";
		playerMoney += currentBet;
		endGame();
	}

	private void resetGame() {
		isPlaying = false;
		
		// reset points
		playerPoints = 0;
		actualDealerPoints = 0;
		playerCardPointer = 0;
		dealerCardPointer = 0;

		// reset cards
		playingDeck = new Deck(cardPrefabs, 2);
		playerCards = new List<Card>();
		dealerCards = new List<Card>();

		// reset UI
		primaryBtn.gameObject.SetActive(true);
		primaryBtn.GetComponentInChildren<Text>().text = "DEAL";
		secondaryBtn.gameObject.SetActive(false);
		betSlider.gameObject.SetActive(true);
		betSlider.maxValue = playerMoney;
		textSelectingBet.gameObject.SetActive(true);
		textSelectingBet.text = "$" + currentBet.ToString();
		textPlaceYourBet.gameObject.SetActive(true);
		textPlayerPoints.text = "";
		textDealerPoints.text = "";
		textBet.text = "";
		textWinner.text = "";
		resetImgBtn.gameObject.SetActive(false);
		resetBalanceBtn.gameObject.SetActive(true);

		// clear cards on table
		clearCards();
	}

	private void clearCards() {
		foreach(GameObject g in playerCardPosition)
		{
			if (g.transform.childCount > 0)
				for (int i = 0; i < g.transform.childCount; i++)
				{
					Destroy(g.transform.GetChild(i).gameObject);
				}
		}
		foreach(GameObject g in dealerCardPosition)
		{
			if (g.transform.childCount > 0)
				for (int i = 0; i < g.transform.childCount; i++)
				{
					Destroy(g.transform.GetChild(i).gameObject);
				}
		}
	}

}
