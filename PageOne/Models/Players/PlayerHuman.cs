using PageOne.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne.Models.Players
{
    /// <summary>
    /// 操作するプレイヤーです。
    /// </summary>
    public class PlayerHuman : Player
    {
        #region コンストラクタ

        /// <summary>
        /// 操作するプレイヤーを作成します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        public PlayerHuman(string name) : base(name) { }

        #endregion

        #region public override メソッド

        /// <summary>
        /// 1枚ドローしていない状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。カードを引く場合は -1 を返します。</returns>
        public override int TurnAction()
        {
            return SelectTurnAction(true);
        }

        /// <summary>
        /// 1枚ドローした状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もしない場合は -1 を返します。</returns>
        public override int TurnActionAfterDraw()
        {
            return SelectTurnAction(false);
        }

        /// <summary>
        /// スキップが回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー効果中の場合は、ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します</returns>
        public override int EffectSkipAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDrawAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// 凶ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectQueenDrawAction(int drawNum)
        {
            return -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに出せる 5 のカードを持っていた場合に取る行動を決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDiscloseActionDiscard()
        {
            return -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに公開するカードを決定して返します。
        /// </summary>
        /// <returns>公開する手札のインデックス。</returns>
        public override int EffectDiscloseActionDisclose()
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
        public override int EffectGiveAction()
        {
            return -1;
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// プレイヤーに、このターンに出すカードを決定させます。
        /// </summary>
        /// <param name="drawable">1枚カードを引く権利が残っているか。</param>
        /// <returns>このターンに出す手札のインデックス。カードを引く or 何もしない場合は -1 を返します。</returns>
        private int SelectTurnAction(bool drawable)
        {
            while (true)
            {
                var option = new Dictionary<int, string>(
                    Option.ToDictionary(x => x.Key + 1, x => x.Value.ToString()));
                option.Add(88, drawable ? "カードを引く" : "パスする");
                option.Add(99, "ヘルプ");
                int input = Utility.ReadNumber(
                    $"出すカード または その他の行動を選択してください。", option);

                if (input == 88)
                {
                    return -1;
                }
                else if (input == 99)
                {
                    Help.Top(UnvalidatedOption);
                    Console.WriteLine(GameMaster.Instance.Status);
                }
                else
                {
                    return input - 1;
                }
            }
        }

        #endregion
    }
}
