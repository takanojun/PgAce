
# PgAce – ポータブル PostgreSQL デスクトップクライアント  
*Codex による自動実装向け仕様書（日本語版）*

---

## 0. 目次
1. 概要と目的  
2. 対象環境  
3. 機能一覧（概要）  
4. UI / UX 要件  
5. 機能要件（詳細）  
6. 非機能要件  
7. ビルド・配布  
8. マイルストーンとブランチ構成  
9. 受け入れ基準  
10. 付録 – 参照 SQL

---

## 1. 概要と目的
PgAce は **単一フォルダ / 単一 EXE** で動作する Windows 専用の PostgreSQL クライアントです。  
主な設計目標:

| ID | 目標 |
|----|------|
| G1 | Zip を展開して実行するだけ。インストール不要。 |
| G2 | CSE の操作感：Host/IP + DB 名 + 資格情報で即接続。 |
| G3 | ドッキング可能なモダン UI、キーボード中心操作。 |
| G4 | 機密情報は DPAPI (CurrentUser) で暗号化保存。 |
| G5 | モジュール分割で将来拡張（Graphviz プラン表示など）が容易。 |

---

## 2. 対象環境
| 項目 | 値 |
|------|----|
| OS | Windows 10 / 11 (x64) |
| ランタイム | **.NET 8**（自己完結シングルファイル EXE） |
| DB サーバー | PostgreSQL 12 以上（通信は **Npgsql 8.x** バイナリプロトコル） |
| UI スタック | WinForms + WeifenLuo DockPanelSuite + ScintillaNET + Fluent System Icons |
| ライセンス | PgAce 本体は MIT（依存ライブラリのライセンスを遵守） |

---

## 3. 機能一覧（概要）
| # | 機能 | リリース |
|---|------|----------|
| F1 | 接続ダイアログ（DPAPI パスワード保存） | v1.0 |
| F2 | SQL エディタ（ハイライト、折りたたみ、Ctrl+L 整形、F5 実行） | v1.0 |
| F3 | 結果グリッド（コピー、CSV/Excel 出力） | v1.0 |
| F4 | **DB エクスプローラ**（ツリー + プロパティパネル） | v1.1 |
| F5 | **SQL エクスプローラ**（フォルダツリー → タブ編集） | v1.2 |
| F6 | INSERT 文生成（列名あり／なし／COPY 形式） | v1.3 |
| F7 | EXPLAIN ビューア（テキスト → JSON ツリー → グラフ） | v1.0 / 1.4 |
| F8 | DDL エクスポート（SQL / Markdown） | v1.1 |
| F9 | 実行履歴（最大 500 件、ローカル保存） | v1.0 |
| F10| レイアウトカスタマイズ（ドッキング） | v1.0 |

---

## 4. UI / UX 要件

### 4.1 デフォルトレイアウト
```
+---------------- PgAce -----------------------------------+
| DB Explorer |  SQL Editor Tabs          | SQL Explorer   |
|             +---------------------------+----------------|
|             |  Result Grid              |Properties Panel|
+----------------------------------------------------------+
```
* 各ペインは DockPanelSuite により **ドック／フロート／タブ** が可能  
* レイアウト変更は `layout.json` に自動保存

### 4.2 ショートカット
| 操作 | キー |
|------|------|
| SQL 実行（選択／全体） | **F5** |
| SQL 整形 ⇔ 一行化 | **Ctrl + L** |
| 履歴パネル表示 | **Ctrl + H** |
| INSERT 生成メニュー | グリッド右クリック |

---

## 5. 機能要件（詳細）

### 5.1 接続 & セキュリティ
* 入力項目: **Host / Port / Database / User / Password**  
* 「パスワードを保存」チェック → `ProtectedData.Protect(..., CurrentUser)` で暗号化  
* 復号に失敗（PC 変更・OS 再セットアップなど）した場合は再入力を要求

