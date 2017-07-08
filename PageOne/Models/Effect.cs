using System;

namespace PageOne.Models
{
    /// <summary>
    /// 発生中のカード効果を管理するクラスです。
    /// </summary>
    public sealed class Effect
    {
        #region 定数

        /// <summary>カード効果の種類。</summary>
        public enum EffectType
        {
            /// <summary>何も発生していない。</summary>
            None,
            /// <summary>スキップ(A)。</summary>
            Skip,
            /// <summary>ドロー効果(2)。</summary>
            Draw,
            /// <summary>凶ドロー効果(スペード Q が出されて以降)。</summary>
            QueenDraw,
            /// <summary>知る権利(4)。</summary>
            Disclose,
            /// <summary>ロック(6)。</summary>
            Lock,
            /// <summary>7渡し(7)。</summary>
            Give,
            /// <summary>憎しみの連鎖(9)。</summary>
            ChainOfHate,
            /// <summary>連続行動(J)。</summary>
            Quick
        }

        #endregion

        #region プロパティ

        /// <summary>現在発生しているカード効果の種類。</summary>
        public EffectType Type { get; private set; }

        /// <summary>現在発生しているカード効果でのドロー枚数。</summary>
        public int DrawNum { get; private set; }

        /// <summary>現在発生しているカード効果での2のカードが出された枚数。</summary>
        public int CardTwoNum { get; private set; }

        /// <summary>7渡しで次のプレイヤーに渡されるカード。</summary>
        public Card GiftCard { get; private set; }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public Effect()
        {
            Init();
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果発動後に呼ぶべきメソッドです。
        /// </summary>
        public void Reset()
        {
            if (Type == EffectType.Skip)
            {
                Type = DrawNum > 0 ? EffectType.Draw : EffectType.None;
            }
            else
            {
                Init();
            }
        }

        /// <summary>
        /// カード効果を更新します。
        /// </summary>
        /// <param name="card">更新判定を行うカード。</param>
        /// <param name="giftCard">7渡しで次のプレイヤーに渡されるカード。</param>
        public void Update(Card card, Card giftCard = null)
        {
            switch (card.Number)
            {
                case 1:
                    Type = EffectType.Skip;
                    break;
                case 2:
                    Type = EffectType.Draw;
                    DrawNum += (int)Math.Pow(2, CardTwoNum == 0 ? 1 : CardTwoNum);
                    CardTwoNum += 1;
                    break;
                case 4:
                    Type = EffectType.Disclose;
                    break;
                case 6:
                    Type = EffectType.Lock;
                    break;
                case 7:
                    Type = EffectType.Give;
                    GiftCard = giftCard;
                    break;
                case 9:
                    Type = EffectType.ChainOfHate;
                    break;
                case 11:
                    Type = EffectType.Quick;
                    break;
                case 12:
                    if (card.declaredSuit == Card.SuitType.Spade)
                    {
                        DrawNum += 5;
                        Type = EffectType.QueenDraw;
                    }
                    Type = Type == EffectType.QueenDraw ? EffectType.QueenDraw : EffectType.None;
                    break;
                case 13:
                    if (Type == EffectType.Draw || Type == EffectType.QueenDraw)
                    {
                        DrawNum += 1;
                    }
                    break;
                default:
                    Init();
                    break;
            }
        }

        /// <summary>
        /// 指定したカードを出すことでカード効果を回避できるかを返します。
        /// </summary>
        /// <param name="card">出すカード。</param>
        /// <returns>カード効果を回避できるか。</returns>
        public bool Avoidable(Card card)
        {
            var ret = false;
            switch (Type)
            {
                case EffectType.Skip:
                    ret = card.Number == 10;
                    break;
                case EffectType.Draw:
                    ret = card.Suit == Card.SuitType.Joker || card.Number == 1 || card.Number == 2 || card.Number == 3 ||
                        (card.declaredSuit == Card.SuitType.Spade && card.Number == 12) || card.Number == 13;
                    break;
                case EffectType.QueenDraw:
                    ret = card.Suit == Card.SuitType.Joker || card.Number == 12 || card.Number == 13;
                    break;
                case EffectType.Disclose:
                    ret = card.Number == 5;
                    break;
            }
            return ret;
        }

        #endregion

        #region private メソッド

        /// <summary>
        /// 状態を初期化します。
        /// </summary>
        private void Init()
        {
            Type = EffectType.None;
            DrawNum = 0;
            CardTwoNum = 0;
            GiftCard = null;
        }

        #endregion
    }
}
