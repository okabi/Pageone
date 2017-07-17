# Page One

## 環境

- .NET Framework v4.6.1

## 遊び方

- `Product/PageOne.exe` を起動します。

----

# AI の作り方

## 推奨エディタ

Visual Studio をおすすめします。
`PageOne.sln` からソリューションを開くことができます。

## オリジナル AI の作り方概要

- `Models.Players` 名前空間にオリジナル AI を作成します。
- `Program` の `Main` メソッドの内容を書き換えて、オリジナル AI を適用します。
- 手動で対戦するなり、シミュレーションを回すなりしてオリジナル AI の強さを評価します。

改造する必要があるのは、「新しく作成するオリジナル AI 用クラスファイル」と「Program」ファイルのみです。
ゲームロジック部分を触る必要はありません。

## AI を作成する

`Models.Players.PlayerAITemplate` を参考にしてください。

AI は `Player` クラスを継承する必要があります。
`Player` クラスおよびその派生クラスは、ゲーム中に選択肢を与えられたときのプレイヤーの思考を定義します。
このクラス自身は状態を持っていないことに注意してください。

`Player` の派生クラスが参照できる（すなわち、AI 作成に利用できる）プロパティは、以下の通りです。
有用なもののみリストアップしています。

### `Player` クラスに属するプロパティ
#### `Name` (`string`)

プレイヤー名です。

#### `Cards` (`List<Card>`)

手札にあるカードのリストの深いコピーです。

#### `Option` (`Dictionary<int, Card>`)

場に出すことのできる、手札の選択肢を表す辞書です。

#### `UnvalidatedOption` (`Dictionary<int, Card>`)

全手札の選択肢を表す辞書です。

#### `EffectAvoidableOption` (`Dictionary<int, Card>`)

カード効果を受けているときに利用します。
現在受けているカード効果を防ぐことのできる手札の選択肢を表す辞書です。

#### `DiscloseOption` (`Dictionary<int, Card>`)

「知る権利」のカード効果を受けているときに利用します。
公開することのできる手札の選択肢を表す辞書です。

#### `Status` (`string`)

仮想プロパティなので派生クラスで再定義可能です。
名前と手札の状態を表す文字列です。

### `GameMaster` シングルトンに属するプロパティ

#### `DeckCount` (`int`)

山札の枚数です。

#### `TopOfGrave` (`Card`)

捨て札の一番上のカードです。

#### `Status` (`string`)

ゲーム状態を表す文字列です。

#### `Cards` (`Dictionary<string, List<Card>>`)

プレイヤーの名前をキーとした、各プレイヤーの手札の公開情報です。
非公開カードは `null` として格納されます。

#### `Status` (`string`)

ゲーム状態を表す文字列です。

### `EffectManager` シングルトンに属するプロパティ

#### `Reversing` (`bool`)

リバース効果発動中かを返します。

### `HistoryManager` シングルトンに属するプロパティ

#### `History` (`List<KeyValuePair<int, List<Event>>>`)

各ターンの公開情報です。
プレイヤーのインデックス(最初の捨て札については -1)と、そのプレイヤーが取った公開行動のリストがターン毎に記録されています。
