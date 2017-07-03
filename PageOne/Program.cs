using System;
using System.Collections.Generic;

namespace PageOne
{
    class Program
    {
        /// <summary>
        /// エントリポイントです。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        static void Main(string[] args)
        {
            // 初期設定
            var names = Init();

            // ゲームの開始
            GameMaster.Instance.Start(names);
        }

        /// <summary>
        /// ゲームの初期設定です。
        /// </summary>
        /// <returns>設定されたプレイヤー名のリスト。</returns>
        static List<string> Init()
        {
            // プレイヤーの設定
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
            var names = new List<string>(playerNum);
            for (int i = 0; i < playerNum; i++)
            {
                Console.Write($"プレイヤー {i + 1} の名前を入力してください: ");
                names.Add(Console.ReadLine());
            }

            return names;
        }
    }
}
