using PageOne.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PageOne.Models
{
    /// <summary>
    /// 操作するプレイヤーです。
    /// </summary>
    public class Player : IPlayer
    {
        #region フィールド

        /// <summary>手札。</summary>
        private List<Card> cards;

        /// <summary>カードを1枚引ける状態か。</summary>
        private bool drawable;

        #endregion

        #region プロパティ

        /// <summary>プレイヤー名。</summary>
        public string Name { get; private set; }

        /// <summary>名前と手札の状態を表す文字列。</summary>
        public string Status
        {
            get
            {
                var ret = $"{Name}: ";
                ret += string.Join(" ", cards.Select(x => x.ToString()));
                return ret;
            }
        }

        /// <summary>手札の選択肢を表す辞書。</summary>
        public Dictionary<int, string> Option
        {
            get
            {
                var ret = new Dictionary<int, string>();
                for (int i = 0; i < cards.Count; i++)
                {
                    ret.Add(i + 1, cards[i].ToString());
                }
                return ret;
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
        public void RemoveCard(int index)
        {
            if (index < 0 || index >= cards.Count)
            {
                throw new Exception("手札の指定インデックスが不正です。" +
                    $"{cards.Count}枚に対して{index}が指定されています。");
            }
            cards.RemoveAt(index);
        }

        /// <summary>
        /// 自分のターンを処理し、次のターンに移します。
        /// </summary>
        public void Next()
        {
            // ターンの開始処理
            drawable = true;

            // ターンの処理
            while (true)
            {
                Console.WriteLine(GameMaster.Instance.Status + "\n");

                var option = new Dictionary<int, string>(Option);
                if (drawable)
                {
                    option.Add(88, "カードを引く");
                }
                option.Add(99, "ヘルプ");
                int input = Utility.ReadNumber(
                    $"{Name} のターン\n出すカード または その他の行動を選択してください。", option);
                if (input == 88)
                {
                    AddCard(GameMaster.Instance.Draw());
                    drawable = false;
                }
                else if (input == 99)
                {
                    Help.Top();
                }
                else
                {
                    break;
                }
            }
        }

        #endregion
    }
}
