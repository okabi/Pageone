using System;
using System.Linq;
using static PageOne.Models.Card;

namespace PageOne.Models.Players
{
    /// <summary>
    /// AI の雛形です。このクラスをコピーして改造することでオリジナル AI を作成できます。
    /// </summary>
    public class PlayerAITemplate : Player
    {
        #region フィールド

        /// <summary>乱数生成器。</summary>
        private Random random;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// AI を作成します。
        /// </summary>
        public PlayerAITemplate() : base("AI name")
        {
            random = new Random();
        }

        #endregion

        #region public override メソッド

        /// <summary>
        /// 1枚ドローしていない状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。カードを引く場合は -1 を返します。</returns>
        public override int TurnAction()
        {
            // 出せるカードをランダムに出す
            var option = Option.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// 1枚ドローした状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もしない場合は -1 を返します。</returns>
        public override int TurnActionAfterDraw()
        {
            // 出せるカードをランダムに出す
            var option = Option.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// スート変更効果を持つカードを出すときに呼ばれます。
        /// 変更するスートを決定して返します。
        /// </summary>
        /// <returns>変更するスート。ただし、ジョーカー以外にしてください。</returns>
        public override SuitType SelectSuitAction()
        {
            // ランダムにスートを選ぶ
            SuitType suit;
            do
            {
                suit = Enum.GetValues(typeof(SuitType))
                    .Cast<SuitType>()
                    .OrderBy(x => random.Next())
                    .First();
            } while (suit == SuitType.Joker);
            return suit;
        }

        /// <summary>
        /// スキップが回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー効果中の場合は、ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します</returns>
        public override int EffectSkipAction(int drawNum)
        {
            // 出せるカードをランダムに出す
            var option = EffectAvoidableOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDrawAction(int drawNum)
        {
            // 出せるカードをランダムに出す
            var option = EffectAvoidableOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// 凶ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectQueenDrawAction(int drawNum)
        {
            // 出せるカードをランダムに出す
            var option = EffectAvoidableOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに出せる 5 のカードを持っていた場合に取る行動を決定して返します。
        /// </summary>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDiscloseActionDiscard()
        {
            // 出せるカードをランダムに出す
            var option = EffectAvoidableOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// 知る権利が回ってきたときに公開するカードを決定して返します。
        /// </summary>
        /// <returns>公開する手札のインデックス。</returns>
        public override int EffectDiscloseActionDisclose()
        {
            // カードをランダムに出す
            var option = DiscloseOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        /// <summary>
        /// 7渡しの効果発動時に取る行動を決定して返します。
        /// </summary>
        /// <returns>渡す手札のインデックス。何も渡さない場合は -1 を返します。</returns>
        public override int EffectGiveAction()
        {
            // カードをランダムに渡す
            var option = UnvalidatedOption.Select(x => x.Key).ToArray();
            return option.Length > 0 ? option[random.Next(option.Length)] : -1;
        }

        #endregion
    }
}
