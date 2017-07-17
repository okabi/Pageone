using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PageOne.Models;
using PageOne.Models.Players;
using PageOne.Singletons;

namespace PageOne
{
    /// <summary>
    /// メインエントリポイントとなるクラスです。
    /// </summary>
    class Program
    {
        #region エントリポイント

        /// <summary>
        /// エントリポイントです。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        static void Main(string[] args)
        {
            try
            {
                // CUI でゲームをプレイする場合は Play() を利用します。
                // Play(Init);  // 全員操作プレイヤーの場合。
                // Play(Init2); // 全員コンちゃんの場合。
                // Play(Init3); // 1P操作プレイヤー、その他コンちゃんの場合。


                // AI 同士の対戦をシミュレートして結果だけ得る場合は Simulate() を利用します。
                Simulate(Init4, 10000, true);

                // 最後にキー入力を待って終了します。
                Console.ReadKey();
            }
            catch (Exception e)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} - {e.Message}{Environment.NewLine}{e.StackTrace}{Environment.NewLine}");
            }
        }

        #endregion

        #region 初期化メソッド

        /// <summary>
        /// 全員操作プレイヤーにする場合の、ゲームの初期設定です。
        /// </summary>
        /// <returns>設定されたプレイヤーのリスト。</returns>
        static List<Player> Init()
        {
            // プレイヤー人数の設定
            int playerNum;
            while (true)
            {
                Console.Write("プレイヤー人数を入力してください: ");
                try
                {
                    playerNum = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine("整数を入力してください。");
                    continue;
                }
                if (playerNum >= 2 && playerNum <= 8)
                {
                    break;
                }
                Console.Error.WriteLine("2 人 ～ 8 人を入力してください。");
            }

            // プレイヤー名の設定
            var names = new List<string>(playerNum);
            for (int i = 0; i < playerNum; i++)
            {
                Console.Write($"プレイヤー {i + 1} の名前を入力してください: ");
                names.Add(Console.ReadLine());
            }

            // プレイヤーインスタンスを生成して返す
            return names.Select(x => new PlayerHuman(x) as Player).ToList();
        }

        /// <summary>
        /// 全員コンちゃんにする場合の、ゲームの初期設定です。
        /// </summary>
        /// <returns>設定されたプレイヤーのリスト。</returns>
        static List<Player> Init2()
        {
            // プレイヤー人数の設定
            int playerNum;
            while (true)
            {
                Console.Write("コンちゃんの人数を入力してください: ");
                try
                {
                    playerNum = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine("整数を入力してください。");
                    continue;
                }
                if (playerNum >= 2 && playerNum <= 8)
                {
                    break;
                }
                Console.Error.WriteLine("2 人 ～ 8 人を入力してください。");
            }

            // プレイヤー名の設定
            var names = new List<string>(playerNum);
            for (int i = 0; i < playerNum; i++)
            {
                Console.Write($"コンちゃん {i + 1} の名前を入力してください: ");
                names.Add(Console.ReadLine());
            }

            // プレイヤーインスタンスを生成して返す
            return names.Select(x => new PlayerAI(x, true, true) as Player).ToList();
        }

        /// <summary>
        /// 1Pプレイヤー、その他コンちゃんにする場合の、ゲームの初期設定です。
        /// </summary>
        /// <returns>設定されたプレイヤーのリスト。</returns>
        static List<Player> Init3()
        {
            // プレイヤー人数の設定
            int playerNum;
            while (true)
            {
                Console.Write("コンちゃんの人数を入力してください: ");
                try
                {
                    playerNum = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Error.WriteLine("整数を入力してください。");
                    continue;
                }
                if (playerNum >= 2 && playerNum <= 7)
                {
                    break;
                }
                Console.Error.WriteLine("2 人 ～ 7 人を入力してください。");
            }

            // プレイヤー名の設定
            var names = new List<string>(playerNum + 1);
            Console.Write($"プレイヤーの名前を入力してください: ");
            names.Add(Console.ReadLine());
            for (int i = 0; i < playerNum; i++)
            {
                Console.Write($"コンちゃん {i + 1} の名前を入力してください: ");
                names.Add(Console.ReadLine());
            }

            // プレイヤーインスタンスを生成して返す
            return names.Select((x, i) => i == 0 ? new PlayerHuman(x) : new PlayerAI(x, true, false) as Player).ToList();
        }

        /// <summary>
        /// 4人全員コンちゃんで名前も自動設定する、ゲームの初期設定です。
        /// </summary>
        /// <returns>設定されたプレイヤーのリスト。</returns>
        static List<Player> Init4()
        {
            var names = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                names.Add($"COM{i + 1}");
            }
            return names.Select(x => new PlayerAI(x, false, false) as Player).ToList();
        }

        #endregion

        #region メインメソッド

        /// <summary>
        /// CUI でゲームをプレイします。
        /// </summary>
        /// <param name="init">参加プレイヤーのリストを返す初期化関数。</param>
        static void Play(Func<List<Player>> init)
        {
            // 初期設定
            var players = init();

            // ゲームの開始
            var ranking = GameMaster.Instance.Run(players);

            // 結果の表示
            Console.Clear();
            Console.WriteLine(GameMaster.Instance.Status);
            Console.WriteLine("結果発表");
            foreach (var r in ranking)
            {
                Console.WriteLine($"{r.Value}位: {r.Key}");
            }
        }

        /// <summary>
        /// AI 同士の対戦をシミュレートして結果を表示します。
        /// </summary>
        /// <param name="init">参加プレイヤーのリストを返す初期化関数。</param>
        /// <param name="matchNum">シミュレートする試合回数。</param>
        /// <param name="shuffle">1試合ごとに順番をシャッフルするか。</param>
        static void Simulate(Func<List<Player>> init, int matchNum, bool shuffle)
        {
            var random = new Random();
            var result = new Dictionary<string, List<int>>();
            int beforeProgress = 0;

            for (int i = 0; i < matchNum; i++)
            {
                // 進捗を表示
                int progress = 100 * (i + 1) / matchNum;
                if (progress > beforeProgress)
                {
                    beforeProgress = progress;
                    Console.Clear();
                    var str = string.Format("シミュレート中...\n【{0}{1}】{2}%",
                        new string('*', progress / 2), new string('-', 50 - progress / 2), progress);
                    Console.WriteLine(str);
                }

                // プレイヤーリストを取得する
                var players = init();

                // 初回のみ result を初期化
                if (i == 0)
                {
                    foreach (var p in players)
                    {
                        result[p.Name] = new List<int>();
                    }
                }

                // 必要があればシャッフル
                if (shuffle) players = players.OrderBy(x => random.Next()).ToList();

                // ゲームを実施
                var ranking = GameMaster.Instance.Run(players);

                // 結果を記録
                foreach (var r in ranking)
                {
                    if (r.Value > players.Count)
                    {
                        throw new Exception(string.Join(", ", ranking.Select(x => $"{x.Key}: {x.Value}")));
                    }
                    result[r.Key].Add(r.Value);
                }
            }

            // シミュレート結果を表示
            foreach (var p in result.Keys)
            {
                var str = string.Format("{0}:\n\t平均順位: {1:F3} 位", p, result[p].Average());
                for (int i = 0; i < result[p].Max(); i++)
                {
                    var c = result[p].Where(x => x == i + 1).Count();
                    str += string.Format("\n\t{0}位率: {1:F3}% ({2} 回)", i + 1, 100f * c / result[p].Count, c);
                }
                Console.WriteLine(str);
            }
        }

        #endregion
    }
}
