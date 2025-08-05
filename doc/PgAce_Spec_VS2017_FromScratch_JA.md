
# PgAce – ポータブル PostgreSQL クライアント（**Visual Studio 2017 / .NET Framework 4.7.2** 版）
*Codex での自動実装・自動生成を想定した完全仕様書（日本語 / From Scratch）*

---

## 1. 目的・スコープ
- **目的**: Windows 環境でフォルダコピーだけで動作する、**PostgreSQL 専用**の軽量 SQL クライアントを作る。  
- **想定利用**: 開発・運用現場でのクエリ実行、スキーマ参照、DDL 取得、INSERT/COPY スクリプト作成、SQL ファイル編集。  
- **非対象**: 他 DB、ETL、自動バックアップ、スケジューラ。

---

## 2. 対象環境（固定）
| 項目 | 指定 |
|---|---|
| IDE | **Visual Studio 2017 (15.x)** |
| ランタイム | **.NET Framework 4.7.2** |
| OS | Windows 10 / 11 (x64) |
| DB | PostgreSQL 12 以上 |
| 配布形態 | **フォルダ配布（XCOPY）**。インストール不要 |
| 実行形態 | `PgAce.exe` を直接起動（管理者権限不要） |

---

## 3. 依存コンポーネント（NuGet / バイナリ）
- **Npgsql 4.1.x** … PostgreSQL ドライバ (.NET Framework 対応)  
- **DockPanelSuite 3.x** … ドッキング UI（VS 風レイアウト）  
- **ScintillaNET 3.x** … SQL エディタ（シンタックスハイライト/折りたたみ）  
- **Newtonsoft.Json 13.x** … JSON 処理 (`EXPLAIN (FORMAT JSON)` 解析 等)  
- **CsvHelper 27.x 以降（任意）** … CSV エクスポート補助  
- **Graphviz (dot.exe)（任意）** … 将来バージョンでの実行計画グラフ化用（同梱ファイルとして置くだけ）  
- **Costura.Fody（任意）** … 可能なら DLL 内包で単一 EXE 化（管理アセンブリのみ）  

> 画像アイコンは **16px PNG** をアセンブリに埋め込み。Font 系（Segoe MDL2 Assets）も併用可。

---

## 4. アーキテクチャ概要
- **UI**: WinForms + DockPanelSuite による 5 パネル構成（すべてドック/フロート可、ユーザー保存）。  
- **データアクセス**: Npgsql（プール有効、非同期 API は使用しない方針でも可）。  
- **設定/保存**: `config.json` / `layout.json` / `history.log`（UTF-8）。  
- **セキュリティ**: パスワードは **DPAPI (CurrentUser)** で暗号化し `config.json` に `dpapi:<Base64>` 形式で保存。  
- **ロギング**: アプリ直下に `logs/` を作成し、例外やドライバエラーをロールリング記録（任意）。

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
- **DB Explorer** … 左ペイン／スキーマ・テーブル・インデックス・関数等のツリー  
- **SQL Editor Tabs** … 中央／複数タブ・*.sql 編集・実行  
- **SQL Explorer** … 右ペイン／指定フォルダ配下の `.sql` をツリー表示（ダブルクリックで開く）  
- **Result Grid** … 下左／実行結果（コピー・保存・エクスポート）  
- **Properties Panel** … 下右／選択オブジェクトのメタ情報（DDL/Export ボタン付）  

> レイアウトは **DockPanelSuite** で実装し、終了時に `layout.json` に保存・次回復元。

---

## 6. 操作体系（ショートカット）
| 操作 | ショートカット | 備考 |
|---|---|---|
| SQL 実行（選択範囲/なければ全体） | **F5** | |
| SQL 整形 ⇔ 一行化（“崩し”） | **Ctrl + L** | `sql-formatter` 相当の整形器を内蔵 or 既存ロジック |
| 履歴パネル表示 | **Ctrl + H** | 直近 500 件（既定） |
| 結果 → INSERT 生成 | 右クリック | 後述の 3 モード |
| EXPLAIN（テキスト） | 右クリック | v1.0 はテキスト、将来 JSON/グラフ |

---

## 7. 機能要件（詳細）

### 7.1 接続ダイアログ & セキュリティ
- フィールド: **Host / Port / Database / User / Password**  
- チェック: **[ ] パスワードを保存** → DPAPI (CurrentUser) で暗号化して `config.json` へ  
- 復号失敗（他 PC 持ち出し/OS 再インストール等）時はパスワードを空欄にし、再入力を促す  
- 成功後はプロファイルとして保存（複数保持・ドロップダウン選択可）

**設定ファイル（例）**
```json
{
  "profiles": [
    {
      "name": "dev-local",
      "host": "127.0.0.1",
      "port": 5432,
      "database": "postgres",
      "username": "postgres",
      "password": "dpapi:BASE64..."
    }
  ],
  "settings": {
    "savePassword": true,
    "maxHistory": 500
  }
}
```

