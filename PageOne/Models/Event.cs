namespace PageOne.Models
{
    /// <summary>
    /// ゲーム中に発生した公開情報を定義するクラスです。
    /// </summary>
    public class Event
    {
        #region 定数

        /// <summary>イベントの種類。</summary>
        public enum EventType
        {
            /// <summary>カードを山札から引く。</summary>
            Draw,
            /// <summary>カードを出す。</summary>
            Discard,
            /// <summary>カードを公開する(4の効果)。</summary>
            Disclose,
            /// <summary>カードを渡す(7の効果)。</summary>
            Give
        }

        #endregion

        #region フィールド

        /// <summary>イベントの種類。</summary>
        private readonly EventType type;
        
        /// <summary>イベントに関わったカード情報。非公開カードの場合は null になります。</summary>
        private readonly Card card;

        #endregion

        #region プロパティ

        /// <summary>イベントの種類。</summary>
        public EventType Type
        {
            get { return type; }
        }

        /// <summary>イベントに関わったカード情報。非公開カードの場合は null になります。</summary>
        public Card Card
        {
            get { return card; }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// イベントを作成します。
        /// </summary>
        /// <param name="type">イベントの種類。</param>
        /// <param name="card">イベントに関わったカード情報。非公開カードの場合は null を指定してください。</param>
        public Event(EventType type, Card card)
        {
            this.type = type;
            this.card = card;
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// イベントを表す文字列です。
        /// </summary>
        /// <returns>イベントを表す文字列。</returns>
        public override string ToString()
        {
            var ret =
                Type == EventType.Draw ? "引く" :
                Type == EventType.Discard ? "出す" :
                Type == EventType.Disclose ? "公開" :
                Type == EventType.Give ? "7渡し" :
                "不明(たぶんバグです)";
            ret += $" {Card}";
            return ret;
        }

        #endregion
    }
}
