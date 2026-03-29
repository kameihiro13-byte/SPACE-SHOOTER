# SPACE SHOOTER

Unityで制作した、2D縦スクロール弾幕シューティングゲームです。
大量の敵と弾幕が飛び交う中での「パフォーマンスの最適化」と、タイトル〜ボス撃破〜次ステージへの「ゲームループの確実な進行管理」を意識して開発しました。

## 🎮 プレイ動画
【ここに先ほどのプレイ動画（MP4）をドラッグ＆ドロップしてください】

## 🌐 ブラウザで遊ぶ (WebGL)
以下のリンクから、ブラウザ上で直接プレイしていただけます。
👉 **[SPACE SHOOTER をプレイする](https://unityroom.com/games/my-shotting-game-2026)**

---

## 💻 技術的なアピールポイント

本プロジェクトでは、特に以下のアーキテクチャ設計とパフォーマンス・チューニングに力を入れています。
各見出しのリンクから、実際のソースコードをご覧いただけます。

### 1. Object Poolパターンによるメモリ管理とGC対策
動画内のような「自機の連射」や「敵の弾幕」によるガベージコレクション（GC）スパイクと処理落ちを防ぐため、`Instantiate` / `Destroy` を避け、`SetActive` を用いたオブジェクトプールを実装しました。
* **関連スクリプト:** [PlayerShooter.cs](https://github.com/kameihiro13-byte/SPACE-SHOOTER/blob/main/Assets/Scripts/Player/PlayerController.cs), [Bullet.cs](https://github.com/kameihiro13-byte/SPACE-SHOOTER/blob/main/Assets/Scripts/Player/Bullet.cs)

### 2. 列挙型(enum)を用いた、拡張性の高い状態管理
動画で取得している「ツインショット」などのアイテム種類や、敵のステータス管理において、マジックナンバー（0や1などの直接的な数値）を排除し、`enum` を採用しました。
これにより、プランナーがUnityのInspector上で直感的にアイテムを設定できる「ミスが起きにくい（チーム開発に強い）設計」にしています。
* **関連スクリプト:** [Item.cs](https://github.com/kameihiro13-byte/SPACE-SHOOTER/blob/main/Assets/Scripts/item/item.cs)

### 3. [単一責任の原則に基づくデータクラスの分離](【ここにGameData.csのURLを貼る】)
ボス撃破後の「STAGE CLEAR!!」から「STAGE 2」へ移行する際、ゲーム全体の進行状態やトータルスコアを管理する `GameData` と、ステージ単体のスコアを管理する `ScoreData` を分離して設計しています。
「1つのクラスは何でもできる神クラスにしない」というオブジェクト指向の基本原則を徹底しました。
* **関連スクリプト:** `GameData.cs`, `ScoreData.cs`

### 4. [演出の制御とエラーハンドリング](【ここにWaveManager系のURLを貼る】)
WARNING演出からボス戦への移行など、外部（Managerクラス）からパラメータを流し込む際、変数を直接 `public` で書き換えるのではなく、メソッドを経由して安全に値を渡すカプセル化を行っています。
また、エディタ上でのアタッチ忘れを検知して `Debug.LogError` で出力する、チーム開発向けのエラーハンドリングも実装しています。
* **関連スクリプト:** `Stage1WaveManager.cs` (※実際のファイル名に合わせてください)

---

## 🕹️ 操作方法
* **移動:** `W` `A` `S` `D` キー または 矢印キー
* **ショット:** `Space` キー（長押しで連射）

## 🛠 開発環境
* **Game Engine:** Unity 6
* **Language:** C#
