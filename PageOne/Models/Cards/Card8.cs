namespace PageOne.Models.Cards
{
    /// <summary>
    /// 8のカードを表すクラスです。
    /// </summary>
    public class Card8 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card8(SuitType suit) : base(suit, 8) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