### 5.2 DB エクスプローラ (v1.1)
* 表示対象: スキーマ ▸ テーブル ▸ ビュー ▸ マテビュー ▸ インデックス ▸ シーケンス ▸ 関数 ▸ 型
* **遅延ロード（Lazy Loading）**  
  1. 接続時に `COUNT(*)` で件数のみ取得  
  2. 閾値 **N=1000 (既定)** を超える場合、ノードに「► (N 件, 未ロード)」ラベル  
  3. ノード展開でダイアログ  
     * 「最初のチャンクをロード / 全件ロード / キャンセル」  
     * チャンクロードを選択した場合、**チャンクサイズ**も入力 (既定 500)  
  4. 追加展開で次チャンクを非同期取得（無限スクロール方式）

### 5.3 プロパティパネル
* ノード種別ごとに内容を切替  
  * **テーブル**: 列名・型・Nullable・PK/FK・コメント・行数推定  
  * **インデックス**: 列、Unique、条件、サイズ  
  * **関数**: 引数、戻り値、ソース  
* ボタン: **[DDL] [Export]**

### 5.4 SQL エクスプローラ (v1.2)
* メニュー *File → Open Folder…* でルートディレクトリを設定  
* `.sql` ファイルをツリー表示（`FileSystemWatcher` で変更監視）  
* ダブルクリックでエディタタブに読み込み、外部変更時は再読み込みを促す

### 5.5 INSERT 文生成 (v1.3)
| モード | 生成例 |
|--------|--------|
| 列名あり | `INSERT INTO tbl (col1, col2) VALUES (1, 'A');` |
| 列名なし | `INSERT INTO tbl VALUES (1, 'A');` |
| COPY 形式 | ```COPY tbl (col1,col2) FROM STDIN; \n1\tA\n\.\n``` |

* 生成結果は **新規タブ**に表示（コピー＆実行可）

### 5.6 DDL エクスポート (v1.1)
* プロパティパネルの **[Export]** → フォーマット選択 (SQL / Markdown)  
* `pg_get_tabledef()` 等で完全 DDL を取得  
* Markdown テンプレート例  
  ```markdown
  ### public.mst_patient
  ```sql
  CREATE TABLE public.mst_patient (
      ...
  );
  ```
  ```

---

## 6. 非機能要件
| 区分 | 要件 |
|------|------|
| パフォーマンス | コールドスタート < 1 秒 |
| メモリ | アイドル時 < 250 MB |
| ロギング | `history.log`（パスワードを自動マスク） |
| アクセシビリティ | 文字列は i18n 対応（既定 英語） |
| ライセンス | MIT + 依存ライブラリライセンス順守 |

---

## 7. ビルド・配布
```bash
dotnet publish PgAce/PgAce.csproj -c Release -r win-x64 ^
  --self-contained true /p:PublishSingleFile=true
```
生成物:
```
dist/
  PgAce.exe
  PgAce.runtimeconfig.json
  ScintillaNET.dll
  README.md
```
`dist` フォルダを Zip し社内サーバーへ配置。

---

## 8. マイルストーン / ブランチ構成
| ブランチ | 内容 |
|----------|------|
| `main` | 常にリリース可能 |
| `feat/v1.1` | DB Explorer & DDL エクスポート |
| `feat/v1.2` | SQL Explorer |
| `feat/v1.3` | INSERT 生成 |
| `feat/v1.4` | EXPLAIN グラフビュー |

---

## 9. 受け入れ基準
1. Win11 クリーン環境で `PgAce.exe` がエラーなく起動  
2. `localhost` の `postgres` DB に保存済みパスワードで接続成功  
3. DB Explorer で 1000 件超ノードに閾値ダイアログが表示  
4. `pgbench_accounts` の列型が正しく表示される  
5. 結果グリッド → INSERT 生成 SQL が実行成功  
6. DDL エクスポート結果が本番 DB 定義と一致

---

## 10. 付録 – 参照 SQL
```sql
-- 行数付きテーブル一覧
SELECT schemaname, tablename, n_live_tup
FROM pg_stat_user_tables;

-- テーブル DDL
SELECT pg_get_tabledef('public.mst_patient'::regclass);

-- インデックス定義
SELECT indexdef FROM pg_indexes WHERE tablename = 'mst_patient';
```

---
