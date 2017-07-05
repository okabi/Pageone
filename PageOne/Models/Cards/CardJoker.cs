namespace PageOne.Models.Cards
{
    /// <summary>
    /// ジョーカーを表すクラスです。
    /// </summary>
    public class CardJoker : Card
    {
        #region コンストラクタ

        /// <summary>
        /// カードを作成します。
        /// </summary>
        public CardJoker() : base(SuitType.Joker, null) { }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果を発動します。
        /// </summary>
        public override void Effect() { }

        #endregion
    }
}
