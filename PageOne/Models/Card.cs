using System;
using System.Collections.Generic;

namespace PageOne.Models
{
    /// <summary>
    /// トランプカードを表すクラスです。
    /// </summary>
    public class Card
    {
        #region 定数

        /// <summary>スート(マーク)の種類。</summary>
        public enum SuitType
        {
            /// <summary>スペード。</summary>
            Spade,
            /// <summary>クラブ。</summary>
            Club,
            /// <summary>ダイヤ。</summary>
            Diamond,
            /// <summary>ハート。</summary>
            Heart,
            /// <summary>ジョーカー。</summary>
            Joker
        }

        #endregion

        #region フィールド

        /// <summary>スート(マーク)。</summary>
        private readonly SuitType suit;

        /// <summary>数字。</summary>
        private readonly int number;

        #endregion

        #region プロパティ

        /// <summary>スート(マーク)。</summary>
        public SuitType Suit
        {
            get { return suit; }
        }

        /// <summary>捨て札時に宣言されているスート(マーク)。</summary>
        public SuitType declaredSuit { get; set; }

        /// <summary>数字。</summary>
        public int Number
        {
            get { return number; }
        }

        /// <summary>他プレイヤーに対してオープンになっているか。</summary>
        public bool Opened { get; set; }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        /// <param name="number">数字。</param>
        public Card(SuitType suit, int number)
        {
            if (suit == SuitType.Joker && number >= 1 && number <= 13)
            {
                throw new Exception("ジョーカーの数字は1～13以外で設定してください。");
            }
            if (suit != SuitType.Joker && (number < 1 || number > 13))
            {
                throw new Exception("カードの数字は1～13の間で設定してください。");
            }

            this.suit = suit;
            this.number = number;
            declaredSuit = suit;
            if (number == 8)
            {
                declaredSuit = SuitType.Joker;
            }
            Opened = false;
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// カードの状態を表す文字列です。
        /// </summary>
        /// <returns>カードの状態を表す文字列。</returns>
        public override string ToString()
        {
            var ret =
                Suit == SuitType.Spade ? "S." :
                Suit == SuitType.Club ? "C." :
                Suit == SuitType.Diamond ? "D." :
                Suit == SuitType.Heart ? "H." :
                "Joker";
            if (Suit != SuitType.Joker)
            {
                ret +=
                    Number == 1 ? "A" :
                    Number == 11 ? "J" :
                    Number == 12 ? "Q" :
                    Number == 13 ? "K" :
                    $"{Number}";
            }
            if (Opened)
            {
                ret += "(表)";
            }
            return ret;
        }

        #endregion
    }
}
