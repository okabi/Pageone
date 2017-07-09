using System;
using System.Collections.Generic;
using System.Linq;

namespace PageOne
{
    /// <summary>
    /// 便利関数を集めた static クラスです。
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 説明を表示した上で数字入力を待ち、入力された数値を返します。
        /// </summary>
        /// <param name="description">表示する説明。</param>
        /// <param name="options">選択肢の数値と説明。</param>
        /// <param name="defaultNumber">空白が入力された場合のデフォルト入力数値。</param>
        /// <returns>入力された数値。</returns>
        public static int ReadNumber(string description, Dictionary<int, string> options, int? defaultNumber = null)
        {
            while (true)
            {
                Console.Write($"{description}\n" +
                    $"{string.Join(" ", options.Select(x => $"[{x.Key}]{x.Value}"))}\n" +
                    $"> ");
                try
                {
                    var input = Console.ReadLine();
                    if (input == "" && defaultNumber != null)
                    {
                        input = defaultNumber.ToString();
                    }

                    int number = int.Parse(input);
                    if (!options.Keys.Contains(number))
                    {
                        throw new Exception();
                    }
                    Console.Clear();
                    return number;
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("入力が不正です。\n");
                    continue;
                }
            }
        }
    }
}
