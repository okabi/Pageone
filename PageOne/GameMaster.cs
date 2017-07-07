using PageOne.Models;
using PageOne.Models.Cards;
using PageOne.Models.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne
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

        /// <summary>乱数発生器。</summary>
        private Random random;

        /// <summary>現在のターンプレイヤーを示すインデックス。</summary>
        private int turnPlayerIndex;

        /// <summary>プレイヤー。</summary>
        private List<Player> players;

        /// <summary>山札。</summary>
        private Stack<Card> deck;

        /// <summary>捨て札。</summary>
        private Stack<Card> grave;

        /// <summary>ロックされているか。</summary>
        private bool locked;

        /// <summary>各ターンの公開情報。</summary>
        private List<List<Event>> history;

        /// <summary>現在ターンの公開情報。</summary>
        private List<Event> turnHistory;

        #endregion

        #region プロパティ

        /// <summary>捨て札の一番上のカード。</summary>
        public Card TopOfGrave
        {
            get
            {
                return grave.Peek();
            }
        }

        /// <summary>各ターンの捨て札。</summary>
        public List<List<Event>> History
        {
            get
            {
                // 履歴を書き換えられたくないのでコピーを渡す
                var h = new List<List<Event>>();
                foreach (var e in history)
                {
                    h.Add(new List<Event>(e));
                }
                return h;
            }
        }

        /// <summary>ゲーム状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                return $"山札: {deck.Count} 枚\n" +
                    $"捨て札の一番上は {TopOfGrave} です。\n\n" +
                    string.Join("\n", players.Select(x => x.Status) + "\n\n" +
                    $"{players[turnPlayerIndex].Name} のターン");
            }
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// ゲームを初期化して開始します。操作プレイヤーは1Pだと想定します。
        /// </summary>
        /// <param name="names">参加プレイヤー名のリスト。</param>
        public void Start(List<string> names)
        {
            // 変数の初期化
            locked = false;
            turnPlayerIndex = 0;
            random = new Random();
            
            // プレイヤーの初期化
            players = new List<Player>();
            for (int i = 0; i < names.Count; i++)
            {
                if (i == 0)
                {
                    players.Add(new PlayerHuman(names[i]));
                }
                else
                {
                    // テスト用
                    players.Add(new PlayerHuman(names[i]));
                }
            }

            // デッキの初期化
            deck = new Stack<Card>();
            for (int i = 0; i < 2; i++)
            {
                deck.Push(new CardJoker());
            }
            foreach (Card.SuitType suit in Enum.GetValues(typeof(Card.SuitType)))
            {
                if (suit == Card.SuitType.Joker) continue;
                deck.Push(new Card1(suit));
                deck.Push(new Card2(suit));
                deck.Push(new Card3(suit));
                deck.Push(new Card4(suit));
                deck.Push(new Card5(suit));
                deck.Push(new Card6(suit));
                deck.Push(new Card7(suit));
                deck.Push(new Card8(suit));
                deck.Push(new Card9(suit));
                deck.Push(new Card10(suit));
                deck.Push(new Card11(suit));
                deck.Push(new Card12(suit));
                deck.Push(new Card13(suit));
            }
            Shuffle();

            // カード配分
            foreach (var player in players)
            {
                for (int i = 0; i < 5; i++)
                {
                    player.AddCard(Draw());
                }
            }

            // 履歴の初期化
            history = new List<List<Event>>();
            turnHistory = new List<Event>();

            // 捨て札の初期化
            grave = new Stack<Card>();
            Discard(Draw());

            // ゲームループ
            while (true)
            {
                Console.Clear();
                Next();
            }
        }

        /// <summary>
        /// 山札からカードを1枚引きます。
        /// </summary>
        /// <returns>引いたカード。</returns>
        public Card Draw()
        {
            if (deck.Count == 0)
            {
                var top = TopOfGrave;
                grave.Pop();
                while(grave.Count > 0)
                {
                    deck.Push(grave.Pop());
                }
                Shuffle();
                grave.Push(top);
            }
            return deck.Pop();
        }

        /// <summary>
        /// 指定したカードを捨て札にします。
        /// </summary>
        /// <param name="card">捨て札にするカード。</param>
        /// <returns>正常に処理が終了したか。</returns>
        public bool Discard(Card card)
        {
            if (!Validate(card))
            {
                return false;
            }
            if (turnPlayerIndex == -1)
            {
                // 最初の捨て札の場合
                turnHistory.Add(new Event(Event.EventType.Discard, card, turnPlayerIndex, "GameMaster"));
            }
            else
            {
                turnHistory.Add(new Event(Event.EventType.Discard, card, turnPlayerIndex, players[turnPlayerIndex].Name));
            }
            card.Opened = false;
            grave.Push(card);
            return true;
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
            if (TopOfGrave.declaredSuit == Card.SuitType.Joker) return true;

            // ジョーカーを出すならOK
            if (card.Suit == Card.SuitType.Joker) return true;

            // ロックされているときにJ、Q、K以外ならNG
            if (locked && card.Number <= 10) return false;

            // スートが一致していればOK
            if (card.Suit == TopOfGrave.declaredSuit) return true;

            // ジョーカーが出されているならNG
            if (TopOfGrave.Suit == Card.SuitType.Joker) return false;

            // 数字が一致していればOK
            if (TopOfGrave.Number == card.Number) return true;

            // いずれにも当てはまらない場合はNG
            return false;
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// 1ターン進めます。
        /// </summary>
        private void Next()
        {
            history.Add(turnHistory);
            turnHistory = new List<Event>();
            turnPlayerIndex++;
            players[(turnPlayerIndex - 1) % players.Count].Next();
        }

        /// <summary>
        /// 山札をシャッフルします。
        /// </summary>
        private void Shuffle()
        {
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
