using PageOne.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne.Models
{
    /// <summary>
    /// 一人のプレイヤーを表す抽象クラスです。
    /// </summary>
    public abstract class Player
    {
        #region フィールド

        /// <summary>手札。</summary>
        private List<Card> cards;

        #endregion

        #region プロパティ

        /// <summary>プレイヤー名。</summary>
        public string Name { get; private set; }

        /// <summary>名前と手札の状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                return $"{Name}: " + string.Join(" ", cards.Select(x => $"{x, 5}"));
            }
        }

        /// <summary>非公開カードが手札に含まれるか。</summary>
        public bool Disclosable
        {
            get
            {
                return cards.Select(x => x.Opened).Contains(false);
            }
        }


        /// <summary>手札。</summary>
        protected List<Card> Cards
        {
            get
            {
                // 派生クラスで手札を制御されたくないのでコピーを返す
                return new List<Card>(cards);
            }
        }

        /// <summary>手札の選択肢を表す辞書。</summary>
        protected Dictionary<int, Card> Option
        {
            get
            {
                return Cards
                    .Select((x, i) => new { Item = x, Index = i })
                    .Where(x => GameMaster.Instance.Validate(x.Item))
                    .ToDictionary(x => x.Index, x => x.Item);
            }
        }

        /// <summary>捨て札にできないものを含めた手札の選択肢を表す辞書。</summary>
        protected Dictionary<int, Card> UnvalidatedOption
        {
            get
            {
                return Cards
                    .Select((x, i) => new { Item = x, Index = i })
                    .ToDictionary(x => x.Index, x => x.Item);
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// 操作するプレイヤーを作成します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        public Player(string name)
        {
            Name = name;
            cards = new List<Card>();
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// 手札を加えます。
        /// </summary>
        /// <param name="card">追加する手札。</param>
        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        /// <summary>
        /// 手札を捨てます。
        /// </summary>
        /// <param name="index">捨てる手札のインデックス。</param>
        /// <returns>捨てる手札。</returns>
        public Card RemoveCard(int index)
        {
            if (index < 0 || index >= cards.Count)
            {
                throw new Exception("手札の指定インデックスが不正です。" +
                    $"{cards.Count}枚に対して{index}が指定されています。");
            }
            var ret = cards[index];
            cards.RemoveAt(index);
            return ret;
        }

        /// <summary>
        /// 手札を公開します。
        /// </summary>
        /// <param name="index">公開する手札のインデックス。</param>
        /// <returns>公開する手札。</returns>
        public Card DiscloseCard(int index)
        {
            if (index < 0 || index >= cards.Count)
            {
                throw new Exception("手札の指定インデックスが不正です。" +
                    $"{cards.Count}枚に対して{index}が指定されています。");
            }
            if (cards[index].Opened)
            {
                throw new Exception($"{cards[index]} は既に公開されています。");
            }
            cards[index].Opened = true;
            return cards[index];
        }

        #endregion

        #region 仮想 public メソッド

        /// <summary>
        /// 1枚ドローしていない状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。カードを引く場合は -1 を返します。</returns>
        public virtual int TurnAction()
        {
            return -1;
        }

        /// <summary>
        /// 1枚ドローした状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もしない場合は -1 を返します。</returns>
        public virtual int TurnActionAfterDraw()
        {
            return -1;
        }

        /// <summary>
        /// スキップが回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー効果中の場合は、ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します</returns>
        public virtual int EffectSkipAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public virtual int EffectDrawAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// 凶ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public virtual int EffectQueenDrawAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに出せる 5 のカードを持っていた場合に取る行動を決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public virtual int EffectDiscloseActionDiscard()
        {
            return -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに公開するカードを決定して返します。
        /// </summary>
        /// <returns>公開する手札のインデックス。</returns>
        public virtual int EffectDiscloseActionDisclose()
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                if (!Cards[i].Opened)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 7渡しの効果発動時に取る行動を決定して返します。
        /// </summary>
        /// <returns>渡す手札のインデックス。何も渡さない場合は -1 を返します。</returns>
        public virtual int EffectGiveAction()
        {
            return -1;
        }

        #endregion
    }
}
