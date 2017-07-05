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
                return $"{Name}: " + string.Join(" ", cards.Select(x => $"{x,5}"));
            }
        }

        /// <summary>手札の選択肢を表す辞書。</summary>
        public Dictionary<int, Card> Option
        {
            get
            {
                return cards
                    .Select((x, i) => new { Item = x, Index = i + 1 })
                    .ToDictionary(x => x.Index, x => x.Item);
            }
        }

        /// <summary>カードを1枚引ける状態か。</summary>
        public bool Drawable { get; protected set; }

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
        public void RemoveCard(int index)
        {
            if (index < 0 || index >= cards.Count)
            {
                throw new Exception("手札の指定インデックスが不正です。" +
                    $"{cards.Count}枚に対して{index}が指定されています。");
            }
            GameMaster.Instance.Discard(cards[index]);
            cards.RemoveAt(index);
        }

        #endregion

        #region 仮想 public メソッド

        /// <summary>
        /// 自分のターンを処理し、次のターンに移します。
        /// ゲームマスターから呼ばれる仮想メソッドです。
        /// </summary>
        public virtual void Next() { }

        #endregion
    }
}
