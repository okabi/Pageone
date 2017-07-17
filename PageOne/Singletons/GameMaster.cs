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

        /// <summary>
        /// ドロー効果中に手札を0枚にした、おそらく上がりになるプレイヤーのリスト。
        /// 先に手札が0枚になったプレイヤーほど先に登録される。
        /// ドロー効果が発動した後にも手札が0枚だった場合は、確定の上がりになる。
        /// </summary>
        private List<int> maybeClearPlayers;

        /// <summary>プレイヤー名をキーとした、順位の辞書。</summary>
        private Dictionary<string, int> ranking;

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

        /// <summary>山札の枚数。</summary>
        public int DeckCount
        {
            get
            {
                return deck.Count;
            }
        }

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

        /// <summary>プレイヤーの名前をキーとした、各プレイヤーの手札の公開情報。非公開カードは null として格納されます。</summary>
        public Dictionary<string, List<Card>> Cards
        {
            get
            {
                var ret = new Dictionary<string, List<Card>>();
                for (int i = 0; i < players.Count; i++)
                {
                    ret[players[i].Name] = hands[i].Cards.Select(x => x.Opened ? x : null).ToList();
                }
                return ret;
            }
        }

        /// <summary>全員の順位が決定してゲームが終了しているか。</summary>
        public bool IsGameEnd
        {
            get
            {
                return ranking.Count == players.Count;
            }
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// ゲームを初期化して開始します。
        /// ゲーム終了時に players の状態が変化するので、複数回呼ぶときは注意してください。
        /// </summary>
        /// <param name="players">参加プレイヤーのリスト。</param>
        /// <returns>プレイヤー名をキーとした、順位の辞書。</returns>
        public Dictionary<string, int> Run(List<Player> players)
        {
            // 名前の被りを確認
            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    if (players[i].Name == players[j].Name)
                    {
                        throw new Exception("プレイヤー名は一意である必要があります。");
                    }
                }
            }

            // 変数の初期化
            this.players = players;
            turnPlayerIndex = 0;
            maybeClearPlayers = new List<int>();
            ranking = new Dictionary<string, int>();
            
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

            // カード効果の初期化
            EffectManager.Instance.Init();

            // 捨て札の初期化
            grave = new Stack<Card>();
            Discard();

            // ゲームループ
            while (true)
            {
                if (Next()) return ranking;
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
            if (TopOfGrave.DeclaredSuit == SuitType.Joker) return true;

            // ジョーカーを出すならOK
            if (card.Suit == SuitType.Joker) return true;

            // ロックされているときにJ、Q、K以外ならNG
            if (EffectManager.Instance.Type == EffectType.Lock && card.Number <= 10) return false;

            // スートが一致していればOK
            if (card.Suit == TopOfGrave.DeclaredSuit) return true;

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
        /// <returns>ゲームが終了したか。</returns>
        private bool Next()
        {
            // ターン前処理
            BeforeTurnAction();
            if (IsGameEnd) return true;

            // ターンアクション処理
            TurnAction(HistoryManager.Instance.TurnDiscard);
            if (IsGameEnd) return true;

            // ターン後処理
            AfterTurnAction(HistoryManager.Instance.TurnDiscard);
            if (IsGameEnd) return true;

            // 履歴処理
            HistoryManager.Instance.Next(turnPlayerIndex);

            // ターン進行処理
            if (EffectManager.Instance.Type != EffectType.Quick)
            {
                do
                {
                    if (EffectManager.Instance.Reversing)
                    {
                        turnPlayerIndex = (turnPlayerIndex + players.Count - 1) % players.Count;
                    }
                    else
                    {
                        turnPlayerIndex = (turnPlayerIndex + 1) % players.Count;
                    }
                } while (!maybeClearPlayers.Contains(turnPlayerIndex) && hands[turnPlayerIndex].Cards.Count == 0);
            }
            return false;
        }

        /// <summary>
        /// プレイヤーのターン前処理です。
        /// </summary>
        private void BeforeTurnAction()
        {
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
                var card = hands[turnPlayerIndex].Cards[index];
                if (!EffectManager.Instance.Avoidable(card))
                {
                    throw new Exception($"{TopOfGrave} の効果は {card} では防げません。");
                }
                players[turnPlayerIndex].DiscardAction(hands[turnPlayerIndex].Cards[index]);
                Discard(turnPlayerIndex, index);
                if (IsGameEnd) return;
            }
        }

        /// <summary>
        /// プレイヤーのターン処理です。
        /// </summary>
        /// <param name="card">ターン前処理でプレイヤーが出したカード。出していないなら null。</param>
        private void TurnAction(Card card)
        {
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
                                c.Opened = false;
                                players[turnPlayerIndex].DiscloseAction(c);
                                HistoryManager.Instance.Add(EventType.Disclose, c);
                            }
                        }
                        break;
                    case EffectType.Give:
                        hands[turnPlayerIndex].AddCard(EffectManager.Instance.GiftCard);
                        players[turnPlayerIndex].ReceiveAction(new Card(EffectManager.Instance.GiftCard));
                        break;
                    case EffectType.Draw:
                    case EffectType.QueenDraw:
                    case EffectType.ChainOfHate:
                        players[turnPlayerIndex].DrawAction(EffectManager.Instance.DrawNum);
                        for (int i = 0; i < EffectManager.Instance.DrawNum; i++)
                        {
                            var c = Draw();
                            if (c == null)
                            {
                                // カードを引けなかったらゲーム終了
                                CheckClearPlayers();
                                return;
                            }
                            hands[turnPlayerIndex].AddCard(c);
                            HistoryManager.Instance.Add(EventType.Draw, null);
                        }
                        break;
                }
                EffectManager.Instance.Reset();
            }

            // まだカードを出していない場合はターン行動させる
            if (card == null)
            {
                var index = players[turnPlayerIndex].TurnAction();
                if (index == -1)
                {
                    players[turnPlayerIndex].DrawAction(1);
                    var c = Draw();
                    if (c == null)
                    {
                        // カードを引けなかったらゲーム終了
                        CheckClearPlayers();
                        return;
                    }
                    hands[turnPlayerIndex].AddCard(c);
                    HistoryManager.Instance.Add(EventType.Draw, null);
                    index = players[turnPlayerIndex].TurnActionAfterDraw();
                }
                if (index >= 0)
                {
                    players[turnPlayerIndex].DiscardAction(hands[turnPlayerIndex].Cards[index]);
                    Discard(turnPlayerIndex, index);
                    if (IsGameEnd) return;
                }
            }
        }

        /// <summary>
        /// このターンのプレイヤーが出したカードについて処理します。
        /// </summary>
        /// <param name="card">このターンでプレイヤーが出したカード。出していないなら null。</param>
        private void AfterTurnAction(Card card)
        {
            // カードを出していない場合はカード効果が消える
            if (card == null)
            {
                EffectManager.Instance.Reset();
                return;
            }

            // カード効果を記録して次ターンに持ち込む
            Card giftCard = null;
            if (card.Number == 7 && hands[turnPlayerIndex].Cards.Count > 0)
            {
                // 7を出した場合は渡すカードを決める
                var index = players[turnPlayerIndex].EffectGiveAction();
                if (index != -1)
                {
                    giftCard = hands[turnPlayerIndex].RemoveCard(index);
                    players[turnPlayerIndex].GiveAction(new Card(giftCard));
                    HistoryManager.Instance.Add(EventType.Give, null);
                    if (hands[turnPlayerIndex].Cards.Count == 1)
                    {
                        // ページワン宣言
                        players[turnPlayerIndex].PageOneAction();
                    }
                }
            }
            EffectManager.Instance.Update(card, giftCard);

            // もし J を出して上がっていたならカード効果を消去する
            if (card.Number == 11 && hands[turnPlayerIndex].Cards.Count == 0)
            {
                EffectManager.Instance.Reset();
            }

            // ゲーム終了判定
            CheckClearPlayers();
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
            card.DeclaredSuit = card.Suit;  // 最初の捨て札のみスートが自動設定される
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
            if (card.DeclaredSuit == SuitType.Joker)
            {
                card.DeclaredSuit = players[playerIndex].SelectSuitAction();
                if (card.DeclaredSuit == SuitType.Joker)
                {
                    throw new Exception($"{card} の宣言スートがジョーカーになっています。");
                }
            }
            if (hands[playerIndex].Cards.Count == 1)
            {
                // ページワン宣言
                players[playerIndex].PageOneAction();
            }
            if (hands[playerIndex].Cards.Count == 0)
            {
                // ジョーカー上がりの確認
                if (card.Suit == SuitType.Joker)
                {
                    players[playerIndex].DrawAction(5);
                    for (int i = 0; i < 5; i++)
                    {
                        var c = Draw();
                        if (c == null)
                        {
                            // カードを引けなかったらゲーム終了
                            CheckClearPlayers();
                            return;
                        }
                        hands[playerIndex].AddCard(c);
                    }
                }
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
                deck.Push(new Card(d[idx].Suit, d[idx].Number));
                d.RemoveAt(idx);
            }
        }

        /// <summary>
        /// 現在の手札状況やカード効果、山札と捨て札の枚数からクリア状況を更新します。
        /// </summary>
        /// <returns>ゲーム終了条件が満たされているか。</returns>
        private bool CheckClearPlayers()
        {
            var nextRank = ranking.Count == 0 ? 1 : ranking.Max(x => x.Value) + 1;

            if (deck.Count == 0 && grave.Count == 1)
            {
                // 山札が無く捨て札が1枚の場合、残りのプレイヤーを同率最下位としてゲーム終了
                foreach (var p in players)
                {
                    if (!ranking.ContainsKey(p.Name))
                    {
                        ranking[p.Name] = nextRank;
                    }
                }
                return true;
            }
            else if (EffectManager.Instance.DrawNum > 0)
            {
                // ドロー効果発動中の場合は、残りのプレイヤーの中で手札が0枚になった人は上がり候補となる
                for (int i = 0; i < players.Count; i++)
                {
                    if (!ranking.ContainsKey(players[i].Name) && hands[i].Cards.Count == 0 && !maybeClearPlayers.Contains(i))
                    {
                        maybeClearPlayers.Add(i);
                    }
                }
            }
            else
            {
                // それ以外のときで手札が0枚になっているプレイヤーは上がり
                // 先に手札を出し切っていた人の方が順位は上
                foreach (var i in maybeClearPlayers)
                {
                    if (hands[i].Cards.Count == 0)
                    {
                        ranking[players[i].Name] = nextRank;
                        players[i].ClearAction(nextRank);
                        nextRank++;
                    }
                }
                maybeClearPlayers.Clear();
                for (int i = 0; i < players.Count; i++)
                {
                    if (!ranking.ContainsKey(players[i].Name) && hands[i].Cards.Count == 0)
                    {
                        ranking[players[i].Name] = nextRank;
                        players[i].ClearAction(nextRank);
                        nextRank++;
                    }
                }
            }
            // 残りプレイヤーが1人になっていたらその人は最下位となりゲーム終了
            if (ranking.Count == players.Count - 1)
            {
                var loser = players[hands.Select((x, i) => x.Cards.Count > 0 ? i : -1).Where(x => x >= 0).ToArray()[0]].Name;
                ranking[loser] = nextRank;
                return true;
            }
            return false;
        }

        #endregion
    }
}
