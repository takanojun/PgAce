
# PgAce – ポータブル PostgreSQL クライアント  
**C# / Visual Studio 2017 (.NET Framework 4.7.2) 向け 仕様書（From Scratch）**

---

## 1. 目的・スコープ
- **目的**: Windows でフォルダコピーのみで動作する **PostgreSQL 専用** SQL クライアントを C#（WinForms）で開発する。  
- **主な利用シーン**: クエリ実行、スキーマ参照（DB エクスプローラ）、DDL 取得、抽出結果からの INSERT/COPY 生成、SQL ファイル編集（SQL エクスプローラ）。  
- **非対象**: 他 DB 接続、スケジューラ/ETL、ER 図自動生成（将来拡張対象）。

---

## 2. 開発・実行環境（固定）
| 項目 | 指定 |
|---|---|
| 言語 / IDE | **C# / Visual Studio 2017 (15.9 以上推奨)** |
| C# 言語バージョン | **C# 7.3**（[プロジェクトの詳細設定] → [詳細] → [言語バージョン] で 7.3 を選択） |
| ターゲット | **.NET Framework 4.7.2** |
| OS | Windows 10 / 11 (x64) |
| DB | PostgreSQL 12 以上 |
| 配布 | **フォルダ配布（XCOPY）**。インストール不要・管理者権限不要 |
| 実行形式 | `PgAce.exe` を直接起動 |

---

## 3. 依存関係（NuGet / バイナリ）
- **Npgsql 4.1.x** … PostgreSQL ドライバ（Framework 対応）  
- **DockPanelSuite 3.x** … VS 風のドッキング UI（パネルの移動/分離/復元）  
- **ScintillaNET 3.x** … SQL エディタ（色分け/折りたたみ/行番号）  
- **Newtonsoft.Json 13.x** … `EXPLAIN (FORMAT JSON)` の解析、設定ファイル等の JSON 化  
- **CsvHelper（任意）** … CSV エクスポート補助  
- **Graphviz (dot.exe)（任意）** … 実行計画のグラフ描画（後日対応）  
- **Costura.Fody（任意）** … 可能なら DLL を内包して単一 EXE 化（管理アセンブリのみ対象）  

> アイコンは **16px PNG** をリソースに埋め込み（テーブル/ビュー/関数/インデックス等）。Segoe MDL2 Assets の併用可。

---

## 4. アーキテクチャ / ディレクトリ構成
- **UI 層**: WinForms + DockPanelSuite による 5 ペイン（全てドック/フロート可・ユーザー保存）  
- **アプリ層**: 接続管理 / メタデータ取得 / 実行 / 結果表示 / DDL 組立 / INSERT 生成 / 設定 / 履歴  
- **データ層**: Npgsql による DB 通信（接続プールは標準有効）

```
PgAce.sln
  /PgAce.App (WinForms, .NET Framework 4.7.2, C#)
    /Icons              # 16px PNG
    /Views              # 各パネル（DBExplorerView, SqlEditorView, ResultGridView, PropertiesView, SqlExplorerView）
    /Services           # DbService, MetadataService, ExplainService, InsertGenService, DdlExportService, LoggingService, SettingsService
    /Models             # DTO/設定クラス
    /Resources          # resx（i18n: EN/JA）
    app.config
```

- 永続ファイル（UTF-8）: `config.json` / `layout.json` / `history.log` / `logs/*.log`（任意）  
- パスワードは **DPAPI (CurrentUser)** で暗号化し、`config.json` に `dpapi:<Base64>` で保持（別 PC では復号不可）。

---

## 5. 画面レイアウト（初期配置 / すべて移動可）
```
+---------------- PgAce -----------------------------+
| DB Explorer |  SQL Editor Tabs           | SQL Explorer |
|-------------+---------------------------+--------------|
|             |                           |              |
|-------------+---------------------------+--------------|
| Result Grid |     Properties Panel                     |
+--------------------------------------------------------+
```
- **DB Explorer** … 左：スキーマ配下にテーブル/ビュー/マテビュー/インデックス/シーケンス/関数/型をツリー表示  
- **SQL Editor Tabs** … 中央：複数タブ編集（*.sql）・F5 実行・Ctrl+L 整形/一行化  
- **SQL Explorer** … 右：任意フォルダの `.sql` をツリー表示（ダブルクリックでタブに展開）  
- **Result Grid** … 下左：結果表示（コピー・CSV/Excel 出力・右クリックメニュー）  
- **Properties** … 下右：選択オブジェクトの詳細（DDL/Export ボタンあり）  

> パネル配置は終了時に `layout.json` へ保存し次回復元。

---

