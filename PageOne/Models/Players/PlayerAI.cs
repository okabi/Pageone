using System;
using System.Linq;
using System.Threading;
using PageOne.Singletons;
using static PageOne.Models.Card;

namespace PageOne.Models.Players
{
    /// <summary>
    /// コンちゃんです。
    /// </summary>
    public class PlayerAI : Player
    {
        #region フィールド

        /// <summary>コンソール表示にかけるウェイトミリ秒。</summary>
        private const int WaitTime = 1200;

        /// <summary>コンソール表示するか。</summary>
        private bool isDebug;

        /// <summary>常に手札を公開するか。</summary>
        private bool showStatus;

        /// <summary>乱数生成器。</summary>
        private Random random;

        #endregion

        #region 仮想プロパティ

        /// <summary>名前と手札の状態を表す文字列。</summary>
        public override string Status
        {
            get
            {
                if (showStatus)
                {
                    return base.Status;
                }
                else
                {
                    return $"{Name}: 手札合計 {UnvalidatedOption.Count} 枚、公開カード: " +
                        string.Join(" ", UnvalidatedOption.Where(x => x.Value.Opened).Select(x => x.Value));
                }
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンちゃんを作成します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        /// <param name="isDebug">コンソール表示するか。</param>
        /// <param name="showStatus">常に手札を公開するか。</param>
        public PlayerAI(string name, bool isDebug, bool showStatus) : base(name)
        {
            this.isDebug = isDebug;
            this.showStatus = showStatus;
            random = new Random();
        }

        #endregion

        #region public override メソッド

        /// <summary>
        /// ページワン(残り手札1枚)になったときに呼ばれるメソッドです。
        /// </summary>
        public override void PageOneAction()
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name}「ページワン」");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// 上がったときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="rank">順位。</param>
        public override void ClearAction(int rank)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {rank} 位で上がりました！");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを山札から引くときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="drawNum">山札から引く枚数。</param>
        public override void DrawAction(int drawNum)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {drawNum}枚ドローします...\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを出すときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">出すカード。</param>
        public override void DiscardAction(Card card)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {card} を出しました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを公開するときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">公開するカード。</param>
        public override void DiscloseAction(Card card)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {card} を公開しました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを渡すときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">渡すカード。</param>
        public override void GiveAction(Card card)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {card} を次のプレイヤーに渡します。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// カードを渡されるときに呼ばれるメソッドです。
        /// </summary>
        /// <param name="card">渡されるカード。</param>
        public override void ReceiveAction(Card card)
        {
            if (!isDebug) return;
            Console.WriteLine($"{Name} は {card} を前のプレイヤーから受け取りました。\n");
            Thread.Sleep(WaitTime);
        }

        /// <summary>
        /// 1枚ドローしていない状態で、このターンに出すカードを決定して返します。
        /// </summary>
        /// <returns>このターンに出す手札のインデックス。カードを引く場合は -1 を返します。</returns>
        public override int TurnAction()
        {
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }

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
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }

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
            var suitString =
                suit == SuitType.Spade ? "スペード" :
                suit == SuitType.Club ? "クローバー" :
                suit == SuitType.Diamond ? "ダイヤ" : "ハート";
            if (isDebug)
            {
                Console.WriteLine($"{Name} は {suitString} を宣言しました。\n");
                Thread.Sleep(WaitTime);
            }
            return suit;
        }

        /// <summary>
        /// スキップが回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー効果中の場合は、ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します</returns>
        public override int EffectSkipAction(int drawNum)
        {
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }
            if (EffectAvoidableOption.Count == 0)
            {
                if (isDebug)
                {
                    Console.WriteLine("スキップ効果を受けました。\n");
                    Thread.Sleep(WaitTime);
                }
                return -1;
            }

            // 出せるカードを出す
            return EffectAvoidableOption.First().Key;
        }

        /// <summary>
        /// ドロー効果が回ってきたときに取る行動を決定して返します。
        /// </summary>
        /// <param name="drawNum">ドロー枚数。</param>
        /// <returns>このターンに出す対抗可能な手札のインデックス。何もせず効果を受ける場合は -1 を返します。</returns>
        public override int EffectDrawAction(int drawNum)
        {
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }
            if (EffectAvoidableOption.Count == 0)
            {
                if (isDebug)
                {
                    Console.WriteLine($"ドロー{EffectManager.Instance.DrawNum}！\n");
                    Thread.Sleep(WaitTime);
                }
                return -1;
            }

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
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }
            if (EffectAvoidableOption.Count == 0)
            {
                if (isDebug)
                {
                    Console.WriteLine($"ドロー{EffectManager.Instance.DrawNum}！！\n");
                    Thread.Sleep(WaitTime);
                }
                return -1;
            }

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
            if (isDebug)
            {
                Console.Clear();
                Console.WriteLine(GameMaster.Instance.Status);
            }
            if (EffectAvoidableOption.Count == 0)
            {
                return -1;
            }

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
