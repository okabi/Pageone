namespace PageOne.Models.Cards
{
    /// <summary>
    /// 3のカードを表すクラスです。
    /// </summary>
    public class Card3 : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        /// <param name="suit">スート。</param>
        public Card3(SuitType suit) : base(suit, 3) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
