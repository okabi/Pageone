namespace PageOne.Models.Cards
{
    /// <summary>
    /// 4のカードを表すクラスです。
    /// </summary>
    public class Card4 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card4(SuitType suit) : base(suit, 4) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