### 7.2 DB エクスプローラ（ツリー & 遅延ロード）
- **対象**: Schemas ▸ Tables ▸ Views ▸ Materialized Views ▸ Indexes ▸ Sequences ▸ Functions ▸ Types  
- **ロード戦略（Lazy Loading）**  
  1) 接続直後は **件数のみ** 取得（スキーマ別/種別別の総数）  
  2) **閾値 N（既定 1000）超** のノードは「**► (N 件, 未ロード)**」と表示  
  3) ユーザーが展開した際に **ダイアログ** を表示:  
     - 「**最初のチャンクをロード** / **全件ロード** / **キャンセル**」  
     - チャンク選択時は **チャンクサイズ** を入力（既定 500・毎回変更可）  
  4) 追加展開時に次チャンクを **非同期追加**（無限スクロール風）  
  5) ステータスバーに「読み込み中… (現在 X / 全体 N)」を表示  
- **フィルタ**: 非表示オブジェクト（内部用スキーマ等）をトグルで除外可能

> メタデータは `pg_catalog` / `information_schema` を用いる。巨大環境でも初期描画が軽いことを重視。

### 7.3 プロパティパネル（選択オブジェクトの詳細）
- **テーブル**: 列名 / 型 / Nullable / 既定値 / コメント、**PK/FK/Unique**、推定行数 (`pg_class.reltuples`)、サイズ  
- **インデックス**: 対象列、順序、Unique、部分インデックス条件、サイズ、`pg_get_indexdef(oid)`  
- **関数**: 引数、戻り値、言語、セキュリティ属性、`pg_get_functiondef(oid)` のソース  
- **ボタン**:  
  - **[DDL]** … ビュー上に展開（読み取り専用タブ）  
  - **[Export]** … **SQL** / **Markdown** を選択して保存 or クリップボード

> テーブルの DDL は `pg_catalog` 情報と `pg_get_*def` 系を組み合わせて構築（単一関数での完全 DDL は存在しないため `pg_dump` には依存しない）。

### 7.4 SQL エクスプローラ（ファイルツリー）
- **File → Open Folder…** で任意フォルダをルートに設定  
- 右ペインにディレクトリ/ファイルツリーを表示（`.sql` を対象）  
- ダブルクリックで中央のエディタタブに開く  
- `FileSystemWatcher` で外部変更を検知し、未保存の場合は再読み込み確認

### 7.5 SQL エディタ
- ScintillaNET による **SQL ハイライト・折りたたみ・行番号・タブ**  
- **Ctrl + L** で「整形 ⇔ 一行化」（CSE の“崩し”相当）  
- タブは自動復元（前回終了時の開きっぱなしも復元可能・任意設定）

### 7.6 実行 & 結果グリッド
- **F5** で選択 SQL を優先実行（未選択時は全体）  
- 複数ステートメントはセミコロン区切りで順次実行  
- 結果は DataGridView に表示（列幅自動/並べ替え/コピー/CSV・Excel 出力）  
- 右クリックに **EXPLAIN**（`EXPLAIN ANALYZE`/`EXPLAIN (FORMAT JSON)` 選択）

### 7.7 INSERT 文生成（**3 モード** / A5:SQL Mk-2 相当）
**対象**: 結果グリッドで選択中の行（複数可）。出力先は **新規エディタタブ**（既定）。

| モード | 生成例 | 備考 |
|---|---|---|
| 1) 列名あり | `INSERT INTO tbl (c1,c2) VALUES (...),(...);` | 既定モード |
| 2) 列名なし | `INSERT INTO tbl VALUES (...),(...);` | テーブル列順を信頼できる時 |
| 3) COPY 形式 | `COPY tbl (c1,c2) FROM STDIN; ... \.` | 大量投入向け・タブ区切り/CSV 切替可 |

**フォーマット規則**
- 文字列は単一引用符 `'`、内部の `'` は `''` エスケープ。  
- `NULL` はキーワード `NULL` を出力（空文字と区別）。  
- 日時は `YYYY-MM-DD HH:MM:SS.ffffff`（タイムゾーン列は `+09` 等も出力）。  
- `bytea` は `\xDEADBEEF`（16進）で出力。  
- 真偽値は `TRUE`/`FALSE`。  
- COPY 形式は **タブ区切り**（既定）/ **CSV** を選択可能。

### 7.8 DDL エクスポート
- プロパティパネルの **[Export]** ボタンから実行  
- **形式**:  
  - **SQL** … `CREATE TABLE` / `ALTER TABLE`（制約・デフォルト・コメント・インデックス含む）  
  - **Markdown** … オブジェクト見出し + ```sql コードブロック```  
- **出力先**: クリップボード / ファイル保存 / 新規タブ（既定: 新規タブ）

### 7.9 EXPLAIN ビューア
- **v1.0**: `EXPLAIN` / `EXPLAIN ANALYZE` のテキストそのまま表示（コピー可能）  
- **将来**: `EXPLAIN (FORMAT JSON)` を **Newtonsoft.Json** で解析し、TreeView で表示  
- **任意**: Graphviz (dot.exe) が同梱されていれば DOT 生成 → PNG/SVG を画像表示

