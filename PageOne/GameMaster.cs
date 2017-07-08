﻿using PageOne.Models;
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

        #region 定数

        /// <summary>カード効果の種類。</summary>
        public enum EffectType
        {
            /// <summary>何も発生していない。</summary>
            None,
            /// <summary>スキップ(A)。</summary>
            Skip,
            /// <summary>ドロー効果(2)。</summary>
            Draw,
            /// <summary>凶ドロー効果(スペード Q が出されて以降)。</summary>
            QueenDraw,
            /// <summary>知る権利(4)。</summary>
            Disclose,
            /// <summary>ロック(6)。</summary>
            Lock,
            /// <summary>7渡し(7)。</summary>
            Give,
            /// <summary>憎しみの連鎖(9)。</summary>
            ChainOfHate,
            /// <summary>連続行動(J)。</summary>
            Quick
        }

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

        /// <summary>リバース発動中か。</summary>
        private bool reversed;

        /// <summary>
        /// 各ターンの公開情報。
        /// プレイヤーのインデックス(最初の捨て札については -1)と
        /// そのプレイヤーが取った公開行動のリストがターン毎に記録されています。
        /// </summary>
        private List<KeyValuePair<int, List<Event>>> history;

        /// <summary>現在ターンでのターンプレイヤーの公開行動のリスト。</summary>
        private List<Event> turnHistory;

        /// <summary>現在発生しているカード効果の種類。</summary>
        private EffectType effect;

        /// <summary>現在発生しているカード効果でのドロー枚数。</summary>
        private int effectDrawNum;

        /// <summary>現在発生しているカード効果での2のカードが出された枚数。</summary>
        private int effectCardTwoNum;

        /// <summary>7渡しで次のプレイヤーに渡されるカード。</summary>
        private Card effectGiftCard;

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

        /// <summary>
        /// 各ターンの公開情報。
        /// プレイヤーのインデックス(最初の捨て札については -1)と
        /// そのプレイヤーが取った公開行動のリストがターン毎に記録されています。
        /// </summary>
        public List<KeyValuePair<int, List<Event>>> History
        {
            get
            {
                // 履歴を書き換えられたくないのでコピーを渡す
                var h = new List<KeyValuePair<int, List<Event>>>();
                foreach (var e in history)
                {
                    h.Add(new KeyValuePair<int, List<Event>>(e.Key, new List<Event>(e.Value)));
                }
                return h;
            }
        }

        /// <summary>ゲーム状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                var effectString =
                    effect == EffectType.Skip ? $"スキップ効果発動中  ドロー枚数: {effectDrawNum}" :
                    effect == EffectType.Draw ? $"ドロー効果発生中！  ドロー枚数: {effectDrawNum}" :
                    effect == EffectType.QueenDraw ? $"凶ドロー効果発生中！  ドロー枚数: {effectDrawNum}" :
                    effect == EffectType.Disclose ? "知る権利発動" :
                    effect == EffectType.Lock ? "ロック発動中" :
                    effect == EffectType.Give ? "7渡し発動" :
                    effect == EffectType.ChainOfHate ? "憎しみの連鎖発動" :
                    effect == EffectType.Quick ? "連続行動発動！" : "";
                var reverseString = reversed ? "リバース発動中\n" : "";

                return $"山札: {deck.Count} 枚\n" +
                    $"捨て札の一番上は {TopOfGrave} です。{reverseString}\n\n" +
                    $"{string.Join("\n", players.Select(x => x.Status))}\n\n" +
                    $"{players[turnPlayerIndex].Name} のターン\n" +
                    $"{effectString}\n";
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
            effect = EffectType.None;
            effectDrawNum = 0;
            effectCardTwoNum = 0;
            effectGiftCard = null;
            reversed = false;
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
            foreach (Card.SuitType suit in Enum.GetValues(typeof(Card.SuitType)))
            {
                if (suit == Card.SuitType.Joker)
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

            // カード配分
            foreach (var player in players)
            {
                for (int i = 0; i < 5; i++)
                {
                    player.AddCard(Draw());
                }
            }

            // 捨て札の初期化
            grave = new Stack<Card>();
            Discard(Draw());

            // 履歴の初期化
            turnHistory = new List<Event>();
            history = new List<KeyValuePair<int, List<Event>>>()
            {
                new KeyValuePair<int, List<Event>>(
                    -1,
                    new List<Event>()
                    {
                        new Event(Event.EventType.Discard, TopOfGrave)
                    })
            };

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
            if (TopOfGrave.declaredSuit == Card.SuitType.Joker) return true;

            // ジョーカーを出すならOK
            if (card.Suit == Card.SuitType.Joker) return true;

            // ロックされているときにJ、Q、K以外ならNG
            if (effect == EffectType.Lock && card.Number <= 10) return false;

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

        #region ゲームロジック(private メソッド)

        /// <summary>
        /// 1ターン進めます。
        /// </summary>
        private void Next()
        {
            // ターン前処理
            BeforeTurnAction();

            // ターンアクション処理
            Card card = null;
            var discards = turnHistory.Where(x => x.Type == Event.EventType.Discard).ToArray();
            if (discards.Length > 0)
            {
                card = discards[0].Card;
            }
            TurnAction(card);

            // カード効果の保持処理
            discards = turnHistory.Where(x => x.Type == Event.EventType.Discard).ToArray();
            if (discards.Length > 0)
            {
                card = discards[0].Card;
            }
            CardEffect(card);

            // 履歴処理
            history.Add(new KeyValuePair<int, List<Event>>(turnPlayerIndex, new List<Event>(turnHistory)));
            turnHistory = new List<Event>();

            // ターン進行処理
            if (effect != EffectType.Quick)
            {
                int c = players.Count;
                if (reversed)
                {
                    turnPlayerIndex = (turnPlayerIndex + c - 1) % c;
                }
                else
                {
                    turnPlayerIndex = (turnPlayerIndex + 1) % c;
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
            switch (effect)
            {
                case EffectType.Skip:
                    index = players[turnPlayerIndex].EffectSkipAction(effectDrawNum);
                    break;
                case EffectType.Draw:
                    index = players[turnPlayerIndex].EffectDrawAction(effectDrawNum);
                    break;
                case EffectType.QueenDraw:
                    index = players[turnPlayerIndex].EffectQueenDrawAction(effectDrawNum);
                    break;
                case EffectType.Disclose:
                    index = players[turnPlayerIndex].EffectDiscloseActionDiscard();
                    break;
            }

            // ゲーム状態の更新
            if (index >= 0)
            {
                var c = players[turnPlayerIndex].RemoveCard(index);
                Discard(c);
                turnHistory.Add(new Event(Event.EventType.Discard, c));
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
            Console.WriteLine(Status);

            // カード効果を飛ばせるか確認
            var skippable = false;
            if (card != null)
            {
                switch (effect)
                {
                    case EffectType.Skip:
                        skippable = card.Number == 10;
                        break;
                    case EffectType.Draw:
                        skippable = card.Suit == Card.SuitType.Joker || card.Number == 1 || card.Number == 2 || card.Number == 3 ||
                            (card.declaredSuit == Card.SuitType.Spade && card.Number == 12) || card.Number == 13;
                        break;
                    case EffectType.QueenDraw:
                        skippable = card.Suit == Card.SuitType.Joker || card.Number == 12 || card.Number == 13;
                        break;
                    case EffectType.Disclose:
                        skippable = card.Number == 5;
                        break;
                }
            }

            // 次プレイヤーに回せないカード効果を受ける
            if (!skippable)
            {
                if (effect == EffectType.Skip)
                {
                    effect = EffectType.None;
                    return;
                }
                else if (effect == EffectType.Draw || effect == EffectType.QueenDraw)
                {
                    for (int i = 0; i < effectDrawNum; i++)
                    {
                        players[turnPlayerIndex].AddCard(Draw());
                        turnHistory.Add(new Event(Event.EventType.Draw, null));
                    }
                    effect = EffectType.None;
                    effectDrawNum = 0;
                    effectCardTwoNum = 0;
                }
                else if (effect == EffectType.Disclose)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (players[turnPlayerIndex].Disclosable)
                        {
                            int index = players[turnPlayerIndex].EffectDiscloseActionDisclose();
                            var c = players[turnPlayerIndex].DiscloseCard(index);
                            turnHistory.Add(new Event(Event.EventType.Disclose, c));
                        }
                    }
                    effect = EffectType.None;
                }
                else if (effect == EffectType.Give)
                {
                    players[turnPlayerIndex].AddCard(effectGiftCard);
                    effect = EffectType.None;
                    effectGiftCard = null;
                }
                else if (effect == EffectType.ChainOfHate)
                {
                    players[turnPlayerIndex].AddCard(Draw());
                    turnHistory.Add(new Event(Event.EventType.Draw, null));
                    effect = EffectType.None;
                }
            }

            // まだカードを出していない場合はターン行動させる
            if (card == null)
            {
                var index = players[turnPlayerIndex].TurnAction();
                if (index == -1)
                {
                    players[turnPlayerIndex].AddCard(Draw());
                    turnHistory.Add(new Event(Event.EventType.Draw, null));
                    // ステータス表示
                    Console.Clear();
                    Console.WriteLine(Status);
                    index = players[turnPlayerIndex].TurnActionAfterDraw();
                }
                if (index >= 0)
                {
                    var c = players[turnPlayerIndex].RemoveCard(index);
                    Discard(c);
                    turnHistory.Add(new Event(Event.EventType.Discard, c));
                }
            }
        }

        /// <summary>
        /// このターンのプレイヤーが出したカード効果を処理します。
        /// </summary>
        /// <param name="card">このターンでプレイヤーが出したカード。出していないなら null。</param>
        private void CardEffect(Card card)
        {
            // ステータス表示
            Console.Clear();
            Console.WriteLine(Status);

            // カードを出していない場合はカード効果が消える
            if (card == null)
            {
                effect = EffectType.None;
                effectDrawNum = 0;
                return;
            }

            // カード効果を記録して次ターンに持ち込む
            switch (card.Number)
            {
                case 1:
                    effect = EffectType.Skip;
                    break;
                case 2:
                    effect = EffectType.Draw;
                    effectDrawNum += (int)Math.Pow(2, effectCardTwoNum == 0 ? 1 : effectCardTwoNum);
                    effectCardTwoNum += 1;
                    break;
                case 3:
                    reversed = !reversed;
                    break;
                case 4:
                    effect = EffectType.Disclose;
                    break;
                case 6:
                    effect = EffectType.Lock;
                    break;
                case 7:
                    var index = players[turnPlayerIndex].EffectGiveAction();
                    if (index != -1)
                    {
                        effect = EffectType.Give;
                        effectGiftCard = players[turnPlayerIndex].RemoveCard(index);
                        turnHistory.Add(new Event(Event.EventType.Give, null));
                    }
                    break;
                case 9:
                    effect = EffectType.ChainOfHate;
                    break;
                case 11:
                    effect = EffectType.Quick;
                    break;
                case 12:
                    if (card.declaredSuit == Card.SuitType.Spade)
                    {
                        effectDrawNum += 5;
                    }
                    effect = effect == EffectType.QueenDraw ? EffectType.QueenDraw : EffectType.None;
                    break;
                case 13:
                    if (effect == EffectType.Draw || effect == EffectType.QueenDraw)
                    {
                        effectDrawNum += 1;
                    }
                    break;
                default:
                    effect = EffectType.None;
                    effectDrawNum = 0;
                    effectCardTwoNum = 0;
                    break;
            }
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
        /// 指定したカードを捨て札にします。捨て札にできないカードが指定された場合は例外を発生させます。
        /// </summary>
        /// <param name="card">捨て札にするカード。</param>
        /// <returns>正常に処理が終了したか。</returns>
        private void Discard(Card card)
        {
            if (!Validate(card))
            {
                throw new Exception($"捨て札の一番上の {TopOfGrave} に対して {card} が捨て札として選択されました。");
            }
            if (card.declaredSuit == Card.SuitType.Joker)
            {
                throw new Exception($"{card} を捨て札にする前にスートを宣言する必要があります。");
            }
            card.Opened = false;
            grave.Push(card);
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
