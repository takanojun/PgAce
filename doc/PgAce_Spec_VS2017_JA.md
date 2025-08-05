
# PgAce – ポータブル PostgreSQL クライアント（VS2017 / .NET Framework 版）
*Visual Studio 2017 互換・Codex 実装用 仕様書*

---

## 0. 変更点（.NET 8 版からの差分）
- **ランタイム**：*.NET 8* → **.NET Framework 4.7.2**（VS2017 互換）
- **IDE**：**Visual Studio 2017 (15.x)**
- **JSON**：`System.Text.Json` → **Newtonsoft.Json**
- **Npgsql**：**4.1.x** を使用（Framework 対応版）
- **アイコン**：**16px PNG 埋め込み**（Fluent 風）または **Segoe MDL2 Assets** を優先
- **配布**：標準は **フォルダ配布**。**Costura.Fody** で単一 EXE も可能（管理アセンブリのみ）
- **グラフ**：**Graphviz (dot.exe)** を同梱すると EXPLAIN グラフ表示に対応

---

## 1. 概要と目的
PgAce は **WinForms（.NET Framework 4.7.2）** と **Visual Studio 2017** で構築する、**PostgreSQL 専用**のフォルダ配布型クライアントです。

| ID | 目標 |
|----|------|
| G1 | Zip 展開のみで実行（MSI なし） |
| G2 | CSE ライクな接続 UX（Host/IP + DB + User/PW） |
| G3 | ドッキング可能ペイン：DB Explorer / SQL Editor / Result / Properties / SQL Explorer |
| G4 | DPAPI(CurrentUser) によるパスワード暗号化保存 |
| G5 | 将来拡張（EXPLAIN JSON/Graphviz）を見据えた分離設計 |

---

## 2. 対象環境
| 項目 | 値 |
|------|----|
| OS | Windows 10/11 x64 |
| IDE | **Visual Studio 2017** |
| ランタイム | **.NET Framework 4.7.2** |
| DB | PostgreSQL 12 以上 |
| ドライバ | **Npgsql 4.1.x** |
| UI | WinForms + **DockPanelSuite 3.x** + **ScintillaNET 3.x** + PNG アイコン |
| JSON | **Newtonsoft.Json 13.x** |
| 追加 | **Graphviz**（任意・グラフ描画用） |

---

## 3. 機能一覧
（英語版と同一。略）

---

## 4. UI / UX 要件
- 初期レイアウト（前仕様と同じ）。DockPanelSuite により自由にドック／フロート可。  
- `layout.json` へ自動保存。  
- ショートカット：F5 実行、Ctrl+L 整形、Ctrl+H 履歴、グリッド右クリックで INSERT 生成。

---

## 5. 機能要件（詳細）

### 5.1 接続 & セキュリティ
- 入力：Host / Port / Database / User / Password  
- 「保存」で DPAPI(CurrentUser) 暗号化。復号不能時は再入力。

### 5.2 DB エクスプローラ（遅延ロード）
- 対象：スキーマ／テーブル／ビュー／マテビュー／インデックス／シーケンス／関数／型。  
- 閾値 **N=1000（既定）** 超は **「► (N 件, 未ロード)」** 表示。  
- 展開時ダイアログ：「最初のチャンク / 全件 / キャンセル」＋**チャンクサイズ入力**（既定 500）。  
- 追加展開で次チャンクを読み込み。件数はステータスバーに表示。

### 5.3 プロパティ
- テーブル：列／型／NULL／PK/FK／コメント／推定件数  
- インデックス：列／Unique／条件／サイズ  
- 関数：引数／戻り値／言語／ソース  
- **[DDL] [Export]** ボタン。

### 5.4 SQL エクスプローラ
- *Open Folder…* でルート設定 → `.sql` ツリー → ダブルクリックでタブ。  
- `FileSystemWatcher` で外部更新を検知し、未保存時は確認ダイアログ。

### 5.5 INSERT 生成
- 3 モード（列名あり／列名なし／COPY）。結果は新規タブに展開。

### 5.6 DDL エクスポート
- プロパティの **[Export]** → SQL / Markdown。制約・コメント含む完全 DDL。

### 5.7 EXPLAIN
- **テキスト**：`EXPLAIN/ANALYZE` をそのまま表示。  
- **JSON ツリー**：`EXPLAIN (FORMAT JSON)` を **Newtonsoft.Json** でパース。  
- **グラフ**：DOT 生成 → **dot.exe** で PNG/SVG → PictureBox 表示。

---

## 6. 非機能要件
| 区分 | 要件 |
|------|------|
| 起動 | コールドスタート < 1 秒 |
| メモリ | アイドル < 250 MB |
| ログ | `history.log`（パスワードは自動マスク） |
| i18n | 表示文字列は外部化（EN/JA） |
| ライセンス | MIT + 依存ライセンス順守 |

---

## 7. 配布（VS2017）
### A. フォルダ配布（推奨）
1. Release ビルド。  
2. `bin\Release\` 一式を Zip。  
3. 展開して `PgAce.exe` を実行。

### B. 単一 EXE（Costura.Fody）
1. NuGet: **Fody**, **Costura.Fody** を追加。  
2. ビルドすると依存 DLL を EXE に内包。  
3. ネイティブツール（Graphviz 等）は別ファイルのまま。

### 事前要件
- **.NET Framework 4.7.2** が端末にインストールされていること。無い場合は README で案内。

---

## 8. マイルストーン / ブランチ
- `main`, `feat/v1.1`, `feat/v1.2`, `feat/v1.3`, `feat/v1.4`（英語版同様）

---

## 9. 受け入れ基準
1. .NET 4.7.2 のみインストールされた Win11 で起動し、欠損 DLL エラーが出ない。  
2. DPAPI 保存済みパスワードで `localhost/postgres` 接続成功。  
3. 1000 件超で閾値ダイアログとチャンクサイズ入力が機能。  
4. `pgbench_accounts` の型が正しく表示。  
5. INSERT 生成（列名あり）を実行して成功。  
6. DDL エクスポートが定義と一致。  
7. （任意）Graphviz 同梱時にプラン画像が表示される。

---

## 10. 推奨 NuGet（VS2017 対応）
- **Npgsql 4.1.x** / **DockPanelSuite 3.x** / **ScintillaNET 3.x**  
- **Newtonsoft.Json 13.x**  
- **Fody** + **Costura.Fody**（単一 EXE 化が必要な場合）  
- **CsvHelper**（エクスポート補助に任意）
