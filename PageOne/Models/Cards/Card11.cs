namespace PageOne.Models.Cards
{
    /// <summary>
    /// 11のカードを表すクラスです。
    /// </summary>
    public class Card11 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card11(SuitType suit) : base(suit, 11) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
