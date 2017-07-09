using System;
using PageOne.Models;

namespace PageOne.Singletons
{
    /// <summary>
    /// 発生中のカード効果を管理するシングルトンです。
    /// </summary>
    public sealed class EffectManager
    {
        #region シングルトンパターン

        private static EffectManager instance = new EffectManager();

        public static EffectManager Instance
        {
            get { return instance; }
        }

        private EffectManager()
        {
            Init();
            Reversing = false;
        }

        #endregion

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

        /// <summary>リバース発動中か。</summary>
        public bool Reversing { get; private set; }

        /// <summary>カード効果の発生状況。</summary>
        public string Status
        {
            get
            {
                var ret = Reversing ? "リバース発動中\n" : "";
                ret +=
                    Type == EffectType.Skip ? "スキップ効果発動中" :
                    Type == EffectType.Draw ? "ドロー効果発生中！" :
                    Type == EffectType.Disclose ? "知る権利発動" :
                    Type == EffectType.Lock ? "ロック発動中" :
                    Type == EffectType.Give ? "7渡し発動" :
                    Type == EffectType.ChainOfHate ? "憎しみの連鎖発動" :
                    Type == EffectType.Quick ? "連続行動発動！" :
                    Type == EffectType.QueenDraw ? "凶ドロー効果発生中！！" : "";
                ret += DrawNum > 0 ? $"  ドロー枚数: {DrawNum}" : "";
                return ret;
            }
        }

        #endregion

        #region public メソッド

        /// <summary>
        /// カード効果の処理後に呼ぶべきメソッドです。
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
        /// 出されたカードに応じてカード効果を更新します。
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
                case 3:
                    Reversing = !Reversing;
                    break;
                case 4:
                    Type = EffectType.Disclose;
                    break;
                case 6:
                    Type = EffectType.Lock;
                    break;
                case 7:
                    if (giftCard == null)
                    {
                        Init();
                    }
                    else
                    {
                        Type = EffectType.Give;
                        GiftCard = giftCard;
                    }
                    break;
                case 9:
                    Type = EffectType.ChainOfHate;
                    DrawNum = 1;
                    break;
                case 11:
                    Type = EffectType.Quick;
                    break;
                case 12:
                    if (card.DeclaredSuit == Card.SuitType.Spade)
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
                        (card.DeclaredSuit == Card.SuitType.Spade && card.Number == 12) || card.Number == 13;
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
        /// リバース以外の状態を初期化します。
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
