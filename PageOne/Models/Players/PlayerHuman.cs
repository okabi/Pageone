using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PageOne.Singletons;
using static PageOne.Models.Card;

namespace PageOne.Models.Players
{
    /// <summary>
    /// 操作するプレイヤーです。
    /// </summary>
    public class PlayerHuman : Player
    {
        #region フィールド

        /// <summary>コンソール表示にかけるウェイトミリ秒。</summary>
        private const int WaitTime = 800;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// 操作するプレイヤーを作成します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        public PlayerHuman(string name) : base(name) { }

        #endregion

        #region public override メソッド

        /// <summary>
        /// ページワン(残り手札1枚)になったときに呼ばれるメソッドです。
        /// </summary>
        public override void PageOneAction()
        {
            Console.WriteLine($"{Name}「ページワン」");
            Console.ReadKey();
        }

        /// <summary>
        /// 上がったときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="rank">順位。</param>
        public override void ClearAction(int rank)
        {
            Console.WriteLine($"{Name} は {rank} 位で上がりました！");
            Console.ReadKey();
        }

        /// <summary>
        /// カードを山札から引くときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="drawNum">山札から引く枚数。</param>
        public override void DrawAction(int drawNum)
        {
            Console.WriteLine($"{Name} は {drawNum}枚ドローします...\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを出すときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">出すカード。</param>
        public override void DiscardAction(Card card)
        {
            Console.WriteLine($"{Name} は {card} を出しました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを公開するときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">公開するカード。</param>
        public override void DiscloseAction(Card card)
        {
            Console.WriteLine($"{Name} は {card} を公開しました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを渡すときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">渡すカード。</param>
        public override void GiveAction(Card card)
        {
            Console.WriteLine($"{Name} は {card} を次のプレイヤーに渡します。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを渡されるときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">渡されるカード。</param>
        public override void ReceiveAction(Card card)
        {
            Console.WriteLine($"{Name} は {card} を前のプレイヤーから受け取りました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// 1枚ドローしていない状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。カードを引く場合は -1 を返します。</returns>
        public override int TurnAction()
        {
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            return SelectCard(
                Option,
                "出すカード または その他の行動を選択してください。",
                "カードを引く");
        }

        /// <summary>
        /// 1枚ドローした状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。何もしない場合は -1 を返します。</returns>
        public override int TurnActionAfterDraw()
        {
            Console.WriteLine(GameMaster.Instance.Status);
            return SelectCard(
                Option,
                "出すカード または その他の行動を選択してください。",
                "パスする");
        }

        /// <summary>
        /// スート変更効果を持つカードを出すときに呼ばれます。
        /// 変更するスートを決定して返します。
        /// </summary>
        /// <returns>変更するスート。ただし、ジョーカー以外にしてください。</returns>
        public override SuitType SelectSuitAction()
        {
            var suit = SelectDeclaredSuit();
            var suitString =
                suit == SuitType.Spade ? "スペード" :
                suit == SuitType.Club ? "クローバー" :
                suit == SuitType.Diamond ? "ダイヤ" : "ハート";
            Console.WriteLine($"{Name} は {suitString} を宣言しました。\n");
            Thread.Sleep(WaitTime);
            return suit;
        }

        /// <summary>
        /// スキップが回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー効果中の場合は、ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します</returns>
        public override int EffectSkipAction(int drawNum)
        {
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            var avoidableCards = Option
                .Where(x => EffectManager.Instance.Avoidable(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);
            if (avoidableCards.Count == 0)
            {
                Console.WriteLine("スキップ効果を受けました。\n");
                Console.ReadKey();
                return -1;
            }
            return SelectCard(
                avoidableCards,
                "スキップ効果を受けました。\n出すカード または その他の行動を選択してください。",
                "スキップ効果を受ける");
        }

        /// <summary>
        /// ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDrawAction(int drawNum)
        {
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            var avoidableCards = Option
                .Where(x => EffectManager.Instance.Avoidable(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);
            if (avoidableCards.Count == 0)
            {
                Console.WriteLine($"ドロー{EffectManager.Instance.DrawNum}！\n");
                Console.ReadKey();
                return -1;
            }
            return SelectCard(
                avoidableCards,
                $"ドロー効果({EffectManager.Instance.DrawNum}枚)を受けました。\n出すカード または その他の行動を選択してください。",
                "ドロー効果を受ける");
        }

        /// <summary>
        /// 凶ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectQueenDrawAction(int drawNum)
        {
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            var avoidableCards = Option
                .Where(x => EffectManager.Instance.Avoidable(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);
            if (avoidableCards.Count == 0)
            {
                Console.WriteLine($"ドロー{EffectManager.Instance.DrawNum}！！\n");
                Console.ReadKey();
                return -1;
            }
            return SelectCard(
                avoidableCards,
                $"凶ドロー効果({EffectManager.Instance.DrawNum}枚)を受けました。\n出すカード または その他の行動を選択してください。",
                "凶ドロー効果を受ける");
        }

        /// <summary>
        /// 知る権利が回ってきたときに出せる 5 のカードを持っていた場合に取る行動を決定して返します。
        /// </summary>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDiscloseActionDiscard()
        {
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            var avoidableCards = Option
                .Where(x => EffectManager.Instance.Avoidable(x.Value))
                .ToDictionary(x => x.Key, x => x.Value);
            if (avoidableCards.Count == 0)
            {
                return -1;
            }
            return SelectCard(
                avoidableCards,
                "知る権利を要求されました。\n出すカード または その他の行動を選択してください。",
                "情報を開示する");
        }

        /// <summary>
        /// 知る権利が回ってきたときに公開するカードを決定して返します。
        /// </summary>
        /// <returns>公開する手札のインデックス。</returns>
        public override int EffectDiscloseActionDisclose()
        {
            var disclosedCards = UnvalidatedOption
                .Where(x => !x.Value.Opened)
                .ToDictionary(x => x.Key, x => x.Value);
            return SelectCard(
                disclosedCards,
                "知る権利を要求されました。\n公開する手札を選択してください。");
        }

        /// <summary>
        /// 7渡しの効果発動時に取る行動を決定して返します。
        /// </summary>
        /// <returns>渡す手札のインデックス。何も渡さない場合は -1 を返します。</returns>
        public override int EffectGiveAction()
        {
            return SelectCard(
                UnvalidatedOption,
                "7渡しが発動しました。\n渡すカード または その他の行動を選択してください。",
                "渡さない");
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// プレイヤーに出すカードを決定させます。
        /// </summary>
        /// <param name="option">カードの選択肢。</param>
        /// <param name="description">表示する説明。</param>
        /// <param name="noAction">「カードを引く」または「何もしない」に対して表示する文字列。null なら選択肢を表示しません。</param>
        /// <returns>このターンに出す手札のインデックス。カードを引く or 何もしない場合は -1 を返します。</returns>
        private int SelectCard(Dictionary<int, Card> option, string description, string noAction = null)
        {
            while (true)
            {
                var optionString = new Dictionary<int, string>(
                    option.ToDictionary(x => x.Key + 1, x => x.Value.ToString()));
                if (noAction != null)
                {
                    optionString.Add(88, noAction);
                }
                optionString.Add(99, "ヘルプ");
                int input = Utility.ReadNumber(description, optionString);

                if (input == 88)
                {
                    return -1;
                }
                else if (input == 99)
                {
                    Help.Top(UnvalidatedOption);
                }
                else
                {
                    return input - 1;
                }
            }
        }

        /// <summary>
        /// プレイヤーに、出したカードのスートを決定させます。
        /// </summary>
        /// <returns>決定したスート。</returns>
        private SuitType SelectDeclaredSuit()
        {
            while (true)
            {
                var option = new Dictionary<int, string>()
                {
                    { 1, "スペード" },
                    { 2, "クローバー" },
                    { 3, "ダイヤ" },
                    { 4, "ハート" }
                };
                int input = Utility.ReadNumber($"宣言スートを選択してください。", option);
                return
                    input == 1 ? SuitType.Spade :
                    input == 2 ? SuitType.Club :
                    input == 3 ? SuitType.Diamond : SuitType.Heart;
            }
        }

        #endregion
    }
}
