namespace PageOne.Models.Cards
{
    /// <summary>
    /// 6のカードを表すクラスです。
    /// </summary>
    public class Card6 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card6(SuitType suit) : base(suit, 6) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
