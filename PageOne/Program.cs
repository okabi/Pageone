using System;
using System.Collections.Generic;
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
            // 初期設定
            var players = Init();

            // ゲームの開始
            GameMaster.Instance.Run(players);
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
