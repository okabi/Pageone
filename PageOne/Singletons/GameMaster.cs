using System;
using System.Collections.Generic;
using System.Linq;
using PageOne.Models;
using static PageOne.Models.Card;
using static PageOne.Models.Event;
using static PageOne.Singletons.EffectManager;

namespace PageOne.Singletons
{
    /// <summary>
    /// ゲームの進行を管理するシングルトンです。
    /// </summary>
    public sealed class GameMaster
    {
        #region シングルトンパターン

        private static GameMaster instance = new GameMaster();

        public static GameMaster Instance
        {
            get { return instance; }
        }

        private GameMaster() { }

        #endregion

        #region フィールド

        /// <summary>現在のターンプレイヤーを示すインデックス。</summary>
        private int turnPlayerIndex;

        /// <summary>プレイヤー。</summary>
        private List<Player> players;

        /// <summary>手札。</summary>
        private List<Hand> hands;

        /// <summary>山札。</summary>
        private Stack<Card> deck;

        /// <summary>捨て札。</summary>
        private Stack<Card> grave;

        #endregion

        #region プロパティ

        /// <summary>捨て札の一番上のカード。</summary>
        public Card TopOfGrave
        {
            get
            {
                return new Card(grave.Peek());
            }
        }

        /// <summary>ゲーム状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                return $"山札: {deck.Count} 枚\n" +
                    $"捨て札の一番上は {TopOfGrave} です。\n\n" +
                    $"{string.Join("\n", players.Select(x => x.Status))}\n\n" +
                    $"{players[turnPlayerIndex].Name} のターン\n" +
                    $"{EffectManager.Instance.Status}\n";
            }
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// ゲームを初期化して開始します。
        /// </summary>
        /// <param name="names">参加プレイヤーのリスト。</param>
        public void Run(List<Player> players)
        {
            // 変数の初期化
            this.players = players;
            turnPlayerIndex = 0;
            
            // デッキの初期化
            deck = new Stack<Card>();
            foreach (SuitType suit in Enum.GetValues(typeof(SuitType)))
            {
                if (suit == SuitType.Joker)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        deck.Push(new Card(suit, 14));
                    }
                }
                else
                {
                    for (int i = 1; i <= 13; i++)
                    {
                        deck.Push(new Card(suit, i));
                    }
                }
            }
            Shuffle();

            // 手札の初期化
            hands = new List<Hand>();
            for (int i = 0; i < players.Count; i++)
            {
                hands.Add(new Hand());
                players[i].SetHandReference(hands[i]);
                for (int j = 0; j < 5; j++)
                {
                    hands[i].AddCard(Draw());
                }
            }

            // 履歴の初期化
            HistoryManager.Instance.Init();

            // 捨て札の初期化
            grave = new Stack<Card>();
            Discard();

            // ゲームループ
            while (true)
            {
                Console.Clear();
                Next();
            }
        }

        /// <summary>
        /// 指定したカードを捨て札にできるかを判定します。
        /// </summary>
        /// <param name="card">捨て札にしたいカード。</param>
        /// <returns>捨て札にできるか。</returns>
        public bool Validate(Card card)
        {
            // 最初の捨て札の場合はOK
            if (grave.Count == 0) return true;

            // 宣言スートがジョーカー(最初の捨て札がジョーカー)ならOK
            if (TopOfGrave.declaredSuit == SuitType.Joker) return true;

            // ジョーカーを出すならOK
            if (card.Suit == SuitType.Joker) return true;

            // ロックされているときにJ、Q、K以外ならNG
            if (EffectManager.Instance.Type == EffectType.Lock && card.Number <= 10) return false;

            // スートが一致していればOK
            if (card.Suit == TopOfGrave.declaredSuit) return true;

            // ジョーカーが出されているならNG
            if (TopOfGrave.Suit == SuitType.Joker) return false;

            // 数字が一致していればOK
            if (TopOfGrave.Number == card.Number) return true;

            // いずれにも当てはまらない場合はNG
            return false;
        }

        #endregion

        #region ゲームロジック(private メソッド)

        /// <summary>
        /// 1ターン進めます。
        /// </summary>
        private void Next()
        {
            // ターン前処理
            BeforeTurnAction();

            // ターンアクション処理
            TurnAction(HistoryManager.Instance.TurnDiscard);

            // ターン後処理
            AfterTurnAction(HistoryManager.Instance.TurnDiscard);

            // 履歴処理
            HistoryManager.Instance.Next(turnPlayerIndex);

            // ターン進行処理
            if (EffectManager.Instance.Type != EffectType.Quick)
            {
                if (EffectManager.Instance.Reversing)
                {
                    turnPlayerIndex = (turnPlayerIndex + players.Count - 1) % players.Count;
                }
                else
                {
                    turnPlayerIndex = (turnPlayerIndex + 1) % players.Count;
                }
            }
        }

        /// <summary>
        /// プレイヤーのターン前処理です。
        /// </summary>
        private void BeforeTurnAction()
        {
            // ステータス表示
            Console.Clear();
            Console.WriteLine(Status);

            // カード効果に対するプレイヤーのアクションを取得
            int index = -1;
            switch (EffectManager.Instance.Type)
            {
                case EffectType.Skip:
                    index = players[turnPlayerIndex].EffectSkipAction(EffectManager.Instance.DrawNum);
                    break;
                case EffectType.Draw:
                    index = players[turnPlayerIndex].EffectDrawAction(EffectManager.Instance.DrawNum);
                    break;
                case EffectType.QueenDraw:
                    index = players[turnPlayerIndex].EffectQueenDrawAction(EffectManager.Instance.DrawNum);
                    break;
                case EffectType.Disclose:
                    index = players[turnPlayerIndex].EffectDiscloseActionDiscard();
                    break;
            }

            // ゲーム状態の更新
            if (index >= 0)
            {
                Discard(turnPlayerIndex, index);
            }
        }

        /// <summary>
        /// プレイヤーのターン処理です。
        /// </summary>
        /// <param name="card">ターン前処理でプレイヤーが出したカード。出していないなら null。</param>
        private void TurnAction(Card card)
        {
            // ステータス表示
            Console.Clear();
            Console.WriteLine(HistoryManager.Instance);
            Console.WriteLine(Status);

            // カード効果を飛ばせるか確認
            var skippable = card == null ? false : EffectManager.Instance.Avoidable(card);

            // 次プレイヤーに回せないカード効果を受ける
            if (!skippable)
            {
                switch (EffectManager.Instance.Type)
                {
                    case EffectType.Skip:
                        EffectManager.Instance.Reset();
                        return;
                    case EffectType.Disclose:
                        for (int i = 0; i < 2; i++)
                        {
                            if (players[turnPlayerIndex].Disclosable)
                            {
                                int index = players[turnPlayerIndex].EffectDiscloseActionDisclose();
                                var c = hands[turnPlayerIndex].DiscloseCard(index);
                                HistoryManager.Instance.Add(EventType.Disclose, c);
                            }
                        }
                        EffectManager.Instance.Reset();
                        break;
                    case EffectType.Give:
                        hands[turnPlayerIndex].AddCard(EffectManager.Instance.GiftCard);
                        EffectManager.Instance.Reset();
                        break;
                    case EffectType.Draw:
                    case EffectType.QueenDraw:
                    case EffectType.ChainOfHate:
                        for (int i = 0; i < EffectManager.Instance.DrawNum; i++)
                        {
                            hands[turnPlayerIndex].AddCard(Draw());
                            HistoryManager.Instance.Add(EventType.Draw, null);
                        }
                        EffectManager.Instance.Reset();
                        break;
                }
            }

            // まだカードを出していない場合はターン行動させる
            if (card == null)
            {
                var index = players[turnPlayerIndex].TurnAction();
                if (index == -1)
                {
                    hands[turnPlayerIndex].AddCard(Draw());
                    HistoryManager.Instance.Add(EventType.Draw, null);
                    // ステータス表示
                    Console.Clear();
                    Console.WriteLine(Status);
                    index = players[turnPlayerIndex].TurnActionAfterDraw();
                }
                if (index >= 0)
                {
                    Discard(turnPlayerIndex, index);
                }
            }
        }

        /// <summary>
        /// このターンのプレイヤーが出したカードについて処理します。
        /// </summary>
        /// <param name="card">このターンでプレイヤーが出したカード。出していないなら null。</param>
        private void AfterTurnAction(Card card)
        {
            // ステータス表示
            Console.Clear();
            Console.WriteLine(Status);

            // カードを出していない場合はカード効果が消える
            if (card == null)
            {
                EffectManager.Instance.Reset();
                return;
            }

            // カード効果を記録して次ターンに持ち込む
            Card giftCard = null;
            if (card.Number == 7)
            {
                // 7を出した場合は渡すカードを決める
                var index = players[turnPlayerIndex].EffectGiveAction();
                if (index != -1)
                {
                    giftCard = hands[turnPlayerIndex].RemoveCard(index);
                    HistoryManager.Instance.Add(EventType.Give, null);
                }
            }
            EffectManager.Instance.Update(card, giftCard);
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// 山札からカードを1枚引きます。
        /// </summary>
        /// <returns>引いたカード。</returns>
        private Card Draw()
        {
            if (deck.Count == 0)
            {
                // 山札が無い場合
                if (grave.Count <= 1)
                {
                    // 捨て札が1枚以下の場合、ゲーム終了
                    // TODO: ゲーム終了処理
                    return null;
                }
                // 捨て札のトップ以外の捨て札を新たな山札とする
                var top = TopOfGrave;
                grave.Pop();
                while (grave.Count > 0)
                {
                    deck.Push(grave.Pop());
                }
                Shuffle();
                grave.Push(top);
            }
            return deck.Pop();
        }

        /// <summary>
        /// ゲーム開始時の処理に利用します。山札を1枚捨て札にします。
        /// </summary>
        private void Discard()
        {
            var card = Draw();
            card.declaredSuit = card.Suit;  // 最初の捨て札のみスートが自動設定される
            HistoryManager.Instance.Add(EventType.Discard, card);
            HistoryManager.Instance.Next(-1);
            grave.Push(card);
        }

        /// <summary>
        /// 指定した手札を捨て札にします。捨て札にできないカードが指定された場合は例外を発生させます。
        /// </summary>
        /// <param name="playerIndex">手札を捨てるプレイヤーのインデックス。</param>
        /// <param name="cardIndex">手札のインデックス。</param>
        private void Discard(int playerIndex, int cardIndex)
        {
            var card = hands[playerIndex].RemoveCard(cardIndex);
            if (!Validate(card))
            {
                throw new Exception($"捨て札の一番上の {TopOfGrave} に対して {card} が捨て札として選択されました。");
            }
            if (card.declaredSuit == SuitType.Joker)
            {
                throw new Exception($"{card} を捨て札にする前にスートを宣言する必要があります。");
            }
            HistoryManager.Instance.Add(EventType.Discard, card);
            card.Opened = false;
            grave.Push(card);
        }

        /// <summary>
        /// 山札をシャッフルします。
        /// </summary>
        private void Shuffle()
        {
            var random = new Random();
            var d = new List<Card>();
            while (deck.Count > 0)
            {
                d.Add(deck.Pop());
            }
            deck = new Stack<Card>();
            while (d.Count > 0)
            {
                int idx = random.Next(d.Count);
                deck.Push(d[idx]);
                d.RemoveAt(idx);
            }
        }

        #endregion
    }
}
