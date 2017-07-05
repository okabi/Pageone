namespace PageOne.Models.Cards
{
    /// <summary>
    /// 9のカードを表すクラスです。
    /// </summary>
    public class Card9 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card9(SuitType suit) : base(suit, 9) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
