using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne.Models
{
    /// <summary>
    /// プレイヤー1人の手札を表すクラスです。
    /// </summary>
    public class Hand
    {
        #region フィールド

        /// <summary>手札にあるカードのリスト。</summary>
        private List<Card> cards;

        #endregion

        #region プロパティ

        /// <summary>手札にあるカードのリストのコピー。</summary>
        public List<Card> Cards
        {
            get
            {
                return new List<Card>(cards.Select(x => new Card(x)));
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public Hand()
        {
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
    }
}
