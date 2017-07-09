﻿using System;
using System.Collections.Generic;
using System.Linq;
using PageOne.Singletons;

namespace PageOne.Models
{
    /// <summary>
    /// 一人のプレイヤーを表す抽象クラスです。
    /// </summary>
    public abstract class Player
    {
        #region フィールド

        /// <summary>手札への参照。</summary>
        private Hand hand;

        #endregion

        #region プロパティ

        /// <summary>プレイヤー名。</summary>
        public string Name { get; private set; }

        /// <summary>名前と手札の状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                return $"{Name}: " + string.Join(" ", hand.Cards.Select(x => $"{x, 5}"));
            }
        }

        /// <summary>非公開カードが手札に含まれるか。</summary>
        public bool Disclosable
        {
            get
            {
                return hand.Cards.Select(x => x.Opened).Contains(false);
            }
        }


        /// <summary>手札にあるカードのリストのコピー。</summary>
        protected List<Card> Cards
        {
            get
            {
                return hand.Cards;
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
            hand = null;
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// このプレイヤーの手札への参照を設定します。
        /// 既に参照が設定されている場合は例外を発生させます。
        /// </summary>
        /// <param name="hand">手札への参照。</param>
        public void SetHandReference(Hand hand)
        {
            if (this.hand != null)
            {
                throw new Exception($"{Name} の手札への参照は既に設定されています。");
            }
            this.hand = hand;
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