## 6. 操作（ショートカット / コンテキスト）
| 操作 | ショートカット / UI | 概要 |
|---|---|---|
| SQL 実行（選択範囲/全体） | **F5** | 選択があれば優先実行 |
| SQL 整形 ⇔ 一行化 | **Ctrl+L** | CSE の「崩し」相当 |
| 履歴表示 | **Ctrl+H** | 直近 500 件（既定） |
| EXPLAIN（テキスト/JSON） | 結果グリッド右クリック | v1.0=テキスト、以降 JSON ツリー |
| INSERT 生成 | 結果グリッド右クリック | 3 モード（後述） |
| SQL Explorer ルート設定 | メニュー [File]→[Open Folder…] | `.sql` をツリー表示/監視 |

---

## 7. 機能要件（詳細）

### 7.1 接続ダイアログ & セキュリティ
- 入力: **Host / Port / Database / User / Password**  
- **[ ] パスワードを保存** チェック時、DPAPI(CurrentUser) で暗号化して `config.json` に保存  
- 別 PC での復号は不可（空欄扱い・再入力を促す）  
- 複数プロファイル保存・ドロップダウンで切替可能

**config.json（例）※値はダミー**
```json
{
  "profiles": [
    {
      "name": "dev-local",
      "host": "127.0.0.1",
      "port": 5432,
      "database": "postgres",
      "username": "postgres",
      "password": "dpapi:BASE64ENCODED..."
    }
  ],
  "settings": {
    "savePassword": true,
    "maxHistory": 500
  }
}
```

### 7.2 DB エクスプローラ（ツリー / 遅延ロード / チャンク）
- 対象: **Schemas ▸ Tables ▸ Views ▸ MatViews ▸ Indexes ▸ Sequences ▸ Functions ▸ Types**  
- **遅延ロード**（Lazy Loading）：  
  1) 接続直後は **件数のみ** 取得（スキーマ・種別単位）  
  2) **閾値 N=1000（初期値）** 超のノードは **「► (N 件, 未ロード)」** と表示  
  3) ユーザーが展開 → ダイアログを表示  
     - **最初のチャンクをロード / 全件ロード / キャンセル**  
     - チャンク選択時に **チャンクサイズ** を入力（初期値 **500**、都度変更可）  
  4) 追加展開で **次チャンク** を非同期取得（無限スクロール）  
  5) ステータスバーに進捗（例: “Loading 500/3,240 …”）を表示  
- 非表示オブジェクトの絞込（内部スキーマ等）はトグルで切替

### 7.3 プロパティパネル（即時メタ情報 / DDL）
- **テーブル**: 列名/型/NULL/既定値/コメント、PK/FK/Unique、推定行数（`pg_class.reltuples`）、サイズ  
- **インデックス**: 対象列、並び順、Unique、部分条件、サイズ、`pg_get_indexdef(oid)`  
- **関数**: 引数、戻り値、言語、セキュリティ属性、`pg_get_functiondef(oid)` のソース  
- **操作**: **[DDL]**（読み取り専用タブに展開）、**[Export]**（SQL / Markdown 保存/コピー）

### 7.4 SQL エクスプローラ（フォルダ→タブ編集）
- [File]→[Open Folder…] でルート指定 → 右ペインにディレクトリ/ファイルツリー（`.sql`）を表示  
- ダブルクリックでエディタタブに読み込み  
- `FileSystemWatcher` で外部変更を監視し、未保存時は差分反映の確認

### 7.5 SQL エディタ（ScintillaNET）
- 色分け/折りたたみ/行番号/複数タブ/ドラッグ&ドロップ  
- Ctrl+L で整形⇔一行化（既定フォーマットは SQL 標準方言をベース）  
- 前回終了時のタブ復元（設定で ON/OFF）

### 7.6 実行 & 結果グリッド
- F5 で選択優先実行（未選択時は全体）  
- 複数ステートメントはセミコロン区切りで順次実行  
- 結果は DataGridView に表示（列幅自動・並べ替え・コピー・CSV/Excel 出力）  
- 右クリックに **EXPLAIN ANALYZE** / **EXPLAIN (FORMAT JSON)** / **INSERT 生成**

### 7.7 INSERT 生成（A5:SQL Mk-2 相当 / 3 モード）
- 対象: **結果グリッドの選択行**（複数可）  
- 出力先: **新規エディタタブ**（既定）  
- モード:
  1) **列名あり** … `INSERT INTO tbl (c1,c2) VALUES (...),(...);`  
  2) **列名なし** … `INSERT INTO tbl VALUES (...),(...);`  
  3) **COPY 形式** … ヘッダ `COPY tbl (c1,c2) FROM STDIN;` + データ + `\.`  
- フォーマット規則（概略）:  
  - 文字列 `'` の二重化、`NULL` はキーワード、`bytea` は `\x...`、日時/真偽値の表記統一  
  - COPY: **タブ区切り（既定）/CSV** を切替可能（区切り/引用/NULL 表現を設定可能）

### 7.8 DDL エクスポート（SQL / Markdown）
- プロパティパネルの **[Export]** から実行  
- SQL … `CREATE TABLE` / `ALTER TABLE`（制約/デフォルト/コメント/インデックス含む）  
- Markdown … 見出し + ```sql コードブロック```（設計書や Wiki に貼り付けやすい）  
- クリップボード/ファイル保存/新規タブの 3 通りを選択（既定: 新規タブ）

