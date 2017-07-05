namespace PageOne.Models.Cards
{
    /// <summary>
    /// Aのカードを表すクラスです。
    /// </summary>
    public class Card1 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card1(SuitType suit) : base(suit, 1) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
