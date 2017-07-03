using PageOne.Interfaces;
using PageOne.Models;
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
        private List<IPlayer> players;

        /// <summary>山札。</summary>
        private Stack<Card> deck;

        /// <summary>捨て札。</summary>
        private Stack<Card> grave;

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
            Turn = 0;
            random = new Random();
            
            // プレイヤーの初期化
            players = new List<IPlayer>();
            for (int i = 0; i < names.Count; i++)
            {
                if (i == 0)
                {
                    players.Add(new Player(names[i]));
                }
                else
                {
                    // テスト用
                    players.Add(new Player(names[i]));
                }
            }

            // デッキの初期化
            deck = new Stack<Card>();
            for (int i = 0; i < 2; i++)
            {
                deck.Push(new Card(Card.SuitType.Joker, null));
            }
            for (int i = 1; i <= 13; i++)
            {
                deck.Push(new Card(Card.SuitType.Spade, i));
                deck.Push(new Card(Card.SuitType.Club, i));
                deck.Push(new Card(Card.SuitType.Diamond, i));
                deck.Push(new Card(Card.SuitType.Heart, i));
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
        public void Discard(Card card)
        {
            grave.Push(card);
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

        #endregion
    }
}
