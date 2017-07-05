namespace PageOne.Models.Cards
{
    /// <summary>
    /// 7のカードを表すクラスです。
    /// </summary>
    public class Card7 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card7(SuitType suit) : base(suit, 7) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