### 7.10 履歴（history.log）
- 実行日時、プロファイル名、SQL（一部マスク可）を追記。最大 **500 件（既定）** を保持。  
- ローテーション: 上限超過時に古いものから削除。  
- **マスク例**: `password = '***'` / `using key '***'`（単純ルールでよい）

---

## 8. 非機能要件
| 区分 | 要件 |
|---|---|
| 起動 | コールドスタート < **1 秒**（標準的 PC） |
| メモリ | アイドル < **250 MB** |
| 応答 | 1 万行程度の SELECT 表示が体感ストレスなく操作可能 |
| i18n | UI テキストは外部化（EN/JA）。既定は英語 |
| 例外 | ユーザーフレンドリーなメッセージ + 詳細はログへ |
| 互換 | VS2017 + .NET 4.7.2 のみでビルド可能 |

---

## 9. ビルド & 配布（**VS2017 専用**）
### 9.1 プロジェクト構成（推奨）
```
PgAce.sln
  /PgAce.App (WinForms .NET Framework 4.7.2)
    /Icons      (16px PNG)
    /Views      (各パネル)
    /Services   (DB, DDL, Explain, InsertGen, Logging, Settings)
    /Models     (DTO/設定)
    /Resources  (resx/i18n)
    app.config
```

### 9.2 手順
1. VS2017 でソリューションを開く → ターゲット フレームワーク **.NET Framework 4.7.2**  
2. NuGet: **Npgsql 4.1.x / DockPanelSuite 3.x / ScintillaNET 3.x / Newtonsoft.Json 13.x** を導入  
3. Release ビルド（AnyCPU も可だが推奨は **x64**）  
4. `bin\Release\` をそのまま Zip 配布（**フォルダ配布**）  
5. 単一 EXE が必要なら **Fody + Costura.Fody** を追加（ネイティブは内包不可）

### 9.3 配布物（既定）
```
PgAce\
  PgAce.exe
  DockPanelSuite.dll
  ScintillaNET.dll
  Npgsql.dll (+ 依存)
  Newtonsoft.Json.dll
  /Icons
  config.json   (初回起動で生成)
  layout.json   (起動後に生成)
  history.log   (実行後に生成)
```
> Graphviz 利用時は `\tools\graphviz\dot.exe` を同梱し、設定でパス指定可。

---

## 10. 受け入れ基準（テスト観点）
1. **起動**: クリーンな Win11（.NET 4.7.2 のみ）で起動し、欠損 DLL の警告が出ない。  
2. **接続**: `localhost/postgres` へ接続し、**保存パスワード（DPAPI）**が再起動後も復号される。  
3. **DB Explorer**: 大量オブジェクト環境で **閾値ダイアログ** と **チャンクサイズ入力** が動作。  
4. **プロパティ**: `pgbench_accounts` で列型・PK・インデックスが正しく表示。  
5. **EXPLAIN**: テキスト出力が正しく表示・コピーできる。  
6. **INSERT 生成**: 3 モードが正しく出力され、実行可能（NULL/bytea/日時を含む）。  
7. **DDL エクスポート**: SQL/Markdown が期待どおり（制約・コメント含む）。  
8. **SQL Explorer**: 外部での保存を検知し、再読み込みダイアログが出る。  
9. **レイアウト**: パネル配置変更が `layout.json` に保存され、次回復元される。  
10. **履歴**: `history.log` が 500 件でローテートされ、パスワード類はマスク。

---

## 11. 参考クエリ（実装補助）
```sql
-- スキーマ別オブジェクト件数（例: テーブル）
SELECT n.nspname AS schema, COUNT(*) AS cnt
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE c.relkind IN ('r','p')
GROUP BY n.nspname;

-- 列情報
SELECT att.attnum, att.attname, pg_catalog.format_type(att.atttypid, att.atttypmod) AS type,
       att.attnotnull AS not_null, def.adsrc AS default_expr, col_description(att.attrelid, att.attnum) AS comment
FROM pg_attribute att
LEFT JOIN pg_attrdef def ON def.adrelid = att.attrelid AND def.adnum = att.attnum
WHERE att.attrelid = 'public.your_table'::regclass AND att.attnum > 0 AND NOT att.attisdropped
ORDER BY att.attnum;

-- インデックス定義
SELECT i.indexrelid::regclass AS index_name, pg_get_indexdef(i.indexrelid) AS indexdef
FROM pg_index i
WHERE i.indrelid = 'public.your_table'::regclass;

-- 関数定義
SELECT pg_get_functiondef(p.oid)
FROM pg_proc p
JOIN pg_namespace n ON n.oid = p.pronamespace
WHERE n.nspname='public' AND p.proname='your_func';
```

---

## 12. ライセンスと表記
- PgAce 本体: **MIT License**（ヘッダに著作権表記）  
- 依存ライブラリ: 各ライセンス（MIT/BSD/Apache 等）を `THIRD-PARTY-NOTICES.txt` に記載

---

## 13. 今後の拡張（任意）
- EXPLAIN JSON の可視化（ツリー/棒グラフ）  
- 実行計画の差分比較ビュー  
- スキーマ比較（2 接続間）  
- ER 図自動生成（将来）

---

（以上）
