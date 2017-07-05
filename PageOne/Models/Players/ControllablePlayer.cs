using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne.Models.Players
{
    /// <summary>
    /// 操作するプレイヤーです。
    /// </summary>
    public class ControllablePlayer : Player
    {
        #region コンストラクタ

        /// <summary>
        /// 操作するプレイヤーを作成します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        public ControllablePlayer(string name) : base(name) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// 自分のターンを処理し、次のターンに移します。
        /// </summary>
        public override void Next()
        {
            // ターンの開始処理
            Drawable = true;

            // ターンの処理
            while (true)
            {
                Console.WriteLine(GameMaster.Instance.Status + "\n");

                var option = new Dictionary<int, string>(
                    Option.ToDictionary(x => x.Key, x => x.Value.ToString()));
                option.Add(88, Drawable ? "カードを引く" : "パスする");
                option.Add(99, "ヘルプ");
                int input = Utility.ReadNumber(
                    $"{Name} のターン\n出すカード または その他の行動を選択してください。", option);

                if (input == 88)
                {
                    if (Drawable)
                    {
                        AddCard(GameMaster.Instance.Draw());
                        Drawable = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (input == 99)
                {
                    Help.Top(Option);
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
