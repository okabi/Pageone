using System.Collections.Generic;
using System.Linq;
using PageOne.Models;
using static PageOne.Models.Event;

namespace PageOne.Singletons
{
    /// <summary>
    /// プレイ履歴を管理するシングルトンです。
    /// </summary>
    public sealed class HistoryManager
    {
        #region シングルトンパターン

        private static HistoryManager instance = new HistoryManager();

        public static HistoryManager Instance
        {
            get { return instance; }
        }

        private HistoryManager() { }

        #endregion

        #region フィールド

        /// <summary>
        /// 各ターンの公開情報。
        /// プレイヤーのインデックス(最初の捨て札については -1)と
        /// そのプレイヤーが取った公開行動のリストがターン毎に記録されています。
        /// </summary>
        private List<KeyValuePair<int, List<Event>>> history;

        /// <summary>現在ターンでのターンプレイヤーの公開行動のリスト。</summary>
        private List<Event> turnHistory;

        #endregion

        #region プロパティ

        /// <summary>
        /// 各ターンの公開情報。
        /// プレイヤーのインデックス(最初の捨て札については -1)と
        /// そのプレイヤーが取った公開行動のリストがターン毎に記録されています。
        /// </summary>
        public List<KeyValuePair<int, List<Event>>> History
        {
            get
            {
                // 履歴を書き換えられたくないのでコピーを渡す
                var h = new List<KeyValuePair<int, List<Event>>>();
                foreach (var e in history)
                {
                    h.Add(new KeyValuePair<int, List<Event>>(
                        e.Key,
                        new List<Event>(e.Value.Select(x => new Event(x)))));
                }
                return h;
            }
        }

        /// <summary>
        /// このターンに出されたカード。
        /// カードが出されていなければ null を返します。
        /// </summary>
        public Card TurnDiscard
        {
            get
            {
                var discards = turnHistory.Where(x => x.Type == EventType.Discard).ToArray();
                return discards.Length > 0 ? new Card(discards[0].Card) : null;
            }
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// すべての履歴を文字列にして返します。
        /// </summary>
        /// <returns>すべての履歴を表した文字列。</returns>
        public override string ToString()
        {
            var ret = "";
            foreach (var h in history)
            {
                ret += $"プレイヤー {h.Key} の行動\n {string.Join("\n ", h.Value)}\n";
            }
            return ret;
        }
        
        /// <summary>
        /// 履歴を初期化します。
        /// </summary>
        public void Init()
        {
            history = new List<KeyValuePair<int, List<Event>>>();
            turnHistory = new List<Event>();
        }

        /// <summary>
        /// 現在ターンのプレイヤー行動を記録します。
        /// ターン終了時に呼んでください。
        /// </summary>
        /// <param name="playerIndex">現在ターンのインデックス。</param>
        public void Next(int playerIndex)
        {
            history.Add(new KeyValuePair<int, List<Event>>(playerIndex, new List<Event>(turnHistory)));
            turnHistory = new List<Event>();
        }

        /// <summary>
        /// 現在ターンにイベントを追加します。
        /// </summary>
        /// <param name="type">イベントの種類。</param>
        /// <param name="card">イベントに関わったカード。</param>
        public void Add(EventType type, Card card)
        {
            turnHistory.Add(new Event(type, card == null ? null : new Card(card)));
        }

        #endregion
    }
}
