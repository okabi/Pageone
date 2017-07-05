namespace PageOne.Models.Cards
{
    /// <summary>
    /// 13のカードを表すクラスです。
    /// </summary>
    public class Card13 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card13(SuitType suit) : base(suit, 13) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