### 7.9 EXPLAIN ビューア
- **v1.0**: テキスト（`EXPLAIN`/`EXPLAIN ANALYZE`）をそのまま表示（コピー可）  
- **将来**: `EXPLAIN (FORMAT JSON)` を JSON ツリー表示（Newtonsoft.Json）  
- **任意**: Graphviz (dot.exe) があれば DOT → PNG/SVG でプランを図示

### 7.10 履歴（history.log）
- 実行日時/接続プロファイル/SQL（パスワードなどはマスク）  
- 既定 **500 件** を保持しローテーション  
- ファイルはアプリ直下（ポータブル運用）

---

## 8. 非機能要件
| 区分 | 要件 |
|---|---|
| 起動 | コールドスタート < **1 秒** |
| メモリ | アイドル < **250 MB** |
| 応答 | 1 万行の結果でも操作がストレスにならない |
| i18n | UI 文字列は EN/JA 外部化（既定 EN） |
| 例外 | ユーザー向け簡潔メッセージ + 詳細はログへ |
| 互換 | **VS2017 + .NET 4.7.2 + C# 7.3** のみでビルド可能 |

---

## 9. ビルド / 配布（C# / VS2017）
1. VS2017 でソリューションを作成（WinForms, .NET Framework 4.7.2, C#）  
2. [プロジェクトの詳細設定] → [詳細] → **言語バージョン=7.3** に設定  
3. NuGet: **Npgsql 4.1.x / DockPanelSuite 3.x / ScintillaNET 3.x / Newtonsoft.Json 13.x** を導入  
4. Release（推奨 x64）でビルド → `bin\Release\` を **Zip 配布**（XCOPY）  
5. 単一 EXE を希望する場合は **Fody + Costura.Fody** を導入（ネイティブは同梱のまま）

**配布内容（例）**
```
PgAce\
  PgAce.exe
  DockPanelSuite.dll
  ScintillaNET.dll
  Npgsql.dll (+ 依存)
  Newtonsoft.Json.dll
  /Icons
  config.json   # 初回起動で生成
  layout.json   # 起動後に生成
  history.log   # 実行後に生成
  /tools/graphviz/dot.exe   # 任意
```

---

## 10. 受け入れ基準（テスト）
1. **起動**: .NET 4.7.2 の Win11 で起動し、欠損 DLL エラーなし。  
2. **接続**: `localhost/postgres` へ接続でき、DPAPI 保存パスワードが再起動後も復号される。  
3. **DB Explorer**: 1000 件超で **閾値ダイアログ** と **チャンクサイズ入力** が表示/機能。  
4. **プロパティ**: `pgbench_accounts` で列型/PK/インデックス/コメントが正しく見える。  
5. **EXPLAIN**: テキスト結果が表示/コピーできる。  
6. **INSERT 生成**: 3 モードで生成され、実行可能（NULL/bytea/日時を含む）。  
7. **DDL エクスポート**: SQL/Markdown が期待通り（制約/コメント/インデックス含む）。  
8. **SQL Explorer**: 外部更新検知 → 再読み込み確認が出る。  
9. **レイアウト**: パネル移動が `layout.json` に保存され、次回復元。  
10. **履歴**: 500 件でローテーションされ、機微情報はマスク。

---

## 11. 実装補助（参照クエリ）
```sql
-- スキーマ別テーブル件数
SELECT n.nspname AS schema, COUNT(*) AS cnt
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE c.relkind IN ('r','p')
GROUP BY n.nspname;

-- 列情報（概要）
SELECT att.attnum, att.attname, pg_catalog.format_type(att.atttypid, att.atttypmod) AS type,
       att.attnotnull AS not_null, col_description(att.attrelid, att.attnum) AS comment
FROM pg_attribute att
WHERE att.attrelid = 'public.your_table'::regclass AND att.attnum > 0 AND NOT att.attisdropped
ORDER BY att.attnum;

-- インデックス定義
SELECT pg_get_indexdef(indexrelid) FROM pg_index WHERE indrelid='public.your_table'::regclass;

-- 関数定義
SELECT pg_get_functiondef(p.oid)
FROM pg_proc p
JOIN pg_namespace n ON n.oid = p.pronamespace
WHERE n.nspname='public' AND p.proname='your_func';
```

---

## 12. ライセンス / 表記
- PgAce 本体: **MIT License**（著作権表記をヘッダ/NOTICE に明記）  
- 依存ライブラリ: 各ライセンスに基づき `THIRD-PARTY-NOTICES.txt` を同梱

---

## 13. 将来拡張（候補）
- EXPLAIN JSON ツリー + グラフ化（Graphviz）  
- 実行計画の差分比較ビュー  
- 2 接続間のスキーマ差分（DDL 比較）  
- ER 図の自動生成（外部ツール連携含む）

---

（以上）
