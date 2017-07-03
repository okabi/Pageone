using PageOne.Models;

namespace PageOne.Interfaces
{
    /// <summary>
    /// 一人のプレイヤーを定義するインターフェイスです。
    /// </summary>
    public interface IPlayer
    {
        /// <summary>プレイヤー名。</summary>
        string Name { get; }

        /// <summary>名前と手札の状態を表す文字列。</summary>
        string Status { get; }

        /// <summary>
        /// 手札を加えます。
        /// </summary>
        /// <param name="card">追加する手札。</param>
        void AddCard(Card card);

        /// <summary>
        /// 手札を捨てます。
        /// </summary>
        /// <param name="index">捨てる手札のインデックス。</param>
        void RemoveCard(int index);

        /// <summary>
        /// 自分のターンを処理し、次のターンに移します。
        /// </summary>
        void Next();
    }
}
