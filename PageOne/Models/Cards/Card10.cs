namespace PageOne.Models.Cards
{
    /// <summary>
    /// 10のカードを表すクラスです。
    /// </summary>
    public class Card10 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card10(SuitType suit) : base(suit, 10) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
