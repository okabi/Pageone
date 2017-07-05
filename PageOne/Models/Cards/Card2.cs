namespace PageOne.Models.Cards
{
    /// <summary>
    /// 2のカードを表すクラスです。
    /// </summary>
    public class Card2 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card2(SuitType suit) : base(suit, 2) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
