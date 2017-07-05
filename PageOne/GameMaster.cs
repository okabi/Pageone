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
    public class GameMaster
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

        /// <summary>プレイヤー。</summary>
        private List<Player> players;

        /// <summary>山札。</summary>
        private Stack<Card> deck;

        /// <summary>捨て札。</summary>
        private Stack<Card> grave;

        /// <summary>ロックされているか。</summary>
        private bool locked;

        /// <summary>各ターンの捨て札。</summary>
        private List<Card> history;

        #endregion

        #region プロパティ

        /// <summary>現在のターン数。</summary>
        public int Turn { get; private set; }

        /// <summary>捨て札の一番上のカード。</summary>
        public Card TopOfGrave
        {
            get
            {
                return grave.Peek();
            }
        }

        /// <summary>各ターンの捨て札。</summary>
        public List<Card> History
        {
            get
            {
                return new List<Card>(history);
            }
        }

        /// <summary>ゲーム状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                return $"{Turn} ターン目  山札: {deck.Count} 枚\n" +
                    $"捨て札の一番上は {TopOfGrave} です。\n\n" +
                    string.Join("\n", players.Select(x => x.Status));
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
            Turn = 0;
            random = new Random();
            
            // プレイヤーの初期化
            players = new List<Player>();
            for (int i = 0; i < names.Count; i++)
            {
                if (i == 0)
                {
                    players.Add(new ControllablePlayer(names[i]));
                }
                else
                {
                    // テスト用
                    players.Add(new ControllablePlayer(names[i]));
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

            // 捨て札の初期化
            grave = new Stack<Card>();
            grave.Push(Draw());

            // カード配分
            foreach (var player in players)
            {
                for (int i = 0; i < 5; i++)
                {
                    player.AddCard(Draw());
                }
            }

            // ゲームループ
            while (true)
            {
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
            grave.Push(card);
            return true;
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// 1ターン進めます。
        /// </summary>
        private void Next()
        {
            Turn++;
            Console.Clear();
            players[(Turn - 1) % players.Count].Next();
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

        /// <summary>
        /// 指定したカードを捨て札にできるかを判定します。
        /// </summary>
        /// <param name="card">捨て札にしたいカード。</param>
        /// <returns>捨て札にできるか。</returns>
        private bool Validate(Card card)
        {
            // ジョーカーを出すならOK
            if (card.Suit == Card.SuitType.Joker)   return true;

            // ロックされているときにJ、Q、K以外ならNG
            if (locked && card.Number <= 10) return false;

            // スートが一致していればOK
            if (card.Suit == TopOfGrave.declaredSuit)   return true;

            // ジョーカーが出されているならNG
            if (TopOfGrave.Suit == Card.SuitType.Joker) return false;

            // 数字が一致していればOK
            if (TopOfGrave.Number == card.Number) return true;

            // いずれにも当てはまらない場合はNG
            return false;
        }

        #endregion
    }
}
