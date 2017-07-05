namespace PageOne.Models.Cards
{
    /// <summary>
    /// 12のカードを表すクラスです。
    /// </summary>
    public class Card12 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card12(SuitType suit) : base(suit, 12) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
