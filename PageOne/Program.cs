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
        /// <summary>
        /// エントリポイントです。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        static void Main(string[] args)
        {
            try
            {
                // 初期設定
                var players = Init();

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
                Console.ReadKey();
            }
            catch (Exception e)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} - {e.Message}\n{e.StackTrace}\n");
            }
        }

        /// <summary>
        /// ゲームの初期設定です。
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
    }
}
