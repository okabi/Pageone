namespace PageOne.Models.Cards
{
    /// <summary>
    /// 5のカードを表すクラスです。
    /// </summary>
    public class Card5 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card5(SuitType suit) : base(suit, 5) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
